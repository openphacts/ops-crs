using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Search.Core
{
	/// <summary>
	/// Base class for all searches
	/// </summary>
	public abstract class CSSearch
	{
		public SearchOptions Options { get; private set; }

		public CommonSearchOptions CommonOptions { get; private set; }

		public SearchScopeOptions ScopeOptions { get; private set; }

		public SearchResultOptions ResultOptions { get; private set; }

		public virtual string Description { get; protected set; }

		public virtual void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
		{
			Options = options;
			CommonOptions = common;
			ScopeOptions = scopeOptions;
			ResultOptions = resultOptions;
		}

		public abstract void Run(/*CSSearchResult result*/);
	}
}
