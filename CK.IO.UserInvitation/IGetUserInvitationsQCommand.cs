using CK.Auth;
using CK.Cris;
using System.Collections.Generic;

namespace CK.IO.UserInvitation;

public interface IGetUserInvitationsQCommand : ICommand<IList<IUserInvitation>>, ICommandAuthNormal
{
}
