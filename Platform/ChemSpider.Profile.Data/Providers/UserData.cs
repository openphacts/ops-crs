using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ChemSpider.Data.Database;
using ChemSpider.Profile.Data.Models;

namespace ChemSpider.Profile.Data.Providers
{
    public class UserData
    {
        private ChemUsersDB db = new ChemUsersDB();

        public int GetUserId(string username)
        {
            if (string.IsNullOrEmpty(username))
                return -1;

            try
            {
                return db.getUserId(username);
            }
            catch (Exception)
            {
            }

            return -1;
        }

        public string GetUserName(int id)
        {
            return db.getUsrName(id);
        }

        public string GetUserDisplayName(int id)
        {
            return db.getUserDisplayName(id);
        }

        public string GetUserAvatar(int id)
        {
            return db.getUserAvatar(id);
        }

        public bool IsUserNameRegistered(string username)
        {
            try
            {
                return db.getUserId(username) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsEmailRegistered(string email)
        {
            return db.IsEmailRegistered(email);
        }


        public void AddOAuthMembership(int usr_id, OAuthMembership membership)
        {
            db.AddSocialNetwork(usr_id, membership.Provider, membership.ProviderUserId, membership.FirstName, membership.LastName, membership.Name, membership.Username, membership.Email, membership.AvatarUrl, membership.ProfileUrl, membership.IsEmailVerified, membership.Raw);
        }

        public void UpdateOAuthMembership(OAuthMembership membership)
        {
            db.UpdateSocialNetwork(membership.Provider, membership.ProviderUserId, membership.FirstName, membership.LastName, membership.Name, membership.Username, membership.Email, membership.AvatarUrl, membership.ProfileUrl, membership.IsEmailVerified, membership.Raw);
        }

        public int FindByExternalID(string id, string provider)
        {
            string username = db.GetUserNameBySocialID(provider, id);

            return string.IsNullOrEmpty(username) ? -1 : db.getUserId(username);
        }

        public string GetUsernameByExternalID(string provider, string id)
        {
            return db.GetUserNameBySocialID(provider, id);
        }

        public string GetSocialNetworkRawData(string id, string provider)
        {
            DataRow r = db.GetSocialNetwork(id, provider);

            return r != null ? r.Field<string>("raw") : null;
        }

        public string GetUsernameByEmail(string email)
        {
            return db.GetUserNameByEmail(email);
        }

        public IEnumerable<OAuthMembership> GetOAuthMemberships(string username)
        {
            int usr_id = db.getUserId(username);

            return GetOAuthMemberships(usr_id);
        }

        public IEnumerable<OAuthMembership> GetOAuthMemberships(int usr_id)
        {
            return from r in db.GetSocialNetworks(usr_id).AsEnumerable()
                   select new OAuthMembership
                   {
                       Provider = r.Field<string>("type"),
                       ProviderUserId = r.Field<string>("id"),
                       UserId = r.Field<int>("usr_id"),
                       FirstName = r.Field<string>("first_name"),
                       LastName = r.Field<string>("last_name"),
                       Name = r.Field<string>("name"),
                       Username = r.Field<string>("username"),
                       Email = r.Field<string>("email"),
                       AvatarUrl = r.Field<string>("avatar_url"),
                       ProfileUrl = r.Field<string>("profile_url"),
                       Raw = r.Field<string>("raw")
                   };
        }

        public RSCUser GetRSCUser(string username)
        {
            IEnumerable<OAuthMembership> memberships = GetOAuthMemberships(username);

            OAuthMembership rscMembership = memberships.Where(m => m.Provider.Equals("rsc")).FirstOrDefault();

            if(rscMembership != null)
                return new RSCUser(rscMembership.Raw);

            return null;
        }

        public void DisconnectOAuthMembership(string provider, string id)
        {
            db.DisconnectSocialNetwork(id, provider);
        }

        public IEnumerable<ChemSpiderUser> SearchUsers(string filter, bool confirmed)
        {
            return from r in db.SearchUsers(filter, null, null, confirmed).AsEnumerable()
                   select new ChemSpiderUser
                   {
                        UserId = r.Field<int>("usr_id"),
                        Name = r.Field<string>("name"),
                        UserName = r.Field<string>("UserName"),
                        Email = r.Field<string>("email"),
                        CreatedDate = r.Field<DateTime>("created_date"),
                        IsApproved = r.Field<bool>("IsApproved")
                   };
        }
    }
}
