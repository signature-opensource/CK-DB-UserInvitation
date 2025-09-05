using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.UserInvitation;

[SqlTable( "tUserInvitationGroup", Package = typeof( Package ), ResourcePath = "Res" )]
[Versions( "1.0.0" )]
public abstract class UserInvitationGroupTable : SqlTable
{
    void StObjConstruct( UserInvitationTable userInvitation, CK.DB.Actor.GroupTable group )
    {
    }

    [SqlProcedure( "sUserInvitationGroupAdd" )]
    public abstract Task AddGroupAsync( ISqlCallContext ctx, int actorId, int invitationId, int groupId );

    [SqlProcedure( "sUserInvitationGroupDelete" )]
    public abstract Task DeleteGroupAsync( ISqlCallContext ctx, int actorId, int invitationId, int groupId );
}
