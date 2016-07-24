using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemSpider.Profile.RSCID
{
    /*
        {
        "error":"invalid_grant",
        "error_title":"Invalid Grant",
        "error_description":"The provided authorization grant (e.g. authorization code, resource owner credentials) or refresh token is invalid, expired, revoked, does not match the redirection URI used in the authorization request, or was issued to another client.",
        "error_uri":""
        }
     */

    public class RSCError
    {
        public string error { get; set; }
        public string error_title { get; set; }
        public string error_description { get; set; }
        public string error_uri { get; set; }

        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(error); }
        }
    }
}
