-- SetupConfig: {}
create function CK.fGetUserInvitationByUser(
    @ActorId int
)
returns table
as
return
    select ui.InvitationId
          ,ui.CreatedById
          ,ui.UserTargetAddress
          ,ui.ExpirationDateUtc
          ,ui.IsActive
          ,ui.LCID
          ,uig.GroupId
          ,ap.ProviderName
    from CK.tUserInvitation ui
    left join CK.tUserInvitationGroup uig on ui.InvitationId = uig.InvitationId
    left join CK.tUserInvitationAuthProvider uiap on ui.InvitationId = uiap.InvitationId
    left join CK.tAuthProvider ap on uiap.ProviderName = ap.ProviderName
    where ui.CreatedById = @ActorId;
