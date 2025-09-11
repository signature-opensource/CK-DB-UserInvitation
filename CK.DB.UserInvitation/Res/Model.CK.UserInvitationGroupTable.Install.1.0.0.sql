--[beginscript]

create table CK.tUserInvitationGroup(
    InvitationId int not null,
    GroupId int not null,
    
    constraint PK_CK_tUserInvitationGroup primary key( InvitationId, GroupId ),
    constraint FK_CK_tUserInvitationGroup_UserInvitationId foreign key( InvitationId ) references CK.tUserInvitation( InvitationId ),
    constraint FK_CK_tUserInvitationGroup_GroupId foreign key( GroupId ) references CK.tGroup( GroupId )
);

insert into CK.tUserInvitationGroup( InvitationId, GroupId ) values( 0, 0 );

--[endscript]
