using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace ChemSpider.Profile.Data.Models
{
    [Serializable]
    public class RSCUser
    {
        /*
        {
        "UserDetails":
            {
            "UserID":"3ecdf552-a1cd-45f9-9ee9-8bfa9bf0493f",
            "Email":"pshenichnova@rsc.org",
            "Title":"",
            "GivenName":"Alexey",
            "FamilyName":"Pshenichnov",
            "PreferredDisplayName":"Alexey Pshenichnov",
            "IsEmailVerified":true,
            "AccessedSites":"896e069e-edb9-43f8-9cec-7499758f21a7,45632fb3-85ba-49f9-894e-36af6e550267,896E069E-EDB9-43F8-9CEC-7499758F21A7,45632FB3-85BA-49F9-894E-36AF6E550267,2A4938FD-1A23-486D-8E94-40E20EF6C34B,B7D13CB9-BB11-4EFF-A2A6-7E63ADF52220,4A19AD1B-1218-45D8-89BA-97F4,72992A8B-A493-42B6-9948-E97C68EA8683"
            },
        "AdditionalInfos":
            {
            "AdditionalInfoList":
                [{
                "BooleanOperator":null,"
                DisplayName":null,
                "Name":"Institution",
                "Value":"RSC"
                },{
                "BooleanOperator":null,
                "DisplayName":null,
                "Name":"Job Title",
                "Value":"aaaa"
                }],
            "URL":null
            },
        "IsAuthorized":true,
        "IsEmailVerified":true,
        "IsRoleRequired":false,
        "IsValidUser":true,
        "IsFirstTimeLogin":false,
        "ProfileImageUrl":"https://ravdev.in/rsc-id/account/getprofileimage?userId=3ecdf552-a1cd-45f9-9ee9-8bfa9bf0493f",
        "UserRoles":["Developer","Reader"]
        }
        */

        public RSCUser()
        {
        }

        public RSCUser(string raw)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            RSCUser user = javaScriptSerializer.Deserialize<RSCUser>(raw);

            UserDetails = user.UserDetails;
            AdditionalInfos = user.AdditionalInfos;
            IsAuthorized = user.IsAuthorized;
            IsEmailVerified = user.IsEmailVerified;
            IsRoleRequired = user.IsRoleRequired;
            IsValidUser = user.IsValidUser;
            IsFirstTimeLogin = user.IsFirstTimeLogin;
            ProfileImageUrl = user.ProfileImageUrl;
            ProfileUrl = user.ProfileUrl;
            UserRoles = user.UserRoles;

            Raw = raw;
        }

        [Serializable]
        public class Details
        {
            public string UserID { get; set; }
            public string Email { get; set; }
            public string Username { get { return Email; } }
            public string Title { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public string PreferredDisplayName { get; set; }
            public bool IsEmailVerified { get; set; }
            public string AccessedSites { get; set; }
        }

        [Serializable]
        public class AdditionalInformation
        {
            public IEnumerable<AdditionalInfo> AdditionalInfoList { get; set; }
        }

        [Serializable]
        public class AdditionalInfo
        {
            public string BooleanOperator { get; set; }
            public string DisplayName { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public Details UserDetails { get; set; }
        public AdditionalInformation AdditionalInfos { get; set; }
        public bool IsAuthorized { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsRoleRequired { get; set; }
        public bool IsValidUser { get; set; }
        public bool IsFirstTimeLogin { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ProfileUrl { get; set; }
        public IEnumerable<string> UserRoles { get; set; }

        [ScriptIgnore]
        public string Raw { get; set; }

        public override string ToString()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(this);
        }
    }
}
