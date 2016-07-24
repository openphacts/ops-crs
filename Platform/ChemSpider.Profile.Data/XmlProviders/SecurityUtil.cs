using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace ChemSpider.Profile
{
    /// <summary>
    /// Summary description for ProviderUtil
    /// </summary>
    internal static class SecurityUtil {

        #region Static Fields /////////////////////////////////////////////////////////////////////

        public static readonly string DefaultFolder = "~/App_Data/";

        #endregion

        #region Static Methods //////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public static void EnsureDataFoler() {

            if(HttpContext.Current != null) {
                string folder = HttpContext.Current.Server.MapPath("~/App_Data/");
                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
            }
        }

        /// <summary>
        /// Gets the config value.
        /// </summary>
        /// <param name="configValue">The config value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetConfigValue(string configValue, string defaultValue) {
            return (String.IsNullOrEmpty(configValue))
                ? defaultValue : configValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public static string MapPath(string virtualPath) {
            // FIX: http://www.codeplex.com/aspnetxmlproviders/WorkItem/View.aspx?WorkItemId=6744
            return HostingEnvironment.MapPath(virtualPath);
        }
        #endregion
    }
}