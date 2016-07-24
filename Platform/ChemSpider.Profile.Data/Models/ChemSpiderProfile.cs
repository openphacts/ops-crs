using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Profile;
using System.Web.Security;

namespace ChemSpider.Profile.Data.Models
{
    public class ChemSpiderProfile : ProfileBase
    {
        [CustomProviderData("usr_id;int")]
        public int UserId
        {
            get { return (int)base["UserId"]; }
            set { base["UserId"] = value; }
        }

		[CustomProviderData("user_guid;uniqueidentifier")]
		public string UserGuid
		{
			get { return base["UserGuid"] as string; }
			set { base["UserGuid"] = value; }
		}

		public Guid UserKey
		{
			get
			{
				string guid = UserGuid;

				if(string.IsNullOrEmpty(guid))
					return Guid.Empty;

				return Guid.Parse(guid);
			}
		}

        [SettingsAllowAnonymous(false), CustomProviderData("first_name;varchar")]
        public string FirstName
        {
            get { return base["FirstName"] as string; }
            set { base["FirstName"] = value; }
        }

        [SettingsAllowAnonymous(false), CustomProviderData("last_name;varchar")]
        public string LastName
        {
            get { return base["LastName"] as string; }
            set { base["LastName"] = value; }
        }

        [SettingsAllowAnonymous(false), CustomProviderData("email;varchar")]
        public string Email
        {
            get { return base["Email"] as string; }
            set { base["Email"] = value; }
        }

        [CustomProviderData("display_name;varchar")]
        public string DisplayName
        {
            get { return base["DisplayName"] as string; }
            set { base["DisplayName"] = value; }
        }

        [CustomProviderData("company_name;varchar")]
        public string CompanyName
        {
            get
			{
				string company = base["CompanyName"] as string;
				return company;
			}
            set { base["CompanyName"] = value; }
        }

        [CustomProviderData("display_public_profile;varchar")]
        public string DisplayPublicProfile
        {
            get { return base["DisplayPublicProfile"] as string; }
            set { base["DisplayPublicProfile"] = value; }
        }

        [CustomProviderData("street_address;varchar")]
        public string StreetAddress
        {
            get { return base["StreetAddress"] as string; }
            set { base["StreetAddress"] = value; }
        }

        [CustomProviderData("city;varchar")]
        public string City
        {
            get { return base["City"] as string; }
            set { base["City"] = value; }
        }

        [CustomProviderData("state;varchar")]
        public string State
        {
            get { return base["State"] as string; }
            set { base["State"] = value; }
        }

        [CustomProviderData("zip_code;varchar")]
        public string ZipCode
        {
            get { return base["ZipCode"] as string; }
            set { base["ZipCode"] = value; }
        }

        [CustomProviderData("country;varchar")]
        public string Country
        {
            get { return base["Country"] as string; }
            set { base["Country"] = value; }
        }

        [CustomProviderData("home_phone;varchar")]
        public string Phone
        {
            get { return base["Phone"] as string; }
            set { base["Phone"] = value; }
        }

        [CustomProviderData("subscribed_yn;varchar")]
        public string SubscribeYN
        {
            get { return base["SubscribeYN"] as string; }
            set { base["SubscribeYN"] = value; }
        }

        [CustomProviderData("subscribed_rsc;varchar")]
        public string SubscribeRSC
        {
            get { return base["SubscribeRSC"] as string; }
            set { base["SubscribeRSC"] = value; }
        }

        [CustomProviderData("subscribed_thirdparty;varchar")]
        public string SubscribeThirdParty
        {
            get { return base["SubscribeThirdParty"] as string; }
            set { base["SubscribeThirdParty"] = value; }
        }

        public static ChemSpiderProfile GetUserProfile(string username)
        {
            return string.IsNullOrEmpty(username) ? null : Create(username) as ChemSpiderProfile;
        }

        public static ChemSpiderProfile GetUserProfile()
        {
            return Create(Membership.GetUser().UserName) as ChemSpiderProfile;
        }
    }
}
