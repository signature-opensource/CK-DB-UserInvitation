using CK.Auth;
using CK.Cris;
using System;

namespace CK.IO.UserInvitation;

public interface ISetUserInvitationExpirationDateCommand : ICommand<ISetUserInvitationExpirationDateCommandResult>, ICommandAuthNormal
{
    int InvitationId { get; set; }

    DateTime NewExpirationDate { get; set; }
}

public interface ISetUserInvitationExpirationDateCommandResult : IStandardResultPart { }
