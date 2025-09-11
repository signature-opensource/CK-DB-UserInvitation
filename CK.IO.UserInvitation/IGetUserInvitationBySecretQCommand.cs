using CK.Cris;

namespace CK.IO.UserInvitation;

public interface IGetUserInvitationBySecretQCommand : ICommandCurrentCulture, ICommand<IGetUserInvitationBySecretResult>
{
    /// <summary>
    /// Gets the secret in UTF-8 string format.
    /// </summary>
    string Secret { get; set; }
}
