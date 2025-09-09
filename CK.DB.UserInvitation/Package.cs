using CK.Core;
using CK.Cris;
using CK.IO.UserInvitation;
using CK.SqlServer;
using Dapper;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.UserInvitation;

[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
public abstract class Package : SqlPackage
{
    void StObjConstruct( CK.DB.Actor.Package actor, CK.DB.Auth.Package auth )
    {
    }

    [InjectObject, AllowNull]
    public UserInvitationTable UserInvitationTable { get; protected set; }

    [InjectObject, AllowNull]
    public UserInvitationAuthProviderTable UserInvitationAuthProviderTable { get; protected set; }

    [InjectObject, AllowNull]
    public UserInvitationGroupTable UserInvitationGroupTable { get; protected set; }

    [SqlProcedure( "sUserInvitationCreate" )]
    protected abstract Task<int> CreateUserInvitationAsync( ISqlCallContext ctx, [ParameterSource] ICreateUserInvitationCommand cmd, byte[] secret );

    [CommandHandler]
    public async Task<IUserInvitation> CreateUserInvitationAsync( ISqlTransactionCallContext ctx, ICreateUserInvitationCommand cmd )
    {
        Throw.DebugAssert( cmd.ActorId is not null );

        using( var transaction = ctx.GetConnectionController( this ).BeginTransaction() )
        {
            var invitationId = await CreateUserInvitationAsync( ctx, cmd, GenerateSecret( length: 12 ) );

            foreach( var provider in cmd.RestrictedProviders )
            {
                await UserInvitationAuthProviderTable.AddAuthenticationProviderAsync( ctx, cmd.ActorId.Value, invitationId, provider );
            }
            foreach( var group in cmd.GroupIdentifiers )
            {
                await UserInvitationGroupTable.AddGroupAsync( ctx, cmd.ActorId.Value, invitationId, group );
            }

            var invitation = await GetUserInvitationAsync( ctx, cmd.ActorId.Value, invitationId );
            // TODO: Wait that Spi implements this awesome method to create a poco from a poco.
            //var invitation = await GetUserInvitationAsync( ctx, cmd.Create<IGetUserInvitationQCommand>( i => i.InvitationId = invitationId ) );

            if( invitation is null )
            {
                throw new InvalidOperationException( $"Cannot obtain user invitation with id {invitationId}." );
            }

            transaction.Commit();

            return invitation;
        }
    }

    [CommandHandler]
    [SqlProcedure( "sUserInvitationExpirationDateSet" )]
    public abstract Task SetUserInvitationExpirationDateAsync( ISqlCallContext ctx, [ParameterSource] ISetUserInvitationExpirationDateCommand cmd );

    [CommandHandler]
    [SqlProcedure( "sUserInvitationIsActiveSet" )]
    public abstract Task SetUserInvitationIsActiveAsync( ISqlCallContext ctx, [ParameterSource] ISetUserInvitationIsActiveCommand cmd );

    [CommandHandler]
    public async Task<IGetUserInvitationBySecretResult?> GetUserInvitationBySecretAsync( ISqlCallContext ctx, UserMessageCollector collector, IGetUserInvitationBySecretQCommand cmd )
    {
        var invitation = await GetUserInvitationAsync( ctx, Encoding.UTF8.GetBytes( cmd.Secret ) );

        if( invitation is null )
        {
            collector.Error( "Invitation not found.", "UserInvitation.InvitationNotFound" );
        }
        else
        {
            if( invitation.ExpirationDateUtc < DateTime.UtcNow )
            {
                collector.Error( "Invitation has expired.", "UserInvitation.InvitationExpired" );
            }

            if( !invitation.IsActive )
            {
                collector.Error( "Invitation is inactive.", "UserInvitation.InvitationInactive" );
            }
        }

        var result = cmd.CreateResult( r =>
        {
            r.Invitation = collector.ErrorCount is 0 ? invitation : null;
        } );
        result.SetUserMessages( collector );

        return result;
    }

    public async Task<IUserInvitation?> GetUserInvitationAsync( ISqlCallContext ctx, byte[] secret )
    {
        IUserInvitation? invitation = null;

        await ctx.GetConnectionController( UserInvitationTable ).QueryAsync(
            @"select ui.InvitationId
                    ,ui.CreatedById
                    ,ui.UserTargetAddress
                    ,ui.ExpirationDateUtc
                    ,ui.IsActive
                    ,ui.LCID
                    ,uig.GroupId
                    ,ap.ProviderName
              from CK.tUserInvitation ui
              left join CK.tUserInvitationGroup uig on ui.InvitationId = uig.InvitationId
              left join CK.tUserInvitationAuthProvider uiap on ui.InvitationId = uiap.InvitationId
              left join CK.tAuthProvider ap on uiap.ProviderName like ap.ProviderName + '%'
              where ui.Secret = @Secret;",
            GetMapper( inv => invitation ??= inv ),
            new { Secret = secret },
            splitOn: "GroupId,ProviderName" );

        return invitation;
    }

    [CommandHandler]
    public Task<IUserInvitation?> GetUserInvitationAsync( ISqlCallContext ctx, IGetUserInvitationQCommand cmd )
    {
        Throw.DebugAssert( cmd.ActorId is not null );

        return GetUserInvitationAsync( ctx, cmd.ActorId.Value, cmd.InvitationId );
    }

    async Task<IUserInvitation?> GetUserInvitationAsync( ISqlCallContext ctx, int actorId, int invitationId )
    {
        IUserInvitation? invitation = null;

        await ctx.GetConnectionController( UserInvitationTable ).QueryAsync(
            @"select InvitationId
                        ,CreatedById
                        ,UserTargetAddress
                        ,ExpirationDateUtc
                        ,IsActive
                        ,LCID
                        ,GroupId
                        ,ProviderName
                  from CK.fGetUserInvitationByUser( @ActorId )
                  where InvitationId = @InvitationId;",
            GetMapper( inv => invitation ??= inv ),
            new { ActorId = actorId, InvitationId = invitationId },
            splitOn: "GroupId,ProviderName" );

        return invitation;
    }

    [CommandHandler]
    public async Task<IList<IUserInvitation>> GetUserInvitationsAsync( ISqlCallContext ctx, IGetUserInvitationsQCommand cmd )
    {
        Throw.DebugAssert( cmd.ActorId is not null );

        var invitations = new Dictionary<int, IUserInvitation>();

        await ctx.GetConnectionController( UserInvitationTable ).QueryAsync(
            @"select InvitationId
                        ,CreatedById
                        ,UserTargetAddress
                        ,ExpirationDateUtc
                        ,IsActive
                        ,LCID
                        ,GroupId
                        ,ProviderName
                  from CK.fGetUserInvitationByUser( @ActorId );",
            GetMapper( inv =>
            {
                if( !invitations.TryGetValue( inv.InvitationId, out var invitation ) )
                {
                    invitations.Add( inv.InvitationId, invitation = inv );
                }
                return invitation;
            } ),
            new { ActorId = cmd.ActorId.Value },
            splitOn: "GroupId,ProviderName" );

        return invitations.Values.ToList();
    }

    public async Task<IUserInvitation?> GetUserInvitationAsync( ISqlCallContext ctx, int actorId, string userTargetAddress )
    {
        IUserInvitation? invitation = null;

        await ctx.GetConnectionController( UserInvitationTable ).QueryAsync(
            @"select InvitationId
                        ,CreatedById
                        ,UserTargetAddress
                        ,ExpirationDateUtc
                        ,IsActive
                        ,LCID
                        ,GroupId
                        ,ProviderName
                  from CK.fGetUserInvitationByUser( @ActorId )
                  where UserTargetAddress = @UserTargetAddress;",
            GetMapper( inv => invitation ??= inv ),
            new { ActorId = actorId, UserTargetAddress = userTargetAddress },
            splitOn: "GroupId,ProviderName" );

        return invitation;
    }

    [CommandHandler]
    [SqlProcedure( "sUserInvitationDestroy" )]
    public abstract Task DestroyUserInvitationAsync( ISqlCallContext ctx, [ParameterSource] IDestroyUserInvitationCommand cmd );

    static Func<IUserInvitation, int?, string, IUserInvitation> GetMapper( Func<IUserInvitation, IUserInvitation> getInvitation )
    {
        return ( IUserInvitation inv, int? groupId, string providerName ) =>
        {
            var invitation = getInvitation( inv );
            if( groupId is not null && !invitation.GroupIdentifiers.Contains( groupId.Value ) )
            {
                invitation.GroupIdentifiers.Add( groupId.Value );
            }
            if( providerName is not null && !invitation.RestrictedProviders.Contains( providerName ) )
            {
                invitation.RestrictedProviders.Add( providerName );
            }
            return invitation;
        };
    }

    static byte[] GenerateSecret( int length )
    {
        var requiredEntropy = 3 * length / 4 + 1;
        var safeSize = Base64.GetMaxEncodedToUtf8Length( length );
        Span<byte> buffer = new byte[safeSize];
        RandomNumberGenerator.Fill( buffer.Slice( 0, requiredEntropy ) );
        Base64.EncodeToUtf8InPlace( buffer, requiredEntropy, out int bytesWritten );
        Base64UrlHelper.UncheckedBase64ToUrlBase64NoPadding( buffer, ref bytesWritten );
        Debug.Assert( bytesWritten > length );
        return buffer.Slice( 0, length ).ToArray();
    }
}
