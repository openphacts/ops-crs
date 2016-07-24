using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Linq;
using System.Xml.Linq;

namespace ChemSpider.Profile.Store
{
	/// <summary>
	/// 
	/// </summary>
	public partial class XmlRoleStore : XmlStore<List<XmlRole>>
	{
		#region Properties  /////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the roles.
		/// </summary>
		/// <value>The roles.</value>
		public virtual List<XmlRole> Roles { get { return Value; } }
		#endregion

		#region Construct  //////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlRoleStore"/> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		public XmlRoleStore(string fileName)
			: base(fileName)
		{
		}
		#endregion

		#region Methods /////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the role.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns></returns>
		public virtual XmlRole GetRole(string roleName)
		{

			lock (SyncRoot)
			{
				return Roles.Where(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			}
		}

		/// <summary>
		/// Gets the roles for user.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns></returns>
		public virtual List<XmlRole> GetRolesForUser(string userName)
		{

			lock (SyncRoot)
			{
				List<XmlRole> results = new List<XmlRole>();

				foreach (var role in Roles)
				{
					if (role.Users.Contains(userName))
						results.Add(role);
				}

				return results;
			}
		}

		/// <summary>
		/// Gets the users in role.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns></returns>
		public virtual string[] GetUsersInRole(string roleName)
		{

			lock (SyncRoot)
			{
				XmlRole role = GetRole(roleName);
				if (role != null)
				{
					return role.Users.ToArray();
				}
				else
					throw new Exception(string.Format("Role with name {0} does not exist!", roleName));
			}
		}

		/// <summary>
		/// Load roles from Xml file
		/// </summary>
		protected override void Load()
		{
			_value = (from r in Xml.Root.Elements("XmlRole")
					  select new XmlRole()
					  {
						  Name = r.Element("Name").Value,
						  Users = r.Element("Users") != null ? r.Element("Users").Descendants("User").Select(u => u.Value).ToList() : new List<string>()
					  }).ToList();

			if (_value == null)
				_value = new List<XmlRole>();
		}

		/// <summary>
		/// Save roles to Xml file
		/// </summary>
		public override void Save()
		{
			Xml = new XDocument();
			var root = new XElement("XmlRoles");
			Xml.Add(root);

			foreach (var role in Roles)
			{
				var xmlRole = new XElement("XmlRole");
				xmlRole.Add(new XElement("Name", role.Name));
				if (role.Users.Count > 0)
				{
					var xmlUsers = new XElement("Users");
					foreach (var user in role.Users)
						xmlUsers.Add(new XElement("User", user));

					xmlRole.Add(xmlUsers);
				}

				root.Add(xmlRole);
			}

			Xml.Save(FileName);
		}
		#endregion
	}
}
