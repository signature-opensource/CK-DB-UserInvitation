using System.Collections.Generic;
using System;

namespace CK.IO.UserInvitation;

public interface IUserInvitation : IPartialUserInvitation
{

    /// <summary>
    /// Gets the invitation author.
    /// </summary>
    int CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the expiration date.
    /// </summary>
    DateTime ExpirationDateUtc { get; set; }

    /// <summary>
    /// Gets of sets whether the invitation is active.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Gets the list of groups to which the user should be added once created.
    /// </summary>
    IList<int> GroupIdentifiers { get; }

    /// <summary>
    /// When empty, the user can use any provider.
    /// Otherwise, the user must only use the given providers.
    /// </summary>
    IList<string> RestrictedProviders { get; }
}
