using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.UserInvitation;

[SqlTable( "tUserInvitationAuthProvider", Package = typeof( Package ), ResourcePath = "Res" )]
[Versions( "1.0.0" )]
public abstract class UserInvitationAuthProviderTable : SqlTable
{
    void StObjConstruct( UserInvitationTable userInvitation, CK.DB.Auth.AuthProviderTable authProvider )
    {
    }

    [SqlProcedure( "sUserInvitationAuthProviderAdd" )]
    public abstract Task AddAuthenticationProviderAsync( ISqlCallContext ctx, int actorId, int invitationId, string providerName );

    [SqlProcedure( "sUserInvitationAuthProviderDelete" )]
    public abstract Task DeleteAuthenticationProviderAsync( ISqlCallContext ctx, int actorId, int invitationId, string providerName );
}
