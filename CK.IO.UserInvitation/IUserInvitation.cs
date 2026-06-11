using System.Collections.Generic;
using System;
using CK.Core;

namespace CK.IO.UserInvitation;

public interface IUserInvitation : IPoco
{
    /// <summary>
    /// Gets the token identifier (available only when this is read from the database).
    /// </summary>
    int InvitationId { get; set; }

    /// <summary>
    /// Gets or sets the target address that must be used to send the invitation.
    /// </summary>
    string UserTargetAddress { get; set; }

    /// <summary>
    /// Gets or sets the LCID for the future user.
    /// </summary>
    int CultureId { get; set; }

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
