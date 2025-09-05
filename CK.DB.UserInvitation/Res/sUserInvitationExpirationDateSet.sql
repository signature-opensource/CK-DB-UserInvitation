-- SetupConfig: {}
create procedure CK.sUserInvitationExpirationDateSet
(
    @ActorId int,
    @InvitationId int,
    @NewExpirationDate datetime2( 2 )
)
as
begin
    if @ActorId is null or @ActorId <= 0 throw 50000, 'UserInvitation.InvalidActorId', 1;
    if @InvitationId is null or @InvitationId <= 0 throw 50000, 'UserInvitation.InvalidInvitationId', 1;

    --[beginsp]

    --<PreUpdate revert />

    update CK.tUserInvitation
    set ExpirationDateUtc = @NewExpirationDate
    where InvitationId = @InvitationId; 

	--<PostUpdate />

	--[endsp]
end
