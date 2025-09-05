using CK.Core;
using CK.SqlServer;
using Dapper;
using System.Threading.Tasks;

namespace CK.DB.UserInvitation;

[SqlTable( "tUserInvitation", Package = typeof( Package ), ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "fGetUserInvitationByUser" )]
public abstract class UserInvitationTable : SqlTable
{
    void StObjConstruct( CK.DB.Actor.ActorTable actor, CK.DB.Culture.LCIDTable lcid )
    {
    }

    public Task<byte[]?> GetUserInvitationSecretAsync( ISqlCallContext ctx, int actorId, int invitationId )
    {
        return ctx.GetConnectionController( this ).QuerySingleOrDefaultAsync<byte[]?>(
            @"select tui.[Secret]
              from CK.fGetUserInvitationByUser( @ActorId ) fui
              inner join CK.tUserInvitation tui on fui.InvitationId = tui.InvitationId
              where tui.InvitationId = @InvitationId;",
            new { ActorId = actorId, InvitationId = invitationId } );
    }
}
