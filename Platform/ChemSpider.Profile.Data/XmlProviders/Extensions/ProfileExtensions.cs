using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Profile;

namespace ChemSpider.Profile
{
	public static class ProfileExtensions
	{
		public static string FirstName(this ProfileBase profile)
		{
			try
			{
				return profile.GetPropertyValue("FirstName").ToString();
			}
			catch(Exception)
			{
				return string.Empty;
			}
		}

		public static string LastName(this ProfileBase profile)
		{
			try
			{
				return profile.GetPropertyValue("LastName").ToString();
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static string DisplayName(this ProfileBase profile)
		{
			try
			{
				return profile.GetPropertyValue("DisplayName").ToString();
			}
			catch (Exception)
			{
				return profile.FirstName() + " " + profile.LastName();
			}
		}

		public static string Email(this ProfileBase profile)
		{
			try
			{
				return profile.GetPropertyValue("Email").ToString();
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}
