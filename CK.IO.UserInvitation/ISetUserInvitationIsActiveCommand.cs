using CK.Auth;
using CK.Cris;

namespace CK.IO.UserInvitation;

[Internal]
public interface ISetUserInvitationIsActiveCommand : ICommand<ISetUserInvitationIsActiveCommandResult>, ICommandAuthNormal
{
    int InvitationId { get; set; }

    bool IsActive { get; set; }
}

public interface ISetUserInvitationIsActiveCommandResult : IStandardResultPart { }
