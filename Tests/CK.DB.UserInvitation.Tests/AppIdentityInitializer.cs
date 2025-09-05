using CK.AppIdentity;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CK.DB.UserInvitation.Tests;

[SetUpFixture]
public class AppIdentityInitializer
{
    [OneTimeSetUp]
    public void Setup()
    {
        SharedEngine.AutoConfigureServices = services =>
        {
            services.AddSingleton( ApplicationIdentityServiceConfiguration.CreateEmpty() );
        };
    }
}
