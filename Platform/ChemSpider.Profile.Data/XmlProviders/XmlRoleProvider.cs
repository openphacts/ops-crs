using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Security;

using ChemSpider.Profile.Store;

namespace ChemSpider.Profile
{

    /// <summary>
    /// Specialized RoleProvider that uses a file (Roles.config) to store its data.
    /// </summary>
    public class XmlRoleProvider : RoleProvider {

        #region Static Fields ///////////////////////////////////////////////////////////

        public static string DefaultFileName = "Roles.xml";
        public static string DefaultProviderName = "XmlRoleProvider";
        public static string DefaultProviderDescription = "XML Role Provider";

        #endregion

        #region Fields  /////////////////////////////////////////////////////////////////

        string _applicationName;
        string _fileName;
        //XmlRoleStore _store;
        WeakReference _storeRef;
        object _syncRoot = new object();

        #endregion

        #region Properties  /////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application to store and retrieve role information for.</returns>
        public override string ApplicationName {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        /// <summary>
        /// Gets the role store.
        /// </summary>
        /// <value>The role store.</value>
        protected XmlRoleStore Store {
            get {
                XmlRoleStore store = this.StoreRef.Target as XmlRoleStore;
                if (store == null) {
                    this.StoreRef.Target = store = new XmlRoleStore(_fileName);
                }
                return store;
                //if (_store == null) {
                //Interlocked.CompareExchange(
                //    ref this._store,
                //    new XmlRoleStore(_fileName),
                //    null);
                //}
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
                        (_storeRef = new WeakReference(new XmlRoleStore(_fileName)));
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

        #region Construct  //////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRoleProvider"/> class.
        /// </summary>
        public XmlRoleProvider() {
        }
        #endregion

        #region Methods /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames) {

            // TODO way to optimize checks and adds!?
            try {
                lock (SyncRoot) {
                    foreach (string rolename in roleNames) {
                        if (!RoleExists(rolename)) {
                            throw new ProviderException("Role name not found.");
                        }
                    }
                    foreach (string username in usernames) {
                        if (username.IndexOf(',') > 0) {
                            throw new ArgumentException("User names cannot contain commas.");
                        }
                        foreach (string rolename in roleNames) {
                            if (IsUserInRole(username, rolename)) {
                                throw new ProviderException("User is already in role.");
                            }
                        }
                    }
                    foreach (string username in usernames) {
                        foreach (string rolename in roleNames) {
                            XmlRole role = this.Store.GetRole(rolename);
                            if (role != null)
                                role.Users.Add(username);
                        }
                    }
                    this.Store.Save();
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName) {

            if (roleName.IndexOf(',') > 0)
                throw new ArgumentException("Role name cannot contain commas.");
            if (RoleExists(roleName))
                throw new ProviderException("Role name already exists.");

            try {
                lock (SyncRoot) {
                    XmlRole role = new XmlRole();
                    role.Name = roleName;
                    role.Users = new List<string>();
                    this.Store.Roles.Add(role);
                    this.Store.Save();
                }
            }
            catch(Exception)
			{
				throw;
			}
        }

        /// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <param name="throwOnPopulatedRole">If true, throw an exception if roleName has one or more members and do not delete roleName.</param>
        /// <returns>
        /// true if the role was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) {

            if (!RoleExists(roleName))
                throw new ProviderException("Role does not exist.");
            if (GetUsersInRole(roleName).Length > 0)
                throw new ProviderException("Cannot delete a populated role.");

            try {
                lock (SyncRoot) {
                    XmlRole role = this.Store.GetRole(roleName);
                    if (role != null) {
                        this.Store.Roles.Remove(role);
                        this.Store.Save();
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches usernameToMatch and the user is a member of the specified role.
        /// </returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch) {

            if (!RoleExists(roleName))
                throw new ProviderException("Role does not exist.");

            try {
                lock (SyncRoot) {
                    XmlRole role = this.Store.GetRole(roleName);
                    if (role != null) {
                        return role.Users.ToArray();
                    }
                    else
                        return new string[] { };
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles() {

            try {
                lock (SyncRoot) {
                    int count = this.Store.Roles.Count;
                    string[] roles = new string[count];
                    for (int i = 0; i < count; i++) {
                        roles[i] = this.Store.Roles[i].Name;
                    }
                    return roles;
                }
            }
            catch(Exception) {
				throw;
			}
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username) {

            List<string> foundRoles = new List<string>();
            try {
                lock (SyncRoot) {
                    foreach (XmlRole role in this.Store.Roles) {
                        if (role.Users.Contains(username))
                            foundRoles.Add(role.Name);
                    }
                }
            }
            catch { throw; }
            return foundRoles.ToArray();
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        public override string[] GetUsersInRole(string roleName) {

            if (!RoleExists(roleName))
                throw new ProviderException("Role does not exist.");

            try {
                lock (SyncRoot) {
                    return this.Store.GetRole(roleName).Users.ToArray();
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName) {

            if (!RoleExists(roleName))
                throw new ProviderException("Role does not exist.");

            try {
                lock (SyncRoot) {
                    return this.Store.GetRole(roleName).Users.Contains(username);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) {

            try {
                lock (SyncRoot) {
                    foreach (string rolename in roleNames) {
                        if (!RoleExists(rolename)) {
                            throw new ProviderException("Role name not found.");
                        }
                    }
                    foreach (string rolename in roleNames) {
                        XmlRole role = this.Store.GetRole(rolename);
                        foreach (string username in usernames) {
                            role.Users.Remove(username);
                        }
                    }
                    this.Store.Save();
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool RoleExists(string roleName) {

            try {
                lock (SyncRoot) {
                    foreach (XmlRole role in this.Store.Roles) {
                        if (role.Name.Equals(roleName, StringComparison.CurrentCulture))
                            return true;
                    }
                }
            }
            catch { throw; }
            return false;
        }

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

            // initialize custom fields
            _applicationName = SecurityUtil.GetConfigValue(config["applicationName"],
                System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            string fileName = SecurityUtil.GetConfigValue(config["fileName"], DefaultFileName);
            string folder = SecurityUtil.GetConfigValue(config["folder"], SecurityUtil.DefaultFolder);
            if (!folder.EndsWith("/")) folder += "/";
            _fileName = SecurityUtil.MapPath(string.Format("{0}{1}", folder, fileName));
        }
        #endregion
        #endregion
    }
}