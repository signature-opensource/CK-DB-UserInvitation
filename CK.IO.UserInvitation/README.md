# CK.IO.UserInvitation

The Pocos and the Cris commands of the user invitations: the contract shared by the party that holds the database
([CK.DB.UserInvitation](../CK.DB.UserInvitation/README.md) handles the commands) and the parties that send them.

## The invitation

[`IUserInvitation`](IUserInvitation.cs):

| Property | |
|---|---|
| `InvitationId` | The identifier. |
| `UserTargetAddress` | The e-mail address the invitation is sent to (unique among the invitations). |
| `CultureId` | The culture of the invitation (its mail). |
| `CreatedById` | The actor that created it. |
| `ExpirationDateUtc`, `IsActive` | An invitation is usable while it is active and not expired. |
| `GroupIdentifiers` | The groups the new user joins. |
| `RestrictedProviders` | The authentication providers the new user may use. |
| `IssuingPartyId` | The PartyActorId of the party through which the invitation was issued (0: unbound). It is set by the server code, never by a command. |

[`IPartialUserInvitation`](IPartialUserInvitation.cs) is what an anonymous caller gets from a secret: `InvitationId`,
`UserTargetAddress` and `CultureId`.

## The commands

| Command | Result | Exposure, authentication |
|---|---|---|
| [`ICreateUserInvitationCommand`](ICreateUserInvitationCommand.cs) | `IUserInvitation` | `[Internal]`, `ICommandAuthNormal` |
| [`IGetUserInvitationQCommand`](IGetUserInvitationQCommand.cs) (`InvitationId`) | `IUserInvitation?` | `[Internal]`, `ICommandAuthNormal` |
| [`IGetUserInvitationsQCommand`](IGetUserInvitationsQCommand.cs) | `IList<IUserInvitation>` | `[Internal]`, `ICommandAuthNormal` |
| [`ISetUserInvitationIsActiveCommand`](ISetUserInvitationIsActiveCommand.cs) (`InvitationId`, `IsActive`) | `ISetUserInvitationIsActiveCommandResult` | `[Internal]`, `ICommandAuthNormal` |
| [`ISetUserInvitationExpirationDateCommand`](ISetUserInvitationExpirationDateCommand.cs) (`InvitationId`, `NewExpirationDate`) | `ISetUserInvitationExpirationDateCommandResult` | `[Internal]`, `ICommandAuthNormal` |
| [`IDestroyUserInvitationCommand`](IDestroyUserInvitationCommand.cs) (`InvitationId`) | `IDestroyUserInvitationCommandResult` | `[Internal]`, `ICommandAuthNormal` |
| [`IGetUserInvitationBySecretQCommand`](IGetUserInvitationBySecretQCommand.cs) (`Secret`) | [`IGetUserInvitationBySecretResult`](IGetUserInvitationBySecretResult.cs): an `IPartialUserInvitation` and the user messages | public, none (the secret is the proof) |

**The management commands are `[Internal]`: they never come from HTTP.** This package has no rights model: it doesn't
know who can give which group, nor who can manage which invitation (`ISetUserInvitationIsActiveCommand` and
`ISetUserInvitationExpirationDateCommand` don't check the actor). The application checks the rights, then sends these
commands from the System or calls the package (CK-UserManagement does). Only the registration query, whose secret is the
proof, is public.

The [`IncomingValidators`](IncomingValidators.cs) check an `ICreateUserInvitationCommand`: a `UserTargetAddress`, at least
one group (positive identifiers), at least one restricted provider (not empty), a positive `CultureId`.
