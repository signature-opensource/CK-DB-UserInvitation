-- SetupConfig: {}
create procedure CK.sUserInvitationCreate
(
    @ActorId int,
    @UserTargetAddress nvarchar( 255 ),
    @ExpirationDateUtc datetime2( 2 ),
    @IsActive bit,
    @CultureId int,
    @Secret binary( 24 ),
    @InvitationId int output,
    @IssuingPartyId int = 0
)
as
begin
    if @ActorId is null or @ActorId <= 0 throw 50000, 'UserInvitation.InvalidActorId', 1;
    if @UserTargetAddress is null or len( @UserTargetAddress ) = 0 throw 50000, 'UserInvitation.InvalidUserTargetAddress', 1;
    if @CultureId is null or @CultureId <= 0 throw 50000, 'UserInvitation.InvalidCultureId', 1;

    --[beginsp]
	
    --<PreCreate revert />

    insert into CK.tUserInvitation( CreatedById, UserTargetAddress, ExpirationDateUtc, IsActive, CultureId, [Secret], IssuingPartyId ) values( @ActorId, @UserTargetAddress, @ExpirationDateUtc, @IsActive, @CultureId, @Secret, @IssuingPartyId );
    set @InvitationId = scope_identity();

	--<PostCreate />	
	
	--[endsp]
end
