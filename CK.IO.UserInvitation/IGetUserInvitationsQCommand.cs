using CK.Auth;
using CK.Cris;
using CK.TypeScript;
using System.Collections.Generic;

namespace CK.IO.UserInvitation;

[TypeScriptType]
public interface IGetUserInvitationsQCommand : ICommand<IList<IUserInvitation>>, ICommandAuthNormal
{
}
