using CK.Core;
using CK.DB.Actor;
using CK.DB.Auth;
using CK.IO.UserInvitation;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CK.DB.UserInvitation.Tests;

[TestFixture]
public class UserInvitationServiceTests
{
    [Test]
    public async Task Can_create_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var package = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
            c.GroupIdentifiers.Clear();
            c.RestrictedProviders.Clear();
        } );

        var invitation = await package.CreateUserInvitationAsync( ctx, cmd );
        invitation.ShouldNotBeNull();
        invitation!.InvitationId.ShouldBeGreaterThan( 0 );
        invitation!.CreatedById.ShouldBe( cmd.ActorId!.Value );
        invitation!.UserTargetAddress.ShouldBe( cmd.UserTargetAddress );
        invitation!.ExpirationDateUtc.ShouldBe( cmd.ExpirationDateUtc, TimeSpan.FromMilliseconds( 10 ) /* Because datetime2( 2 )*/ );
        invitation!.IsActive.ShouldBe( cmd.IsActive );
        invitation!.LCID.ShouldBe( cmd.LCID );
        invitation!.GroupIdentifiers.ShouldBeEquivalentTo( cmd.GroupIdentifiers );
        invitation!.RestrictedProviders.ShouldBeEquivalentTo( cmd.RestrictedProviders );
    }

    [Test]
    public async Task User_target_address_must_be_unique_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
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

        await Util.Awaitable( () => pkg.CreateUserInvitationAsync( ctx, cmd ) ).ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Anonymous_cannot_create_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 0;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );

        await Util.Awaitable( () => pkg.CreateUserInvitationAsync( ctx, cmd ) ).ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task User_target_address_cannot_be_empty_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = string.Empty;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );

        await Util.Awaitable( () => pkg.CreateUserInvitationAsync( ctx, cmd ) ).ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Cannot_create_invitation_with_LCID_0_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 0;
        } );

        await Util.Awaitable( () => pkg.CreateUserInvitationAsync( ctx, cmd ) ).ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Can_update_expiration_date_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
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

        result.ExpirationDateUtc.ShouldBe( cmd.ExpirationDateUtc, TimeSpan.FromMilliseconds( 10 ) );

        var cmd2 = dir.Create<ISetUserInvitationExpirationDateCommand>( c =>
        {
            c.ActorId = 1;
            c.InvitationId = result.InvitationId;
            c.NewExpirationDate = cmd.ExpirationDateUtc.AddDays( 1 );
        } );

        await pkg.SetUserInvitationExpirationDateAsync( ctx, cmd2 );

        var invitation = await pkg.GetUserInvitationAsync( ctx, dir.Create<IGetUserInvitationQCommand>( c => { c.ActorId = 1; c.InvitationId = result.InvitationId; } ) );
        invitation.ShouldNotBeNull();
        invitation!.ExpirationDateUtc.ShouldBe( cmd2.NewExpirationDate, TimeSpan.FromMilliseconds( 10 ) );
    }

    [Test]
    public async Task Anonymous_cannot_update_expiration_date_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
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

        result.ExpirationDateUtc.ShouldBe( cmd.ExpirationDateUtc, TimeSpan.FromMilliseconds( 10 ) );

        var cmd2 = dir.Create<ISetUserInvitationExpirationDateCommand>( i =>
        {
            i.ActorId = 0;
            i.InvitationId = result.InvitationId;
            i.NewExpirationDate = cmd.ExpirationDateUtc.AddDays( 1 );
        } );

        await Util.Awaitable( () => pkg.SetUserInvitationExpirationDateAsync( ctx, cmd2 ) ).ShouldThrowAsync<Exception>();

        var invitation = await pkg.GetUserInvitationAsync( ctx, dir.Create<IGetUserInvitationQCommand>( c => { c.ActorId = cmd.ActorId; c.InvitationId = result.InvitationId; } ) );
        invitation.ShouldNotBeNull();
        invitation!.ExpirationDateUtc.ShouldBe( cmd.ExpirationDateUtc, TimeSpan.FromMilliseconds( 10 ) );
    }

    [Test]
    public async Task Can_update_active_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
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

        result.IsActive.ShouldBe( cmd.IsActive );

        var cmd2 = dir.Create<ISetUserInvitationIsActiveCommand>( c =>
        {
            c.ActorId = 1;
            c.InvitationId = result.InvitationId;
            c.IsActive = !cmd.IsActive;
        } );
        await pkg.SetUserInvitationIsActiveAsync( ctx, cmd2 );

        var invitation = await pkg.GetUserInvitationAsync( ctx, dir.Create<IGetUserInvitationQCommand>( i => { i.ActorId = 1; i.InvitationId = result.InvitationId; } ) );
        invitation.ShouldNotBeNull();
        invitation!.IsActive.ShouldBe( cmd2.IsActive );
    }

    [Test]
    public async Task Anonymous_cannot_update_active_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
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

        result.IsActive.ShouldBe( cmd.IsActive );

        var cmd2 = dir.Create<ISetUserInvitationIsActiveCommand>( c =>
        {
            c.ActorId = 0;
            c.InvitationId = result.InvitationId;
            c.IsActive = !cmd.IsActive;
        } );
        await Util.Awaitable( () => pkg.SetUserInvitationIsActiveAsync( ctx, cmd2 ) ).ShouldThrowAsync<Exception>();

        var invitation = await pkg.GetUserInvitationAsync( ctx, dir.Create<IGetUserInvitationQCommand>( i => { i.ActorId = 1; i.InvitationId = result.InvitationId; } ) );
        invitation.ShouldNotBeNull();
        invitation!.IsActive.ShouldBeTrue();
    }

    [Test]
    public async Task Can_destroy_user_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
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

        await pkg.DestroyUserInvitationAsync( ctx, dir.Create<IDestroyUserInvitationCommand>( i => { i.ActorId = 1; i.InvitationId = result.InvitationId; } ) );
        var invitation = await pkg.GetUserInvitationAsync( ctx, dir.Create<IGetUserInvitationQCommand>( i => { i.ActorId = 1; i.InvitationId = result.InvitationId; } ) );
        invitation.ShouldBeNull();
    }

    [Test]
    public async Task Cannot_destroy_invitation_0_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
        } );
        await Util.Awaitable( () => pkg.DestroyUserInvitationAsync( ctx, dir.Create<IDestroyUserInvitationCommand>( i => { i.ActorId = 1; i.InvitationId = 0; } ) ) )
            .ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Anonymous_cannot_destroy_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
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

        await Util.Awaitable( () => pkg.DestroyUserInvitationAsync( ctx, dir.Create<IDestroyUserInvitationCommand>( i => { i.ActorId = 0; i.InvitationId = result.InvitationId; } ) ) )
            .ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Obtain_an_existing_invitation_by_invitation_id_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var package = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
        var groupTable = services.GetRequiredService<GroupTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlTransactionCallContext();

        var group1Id = await groupTable.CreateGroupAsync( ctx, 1 );
        var group2Id = await groupTable.CreateGroupAsync( ctx, 1 );
        var providerName1 = NewGuid;
        var provider1 = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName1, providerName1, false );
        var providerName2 = NewGuid;
        var provider2 = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName2, providerName2, false );

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
            c.GroupIdentifiers.Add( group1Id );
            c.GroupIdentifiers.Add( group2Id );
            c.RestrictedProviders.Add( providerName1 );
            c.RestrictedProviders.Add( providerName2 );
        } );
        Throw.DebugAssert( cmd.ActorId is not null );

        var result = await package.CreateUserInvitationAsync( ctx, cmd );

        var invitation = await package.GetUserInvitationAsync( ctx, dir.Create<IGetUserInvitationQCommand>( i => { i.ActorId = cmd.ActorId; i.InvitationId = result.InvitationId; } ) );
        invitation.ShouldNotBeNull();
        invitation!.InvitationId.ShouldBe( result.InvitationId );
        invitation!.CreatedById.ShouldBe( cmd.ActorId.Value );
        invitation!.UserTargetAddress.ShouldBe( cmd.UserTargetAddress );
        invitation!.ExpirationDateUtc.ShouldBe( cmd.ExpirationDateUtc, TimeSpan.FromMilliseconds( 10 ) );
        invitation!.IsActive.ShouldBe( cmd.IsActive );
        invitation!.LCID.ShouldBe( cmd.LCID );
        invitation!.GroupIdentifiers.Order().ShouldBeEquivalentTo( cmd.GroupIdentifiers.Order() );
        invitation!.RestrictedProviders.Order().ShouldBeEquivalentTo( cmd.RestrictedProviders.Order() );
    }

    [Test]
    public async Task Obtain_an_existing_invitation_by_userTargetAddress_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var package = services.GetRequiredService<Package>();
        var dir = services.GetRequiredService<PocoDirectory>();
        var groupTable = services.GetRequiredService<GroupTable>();
        var authProviderTable = services.GetRequiredService<AuthProviderTable>();
        using var ctx = new SqlTransactionCallContext();

        var group1Id = await groupTable.CreateGroupAsync( ctx, 1 );
        var group2Id = await groupTable.CreateGroupAsync( ctx, 1 );
        var providerName1 = NewGuid;
        var provider1 = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName1, providerName1, false );
        var providerName2 = NewGuid;
        var provider2 = await authProviderTable.RegisterProviderAsync( ctx, 1, providerName2, providerName2, false );

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
            c.LCID = 12;
            c.GroupIdentifiers.Add( group1Id );
            c.GroupIdentifiers.Add( group2Id );
            c.RestrictedProviders.Add( providerName1 );
            c.RestrictedProviders.Add( providerName2 );
        } );
        Throw.DebugAssert( cmd.ActorId is not null );

        var result = await package.CreateUserInvitationAsync( ctx, cmd );

        var invitation = await package.GetUserInvitationAsync( ctx, cmd.ActorId.Value, result.UserTargetAddress );
        invitation.ShouldNotBeNull();
        invitation!.InvitationId.ShouldBe( result.InvitationId );
        invitation!.CreatedById.ShouldBe( cmd.ActorId.Value );
        invitation!.UserTargetAddress.ShouldBe( cmd.UserTargetAddress );
        invitation!.ExpirationDateUtc.ShouldBe( cmd.ExpirationDateUtc, TimeSpan.FromMilliseconds( 10 ) );
        invitation!.IsActive.ShouldBe( cmd.IsActive );
        invitation!.LCID.ShouldBe( cmd.LCID );
        invitation!.GroupIdentifiers.Order().ShouldBeEquivalentTo( cmd.GroupIdentifiers.Order() );
        invitation!.RestrictedProviders.Order().ShouldBeEquivalentTo( cmd.RestrictedProviders.Order() );
    }

    [Test]
    public async Task Cannot_get_an_invitation_where_not_creator_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userTable = services.GetRequiredService<UserTable>();
        using var ctx = new SqlTransactionCallContext();

        var cmd = dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = NewGuid;
            c.ExpirationDateUtc = Tomorrow;
            c.IsActive = true;
        } );
        var result = await pkg.CreateUserInvitationAsync( ctx, cmd );

        var userId = await userTable.CreateUserAsync( ctx, 1, NewGuid );
        userId.ShouldBeGreaterThan( 1 );

        var invitation = await pkg.GetUserInvitationAsync( ctx, dir.Create<IGetUserInvitationQCommand>( i => { i.ActorId = userId; i.InvitationId = result.InvitationId; } ) );
        invitation.ShouldBeNull();
    }

    static DateTime Tomorrow => DateTime.UtcNow.AddDays( 1 );

    static string NewGuid => Guid.NewGuid().ToString();
}
