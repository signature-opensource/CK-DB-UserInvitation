using CK.Auth;
using CK.Cris;
using CK.TypeScript;

namespace CK.IO.UserInvitation;

[TypeScriptType]
public interface IGetUserInvitationQCommand : ICommand<IUserInvitation?>, ICommandAuthNormal
{
    int InvitationId { get; set; }
}
