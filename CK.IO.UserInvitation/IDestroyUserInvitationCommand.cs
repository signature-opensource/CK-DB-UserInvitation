using CK.Auth;
using CK.Cris;

namespace CK.IO.UserInvitation;

public interface IDestroyUserInvitationCommand : ICommand, ICommandAuthNormal
{
    int InvitationId { get; set; }
}
