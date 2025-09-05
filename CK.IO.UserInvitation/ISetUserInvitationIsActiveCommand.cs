using CK.Auth;
using CK.Cris;
using CK.TypeScript;

namespace CK.IO.UserInvitation;

[TypeScriptType]
public interface ISetUserInvitationIsActiveCommand : ICommand, ICommandAuthNormal
{
    int InvitationId { get; set; }

    bool IsActive { get; set; }
}
