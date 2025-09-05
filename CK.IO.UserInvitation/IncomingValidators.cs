using CK.Core;
using CK.Cris;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.IO.UserInvitation;
public class IncomingValidators : IRealObject
{
    [IncomingValidator]
    public virtual void ValidateCreationCommand( UserMessageCollector c, ICreateUserInvitationCommand cmd )
    {
        if( string.IsNullOrEmpty( cmd.UserTargetAddress ) )
        {
            c.Error( "Invalid property: UserTargetAddress cannot be null or empty." );
        }
        if( cmd.GroupIdentifiers is null or { Count: 0 } )
        {
            c.Error( "Invalid property: GroupIdentifiers cannot be null or empty." );
        }
        else if( cmd.GroupIdentifiers.Any( groupId => groupId is <= 0 ) )
        {
            c.Error( "Invalid value: GroupIdentifiers must contains value higher that 0." );
        }
        if( cmd.RestrictedProviders is null or { Count: 0 } )
        {
            c.Error( "Invalid property: RestrictedProviders cannot be null or empty." );
        }
        else if( cmd.RestrictedProviders.Any( string.IsNullOrEmpty ) )
        {
            c.Error( "Invalid value: RestrictedProviders cannot contains null or empty value." );
        }
        if( cmd.LCID is <= 0 )
        {
            c.Error( "Invalid property: LCID must be higher that 0." );
        }
    }
}
