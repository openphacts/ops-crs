using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Web.Security;

using ChemSpider.Profile.Data.Models;

namespace ChemSpider.Profile.RSCID
{
    public class RSCPrincipal : GenericPrincipal
    {
        public RSCPrincipal(IIdentity identity, RSCUser user)
            : base(identity, Roles.GetRolesForUser(identity != null ? identity.Name : null))
		{
            RSCUser = user;
        }

        public RSCUser RSCUser { get; private set; }
    }
}
