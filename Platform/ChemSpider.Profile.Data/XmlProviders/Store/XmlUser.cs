using System;
using System.Collections.Generic;
using System.Text;

namespace ChemSpider.Profile.Store
{

    /// <summary>
    /// 
    /// </summary>
    public class XmlUser {

        public Guid UserKey = Guid.Empty;

        public string UserName = string.Empty;
        public string Password = string.Empty;
        public string PasswordSalt = string.Empty;
        public string Email = string.Empty;
        public string PasswordQuestion = string.Empty;
        public string PasswordAnswer = string.Empty;
        public string Comment;
        // track data
        public DateTime CreationDate = DateTime.Now;
        public DateTime LastActivityDate = DateTime.MinValue;
        public DateTime LastLoginDate = DateTime.MinValue;
        public DateTime LastPasswordChangeDate = DateTime.MinValue;
        public DateTime LastLockoutDate = DateTime.MaxValue;
        public bool IsApproved = true;
        public bool IsLockedOut = false;
    }
}
