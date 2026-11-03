--[beginscript]

-- The party through which the invitation was issued (its PartyActorId). 0: unbound (the existing invitations).
alter table CK.tUserInvitation add
    IssuingPartyId int not null constraint DF_CK_tUserInvitation_IssuingPartyId default( 0 ),
    constraint FK_CK_tUserInvitation_IssuingPartyId foreign key( IssuingPartyId ) references CK.tActor( ActorId );

--[endscript]
