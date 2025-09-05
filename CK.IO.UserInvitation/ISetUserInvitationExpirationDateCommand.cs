using CK.Auth;
using CK.Cris;
using CK.TypeScript;
using System;

namespace CK.IO.UserInvitation;

[TypeScriptType]
public interface ISetUserInvitationExpirationDateCommand : ICommand, ICommandAuthNormal
{
    int InvitationId { get; set; }

    DateTime NewExpirationDate { get; set; }
}
