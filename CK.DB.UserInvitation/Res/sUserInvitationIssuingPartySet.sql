-- SetupConfig: {}
--
-- Sets the party through which an invitation is issued (its PartyActorId): a resent invitation is bound to the party
-- that resends it. This is called by the server code (never by a client command).
--
create procedure CK.sUserInvitationIssuingPartySet
(
    @ActorId int,
    @InvitationId int,
    @IssuingPartyId int
)
as
begin
    if @ActorId is null or @ActorId <= 0 throw 50000, 'UserInvitation.InvalidActorId', 1;
    if @InvitationId is null or @InvitationId <= 0 throw 50000, 'UserInvitation.InvalidInvitationId', 1;
    if @IssuingPartyId is null or @IssuingPartyId < 0 throw 50000, 'UserInvitation.InvalidIssuingPartyId', 1;

    --[beginsp]

    update CK.tUserInvitation
        set IssuingPartyId = @IssuingPartyId
        where InvitationId = @InvitationId;

    --[endsp]
end
