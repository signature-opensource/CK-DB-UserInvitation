using CK.Core;

namespace CK.IO.UserInvitation;

public interface IPartialUserInvitation : IPoco
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
    int LCID { get; set; }
}
