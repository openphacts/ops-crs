using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Profile;
using System.Web.Security;

using ChemSpider.Profile.Store;

namespace ChemSpider.Profile
{

    /// <summary>
    /// Summary description for XmlProfileProvider
    /// TODO: implement some of the methods with respect of ProfileAuthenticationOption and result paging
    /// </summary>
    public class XmlProfileProvider : ProfileProvider {

        #region Static Fields ///////////////////////////////////////////////////////////

        public static string DefaultFileName = "Profile.xml";
        public static string DefaultProviderName = "XmlProfileProvider";
        public static string DefaultProviderDescription = "XML Profile Provider";

        #endregion

        #region Fields  /////////////////////////////////////////////////////////////////

        string _applicationName;
        string _fileName;
        XmlProfileStore _store;
		//WeakReference _storeRef;
        object _syncRoot = new object();

        #endregion

        #region Properties  /////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the name of the currently running application.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.String"></see> that contains the application's shortened name, which does not contain a full path or extension, for example, SimpleAppSettings.</returns>
        public override string ApplicationName {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        /// <summary>
        /// Gets the profiles store.
        /// </summary>
        /// <value>The store.</value>
        protected XmlProfileStore Store {
            get {
				//XmlProfileStore store = this.StoreRef.Target as XmlProfileStore;
				//if (store == null)
				//{
				//	this.StoreRef.Target = store = new XmlProfileStore(_fileName);
				//}
				//return store;

                if (_store == null) {
                    _store = new XmlProfileStore(_fileName);
                }
                return _store;
            }
        }

        /// <summary>
        /// Gets the store ref.
        /// </summary>
        /// <value>The store ref.</value>
		//private WeakReference StoreRef {
		//	get {
		//		return _storeRef ??
		//				(_storeRef = new WeakReference(new XmlProfileStore(_fileName)));
		//	}
		//}

        /// <summary>
        /// Gets the sync root.
        /// </summary>
        /// <value>The sync root.</value>
        protected internal object SyncRoot {
            get { return _syncRoot; }
        }
        #endregion

        #region Construct  //////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlProfileProvider"/> class.
        /// </summary>
        public XmlProfileProvider() {
        }
        #endregion

        #region Methods /////////////////////////////////////////////////////////////////

        /// <summary>
        /// When overridden in a derived class, deletes all user-profile data for profiles in which the last activity date occurred before the specified date.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"></see> values, specifying whether anonymous, authenticated, or both types of profiles are deleted.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"></see> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"></see>  value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        public override int DeleteInactiveProfiles(
            ProfileAuthenticationOption authenticationOption,
            DateTime userInactiveSinceDate) {

            int totalRecords = 0;
            try {
                lock (SyncRoot) {
                    ProfileInfoCollection coll = GetAllInactiveProfiles(
                        authenticationOption, userInactiveSinceDate, 0, int.MaxValue, out totalRecords);
                    foreach (ProfileInfo info in coll) {
                        this.Store.RemoveByUserName(info.UserName);
                    }
                    this.Store.Save();
                }
            }
            catch { throw; }
            //
            return totalRecords;
        }

        /// <summary>
        /// When overridden in a derived class, deletes profile properties and information for profiles that match the supplied list of user names.
        /// </summary>
        /// <param name="usernames">A string array of user names for profiles to be deleted.</param>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        public override int DeleteProfiles(string[] usernames) {

            if (usernames == null)
                throw new ArgumentNullException("usernames");

            try {
                lock (SyncRoot) {
                    foreach (string username in usernames) {
                        this.Store.RemoveByUserName(username);
                    }
                }
            }
            catch { throw; }
            //
            return usernames.Length;
        }

        /// <summary>
        /// When overridden in a derived class, deletes profile properties and information for the supplied list of profiles.
        /// </summary>
        /// <param name="profiles">A <see cref="T:System.Web.Profile.ProfileInfoCollection"></see>  of information about profiles that are to be deleted.</param>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        public override int DeleteProfiles(ProfileInfoCollection profiles) {

            if (profiles == null)
                throw new ArgumentNullException("profiles");

