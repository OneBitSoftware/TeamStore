using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamStore.Enums
{
    public enum EventType
    {
        Signin = 1, 
        Signout = 2,
        RetrieveAsset = 3,
        UpdateAsset = 4,
        DeleteAsset = 5,
        CreateAsset = 6,
        GrantAccess = 7,
        RevokeAccess = 8
    }
}
