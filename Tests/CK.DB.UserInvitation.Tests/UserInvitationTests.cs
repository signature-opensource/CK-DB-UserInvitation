using CK.Core;
using CK.IO.UserInvitation;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace CK.DB.UserInvitation.Tests;

[TestFixture]
public class UserInvitationTests
{
    [Test]
    public async Task Invitation_creator_can_get_Secret_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var pkg = services.GetRequiredService<Package>();
        var userInvitationTable = services.GetRequiredService<UserInvitationTable>();
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

        var invitation = await pkg.CreateUserInvitationAsync( ctx, cmd );
        invitation.CreatedById.ShouldBe( cmd.ActorId!.Value );

        var resultSecret = await userInvitationTable.GetUserInvitationSecretAsync( ctx, invitation.CreatedById, invitation.InvitationId );
        resultSecret.ShouldNotBeNull();
    }

    [Test]
    public async Task Anonymous_cannot_get_Secret_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var userInvitationTable = services.GetRequiredService<UserInvitationTable>();
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
        var invitation = await pkg.CreateUserInvitationAsync( ctx, cmd );
        invitation.CreatedById.ShouldBe( cmd.ActorId!.Value );

        var resultSecret = await userInvitationTable.GetUserInvitationSecretAsync( ctx, 0, invitation.InvitationId );
        resultSecret.ShouldBeNull();
    }

    static DateTime Tomorrow => DateTime.UtcNow.AddDays( 1 );

    static string NewGuid => Guid.NewGuid().ToString();
}
