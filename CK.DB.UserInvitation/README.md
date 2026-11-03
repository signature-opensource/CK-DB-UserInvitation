# CK.DB.UserInvitation

The database of the user invitations: the tables, the procedures and the handlers of the commands of
[CK.IO.UserInvitation](../CK.IO.UserInvitation/README.md). It requires CK.DB.Actor, CK.DB.Auth (the authentication
providers) and CK.DB.Culture.

## The tables

| Table | Content |
|---|---|
| `CK.tUserInvitation` ([`UserInvitationTable`](UserInvitationTable.cs)) | `InvitationId`, `CreatedById`, `UserTargetAddress` (unique), `ExpirationDateUtc`, `IsActive`, `CultureId`, `Secret`, `IssuingPartyId` |
| `CK.tUserInvitationGroup` ([`UserInvitationGroupTable`](UserInvitationGroupTable.cs)) | The groups the new user joins: `AddGroupAsync`, `DeleteGroupAsync` |
| `CK.tUserInvitationAuthProvider` ([`UserInvitationAuthProviderTable`](UserInvitationAuthProviderTable.cs)) | The authentication providers the new user may use: `AddAuthenticationProviderAsync`, `DeleteAuthenticationProviderAsync` |

## The commands ([`Package`](Package.cs))

- **The creation** (`ICreateUserInvitationCommand`) generates the secret (random, base64url), creates the invitation with
  its groups and providers in one transaction, and returns it.
- **The reads by actor** (`IGetUserInvitationQCommand`, `IGetUserInvitationsQCommand`) return the invitations created by
  the actor of the command (`CK.fGetUserInvitationByUser`). `UserInvitationTable.GetUserInvitationSecretAsync` reads the
  secret of an invitation for its creator.
- **The read by secret** (`IGetUserInvitationBySecretQCommand`, anonymous) answers an `IPartialUserInvitation`, or the
  error messages `UserInvitation.InvitationNotFound`, `UserInvitation.InvitationExpired`,
  `UserInvitation.InvitationInactive`. `Package.GetUserInvitationAsync( ctx, secret )` gives the whole invitation to the
  server code.
- **The changes** (`ISetUserInvitationIsActiveCommand`, `ISetUserInvitationExpirationDateCommand`) and **the destruction**
  (`IDestroyUserInvitationCommand`: only its creator can destroy an invitation, `UserInvitation.Only` otherwise).

## The issuing party

`IssuingPartyId` is the PartyActorId of the party through which an invitation is issued: the server code that knows the
caller party of the request records it, a client never chooses it (it is not a property of a command). 0 is an unbound
invitation (the ones created before this was recorded).
- `Package.CreateUserInvitationAsync( ctx, cmd, issuingPartyId )` creates a bound invitation (the command handler
  creates an unbound one).
- `Package.SetUserInvitationIssuingPartyAsync( ctx, actorId, invitationId, issuingPartyId )` binds an invitation to the
  party that resends it.

What the issuing party is used for (the links of the mails, the redemption through this party only) is the business of
the packages that send the invitations.
