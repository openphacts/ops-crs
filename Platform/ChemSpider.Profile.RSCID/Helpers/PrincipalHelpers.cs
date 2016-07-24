using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

using ChemSpider.Profile.Data.Models;

namespace ChemSpider.Profile.RSCID.Helpers
{
    public static class PrincipalHelpers
    {
        public static RSCUser RSCUser(this IPrincipal principal)
        {
            if (principal is RSCPrincipal)
            {
                return (principal as RSCPrincipal).RSCUser;
            }

            return null;
        }
    }
}
