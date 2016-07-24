using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ChemSpider.Profile.RSCID
{
    public class FileElement : ConfigurationElement
    {
        [ConfigurationProperty("path", IsKey = true, IsRequired = true)]
        public string Path
        {
            get { return this["path"] as string; }
        }
    }

    public class FilesToSkipCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileElement)element).Path;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public void Add(FileElement element)
        {
            this.BaseAdd(element);
        }
    }

    public class RSCIDSection : ConfigurationSection
    {
        public class IntValueElement : ConfigurationElement
        {
            [ConfigurationProperty("value", IsKey = true, IsRequired = true)]
            public int Value
            {
                get { return Convert.ToInt32(this["value"]); }
            }
        }

        public class StringValueElement : ConfigurationElement
        {
            [ConfigurationProperty("value", IsKey = true, IsRequired = true)]
            public string Value
            {
                get { return this["value"] as string; }
            }
        }

        public class UrlElement : ConfigurationElement
        {
            [ConfigurationProperty("url", IsKey = true, IsRequired = true)]
            public string Url
            {
                get { return this["url"] as string; }
            }
        }

        [ConfigurationProperty("platformId", IsKey = true, IsRequired = true)]
        public StringValueElement PlatformId
        {
            get { return this["platformId"] as StringValueElement; }
        }

        [ConfigurationProperty("secretKey", IsKey = true, IsRequired = true)]
        public StringValueElement SecretKey
        {
            get { return this["secretKey"] as StringValueElement; }
        }

        [ConfigurationProperty("cookieDomain")]
        public StringValueElement CookieDomain
        {
            get { return this["cookieDomain"] as StringValueElement; }
        }

        [ConfigurationProperty("extensionsToSkip")]
        public StringValueElement ExtensionsToSkip
        {
            get { return this["extensionsToSkip"] as StringValueElement; }
        }

        [ConfigurationProperty("authorization")]
        public UrlElement Authorization
        {
            get { return this["authorization"] as UrlElement; }
        }

        [ConfigurationProperty("accessToken")]
        public UrlElement AccessToken
        {
            get { return this["accessToken"] as UrlElement; }
        }

        [ConfigurationProperty("userDetails")]
        public UrlElement UserDetails
        {
            get { return this["userDetails"] as UrlElement; }
        }

        [ConfigurationProperty("isLoggedIn")]
        public UrlElement IsLoggedIn
        {
            get { return this["isLoggedIn"] as UrlElement; }
        }

        [ConfigurationProperty("header")]
        public UrlElement Header
        {
            get { return this["header"] as UrlElement; }
        }

        [ConfigurationProperty("footer")]
        public UrlElement Footer
        {
            get { return this["footer"] as UrlElement; }
        }

        [ConfigurationProperty("accessDeniedPage")]
        public UrlElement AccessDeniedPage
        {
            get { return this["accessDeniedPage"] as UrlElement; }
        }

        [ConfigurationProperty("manualConnectWithRSCIDPage")]
        public UrlElement ManualConnectWithRSCIDPage
        {
            get { return this["manualConnectWithRSCIDPage"] as UrlElement; }
        }

        [ConfigurationProperty("applicationHost")]
        public UrlElement ApplicationHost
        {
            get { return this["applicationHost"] as UrlElement; }
        }

        [ConfigurationProperty("filesToSkip")]
        [ConfigurationCollection(typeof(FilesToSkipCollection), AddItemName = "add")]
        public FilesToSkipCollection FilesToSkip
        {
            get { return this["filesToSkip"] as FilesToSkipCollection; }
        }

        [ConfigurationProperty("loggedInCookieExpiration")]
        public IntValueElement LoggedInCookieExpiration
        {
            get { return this["loggedInCookieExpiration"] as IntValueElement; }
        }
    }
}
