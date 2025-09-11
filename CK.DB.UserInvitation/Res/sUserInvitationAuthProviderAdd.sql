-- SetupConfig: {}
create procedure CK.sUserInvitationAuthProviderAdd
(
    @ActorId int,
    @InvitationId int,
    @ProviderName varchar( 129 )
)
as
begin
    if @ActorId is null or @ActorId <= 0 throw 50000, 'UserInvitationAuthProvider.InvalidActorId', 1;
    if @InvitationId is null or @InvitationId <= 0 throw 50000, 'UserInvitationAuthProvider.InvalidInvitationId', 1;
    if len(trim(@ProviderName)) = 0 throw 50000, 'UserInvitationAuthProvider.InvalidEmptyProviderName', 1;
    if not exists (select 1 from CK.tAuthProvider where @ProviderName like ProviderName + '%') throw 50000, 'UserInvitationAuthProvider.InvalidAuthProvider', 1;

	--[beginsp]

	--<PreAdd revert />

    if not exists (select top 1 1 from CK.tUserInvitationAuthProvider where InvitationId = @InvitationId and ProviderName = @ProviderName)
    begin
        insert into CK.tUserInvitationAuthProvider( InvitationId, ProviderName ) values( @InvitationId, @ProviderName );
    end
    
	--<PostAdd />	
	
	--[endsp]
end
