-- SetupConfig: {}
create procedure CK.sUserInvitationGroupDelete
(
    @ActorId int,
    @InvitationId int,
    @GroupId int
)
as
begin
    if @ActorId is null or @ActorId <= 0 throw 50000, 'UserInvitationGroup.InvalidActorId', 1;
    if @InvitationId is null or @InvitationId <= 0 throw 50000, 'UserInvitationGroup.InvalidInvitationId', 1;
    if @GroupId is null or @GroupId <= 0 throw 50000, 'UserInvitationGroup.InvalidGroupId', 1;

	--[beginsp]

	--<PreDelete revert />

    delete from CK.tUserInvitationGroup where InvitationId = @InvitationId and GroupId = @GroupId;

	--<PostDelete />
	
	--[endsp]
end
