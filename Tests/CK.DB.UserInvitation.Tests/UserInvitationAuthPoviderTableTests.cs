using CK.Core;
using CK.DB.Auth;
using CK.IO.UserInvitation;
using CK.SqlServer;
using CK.Testing;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CK.DB.UserInvitation.Tests;

[TestFixture]
public class UserInvitationAuthPoviderTableTests
{
    [Test]
    public async Task Can_add_an_auth_provider_to_invitation_Async()
    {

        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );
        var result = await pkg.CreateUserInvitationAsync( ctx, cmd );
        result.InvitationId.ShouldBeGreaterThan( 0 );

        string providerName = NewGuid;
        int providerId = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName, providerName, false );
        providerId.ShouldBeGreaterThan( 0 );

        var providers = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QueryAsync<string>(
            @"select ProviderName from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        providers.ShouldBeEmpty();

        await userInvitationAuthProviderTable.AddAuthenticationProviderAsync( ctx, 1, result.InvitationId, providerName );

        providers = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QueryAsync<string>(
            @"select ProviderName from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        providers.Count( i => i == providerName ).ShouldBe( 1 );
    }

    [Test]
    public async Task Cannot_invite_to_invalid_provider_name_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );
        var result = await pkg.CreateUserInvitationAsync( ctx, cmd );
        result.InvitationId.ShouldBeGreaterThan( 0 );

        await Util.Awaitable( () => userInvitationAuthProviderTable.AddAuthenticationProviderAsync( ctx, 1, result.InvitationId, providerName: string.Empty ) )
            .ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Cannot_add_auth_provider_to_invitation_0_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlStandardCallContext();

        var providerName = NewGuid;
        int providerId = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName, providerName, false );
        providerId.ShouldBeGreaterThan( 0 );

        await Util.Awaitable( () => userInvitationAuthProviderTable.AddAuthenticationProviderAsync( ctx, 1, invitationId: 0, providerName) )
            .ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Anonymous_cannot_add_auth_provider_to_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );
        var result = await pkg.CreateUserInvitationAsync( ctx, cmd );
        result.InvitationId.ShouldBeGreaterThan( 0 );

        var providerName = NewGuid;
        int providerId = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName, providerName, false );
        providerId.ShouldBeGreaterThan( 0 );

        var providers = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QueryAsync<string>(
            @"select ProviderName from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        providers.ShouldBeEmpty();

        await Util.Awaitable( () => userInvitationAuthProviderTable.AddAuthenticationProviderAsync( ctx, 0, result.InvitationId, providerName) )
            .ShouldThrowAsync<Exception>();

        providers = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QueryAsync<string>(
            @"select ProviderName from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        providers.ShouldBeEmpty();
    }

    [Test]
    public async Task Add_auth_provider_to_invitation_is_idempotent_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );
        var result = await pkg.CreateUserInvitationAsync( ctx, cmd );
        result.InvitationId.ShouldBeGreaterThan( 0 );

        var providerName = NewGuid;
        int providerId = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName, providerName, false );
        providerId.ShouldBeGreaterThan( 0 );

        int countProvider = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countProvider.ShouldBe( 0 );

        for( int i = 0; i < 10; i++ )
        {
            await userInvitationAuthProviderTable.AddAuthenticationProviderAsync( ctx, 1, result.InvitationId, providerName );

            countProvider = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QuerySingleOrDefaultAsync<int>(
                @"select count(*) from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
                new { result.InvitationId } );

            countProvider.ShouldBe( 1 );
        }
    }

    [Test]
    public async Task Can_delete_an_auth_provider_to_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );
        var result = await pkg.CreateUserInvitationAsync( ctx, cmd );
        result.InvitationId.ShouldBeGreaterThan( 0 );

        var providerName = NewGuid;
        int providerId = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName, providerName, false );
        providerId.ShouldBeGreaterThan( 0 );

        int countProvider = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countProvider.ShouldBe( 0 );

        await userInvitationAuthProviderTable.AddAuthenticationProviderAsync( ctx, 1, result.InvitationId, providerName );

        countProvider = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countProvider.ShouldBe( 1 );

        await userInvitationAuthProviderTable.DeleteAuthenticationProviderAsync( ctx, 1, result.InvitationId, providerName );

        countProvider = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countProvider.ShouldBe( 0 );
    }

    [Test]
    public async Task Delete_auth_provider_to_invitation_is_idempotent_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );
        var result = await pkg.CreateUserInvitationAsync( ctx, cmd );
        result.InvitationId.ShouldBeGreaterThan( 0 );

        var providerName = NewGuid;
        int providerId = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName, providerName, false );
        providerId.ShouldBeGreaterThan( 0 );

        await userInvitationAuthProviderTable.AddAuthenticationProviderAsync( ctx, 1, result.InvitationId, providerName );

        int countProvider = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countProvider.ShouldBe( 1 );

        for( int i = 0; i < 10; i++ )
        {
            await userInvitationAuthProviderTable.DeleteAuthenticationProviderAsync( ctx, 1, result.InvitationId, providerName );

            countProvider = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QuerySingleOrDefaultAsync<int>(
                @"select count(*) from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
                new { result.InvitationId } );

            countProvider.ShouldBe( 0 );
        }
    }

    [Test]
    public async Task Anonymous_cannot_delete_auth_provider_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );
        var result = await pkg.CreateUserInvitationAsync( ctx, cmd );
        result.InvitationId.ShouldBeGreaterThan( 0 );

        var providerName = NewGuid;
        int providerId = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName, providerName, false );
        providerId.ShouldBeGreaterThan( 0 );

        await userInvitationAuthProviderTable.AddAuthenticationProviderAsync( ctx, 1, result.InvitationId, providerName );

        int countProvider = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countProvider.ShouldBe( 1 );

        await Util.Awaitable( () => userInvitationAuthProviderTable.DeleteAuthenticationProviderAsync( ctx, 0, result.InvitationId, providerName ) )
            .ShouldThrowAsync<Exception>();

        countProvider = await ctx.GetConnectionController( userInvitationAuthProviderTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countProvider.ShouldBe( 1 );
    }

    [Test]
    public async Task Cannot_delete_invitation_to_invalid_auth_provider_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        using var ctx = new SqlStandardCallContext();

        await Util.Awaitable( () => userInvitationAuthProviderTable.DeleteAuthenticationProviderAsync( ctx, 0, invitationId: 1, providerName: string.Empty ) )
            .ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Cannot_delete_to_invitation_0_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var userInvitationAuthProviderTable = services.GetRequiredService<UserInvitationAuthProviderTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlStandardCallContext();

        var providerName = NewGuid;
        int priverId = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName, providerName, false );

        await Util.Awaitable( () => userInvitationAuthProviderTable.DeleteAuthenticationProviderAsync( ctx, 0, invitationId: 0, providerName ) )
            .ShouldThrowAsync<Exception>();
    }

    static DateTime Tomorrow => DateTime.UtcNow.AddDays( 1 );

    static string NewGuid => Guid.NewGuid().ToString();

    static byte[] EmptySecret => new byte[24];
}
