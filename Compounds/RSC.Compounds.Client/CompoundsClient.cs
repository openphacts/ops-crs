using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading;
using ChemSpider.Search;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using ChemSpider.Utilities;

namespace RSC.Compounds
{
	public class CompoundsClient
	{
		protected Dictionary<string, string> ConvertToQueryParameters(object request, string prefix = null)
		{
			Dictionary<string, string> queryParams = new Dictionary<string, string>();

			PropertyInfo[] props = request.GetType().GetProperties();

			foreach (PropertyInfo pi in props)
			{
				object obj = pi.GetValue(request, null);
				if (obj != null)
				{
					string propName = string.IsNullOrEmpty(prefix) ? pi.Name : string.Format("{0}[{1}]", prefix, pi.Name);

					if (pi.PropertyType.IsEnum)
					{
						queryParams.Add(propName, obj.ToString());
					}
					else if (pi.PropertyType.IsGenericType && (
							pi.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
							pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ||
							pi.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
					{
						//	not implemented...
					}
					else if (pi.PropertyType.IsComplex())
					{
						foreach (var param in ConvertToQueryParameters(obj, propName))
							queryParams.Add(param.Key, param.Value);
					}
					else
					{
						queryParams.Add(propName, obj.ToString());
					}
				}
			}

			return queryParams;
		}

		public string SimpleSearch(
			SimpleSearchOptions searchOptions,
			CommonSearchOptions commonOptions = null,
			SearchScopeOptions scopeOptions = null,
			SearchResultOptions resultOptions = null)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(CompoundsIntegration.Url);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var parameters = ConvertToQueryParameters(new
				{
					searchOptions = searchOptions,
					commonOptions = commonOptions,
					scopeOptions = scopeOptions,
					resultOptions = resultOptions
				});

				var query = string.Join("&", parameters.Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value))));

				HttpResponseMessage response = client.GetAsync("api/searches/simple?" + query).Result;

				if (response.IsSuccessStatusCode)
				{
					return response.Content.ReadAsStringAsync().Result.Trim('"');
				}
				else
					return null;
			}
		}

		public string ExactStructureSearch(
			ExactStructureSearchOptions searchOptions,
			CommonSearchOptions commonOptions = null,
			SearchScopeOptions scopeOptions = null,
			SearchResultOptions resultOptions = null)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(CompoundsIntegration.Url);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var parameters = ConvertToQueryParameters(new
				{
					searchOptions = searchOptions,
					commonOptions = commonOptions,
					scopeOptions = scopeOptions,
					resultOptions = resultOptions
				});

				var query = string.Join("&", parameters.Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value))));

				HttpResponseMessage response = client.GetAsync("api/searches/exact?" + query).Result;

				if (response.IsSuccessStatusCode)
					return response.Content.ReadAsStringAsync().Result.Trim('"');
				else
					return null;
			}
		}

		public string SubstructureSearch(
			SubstructureSearchOptions searchOptions,
			CommonSearchOptions commonOptions = null,
			SearchScopeOptions scopeOptions = null,
			SearchResultOptions resultOptions = null)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(CompoundsIntegration.Url);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var parameters = ConvertToQueryParameters(new
				{
					searchOptions = searchOptions,
					commonOptions = commonOptions,
					scopeOptions = scopeOptions,
					resultOptions = resultOptions
				});

				var query = string.Join("&", parameters.Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value))));

				HttpResponseMessage response = client.GetAsync("api/searches/substructure?" + query).Result;

				if (response.IsSuccessStatusCode)
					return response.Content.ReadAsStringAsync().Result.Trim('"');
				else
					return null;
			}
		}

		public string SimilaritySearch(
			SimilaritySearchOptions searchOptions,
			CommonSearchOptions commonOptions = null,
			SearchScopeOptions scopeOptions = null,
			SearchResultOptions resultOptions = null)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(CompoundsIntegration.Url);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var parameters = ConvertToQueryParameters(new
				{
					searchOptions = searchOptions,
					commonOptions = commonOptions,
					scopeOptions = scopeOptions,
					resultOptions = resultOptions
				});

				var query = string.Join("&", parameters.Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value))));

				HttpResponseMessage response = client.GetAsync("api/searches/similarity?" + query).Result;

				if (response.IsSuccessStatusCode)
					return response.Content.ReadAsStringAsync().Result.Trim('"');
				else
					return null;
			}
		}

		public IEnumerable<int> WaitAndGetSearchResults(string rid)
		{
			while (GetSearchStatus(rid).Status == ERequestStatus.Processing)
				Thread.Sleep(1000);

			return GetSearchResult(rid);
		}

		public IEnumerable<ResultRecord> WaitAndGetSearchResultsWithRelevance(string rid)
		{
			while (GetSearchStatus(rid).Status == ERequestStatus.Processing)
				Thread.Sleep(1000);

			return GetSearchResultWithRelevance(rid);
		}

		public RequestStatus GetSearchStatus(string rid)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(CompoundsIntegration.Url);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				HttpResponseMessage response = client.GetAsync("api/searches/status/" + rid).Result;
				if (response.IsSuccessStatusCode)
				{
					return response.Content.ReadAsAsync<RequestStatus>().Result;
				}
			}

			return null;
		}

		public IEnumerable<int> GetSearchResult(string rid, int start = 0, int count = 0)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(CompoundsIntegration.Url);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				HttpResponseMessage response = client.GetAsync("api/searches/result/" + rid).Result;
				if (response.IsSuccessStatusCode)
				{
					return response.Content.ReadAsAsync<IEnumerable<int>>().Result;
				}
			}

			return null;
		}

		public IEnumerable<ResultRecord> GetSearchResultWithRelevance(string rid, int start = 0, int count = 0)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(CompoundsIntegration.Url);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				HttpResponseMessage response = client.GetAsync("api/searches/result/" + rid).Result;
				if (response.IsSuccessStatusCode)
					return response.Content.ReadAsAsync<IEnumerable<ResultRecord>>().Result;
				else
					return null;
			}
		}

		public IEnumerable<Compound> GetSearchResultAsCompounds(string rid, int start = 0, int count = 10)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(CompoundsIntegration.Url);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				HttpResponseMessage response = client.GetAsync("api/searches/resultasrecords/" + rid).Result;
				if (response.IsSuccessStatusCode)
					return response.Content.ReadAsAsync<IEnumerable<Compound>>().Result;
				else
					return null;
			}
		}
	}
}
