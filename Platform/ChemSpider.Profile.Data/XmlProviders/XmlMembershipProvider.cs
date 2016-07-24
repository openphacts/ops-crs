using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;

using ChemSpider.Profile.Store;

namespace ChemSpider.Profile
{

    /// <summary>
    /// Specialized MembershipProvider that uses a file (Users.config) to store its data.
    /// Passwords for the users are always stored as a salted hash (see: http://msdn.microsoft.com/msdnmag/issues/03/08/SecurityBriefs/)
    /// </summary>
    public class XmlMembershipProvider : MembershipProvider {

        #region Static Fields ///////////////////////////////////////////////////////////

        public static string DefaultFileName = "Users.xml";
        public static string DefaultProviderName = "XmlMembershipProvider";
        public static string DefaultProviderDescription = "XML Membership Provider";

        #endregion

        #region Fields  /////////////////////////////////////////////////////////////////

        string _applicationName;
        string _fileName;
        //XmlUserStore _store;
        WeakReference _storeRef;
        object _syncRoot = new object();

        private bool _enablePasswordReset;
        private int _maxInvalidPasswordAttempts;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private int _passwordAttemptWindow;
        private MembershipPasswordFormat _passwordFormat;
        private string _passwordStrengthRegularExpression;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;

        #endregion

