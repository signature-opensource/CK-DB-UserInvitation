using CK.Core;
using CK.Cris;
using CK.IO.UserInvitation;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace CK.DB.UserInvitation.Tests;

/// <summary>
/// The CRIS model of the invitation commands.
/// </summary>
[TestFixture]
public class CrisModelTests
{
    [Test]
    public void the_commands_have_their_handler_and_only_the_registration_query_is_public()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var dir = scopedServices.ServiceProvider.GetRequiredService<PocoDirectory>();

        // The handlers of these two commands had swapped result types: CRIS skipped them.
        ICrisPoco setExpiration = dir.Create<ISetUserInvitationExpirationDateCommand>();
        ICrisPoco setIsActive = dir.Create<ISetUserInvitationIsActiveCommand>();
        setExpiration.CrisPocoModel.IsHandled.ShouldBeTrue();
        setIsActive.CrisPocoModel.IsHandled.ShouldBeTrue();

        // This package has no rights model (who can give which group, who can manage which invitation): its
        // management commands never come from HTTP. The application checks the rights and calls the package.
        foreach( var c in new ICrisPoco[] { dir.Create<ICreateUserInvitationCommand>(), dir.Create<IDestroyUserInvitationCommand>(),
                                            dir.Create<IGetUserInvitationQCommand>(), dir.Create<IGetUserInvitationsQCommand>(),
                                            setExpiration, setIsActive } )
        {
            c.CrisPocoModel.Exposure.ShouldBe( CrisExposure.Internal, c.CrisPocoModel.PocoName );
        }
        // The registration reads its invitation with its secret.
        ( (ICrisPoco)dir.Create<IGetUserInvitationBySecretQCommand>() ).CrisPocoModel.Exposure.ShouldBe( CrisExposure.Public );
    }
}
