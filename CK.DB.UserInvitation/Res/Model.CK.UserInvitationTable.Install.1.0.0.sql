--[beginscript]

create table CK.tUserInvitation(
    InvitationId int not null identity( 0, 1 ),
    CreatedById int not null,
    UserTargetAddress nvarchar( 255 ) collate Latin1_General_100_CI_AI not null,
    ExpirationDateUtc datetime2( 2 ) not null,
    IsActive bit not null,
    CultureId int not null,
    [Secret] binary( 24 ) not null

    constraint PK_CK_tUserInvitation primary key( InvitationId ),
    constraint FK_CK_tUserInvitation_CreatedById foreign key( CreatedById ) references CK.tActor( ActorId ),
    constraint UK_CK_tUserInvitation_UserTargetAddress unique nonclustered( UserTargetAddress ),
    constraint FK_CK_tUserInvitation_CultureId foreign key( CultureId ) references CK.tCulture( CultureId )
);

insert into CK.tUserInvitation( CreatedById, UserTargetAddress, ExpirationDateUtc, IsActive, CultureId, [Secret] ) values( 0, N'', '0001-01-01', 0, 0, convert( binary( 24 ), 0x0 ) );

--[endscript]
