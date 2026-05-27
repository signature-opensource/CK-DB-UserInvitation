using CK.Auth;
using CK.Cris;

namespace CK.IO.UserInvitation;

public interface IDestroyUserInvitationCommand : ICommand<IDestroyUserInvitationCommandResult>, ICommandAuthNormal
{
    int InvitationId { get; set; }
}

public interface IDestroyUserInvitationCommandResult : IStandardResultPart { }
