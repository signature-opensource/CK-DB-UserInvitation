using CK.Cris;
using CK.TypeScript;

namespace CK.IO.UserInvitation;

[TypeScriptType]
public interface IGetUserInvitationBySecretQCommand : ICommandCurrentCulture, ICommand<IGetUserInvitationBySecretResult>
{
    /// <summary>
    /// Gets the secret in UTF-8 string format.
    /// </summary>
    string Secret { get; set; }
}
