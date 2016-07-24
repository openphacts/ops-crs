using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ChemSpider.Profile.Store
{

	/// <summary>
	/// 
	/// </summary>
	public partial class XmlUserStore : XmlStore<List<XmlUser>>
	{

		#region Properties  /////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the users.
		/// </summary>
		/// <value>The users.</value>
		public virtual List<XmlUser> Users { get { return base.Value; } }
		#endregion

		#region Construct  //////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <see cref="UserStore"/> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		public XmlUserStore(string fileName)
			: base(fileName)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlUserStore"/> class.
		/// </summary>
		//protected XmlUserStore()
		//	: base(null) {
		//}
		#endregion

		#region Methods /////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the user by email.
		/// </summary>
		/// <param name="email">The email.</param>
		/// <returns></returns>
		public virtual XmlUser GetUserByEmail(string email)
		{
			lock (SyncRoot)
			{
				return (Users != null)
					? Users.Find(delegate(XmlUser user) { return string.Equals(email, user.Email); })
					: null;
			}
		}

		/// <summary>
		/// Gets the user by key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public virtual XmlUser GetUserByKey(Guid key)
		{
			lock (SyncRoot)
			{
				return (Users != null)
					? Users.Find(delegate(XmlUser user) { return (user.UserKey.CompareTo(key) == 0); })
					: null;
			}
		}

		/// <summary>
		/// Gets the name of the user by.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public virtual XmlUser GetUserByName(string name)
		{
			lock (SyncRoot)
			{
				return (Users != null)
					? Users.Find(delegate(XmlUser user) { return string.Equals(name, user.UserName); })
					: null;
			}
		}

		/// <summary>
		/// Load roles from Xml file
		/// </summary>
		protected override void Load()
		{
			lock (SyncRoot)
			{
				_value = (from user in Xml.Root.Elements("XmlUser")
						  select new XmlUser()
						  {
							  UserKey = Guid.Parse(user.Element("UserKey").Value),
							  UserName = user.Element("UserName").Value,
							  Password = user.Element("Password").Value,
							  PasswordSalt = user.Element("PasswordSalt").Value,
							  Email = user.Element("Email").Value,
							  PasswordQuestion = user.Element("PasswordQuestion").Value,
							  PasswordAnswer = user.Element("PasswordAnswer").Value,
							  CreationDate = Convert.ToDateTime(user.Element("CreationDate").Value),
							  LastLoginDate = Convert.ToDateTime(user.Element("LastLoginDate").Value),
							  LastPasswordChangeDate = Convert.ToDateTime(user.Element("LastPasswordChangeDate").Value),
							  LastLockoutDate = Convert.ToDateTime(user.Element("LastLockoutDate").Value),
							  IsApproved = Convert.ToBoolean(user.Element("IsApproved").Value),
							  IsLockedOut = Convert.ToBoolean(user.Element("IsLockedOut").Value)
						  }).ToList();

				if (_value == null)
					_value = new List<XmlUser>();
			}
		}

		/// <summary>
		/// Save roles to Xml file
		/// </summary>
		public override void Save()
		{
			lock (SyncRoot)
			{
				Xml = new XDocument();
				var root = new XElement("XmlUsers");
				Xml.Add(root);

				foreach (var user in Users)
				{
					var xmlUser = new XElement("XmlUser");
					xmlUser.Add(new XElement("UserKey", user.UserKey));
					xmlUser.Add(new XElement("UserName", user.UserName));
					xmlUser.Add(new XElement("Password", user.Password));
					xmlUser.Add(new XElement("PasswordSalt", user.PasswordSalt));
					xmlUser.Add(new XElement("Email", user.Email));
					xmlUser.Add(new XElement("PasswordQuestion", user.PasswordQuestion));
					xmlUser.Add(new XElement("PasswordAnswer", user.PasswordAnswer));
					xmlUser.Add(new XElement("CreationDate", user.CreationDate));
					xmlUser.Add(new XElement("LastLoginDate", user.LastLoginDate));
					xmlUser.Add(new XElement("LastPasswordChangeDate", user.LastPasswordChangeDate));
					xmlUser.Add(new XElement("LastLockoutDate", user.LastLockoutDate));
					xmlUser.Add(new XElement("IsApproved", user.IsApproved));
					xmlUser.Add(new XElement("IsLockedOut", user.IsLockedOut));

					root.Add(xmlUser);
				}

				Xml.Save(FileName);
			}
		}
		#endregion
	}
}
