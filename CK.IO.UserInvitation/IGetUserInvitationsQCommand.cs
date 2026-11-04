using CK.Auth;
using CK.Cris;
using System.Collections.Generic;

namespace CK.IO.UserInvitation;

[Internal]
public interface IGetUserInvitationsQCommand : ICommand<IList<IUserInvitation>>, ICommandAuthNormal
{
}
