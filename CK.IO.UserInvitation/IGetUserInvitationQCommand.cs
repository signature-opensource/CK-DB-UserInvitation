using CK.Auth;
using CK.Cris;

namespace CK.IO.UserInvitation;

[Internal]
public interface IGetUserInvitationQCommand : ICommand<IUserInvitation?>, ICommandAuthNormal
{
    int InvitationId { get; set; }
}
