--[beginscript]

create table CK.tUserInvitationAuthProvider
(
    InvitationId int not null,
    ProviderName varchar( 129 ) collate Latin1_General_100_CI_AS not null,

    constraint PK_CK_tUserInvitationAuthProvider primary key( InvitationId, ProviderName ),
    constraint FK_CK_tUserInvitationAuthProvider_InvitationId foreign key( InvitationId ) references CK.tUserInvitation( InvitationId )
);

insert into CK.tUserInvitationAuthProvider( InvitationId, ProviderName ) values( 0, '' );

--[endscript]
