using CK.Core;
using CK.IO.UserInvitation;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.UserInvitation.Tests;

/// <summary>
/// An invitation records the party through which it was issued (its PartyActorId): 0 when it is unbound.
/// </summary>
[TestFixture]
public class IssuingPartyTests
{
    [Test]
    public async Task an_invitation_records_its_issuing_party_and_a_resend_can_change_it_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;
        var dir = services.GetRequiredService<PocoDirectory>();
        var pkg = services.GetRequiredService<Package>();
        var userInvitationTable = services.GetRequiredService<UserInvitationTable>();
        using var ctx = new SqlTransactionCallContext();

        ICreateUserInvitationCommand NewCommand() => dir.Create<ICreateUserInvitationCommand>( c =>
        {
            c.ActorId = 1;
            c.UserTargetAddress = Guid.NewGuid().ToString();
            c.ExpirationDateUtc = DateTime.UtcNow.AddDays( 1 );
            c.IsActive = true;
            c.CultureId = 210327884;
        } );

        // The command handler: unbound.
        var unbound = await pkg.CreateUserInvitationAsync( ctx, NewCommand() );
        unbound.IssuingPartyId.ShouldBe( 0 );

        // The server code: issued through a party (an actor: the System one here).
        var bound = await pkg.CreateUserInvitationAsync( ctx, NewCommand(), issuingPartyId: 1 );
        bound.IssuingPartyId.ShouldBe( 1 );

        // Read back by its secret too.
        var secret = await userInvitationTable.GetUserInvitationSecretAsync( ctx, 1, bound.InvitationId );
        (await pkg.GetUserInvitationAsync( ctx, secret! )).ShouldNotBeNull().IssuingPartyId.ShouldBe( 1 );

        // A resend binds it to the party that resends it.
        await pkg.SetUserInvitationIssuingPartyAsync( ctx, 1, unbound.InvitationId, 1 );
        (await pkg.GetUserInvitationAsync( ctx, 1, unbound.UserTargetAddress )).ShouldNotBeNull().IssuingPartyId.ShouldBe( 1 );
    }
}