            try {
                lock (SyncRoot) {
                    foreach (ProfileInfo info in profiles) {
                        this.Store.RemoveByUserName(info.UserName);
                    }
                }
            }
            catch { throw; }
            //
            return profiles.Count;
        }

        /// <summary>
        /// When overridden in a derived class, retrieves profile information for profiles in which the last activity date occurred on or before the specified date and the user name matches the specified user name.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"></see> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"></see> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"></see> value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"></see> containing user profile information for inactive profiles where the user name matches the supplied usernameToMatch parameter.
        /// </returns>
        public override ProfileInfoCollection FindInactiveProfilesByUserName(
            ProfileAuthenticationOption authenticationOption, string usernameToMatch,
            DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords) {

            try {
                lock (SyncRoot) {
                    List<XmlProfile> profiles = this.Store.Profiles.FindAll(
                        delegate(XmlProfile profile) {
                            return (profile.LastUpdated <= userInactiveSinceDate) &&
                                (Membership.GetUser(profile.UserKey).UserName.StartsWith(usernameToMatch));
                        }
                    );
                    ///
                    totalRecords = profiles.Count;
                    return CreateProfileInfoCollection(profiles);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="usernameToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override ProfileInfoCollection FindProfilesByUserName(
            ProfileAuthenticationOption authenticationOption,
            string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {

            try {
                lock (SyncRoot) {
                    List<XmlProfile> profiles = this.Store.Profiles.FindAll(
                        delegate(XmlProfile profile) {
                            return Membership.GetUser(profile.UserKey).UserName.StartsWith(usernameToMatch);
                        }
                    );
                    ///
                    totalRecords = profiles.Count;
                    return CreateProfileInfoCollection(profiles);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="userInactiveSinceDate"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override ProfileInfoCollection GetAllInactiveProfiles(
            ProfileAuthenticationOption authenticationOption,
            DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords) {

            try {
                lock (SyncRoot) {
                    List<XmlProfile> profiles = this.Store.Profiles.FindAll(
                        delegate(XmlProfile profile) {
                            return profile.LastUpdated <= userInactiveSinceDate;
                        }
                    );
                    ///
                    totalRecords = profiles.Count;
                    return CreateProfileInfoCollection(profiles);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override ProfileInfoCollection GetAllProfiles(
            ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords) {

            try {
                lock (SyncRoot) {
                    totalRecords = this.Store.Profiles.Count;
                    return CreateProfileInfoCollection(this.Store.Profiles);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="userInactiveSinceDate"></param>
        /// <returns></returns>
        public override int GetNumberOfInactiveProfiles(
            ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate) {

            try {
                int cout = 0;
                lock (SyncRoot) {
                    GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, 0, int.MaxValue, out cout);
                }
                return cout;
            }
            catch { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection) {

            try {
                SettingsPropertyValueCollection coll = new SettingsPropertyValueCollection();
                if (collection.Count >= 1) {
                    string userName = (string)context["UserName"];
                    foreach (SettingsProperty prop in collection) {
                        if (prop.SerializeAs == SettingsSerializeAs.ProviderSpecific) {
                            if (prop.PropertyType.IsPrimitive || (prop.PropertyType == typeof(string))) {
                                prop.SerializeAs = SettingsSerializeAs.String;
                            }
                            else {
                                prop.SerializeAs = SettingsSerializeAs.Xml;
                            }
                        }
                        coll.Add(new SettingsPropertyValue(prop));
                    }
                    if (!string.IsNullOrEmpty(userName))
                        lock (SyncRoot) {
                            bool isAuthenticated = Convert.ToBoolean(context["IsAuthenticated"]);
                            GetPropertyValues(userName, coll, isAuthenticated);
                        }
                }
                return coll;
            }
            catch { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection) {

            try {
                string userName = context["UserName"] as string;
                bool isAuthenticated = Convert.ToBoolean(context["IsAuthenticated"]);
                if (isAuthenticated && !string.IsNullOrEmpty(userName) && collection.Count > 0) {
					MembershipUser user = Membership.GetUser(userName);

					var profile = Store.GetByUserKey((Guid)user.ProviderUserKey);

					bool needSave = false;
					foreach (SettingsPropertyValue prop in collection)
					{
						if (!prop.IsDirty)
						{
							continue;
						}

						profile.Values[prop.Name] = prop.PropertyValue.ToString();
						needSave = true;
					}

					if(needSave)
						Store.Save();
                }
            }
            catch { throw; }
        }

        #region - Helpers -

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profiles"></param>
        /// <returns></returns>
        protected internal ProfileInfoCollection CreateProfileInfoCollection(List<XmlProfile> profiles) {

            ProfileInfoCollection coll = new ProfileInfoCollection();
            MembershipUser user;
            foreach (var profile in profiles) {
                user = Membership.GetUser(profile.UserKey);
                if (user != null) {
                    //coll.Add(new ProfileInfo(user.UserName, false, user.LastActivityDate, profile.LastUpdated, profile.ValuesBinary.Length));
					coll.Add(new ProfileInfo(user.UserName, false, user.LastActivityDate, profile.LastUpdated, 0));
				}
            }
            ///
            return coll;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="svc"></param>
        protected internal void GetPropertyValues(string userName, SettingsPropertyValueCollection svc, bool isAuthenticated) {

            // get profile
            if (isAuthenticated)
			{
				var profile = this.Store.GetByUserName(userName);
				if (profile != null)
				{
					foreach (var prop in profile.Values)
					{
						SettingsPropertyValue value = svc[prop.Key];

						if (value != null)
							value.SerializedValue = prop.Value;
					}
				}
			}
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
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config) {

            if (config == null)
                throw new ArgumentNullException("config");
            SecurityUtil.EnsureDataFoler();
            /// prerequisite
            if (string.IsNullOrEmpty(name)) {
                name = DefaultProviderName;
            }
            if (string.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", DefaultProviderDescription);
            }

            // base initialize
            base.Initialize(name, config);

            // initialize fields
            _applicationName = SecurityUtil.GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            string fileName = SecurityUtil.GetConfigValue(config["fileName"], DefaultFileName);
			_fileName = SecurityUtil.MapPath(string.Format("~/App_Data/{0}", fileName));
        }
        #endregion
        #endregion
    }
}