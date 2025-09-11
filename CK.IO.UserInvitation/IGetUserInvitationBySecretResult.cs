using CK.Cris;

namespace CK.IO.UserInvitation;

public interface IGetUserInvitationBySecretResult : IStandardResultPart
{
    IPartialUserInvitation? Invitation { get; set; }
}
