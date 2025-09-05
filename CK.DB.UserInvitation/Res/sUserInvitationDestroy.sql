-- SetupConfig: {}
create procedure CK.sUserInvitationDestroy
(
    @ActorId int,
    @InvitationId int
)
as
begin
    if @ActorId is null or @ActorId <= 0 throw 50000, 'UserInvitation.InvalidActorId', 1;
    if @InvitationId is null or @InvitationId <= 0 throw 50000, 'UserInvitation.InvalidInvitationId', 1;

    --[beginsp]

    if exists (select 1 from CK.tUserInvitation where InvitationId = @InvitationId)
    begin
        if not exists (select 1 from CK.tUserInvitation where InvitationId = @InvitationId and CreatedById = @ActorId) throw 50000, 'UserInvitation.Only', 1;

        --<PreCreate revert />

        delete from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId;
        delete from CK.tUserInvitationGroup where InvitationId = @InvitationId;
        delete from CK.tUserInvitation where InvitationId = @InvitationId;

    	--<PostCreate />
    end

	--[endsp]
end
