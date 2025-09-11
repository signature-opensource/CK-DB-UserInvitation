-- SetupConfig: {}
create procedure CK.sUserInvitationIsActiveSet
(
    @ActorId int,
    @InvitationId int,
    @IsActive bit
)
as
begin
    if @ActorId is null or @ActorId <= 0 throw 50000, 'UserInvitation.InvalidActorId', 1;
    if @InvitationId is null or @InvitationId <= 0 throw 50000, 'UserInvitation.InvalidInvitationId', 1;

    --[beginsp]

    --<PreUpdate revert />

    update CK.tUserInvitation
    set IsActive = @IsActive
    where InvitationId = @InvitationId; 

	--<PostUpdate />

	--[endsp]
end
