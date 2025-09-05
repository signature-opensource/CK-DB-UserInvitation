using CK.Cris;
using CK.TypeScript;

namespace CK.IO.UserInvitation;

[TypeScriptType]
public interface IGetUserInvitationBySecretResult : IStandardResultPart
{
    IUserInvtationBySecret? Invitation { get; set; }
}
