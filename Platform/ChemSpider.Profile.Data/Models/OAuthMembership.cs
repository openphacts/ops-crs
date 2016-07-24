using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemSpider.Profile.Data.Models
{
    [Serializable]
    public class OAuthMembership
    {
        public string Provider { get; set; }
        public string ProviderUserId { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string ProfileUrl { get; set; }
        public bool IsEmailVerified { get; set; }
        public string Raw { get; set; }

        public OAuthMembership()
        {
        }
    }
}
