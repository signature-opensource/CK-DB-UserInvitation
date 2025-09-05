using CK.Auth;
using CK.Cris;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CK.IO.UserInvitation;

public interface ICreateUserInvitationCommand : ICommand<IUserInvitation>, ICommandAuthNormal
{
    string UserTargetAddress { get; set; }

    DateTime ExpirationDateUtc { get; set; }

    bool IsActive { get; set; }

    IList<int> GroupIdentifiers { get; }

    IList<string> RestrictedProviders { get; }

    [DefaultValue( 12 )]
    int LCID { get; set; }
}
