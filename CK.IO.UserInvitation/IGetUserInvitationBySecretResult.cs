using CK.Cris;

namespace CK.IO.UserInvitation;

public interface IGetUserInvitationBySecretResult : IStandardResultPart
{
    IUserInvitationBySecret? Invitation { get; set; }
}
