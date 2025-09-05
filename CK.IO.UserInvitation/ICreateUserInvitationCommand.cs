using CK.Auth;
using CK.Cris;
using CK.TypeScript;
using System.ComponentModel;

namespace CK.IO.UserInvitation;

[TypeScriptType]
public interface ICreateUserInvitationCommand : ICommand<IUserInvitation>, ICommandAuthNormal
{
    string UserTargetAddress { get; set; }

    DateTime ExpirationDateUtc { get; set; }

    bool IsActive { get; set; }

    IList<int> GroupIdentifiers { get; }

    IList<string> RestrictedProviders { get; }

    [DefaultValue( 12 )]
    int LCID { get; set; }
}