        #region Properties  /////////////////////////////////////////////////////////////

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
        public override bool EnablePasswordReset {
            get { return _enablePasswordReset; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.</returns>
        public override bool EnablePasswordRetrieval {
            get { return _passwordFormat != MembershipPasswordFormat.Hashed; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>
        public override int MaxInvalidPasswordAttempts {
            get { return _maxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum number of special characters that must be present in a valid password.</returns>
        public override int MinRequiredNonAlphanumericCharacters {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum length required for a password. </returns>
        public override int MinRequiredPasswordLength {
            get { return _minRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>
        public override int PasswordAttemptWindow {
            get { return _passwordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"></see> values indicating the format for storing passwords in the data store.</returns>
        public override MembershipPasswordFormat PasswordFormat {
            get { return _passwordFormat; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <value></value>
        /// <returns>A regular expression used to evaluate a password.</returns>
        public override string PasswordStrengthRegularExpression {
            get { return _passwordStrengthRegularExpression; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <value></value>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>
        public override bool RequiresQuestionAndAnswer {
            get { return _requiresQuestionAndAnswer; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>
        public override bool RequiresUniqueEmail {
            get { return _requiresUniqueEmail; }
        }

        /// <summary>
        /// Gets the store.
        /// </summary>
        /// <value>The store.</value>
        protected XmlUserStore Store {
            get {
                XmlUserStore store = this.StoreRef.Target as XmlUserStore;
                if (store == null) {
                    this.StoreRef.Target = store = new XmlUserStore(_fileName);
                }
                return store;
                //if (_store == null)
                //    Interlocked.CompareExchange(
                //        ref this._store,
                //        new XmlUserStore(_fileName),
                //        null);
                //return _store;
            }
        }

        /// <summary>
        /// Gets the store ref.
        /// </summary>
        /// <value>The store ref.</value>
        private WeakReference StoreRef {
            get {
                return _storeRef ??
                        (_storeRef = new WeakReference(new XmlUserStore(_fileName)));
            }
        }

        /// <summary>
        /// Gets the sync root.
        /// </summary>
        /// <value>The sync root.</value>
        protected internal object SyncRoot {
            get { return _syncRoot; }
        }
        #endregion

        #region Methods /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">The current password for the specified user.</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword) {

            lock (SyncRoot) {
                XmlUser user = this.Store.GetUserByName(username);
                if (user == null)
                    throw new Exception("User does not exist!");

                if (ValidateUserInternal(user, oldPassword)) {
                    ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, false);
                    base.OnValidatingPassword(args);
                    if (args.Cancel) {
                        if (args.FailureInformation != null)
                            throw args.FailureInformation;
                        else
                            throw new MembershipPasswordException("Change password canceled due to new password validation failure.");
                    }
                    if (!ValidatePassword(newPassword))
                        throw new ArgumentException("Password doesn't meet password strength requirements!");
                    ///
                    user.PasswordSalt = string.Empty;
                    user.Password = TransformPassword(newPassword, ref user.PasswordSalt);
                    user.LastPasswordChangeDate = DateTime.Now;
                    this.Store.Save();
                    ///
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <param name="username">The user to change the password question and answer for.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <param name="newPasswordQuestion">The new password question for the specified user.</param>
        /// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {

            try {
                lock (SyncRoot) {
                    XmlUser user = this.Store.GetUserByName(username);
                    if (user != null && ValidateUserInternal(user, password)) {
                        user.PasswordQuestion = newPasswordQuestion;
                        user.PasswordAnswer = newPasswordAnswer;
                        this.Store.Save();
                        return true;
                    }
                }
            }
            catch { throw; }
            //
            return false;
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"></see> enumeration value indicating whether the user was created successfully.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the information for the newly created user.
        /// </returns>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) {

            status = MembershipCreateStatus.Success;

            try {
                // Validate the username and email
                if (!ValidateUserName(username, Guid.Empty)) {
                    status = MembershipCreateStatus.DuplicateUserName;
                    return null;
                }
                if (this.RequiresUniqueEmail && !ValidateEmail(password, Guid.Empty)) {
                    status = MembershipCreateStatus.DuplicateEmail;
                    return null;
                }

                // Validate the password
                ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
                // Raise the event before validating the password
                base.OnValidatingPassword(e);
                if (e.Cancel || !ValidatePassword(password)) {
                    status = MembershipCreateStatus.InvalidPassword;
                    return null;
                }

                // Everything is valid, create the user
                lock (SyncRoot) {
                    XmlUser user = new XmlUser();
                    user.UserKey = Guid.NewGuid();
                    user.UserName = username;
                    user.PasswordSalt = string.Empty;
                    user.Password = this.TransformPassword(password, ref user.PasswordSalt);
                    user.Email = email;
                    user.PasswordQuestion = passwordQuestion;
                    user.PasswordAnswer = passwordAnswer;
                    user.IsApproved = isApproved;
                    user.CreationDate = DateTime.Now;
                    user.LastActivityDate = DateTime.Now;
                    user.LastPasswordChangeDate = DateTime.Now;

                    // Add the user to the store
                    this.Store.Users.Add(user);
                    this.Store.Save();

                    return CreateMembershipFromInternalUser(user);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData) {

            try {
                lock (SyncRoot) {
                    XmlUser user = this.Store.GetUserByName(username);
                    if (user != null) {
                        this.Store.Users.Remove(user);
                        // FIX: http://www.codeplex.com/aspnetxmlproviders/WorkItem/View.aspx?WorkItemId=7186
                        this.Store.Save();
                        return true;
                    }
                }
            }
            catch { throw; }
            //
            return false;
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) {

            try {
                lock (SyncRoot) {
                    List<XmlUser> matchingUsers =
                        this.Store.Users.FindAll(delegate(XmlUser user) {
                        return user.Email.Equals(emailToMatch, StringComparison.OrdinalIgnoreCase);
                    });

                    totalRecords = matchingUsers.Count;
                    return CreateMembershipCollectionFromInternalList(matchingUsers);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {

            try {
                lock (SyncRoot) {
                    // FIX: changed to look up for user names which starts with usernameToMatch instead of exact match
                    string match = "^" + usernameToMatch;

                    List<XmlUser> matchingUsers = this.Store.Users.FindAll(
                        delegate(XmlUser user) {
                            //return user.UserName.Equals(usernameToMatch, StringComparison.OrdinalIgnoreCase);
                            return Regex.IsMatch(user.UserName, match, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        }
                    );

                    totalRecords = matchingUsers.Count;
                    return CreateMembershipCollectionFromInternalList(matchingUsers);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) {

            try {
                lock (SyncRoot) {
                    totalRecords = this.Store.Users.Count;
                    return CreateMembershipCollectionFromInternalList(this.Store.Users);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline() {

            int ret = 0;
            try {
                lock (SyncRoot) {
                    foreach (XmlUser user in this.Store.Users) {
                        if (user.LastActivityDate.AddMinutes(
                               Membership.UserIsOnlineTimeWindow) >= DateTime.Now) {
                            ret++;
                        }
                    }
                }
            }
            catch { throw; }
            //
            return ret;
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        public override string GetPassword(string username, string answer) {

            try {
                if (EnablePasswordRetrieval) {
                    lock (SyncRoot) {
                        XmlUser user = this.Store.GetUserByName(username);
                        if (user != null) {
                            // >> FIX http://www.codeplex.com/aspnetxmlproviders/WorkItem/View.aspx?WorkItemId=9701
                            if (RequiresQuestionAndAnswer && answer.Equals(user.PasswordAnswer, StringComparison.OrdinalIgnoreCase)) {
                                return UnEncodePassword(user.Password);
                            }
                            else if (!RequiresQuestionAndAnswer) {
                                return UnEncodePassword(user.Password);
                            }
                            else {
                                throw new System.Web.Security.MembershipPasswordException();
                            }
                            // << FIX
                        }
                    }
                    return null;
                }
                else
                    throw new Exception("Password retrieval is not enabled!");
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(string username, bool userIsOnline) {

            try {
                lock (SyncRoot) {
                    XmlUser user = this.Store.GetUserByName(username);
                    if (user != null) {
                        if (userIsOnline) {
                            user.LastActivityDate = DateTime.Now;
                            this.Store.Save();
                        }
                        return CreateMembershipFromInternalUser(user);
                    }
                    else
                        return null;
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets information from the data source for a user based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) {

            try {
                lock (SyncRoot) {
                    XmlUser user = this.Store.GetUserByKey((Guid)providerUserKey);
                    if (user != null) {
                        if (userIsOnline) {
                            user.LastActivityDate = DateTime.Now;
                            this.Store.Save();
                        }
                        return CreateMembershipFromInternalUser(user);
                    }
                    else
                        return null;
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        public override string GetUserNameByEmail(string email) {

            try {
                lock (SyncRoot) {
                    XmlUser user = this.Store.GetUserByEmail(email);
                    return (user != null) ? user.UserName : null;
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user.</param>
        /// <returns>The new password for the specified user.</returns>
        public override string ResetPassword(string username, string answer) {

            lock (SyncRoot) {
                XmlUser user = this.Store.GetUserByName(username);
                if (user != null) {
                    if (answer != null && !user.PasswordAnswer.Equals(answer, StringComparison.OrdinalIgnoreCase))
                        throw new Exception("Invalid answer entered!");
                    else if (answer == null && Membership.RequiresQuestionAndAnswer)
                        throw new Exception("Invalid question and answer entered!");

                    try {
                        byte[] NewPassword = new byte[16];
                        RandomNumberGenerator rng = RandomNumberGenerator.Create();
                        rng.GetBytes(NewPassword);

                        string NewPasswordString = Convert.ToBase64String(NewPassword);
                        user.PasswordSalt = string.Empty;
                        user.Password = TransformPassword(NewPasswordString, ref user.PasswordSalt);
                        this.Store.Save();

                        return NewPasswordString;
                    }
                    catch { throw; }
                }
            }
            return null;
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="userName">The membership user to clear the lock status for.</param>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        public override bool UnlockUser(string userName) {
            // This provider doesn't support locking
            return true;
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user.</param>
        public override void UpdateUser(MembershipUser user) {

            lock (SyncRoot) {
                XmlUser suser = this.Store.GetUserByKey((Guid)user.ProviderUserKey);
                if (suser != null) {
                    if (!ValidateUserName(suser.UserName, suser.UserKey))
                        throw new ArgumentException("UserName is not unique!");
                    if (this.RequiresUniqueEmail && !ValidateEmail(suser.Email, suser.UserKey))
                        throw new ArgumentException("Email is not unique!");

                    try {
                        suser.Email = user.Email;
                        suser.LastActivityDate = user.LastActivityDate;
                        suser.LastLoginDate = user.LastLoginDate;
                        suser.Comment = user.Comment;
                        suser.IsApproved = user.IsApproved;
                        this.Store.Save();
                    }
                    catch { throw; }
                }
                else
                    throw new ProviderException("User does not exist!");
            }
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <param name="username">The name of the user to validate.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        public override bool ValidateUser(string username, string password) {

            try {
                lock (SyncRoot) {
                    XmlUser user = this.Store.GetUserByName(username);

                    if (user == null) return false;

                    if (ValidateUserInternal(user, password)) {
                        user.LastLoginDate = DateTime.Now;
                        user.LastActivityDate = DateTime.Now;
                        this.Store.Save();
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch { throw; }
        }

        #region - Helpers -

        /// <summary>
        /// Transforms the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        private string TransformPassword(string password, ref string salt) {

            string ret = string.Empty;
            switch (PasswordFormat) {
                case MembershipPasswordFormat.Clear:
                    ret = password;
                    break;
                case MembershipPasswordFormat.Hashed:
                    // Generate the salt if not passed in
                    if (string.IsNullOrEmpty(salt)) {
                        byte[] saltBytes = new byte[16];
                        RandomNumberGenerator rng = RandomNumberGenerator.Create();
                        rng.GetBytes(saltBytes);
                        salt = Convert.ToBase64String(saltBytes);
                    }
                    ret = FormsAuthentication.HashPasswordForStoringInConfigFile((salt + password), "SHA1");
                    break;
                case MembershipPasswordFormat.Encrypted:
                    byte[] clearText = Encoding.UTF8.GetBytes(password);
                    byte[] encryptedText = base.EncryptPassword(clearText);
                    ret = Convert.ToBase64String(encryptedText);
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Validates the email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="excludeKey">The exclude key.</param>
        /// <returns></returns>
        private bool ValidateEmail(string email, Guid excludeKey) {

            bool IsValid = true;

            foreach (XmlUser user in this.Store.Users) {
                if (user.UserKey.CompareTo(excludeKey) != 0) {
                    if (RequiresUniqueEmail) {
                        if (!string.IsNullOrEmpty(email) && string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)) {
                            IsValid = false;
                            break;
                        }
                    }
                }
            }
            return IsValid;
        }

        /// <summary>
        /// Validates the username.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="email">The email.</param>
        /// <param name="excludeKey">The exclude key.</param>
        /// <returns></returns>
        private bool ValidateUserName(string userName, Guid excludeKey) {

            bool IsValid = true;

            foreach (XmlUser user in this.Store.Users) {
                if (user.UserKey.CompareTo(excludeKey) != 0) {
                    if (string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase)) {
                        IsValid = false;
                        break;
                    }
                }
            }
            return IsValid;
        }

        /// <summary>
        /// Validates the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private bool ValidatePassword(string password) {

            bool isValid = true;
            Regex regex;
            // Validate simple properties
            isValid = isValid && (password.Length >= this.MinRequiredPasswordLength);
            // Validate non-alphanumeric characters
            regex = new Regex(@"\W");
            isValid = isValid && (regex.Matches(password).Count >= this.MinRequiredNonAlphanumericCharacters);
            // Validate regular expression
            regex = new Regex(this.PasswordStrengthRegularExpression);
            isValid = isValid && (regex.Matches(password).Count > 0);
            ///
            return isValid;
        }

        /// <summary>
        /// Uns the encode password.
        /// </summary>
        /// <param name="encodedPassword">The encoded password.</param>
        /// <returns></returns>
        private string UnEncodePassword(string encodedPassword) {
            string password = encodedPassword;

            switch (PasswordFormat) {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                    Encoding.UTF8.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }
            return password;
        }

        /// <summary>
        /// Validates the user internal.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private bool ValidateUserInternal(XmlUser user, string password) {

            if (user != null) {
                string passwordValidate = TransformPassword(password, ref user.PasswordSalt);
                if (string.Compare(passwordValidate, user.Password) == 0) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates the membership from internal user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private MembershipUser CreateMembershipFromInternalUser(XmlUser user) {

            MembershipUser muser = new MembershipUser(base.Name,
                user.UserName, user.UserKey, user.Email, user.PasswordQuestion,
                user.Comment, user.IsApproved, user.IsLockedOut, user.CreationDate, user.LastLoginDate,
                user.LastActivityDate, user.LastPasswordChangeDate, user.LastLockoutDate);

            return muser;
        }

        /// <summary>
        /// Creates the membership collection from internal list.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        private MembershipUserCollection CreateMembershipCollectionFromInternalList(List<XmlUser> users) {

            MembershipUserCollection returnCollection = new MembershipUserCollection();
            foreach (XmlUser user in users) {
                returnCollection.Add(CreateMembershipFromInternalUser(user));
            }
            return returnCollection;
        }
        #endregion

        #region - Initialize -

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        public override void Initialize(string name, NameValueCollection config) {

            if (config == null)
                throw new ArgumentNullException("config");
            SecurityUtil.EnsureDataFoler();
            if (string.IsNullOrEmpty(name)) {
                name = DefaultProviderName;
            }
            if (string.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", DefaultProviderDescription);
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            // initialize fields
            _enablePasswordReset = Convert.ToBoolean(
                SecurityUtil.GetConfigValue(config["enablePasswordReset"], bool.TrueString));
            _maxInvalidPasswordAttempts = Convert.ToInt32(
                SecurityUtil.GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _minRequiredNonAlphanumericCharacters = Convert.ToInt32(
                SecurityUtil.GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "0"));
            _minRequiredPasswordLength = Convert.ToInt32(
                SecurityUtil.GetConfigValue(config["minRequiredPasswordLength"], "5"));
            _passwordAttemptWindow = Convert.ToInt32(
                SecurityUtil.GetConfigValue(config["passwordAttemptWindow"], "10"));
            // >> FIX: http://www.codeplex.com/aspnetxmlproviders/WorkItem/View.aspx?WorkItemId=6743
            _passwordFormat = (MembershipPasswordFormat)Enum.Parse(
                typeof(MembershipPasswordFormat), SecurityUtil.GetConfigValue(config["passwordFormat"], "2"));
            // << FIX
            _passwordStrengthRegularExpression = Convert.ToString(
                SecurityUtil.GetConfigValue(config["passwordStrengthRegularExpression"], @"[\w| !§$%&/()=\-?\*]*"));
            _requiresQuestionAndAnswer = Convert.ToBoolean(
                SecurityUtil.GetConfigValue(config["requiresQuestionAndAnswer"], bool.FalseString));
            _requiresUniqueEmail = Convert.ToBoolean(
                SecurityUtil.GetConfigValue(config["requiresUniqueEmail"], bool.TrueString));

            // initialize custom fields
            _applicationName = SecurityUtil.GetConfigValue(config["applicationName"],
                System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            // >> FIX: http://www.codeplex.com/aspnetxmlproviders/WorkItem/View.aspx?WorkItemId=9700
            string fileName = SecurityUtil.GetConfigValue(config["fileName"], DefaultFileName);
            _fileName = SecurityUtil.MapPath(string.Format("~/App_Data/{0}", fileName));
            // << FIX
            // starter User setup
            string initialRole = config["initialRole"];
            string initialUser = config["initialUser"];
            string initialPassword = config["initialPassword"];
            if (!string.IsNullOrEmpty(initialRole) &&
                !string.IsNullOrEmpty(initialUser) &&
                !string.IsNullOrEmpty(initialPassword)) {
                //
                SetupInitialUser(initialRole, initialUser, initialPassword);
            }
        }

        /// <summary>
        /// Setups the specified saved state.
        /// </summary>
        /// <param name="savedState">State of the saved.</param>
        static void SetupInitialUser(string role, string user, string password) {

            if (Roles.Enabled) {
                if (!Roles.RoleExists(role)) Roles.CreateRole(role);
                Membership.CreateUser(user, password);
                Roles.AddUserToRole(user, role);
            }
        }
        #endregion
        #endregion
    }

    #region SaltedHash Class
    public sealed class SaltedHash {
        private readonly string _salt;
        private readonly string _hash;
        private const int saltLength = 6;

        public string Salt { get { return _salt; } }
        public string Hash { get { return _hash; } }

        public static SaltedHash Create(string password) {
            string salt = _createSalt();
            string hash = _calculateHash(salt, password);
            return new SaltedHash(salt, hash);
        }

        public static SaltedHash Create(string salt, string hash) {
            return new SaltedHash(salt, hash);
        }

        public bool Verify(string password) {
            string h = _calculateHash(_salt, password);
            return _hash.Equals(h);
        }

        private SaltedHash(string s, string h) {
            _salt = s;
            _hash = h;
        }

        private static string _createSalt() {
            byte[] r = _createRandomBytes(saltLength);
            return Convert.ToBase64String(r);
        }

        private static byte[] _createRandomBytes(int len) {
            byte[] r = new byte[len];
            new RNGCryptoServiceProvider().GetBytes(r);
            return r;
        }

        private static string _calculateHash(string salt, string password) {
            byte[] data = _toByteArray(salt + password);
            byte[] hash = _calculateHash(data);
            return Convert.ToBase64String(hash);
        }

        private static byte[] _calculateHash(byte[] data) {
            return new SHA1CryptoServiceProvider().ComputeHash(data);
        }

        private static byte[] _toByteArray(string s) {
            return System.Text.Encoding.UTF8.GetBytes(s);
        }
    }
    #endregion
}
