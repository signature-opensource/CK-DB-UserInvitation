-- SetupConfig: {}
create procedure CK.sUserInvitationAuthProviderDelete
(
    @ActorId int,
    @InvitationId int,
    @ProviderName varchar( 129 )
)
as
begin
    if @ActorId is null or @ActorId <= 0 throw 50000, 'UserInvitationAuthProvider.InvalidActorId', 1;
    if @InvitationId is null or @InvitationId <= 0 throw 50000, 'UserInvitationAuthProvider.InvalidInvitationId', 1;
    if not exists (select 1 from CK.tAuthProvider where @ProviderName like ProviderName + '%') throw 50000, 'UserInvitationAuthProvider.InvalidAuthProvider', 1;

	--[beginsp]

	--<PreDelete revert />
    
    delete from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId and ProviderName = @ProviderName;

	--<PostDelete />	
	
	--[endsp]
end
