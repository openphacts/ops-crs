using System;
using System.Net;

namespace ChemSpider.Profile.RSCID
{
    public class OAuthHttpResponse
    {
        public string response
        {
            get;
            set;
        }
        public HttpStatusCode status_code
        {
            get;
            set;
        }
    }
}
