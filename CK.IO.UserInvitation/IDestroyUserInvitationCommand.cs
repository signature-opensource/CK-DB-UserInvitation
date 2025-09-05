using CK.Auth;
using CK.Cris;
using CK.TypeScript;

namespace CK.IO.UserInvitation;

[TypeScriptType]
public interface IDestroyUserInvitationCommand : ICommand, ICommandAuthNormal
{
    int InvitationId { get; set; }
}
