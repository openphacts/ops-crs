using System.Collections;
using System.Xml.Linq;

namespace ChemSpider.Profile
{
	public abstract class XmlStore<T> where T : new()
	{
		public string FileName { get; private set; }
		public XDocument Xml { get; protected set; }
		protected T _value;
		protected object SyncRoot { get; private set; }

		public XmlStore(string fileName)
		{
			SyncRoot = new object();

			FileName = fileName;
			Xml = XDocument.Load(FileName);
		}

		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty
		{
			get
			{
				return (_value == null || (_value is ICollection && (_value as ICollection).Count == 0));
			}
		}

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		/// <value>The data.</value>
		public virtual T Value
		{
			get
			{
				lock (SyncRoot)
				{
					if (IsEmpty) Load();
					return _value;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_value = value;
				}
			}
		}

		protected abstract void Load();
		public abstract void Save();
	}
}
