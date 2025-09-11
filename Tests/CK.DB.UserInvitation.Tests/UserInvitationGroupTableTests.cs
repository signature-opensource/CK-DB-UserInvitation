using CK.Core;
using CK.DB.Actor;
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
public class UserInvitationGroupTableTests
{
    [Test]
    public async Task Can_add_a_group_to_invitation_Async()
    {
        var dir = SharedEngine.Map.StObjs.Obtain<PocoDirectory>();
        Throw.DebugAssert( dir is not null );
        var pkg = SharedEngine.Map.StObjs.Obtain<Package>();
        Throw.DebugAssert( pkg is not null );
        var groupTable = SharedEngine.Map.StObjs.Obtain<GroupTable>();
        Throw.DebugAssert( groupTable is not null );
        var userInvitationGroupTable = SharedEngine.Map.StObjs.Obtain<UserInvitationGroupTable>();
        Throw.DebugAssert( userInvitationGroupTable is not null );
        //using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        //var services = scopedServices.ServiceProvider;
        //var dir = services.GetRequiredService<PocoDirectory>();
        //var pkg = services.GetRequiredService<Package>();
        //var groupTable = services.GetRequiredService<GroupTable>();
        //var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
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

        int groupId = await groupTable.CreateGroupAsync( ctx, 1 );
        groupId.ShouldBeGreaterThan( 0 );

        var groups = await ctx.GetConnectionController( pkg ).QueryAsync<int>(
            @"select GroupId from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        groups.ShouldBeEmpty();

        await userInvitationGroupTable.AddGroupAsync( ctx, 1, result.InvitationId, groupId );

        groups = await ctx.GetConnectionController( userInvitationGroupTable ).QueryAsync<int>(
            @"select GroupId from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        groups.Count( i => i == groupId).ShouldBe( 1 );
    }

    [Test]
    public async Task Cannot_invite_to_group_0_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
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

        await Util.Awaitable( () => userInvitationGroupTable.AddGroupAsync( ctx, 1, result.InvitationId, groupId: 0 ) )
            .ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Cannot_add_group_to_invitation_0_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
        var groupTable = services.GetRequiredService<GroupTable>();
        using var ctx = new SqlStandardCallContext();

        int groupId = await groupTable.CreateGroupAsync( ctx, 1 );
        groupId.ShouldBeGreaterThan( 0 );

        await Util.Awaitable( () => userInvitationGroupTable.AddGroupAsync( ctx, 1, invitationId: 0, groupId ) )
            .ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Anonymous_cannot_add_group_to_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var groupTable = services.GetRequiredService<GroupTable>();
        var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
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

        int groupId = await groupTable.CreateGroupAsync( ctx, 1 );
        groupId.ShouldBeGreaterThan( 0 );

        var groups = await ctx.GetConnectionController( userInvitationGroupTable ).QueryAsync<int>(
            @"select GroupId from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        groups.ShouldBeEmpty();

        await Util.Awaitable( () => userInvitationGroupTable.AddGroupAsync( ctx, 0, result.InvitationId, groupId ) )
            .ShouldThrowAsync<Exception>();

        groups = await ctx.GetConnectionController( userInvitationGroupTable ).QueryAsync<int>(
            @"select GroupId from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        groups.ShouldBeEmpty();
    }

    [Test]
    public async Task Add_group_to_invitation_is_idempotent_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var groupTable = services.GetRequiredService<GroupTable>();
        var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
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

        int groupId = await groupTable.CreateGroupAsync( ctx, 1 );
        groupId.ShouldBeGreaterThan( 0 );

        int countGroup = await ctx.GetConnectionController( userInvitationGroupTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countGroup.ShouldBe( 0 );

        for( int i = 0; i < 10; i++ )
        {
            await userInvitationGroupTable.AddGroupAsync( ctx, 1, result.InvitationId, groupId );

            countGroup = await ctx.GetConnectionController( userInvitationGroupTable ).QuerySingleOrDefaultAsync<int>(
                @"select count(*) from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
                new { result.InvitationId } );

            countGroup.ShouldBe( 1 );
        }
    }

    [Test]
    public async Task Can_delete_a_group_to_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var groupTable = services.GetRequiredService<GroupTable>();
        var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
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

        int groupId = await groupTable.CreateGroupAsync( ctx, 1 );
        groupId.ShouldBeGreaterThan( 0 );

        int countGroup = await ctx.GetConnectionController( userInvitationGroupTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countGroup.ShouldBe( 0 );

        await userInvitationGroupTable.AddGroupAsync( ctx, 1, result.InvitationId, groupId );

        countGroup = await ctx.GetConnectionController( userInvitationGroupTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countGroup.ShouldBe( 1 );

        await userInvitationGroupTable.DeleteGroupAsync( ctx, 1, result.InvitationId, groupId );

        countGroup = await ctx.GetConnectionController( userInvitationGroupTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countGroup.ShouldBe( 0 );
    }

    [Test]
    public async Task Delete_group_to_invitation_is_idempotent_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var groupTable = services.GetRequiredService<GroupTable>();
        var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
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

        int groupId = await groupTable.CreateGroupAsync( ctx, 1 );
        groupId.ShouldBeGreaterThan( 0 );

        await userInvitationGroupTable.AddGroupAsync( ctx, 1, result.InvitationId, groupId );

        int countGroup = await ctx.GetConnectionController( userInvitationGroupTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countGroup.ShouldBe( 1 );

        for( int i = 0; i < 10; i++ )
        {
            await userInvitationGroupTable.DeleteGroupAsync( ctx, 1, result.InvitationId, groupId );

            countGroup = await ctx.GetConnectionController( userInvitationGroupTable ).QuerySingleOrDefaultAsync<int>(
                @"select count(*) from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
                new { result.InvitationId } );

            countGroup.ShouldBe( 0 );
        }
    }

    [Test]
    public async Task Anonymous_cannot_delete_group_invitation_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var groupTable = services.GetRequiredService<GroupTable>();
        var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
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

        int groupId = await groupTable.CreateGroupAsync( ctx, 1 );
        groupId.ShouldBeGreaterThan( 0 );

        await userInvitationGroupTable.AddGroupAsync( ctx, 1, result.InvitationId, groupId );

        int countGroup = await ctx.GetConnectionController( userInvitationGroupTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countGroup.ShouldBe( 1 );

        await Util.Awaitable( () => userInvitationGroupTable.DeleteGroupAsync( ctx, 0, result.InvitationId, groupId ) )
            .ShouldThrowAsync<Exception>();

        countGroup = await ctx.GetConnectionController( userInvitationGroupTable ).QuerySingleOrDefaultAsync<int>(
            @"select count(*) from CK.tUserInvitationGroup where InvitationId = @InvitationId;",
            new { result.InvitationId } );

        countGroup.ShouldBe( 1 );
    }

    [Test]
    public async Task Cannot_delete_invitation_to_group_0_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
        using var ctx = new SqlStandardCallContext();

        await Util.Awaitable( () => userInvitationGroupTable.DeleteGroupAsync( ctx, 0, invitationId: 1, groupId: 0 ) )
            .ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Cannot_delete_to_invitation_0_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var userInvitationGroupTable = services.GetRequiredService<UserInvitationGroupTable>();
        using var ctx = new SqlStandardCallContext();

        await Util.Awaitable( () => userInvitationGroupTable.DeleteGroupAsync( ctx, 0, invitationId: 0, groupId: 1 ) )
            .ShouldThrowAsync<Exception>();
    }

    static DateTime Tomorrow => DateTime.UtcNow.AddDays( 1 );

    static string NewGuid => Guid.NewGuid().ToString();
}
