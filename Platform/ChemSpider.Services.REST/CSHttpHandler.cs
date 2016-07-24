using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChemSpider.Data.Database;
using System.Security;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml;
using ChemSpider.Utilities;
using System.Runtime.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Resources;
using ChemSpider.ObjectModel;

namespace ChemSpider.Web
{

	/// <summary>
	/// Summary description for CSHttpHandler
	/// </summary>
	public abstract class CSHttpHandler : IHttpHandler
	{
		static Dictionary<string, string> resourceAttrs = new Dictionary<string, string>();

		#region Attributes

		/// <summary>
		/// Specifies who is primarily using this service/operation. Internal ones are not subject to security checks at the moment, but that should be also bound to the presence of referrer field in a header.
		/// </summary>
		public enum ServiceUseType { Internal, External, Both };

		/// <summary>
		/// Service must have this attribute in order to expose its operations.
		/// </summary>
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
		public class CSServicesAttribute : Attribute
		{
			/// <summary>
			/// The name of the services group
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// The name of URL parameter which selects operation
			/// </summary>
			public string OperationParam { get; set; }

			/// <summary>
			/// General description of a group of service operations.
			/// </summary>
			public string Description { get; set; }

			/// <summary>
			/// Specifies class use type. Operation-level attribute can override it.
			/// </summary>
			public ServiceUseType UseType { get; set; }

			/// <summary>
			/// Serializer to use to deserialize complex parameters and serialize return value (if any)
			/// </summary>
			public Type Serializer { get; set; }

			/// <summary>
			/// Surrogate object that provides the methods needed to substitute one type for another by the DataContractSerializer during serialization, deserialization, and export and import of XML schema documents (XSD).
			/// </summary>
			public Type SerializerSurrogate { get; set; }

			/// <summary>
			/// List of known types needed for serialization/deserialization process
			/// </summary>
			public Type[] SerializerKnownTypes { get; set; }

			/// <summary>
			/// Specify xslt file transformation that will be used for service usage description
			/// </summary>
			public string XSLT { get; set; }

			/// <summary>
			/// Specify .resx resource file with ressources related to the service
			/// </summary>
			public string ResourceFile { get; set; }
		}

		/// <summary>
		/// Each method exposed as operation from HttpHandler-based service must be public and have this attribute.
		/// </summary>
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
		public class CSServiceAttribute : Attribute
		{
			/// <summary>
			/// URL parameter which binds operation to a method. Default is method's name.
			/// </summary>
			public string Operation { get; set; }

			/// <summary>
			/// Description of operation.
			/// </summary>
			public string Description { get; set; }

			/// <summary>
			/// Description of operation's return.
			/// </summary>
			public string ReturnDescription { get; set; }

			/// <summary>
			/// The name of service operation as registered in database.
			/// </summary>
			public string Service { get; set; }

			/// <summary>
			/// MIME-type of operation's response. Default is text/plain, unless method returns XDocument or XmlDocument in which case it's text/xml.
			/// </summary>
			public string ResponseMimeType { get; set; }

			/// <summary>
			/// If specified this attribute overrides one in service.
			/// </summary>
			public ServiceUseType UseType { get; set; }

			/// <summary>
			/// Serializer to use to deserialize complex parameters and serialize return value (if any). This attribute superceeds the one from CSServices.
			/// </summary>
			public Type Serializer { get; set; }

			/// <summary>
			/// Surrogate object that provides the methods needed to substitute one type for another by the DataContractSerializer during serialization, deserialization, and export and import of XML schema documents (XSD).
			/// </summary>
			public Type SerializerSurrogate { get; set; }

			/// <summary>
			/// List of known types needed for serialization/deserialization process
			/// </summary>
			public Type[] SerializerKnownTypes { get; set; }
		}

		/// <summary>
		/// Exclude service from documentation so it won't be visible from outside.
		/// </summary>
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
		public class CSHiddenServiceAttribute : Attribute
		{
		}

		/// <summary>
		/// Describe example of the service usage. Some simple example. One attribute per example.
		/// </summary>
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
		public class CSServiceUsageAttribute : Attribute
		{
			/// <summary>
			/// Usage example
			/// </summary>
			public string Example { get; set; }

			/// <summary>
			/// Short description what the usage suppose to do
			/// </summary>
			public string Description { get; set; }
		}

		/// <summary>
		/// Mandatory, discretionary or defined by the underlying type (value-based types are mandatory).
		/// </summary>
		public enum ParameterPresenceType { TypeDerived, Mandatory, Discretionary };

		/// <summary>
		/// Specifies where parameter value is taken from (URL, HTTP body or wherever it's found)
		/// </summary>
		public enum ParameterSource { Url, Body, Any };

		/// <summary>
		/// This attribute allows to specify additional binding rules, description and default value of parameters passed to operation.
		/// </summary>
		[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
		public class CSParameterAttribute : Attribute
		{
			/// <summary>
			/// The name of URL parameter to be bound to this method's parameter. By default parameter is bound if names match.
			/// </summary>
			public string Parameter { get; set; }

			/// <summary>
			/// Parameter description.
			/// </summary>
			public string Description { get; set; }

			/// <summary>
			/// Specifies whether parameter must be supplied or it is discretionary. By default value-type parameters are mandatory and reference-type parameters are discretionary.
			/// </summary>
			public ParameterPresenceType PresenceType { get; set; }

			/// <summary>
			/// Specifies where parameter value is taken from (URL, HTTP body or wherever it's found)
			/// </summary>
			public ParameterSource Source { get; set; }

			/// <summary>
			/// Default value of parameter.
			/// </summary>
			public string DefaultValue { get; set; }

			/// <summary>
			/// Mark parameter as obsolete. Contains deprication description.
			/// </summary>
			public string Obsolete { get; set; }
		}

		#endregion

		/// <summary>
		/// Override this method to return help information
		/// </summary>
		/// <returns></returns>
		protected virtual XDocument GetUsage(string message, bool detailed)
		{
			CSServicesAttribute[] cattrs = GetType().GetCustomAttributes(typeof(CSServicesAttribute), true) as CSServicesAttribute[];
			CSServicesAttribute cattr = cattrs.Length > 0 ? cattrs[0] : null;

			if (cattr != null && !string.IsNullOrEmpty(cattr.ResourceFile))
			{
				resourceAttrs.Clear();

				ResXResourceReader rsxr = new ResXResourceReader(HttpContext.Current.Request.PhysicalApplicationPath + cattr.ResourceFile);

				foreach (DictionaryEntry d in rsxr)
					resourceAttrs.Add(d.Key.ToString(), d.Value.ToString());
			}

			XDocument xdoc = new XDocument();

			if (cattr != null && !String.IsNullOrEmpty(cattr.XSLT))
				xdoc.Add(new XProcessingInstruction("xml-stylesheet", "href=\"" + cattr.XSLT + "\" type=\"text/xsl\""));

			xdoc.Add(new XElement(new XElement("application")));

			if ( !String.IsNullOrEmpty(message) )
				xdoc.Root.Add(new XElement("error", message));

			XElement xsvcs = new XElement("resources", new XAttribute("name", GetType().Name));
			xdoc.Root.Add(xsvcs);

			string appPath = string.Format("{0}://{1}{2}{3}",
										HttpContext.Current.Request.Url.Scheme,
										HttpContext.Current.Request.Url.Host,
										HttpContext.Current.Request.Url.Port == 80 ? string.Empty : ":" + HttpContext.Current.Request.Url.Port,
										HttpContext.Current.Request.ApplicationPath);

			xsvcs.Add(new XAttribute("base", appPath));
			xsvcs.Add(new XAttribute("page", HttpContext.Current.Request.Path));

			xsvcs.Add(new XAttribute("opParamName", cattr != null && !String.IsNullOrEmpty(cattr.OperationParam) ? cattr.OperationParam : "op|oper|cmd"));

			if ( cattr != null && !String.IsNullOrEmpty(cattr.Description) )
				xsvcs.Add(new XElement("description", cattr.Description));

			HashSet<Type> paramTypes = new HashSet<Type>();

			IEnumerable<MethodInfo> methods = GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(CSServiceAttribute), true).Length > 0);
			foreach ( MethodInfo mi in methods ) {
				if (mi.GetCustomAttributes(typeof(CSHiddenServiceAttribute), false).Any())
					continue;

				CSServiceAttribute mattr = ( mi.GetCustomAttributes(typeof(CSServiceAttribute), true) as CSServiceAttribute[] )[0];
				string service = mattr.Service ?? mi.DeclaringType.Name + "." + mi.Name;
				Type serializerType = mattr.Serializer ?? ( cattr != null ? cattr.Serializer : null );

				XElement xop = new XElement("service", new XAttribute("opParamValue", mattr.Operation ?? mi.Name), new XAttribute("name", service));

				xop.Add(new XAttribute("access", CSServiceUtils.GetAccessTypeString(service)));

				string description, returnDescription;

				_getMethodInfo(mi, out description, out returnDescription);

				if (!String.IsNullOrEmpty(description))
					xop.Add(new XElement("description", description));

				foreach (var a in _getUsageInfo(mi))
					xop.Add(new XElement("usage", new XElement("description", a.Description), new XElement("example", a.Example)));

				// Parameters
				bool bStreamReaderPresent = false, bSerialParams = false;
				ParameterInfo[] pis = mi.GetParameters();
				if ( pis.Length > 0 ) {
					XElement xpars = new XElement("parameters");
					foreach ( ParameterInfo pi in pis ) {
						if ( pi.ParameterType.Equals(typeof(TextReader)) || pi.ParameterType.IsSubclassOf(typeof(TextReader)) )
							bStreamReaderPresent = true;
						else if ( !pi.ParameterType.Equals(typeof(HttpContext)) && !pi.ParameterType.Equals(typeof(TextWriter)) && !pi.ParameterType.IsSubclassOf(typeof(TextWriter)) ) {
							bool mandatory;
							string parameter, def_value, obsolete;
							_getParamInfo(mi, pi, out def_value, out mandatory, out parameter, out description, out obsolete);
							XElement xpar = new XElement("parameter",
								new XAttribute("type", pi.ParameterType.Name),
								new XAttribute(pi.ParameterType.IsComplex() && serializerType != null ? "bodyParam" : "urlParam", parameter),
								new XAttribute("mandatory", mandatory ? "true" : "false"));
							if ( detailed )
								xpar.Add(new XAttribute("full-type", pi.ParameterType.FullName));
							if ( !String.IsNullOrEmpty(def_value) )
								xpar.Add(new XAttribute("defaultValue", def_value));
							if ( !String.IsNullOrEmpty(description) )
								xpar.Add(new XElement("description", description));
							if( !string.IsNullOrEmpty(obsolete))
								xpar.Add(new XElement("obsolete", obsolete));
							xpars.Add(xpar);

							// Add to types which we describe later
							if ( pi.ParameterType.IsComplex() && serializerType != null ) {
								bSerialParams = true;
								if ( !paramTypes.Contains(pi.ParameterType) )
									paramTypes.Add(pi.ParameterType);
							}
						}
					}

					// No need for POST as we can load stuff from URL now
					// if ( bStreamReaderPresent || bSerialParams )
					//    xop.Add(new XAttribute("httpRequestType", "POST"));

					xop.Add(xpars);
				}

				// Return type
				if ( mi.ReturnType != null ) {
					XElement xret = new XElement("return");

					if (mi.ReturnType.IsGenericType && (mi.ReturnType.GetGenericTypeDefinition() == typeof(List<>) || mi.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>)))
						xret.Add(new XAttribute("type", string.Format("{0}<{1}>", mi.ReturnType.Name.Replace("`1", ""), mi.ReturnType.GetGenericArguments()[0].Name)));
					else
						xret.Add(new XAttribute("type", mi.ReturnType.Name));

					if ( detailed )
						xret.Add(new XAttribute("full-type", mi.ReturnType.FullName));
					if ( serializerType != null )
						paramTypes.Add(mi.ReturnType);

					if (mattr != null && !String.IsNullOrEmpty(returnDescription))
						xret.Add(new XElement("description", returnDescription));

					xop.Add(xret);

					//  add return type to the list to describe it later
					if (mi.ReturnType.IsGenericType && (mi.ReturnType.GetGenericTypeDefinition() == typeof(List<>) || mi.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>)))
						paramTypes.Add(mi.ReturnType.GetGenericArguments()[0]);
					else
						paramTypes.Add(mi.ReturnType);
					
				}

				xsvcs.Add(xop);
			}

			if ( paramTypes.Count > 0 ) {
				HashSet<Type> describedTypes = new HashSet<Type>();
				XElement xtypes = new XElement("types");
				foreach ( Type t in paramTypes )
					DescribeType(xtypes, t, describedTypes, detailed);
				xdoc.Root.Add(xtypes);
			}

			return xdoc;
		}

		private static void DescribeType(XElement xmlParent, Type type, HashSet<Type> describedTypes, bool detailed)
		{
			if ( !describedTypes.Contains(type) ) {
				describedTypes.Add(type);
				if ( !type.FullName.StartsWith("System") && !type.FullName.StartsWith("Microsoft") ) {
					XElement xmlType = new XElement("type", new XAttribute("name", type.Name));
					if ( detailed )
						xmlType.Add(new XAttribute("full-name", type.FullName));
					xmlParent.Add(xmlType);

					string description = getResourceAttribute("Type_" + type.Name + "_Description");

					DescriptionAttribute attrDescr = (from a in type.GetCustomAttributes(typeof(DescriptionAttribute), true) select a as DescriptionAttribute).FirstOrDefault();
					if (attrDescr != null && string.IsNullOrEmpty(description))
						description = attrDescr.Description;

					if(!string.IsNullOrEmpty(description))
						xmlType.Add(new XElement("description", description));

					// IEnumerable<PropertyInfo> pi = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(DataMemberAttribute), true).Count() > 0);
					IEnumerable<PropertyInfo> pi = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
					foreach ( PropertyInfo p in pi ) {
						XElement xmlProp = new XElement("property", new XAttribute("name", p.Name));
						if ( detailed )
							xmlProp.Add(new XAttribute("full-type", p.PropertyType.FullName));

						if (p.PropertyType.IsGenericType && (
								p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) || 
								p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ||
								p.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
							xmlProp.Add(new XAttribute("type", string.Format("{0}<{1}>", p.PropertyType.Name.Replace("`1", ""), p.PropertyType.GetGenericArguments()[0].Name)));
						else
							xmlProp.Add(new XAttribute("type", p.PropertyType.Name));

						xmlType.Add(xmlProp);

						description = getResourceAttribute("Type_" + type.Name + "_" + p.Name + "_Description");

						attrDescr = (from a in p.GetCustomAttributes(typeof(DescriptionAttribute), true) select a as DescriptionAttribute).FirstOrDefault();
						if (attrDescr != null && string.IsNullOrEmpty(description))
							description = attrDescr.Description;

						if (!string.IsNullOrEmpty(description))
							xmlProp.Add(XElement.Parse("<description>" + description + "</description>"));
							//xmlProp.Add(new XElement("description", description));

						// Drill down
						if (p.PropertyType.IsEnum)
							DescribeEnum(xmlProp, p.PropertyType, describedTypes);
						else if (p.PropertyType.IsGenericType && (
								p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
								p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ||
								p.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
						{
							var genericType = p.PropertyType.GetGenericArguments()[0];

							if (genericType.IsEnum)
								DescribeEnum(xmlProp, genericType, describedTypes);
							else
								DescribeType(xmlProp, genericType, describedTypes, detailed);
						}
						else if (p.PropertyType.IsComplex())
							DescribeType(xmlProp, p.PropertyType, describedTypes, detailed);
					}
				}
			}
		}

		private static void DescribeEnum(XElement xmlParent, Type type, HashSet<Type> describedTypes)
		{
			XElement xmlType = new XElement("enum",
				new XAttribute("name", type.Name),
				new XAttribute("type", type.GetEnumUnderlyingType().Name));
			xmlParent.Add(xmlType);

			string[] names = type.GetEnumNames();
			Array values = type.GetEnumValues();
			for ( int i = 0; i < names.Length; i++ ) {
				XElement xmlEnumValue =
					new XElement("element",
						new XAttribute("name", names[i]),
						new XAttribute("value", (int)Enum.Parse(type, names[i])));
				xmlType.Add(xmlEnumValue);
			}
		}

		/// <summary>
		/// IHttpHandler main method implementation
		/// </summary>
		/// <param name="context">Request context</param>
		public virtual void ProcessRequest(HttpContext context)
		{
			bool detailed_usage = context.Request["usage"] != null;
			CSServicesAttribute[] cattrs = GetType().GetCustomAttributes(typeof(CSServicesAttribute), true) as CSServicesAttribute[];
			CSServicesAttribute cattr = cattrs.Length > 0 ? cattrs[0] : null;
			IEnumerable<MethodInfo> methods = GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(CSServiceAttribute), true).Length > 0);
			bool _internal = cattr == null || cattr.UseType == ServiceUseType.Internal;

			// Figure out operation
			string op =
				cattr != null && !String.IsNullOrEmpty(cattr.OperationParam) ? context.Request[cattr.OperationParam] :
				context.Request["op"] ?? context.Request["oper"] ?? context.Request["cmd"];
			if ( String.IsNullOrEmpty(op) ) {
				context.Response.StatusCode = 200;
				context.Response.ContentType = "text/xml";
				context.Response.Write(GetUsage("Operation parameter is not specified", detailed_usage));
				return;
			}

			// Try to match the method by operation name or method name
			MethodInfo mi = null;
			CSServiceAttribute mattr = null;
			foreach ( MethodInfo _mi in methods ) {
				mattr = ( _mi.GetCustomAttributes(typeof(CSServiceAttribute), true) as CSServiceAttribute[] )[0];
				if ( op.Equals(mattr.Operation, StringComparison.InvariantCultureIgnoreCase) ||
					( String.IsNullOrEmpty(mattr.Operation) && _mi.Name.Equals(op, StringComparison.InvariantCultureIgnoreCase) ) ) {
					mi = _mi;
					break;
				}
			}

			if ( mi == null ) {
				// No matching method found - bail out
				context.Response.StatusCode = 200;
				context.Response.ContentType = "text/xml";
				context.Response.Write(GetUsage(String.Format("Requested operation ({0}) cannot be found", op), detailed_usage));
				return;
			}

			// Figure out service and content type
			string service =
				!String.IsNullOrEmpty(mattr.Service) ? mattr.Service :
				mi.DeclaringType.Name + "." + mi.Name;

			context.Response.ContentType =
				!String.IsNullOrEmpty(mattr.ResponseMimeType) ? context.Response.ContentType = mattr.ResponseMimeType :
				mi.ReturnType.Equals(typeof(XNode)) || mi.ReturnType.Equals(typeof(XmlNode)) || mi.ReturnType.IsSubclassOf(typeof(XNode)) || mi.ReturnType.IsSubclassOf(typeof(XmlNode)) ? "text/xml" :
				"text/plain";

			Type serializerType = mattr.Serializer ?? ( cattr != null ? cattr.Serializer : null );

			// Figure out access rights
			string token = context.Request["token"] ?? context.Request["tk"];
			if ( String.IsNullOrEmpty(token) && CSServiceUtils.IsTokenRequired(service) ) {
				// Token is required, but was not provided - bail out
				context.Response.StatusCode = 200;  // 403
				context.Response.ContentType = "text/xml";
				context.Response.Write(GetUsage(String.Format("Requested operation ({0}) requires token parameter which was not provided", op), detailed_usage));
				return;
			}

			// Loop through method's parameters
			bool bStreamReaderPresent = false, bStreamWriterPresent = false;
			IssuesCollection issues = new IssuesCollection();
			List<object> param = new List<object>();
			Dictionary<string, KeyValuePair<ParameterInfo, int>> paramsInBody = new Dictionary<string, KeyValuePair<ParameterInfo, int>>();
			Dictionary<string, KeyValuePair<ParameterInfo, int>> paramsInUrl = new Dictionary<string, KeyValuePair<ParameterInfo, int>>();
			int i = 0;
			foreach ( ParameterInfo pi in mi.GetParameters() ) {
				bool mandatory;
				string parameter, def_value, description, obsolete;
				_getParamInfo(pi, out def_value, out mandatory, out parameter, out description, out obsolete);

				if ( pi.ParameterType.Equals(typeof(HttpContext)) )
					param.Add(context);
				else if ( pi.ParameterType.Equals(typeof(TextReader)) || pi.ParameterType.IsSubclassOf(typeof(TextReader)) ) {
					param.Add(new StreamReader(context.Request.InputStream));
					bStreamReaderPresent = true;
				}
				else if ( pi.ParameterType.Equals(typeof(TextWriter)) || pi.ParameterType.IsSubclassOf(typeof(TextWriter)) ) {
					param.Add(context.Response.Output);
					bStreamWriterPresent = true;
				}
				else if ( pi.ParameterType.IsComplex() ) {
					(context.Request.HttpMethod == "GET" ? paramsInUrl : serializerType == null ? paramsInUrl : paramsInBody)
						.Add(parameter, new KeyValuePair<ParameterInfo, int>(pi, i));   // Collect parameters to be deserialized
					param.Add(null);
				}
				else if ( !String.IsNullOrEmpty(context.Request[parameter]) )
					param.Add(Convert.ChangeType(context.Request[parameter], pi.ParameterType));
				else if ( context.Request.HttpMethod == "POST" && serializerType != null ) {    // Parameter is not complex, but not provided in URL, so maybe serialized in the body
					paramsInBody.Add(parameter, new KeyValuePair<ParameterInfo, int>(pi, i));   // Collect parameters to be deserialized
					param.Add(String.IsNullOrEmpty(def_value) ? null : Convert.ChangeType(def_value, pi.ParameterType));
				}
				else if ( !String.IsNullOrEmpty(def_value) )
					param.Add(Convert.ChangeType(def_value, pi.ParameterType));
				else if ( !mandatory )
					param.Add(null);
				else {
					issues.Add(Severity.Error, "Mandatory parameter '{0}' cannot be calculated", parameter);
				}

				i++;
			}

			// objects deserialized from the URL
			if ( paramsInUrl.Count > 0 ) {
				foreach ( KeyValuePair<string, KeyValuePair<ParameterInfo, int>> p in paramsInUrl ) {
					bool mandatory;
					string parameter, def_value, description, obsolete;
					_getParamInfo(p.Value.Key, out def_value, out mandatory, out parameter, out description, out obsolete);

					object o = LoadObjectFromRequest(p.Key, p.Value.Key.ParameterType, context.Request);
					if ( o != null )
						param[p.Value.Value] = o;
					else
						issues.Add(mandatory ? Severity.Error : Severity.Warning, "Parameter '{0}' cannot be calculated", parameter);
				}
			}

			// objects deserialized from the body
			if ( paramsInBody.Count > 0 ) {
				if ( bStreamReaderPresent ) {
					issues.Add(Severity.Error, "Method contains both stream reader and complex parameters which are to be deserialized from the body");
				}
				else if ( bStreamWriterPresent && serializerType != null && mi.ReturnType.IsComplex() )
					issues.Add(Severity.Error, "Method contains both stream writer and complex return value which is to be serialized into body");
				else {
					// Prepare properties list for the new type
					Dictionary<string, Type> props = new Dictionary<string, Type>();
					foreach ( string parameter in paramsInBody.Keys )
						props.Add(parameter, paramsInBody[parameter].Key.ParameterType);

					Type type = ReflectUtils.BuildType("__" + GetType().Name + "_" + mi.Name + "_deserialization_type", props);

					try {
						object o = context.Request.InputStream.Deserialize(serializerType, type);
						foreach ( string prop in props.Keys )
							param[paramsInBody[prop].Value] = type.GetProperty(o, prop);
					}
					catch ( Exception ex ) {
						issues.Add(Severity.Warning, "Parameters cannot be deserialized: {0}\n", ex.InnerException != null ? ex.InnerException.Message : ex.Message);
					}
				}
			}

			// Some of mandatory parameters were not provided
			if ( issues.IsError ) {
				context.Response.StatusCode = 200;
				context.Response.ContentType = "text/xml";
				context.Response.Write(GetUsage(issues.ToString(), detailed_usage));
				return;
			}

			try {
				CSServiceUtils.GetWebServiceAccess(token, service);

				object o = mi.Invoke(this, param.ToArray());
				if ( o != null ) {
					if ( o is byte[] )
						context.Response.BinaryWrite(o as byte[]);
					else {
						string callback = context.Request["callback"];

						if ( serializerType != null && o.GetType().IsComplex() ) {
							if ( !String.IsNullOrEmpty(callback) ) {
								context.Response.Write(String.Format("{0}(", callback));
								context.Response.Flush();
							}

							Type surrogateType = mattr.SerializerSurrogate ?? (cattr != null ? cattr.SerializerSurrogate : null);
							if (surrogateType != null)
							{
								IDataContractSurrogate surrogate = (IDataContractSurrogate)surrogateType.Instantiate();
								Type[] knownTypes = mattr.SerializerKnownTypes ?? (cattr != null ? cattr.SerializerKnownTypes : null);
								context.Response.OutputStream.Serialize(serializerType, o, knownTypes, int.MaxValue, false, surrogate, false);
							}
							else
								context.Response.OutputStream.Serialize(serializerType, o);

							if ( !String.IsNullOrEmpty(callback) ) {
								context.Response.OutputStream.Flush();
								context.Response.Write(");");
							}
						}
						else {
							if ( !String.IsNullOrEmpty(callback) ) {
								context.Response.Write(String.Format("{0}({1}", callback, o.GetType().IsQuotable() ? "'" : ""));
								context.Response.Flush();
							}
							context.Response.Write(o);
							if ( !String.IsNullOrEmpty(callback) ) {
								context.Response.OutputStream.Flush();
								context.Response.Write(String.Format("{0});", o.GetType().IsQuotable() ? "'" : ""));
							}
						}
					}
				}

				CSServiceUtils.LogServiceTransaction(token, service);
			}
			catch ( Exception ex ) {
				bool _handled = false;
				if ( ex.InnerException != null ) {
					if ( ex.InnerException is RESTServiceException ) {
						CSServiceUtils.LogServiceError(token, ex.InnerException.ToString(), service);
						context.Response.Clear();
						context.Response.AddHeader("Access-Control-Allow-Origin", "*");
						context.Response.StatusCode = ( ex.InnerException as RESTServiceException ).HttpCode;
						context.Response.Write(ex.InnerException.Message);
						_handled = true;
					}
					else if ( ex.InnerException is SecurityException ) {
						CSServiceUtils.LogServiceError(token, ex.InnerException.ToString(), service);
						context.Response.Clear();
						context.Response.AddHeader("Access-Control-Allow-Origin", "*");
						context.Response.StatusCode = 403;
						context.Response.Write(ex.InnerException.Message);
						_handled = true;
					}
					else {
						CSServiceUtils.LogServiceError(token, ex.InnerException.ToString(), service);
						context.Response.Clear();
						context.Response.AddHeader("Access-Control-Allow-Origin", "*");
						context.Response.StatusCode = 500;
						context.Response.Write(ex.InnerException.Message);
						_handled = true;
					}
				}

				if ( !_handled ) {
					CSServiceUtils.LogServiceError(token, ( ex.InnerException ?? ex ).ToString(), service);
					context.Response.Clear();
					context.Response.AddHeader("Access-Control-Allow-Origin", "*");
					context.Response.StatusCode = 500;
					context.Response.Write(ex.Message);
				}
			}
		}

		public static object LoadObjectFromRequest(string prefix, Type type, HttpRequest request)
		{
			if (type.IsArray || type.GetInterface("IList") != null)
			{
				Regex re = new Regex(prefix.Replace("[", "\\[").Replace("]", "\\]") + "\\[(\\d+)\\]", RegexOptions.IgnoreCase);
				var parameters = request.Params.AllKeys.Where(k => re.Match(k).Success);
				int max = 0;

				if(parameters.Count() > 0)
					max = parameters.Select(k => re.Match(k).Groups[1].Value).Max(k => int.Parse(k)) + 1;

				if (type.IsArray)
				{
					Array os = type.Instantiate(max) as Array;
					for (int i = 0; i < max; i++)
					{
						object o = type.GetElementType().Instantiate();
						x_loadObjectFromRequest(String.Format("{0}[{1}]", prefix, i), type.GetElementType(), request, ref o);
						os.SetValue(o, i);
					}

					return os;
				}
				else
				{
					IList lst = type.Instantiate() as IList;

					for (int i = 0; i < max; i++)
					{
						object o = type.GetGenericArguments()[0].Instantiate("");
						x_loadObjectFromRequest(String.Format("{0}[{1}]", prefix, i), type.GetGenericArguments()[0], request, ref o);
						lst.Add(o);
					}

					return lst;
				}
			}
			else {
				object o = type.Instantiate();
				x_loadObjectFromRequest(prefix, type, request, ref o);
				return o;
			}
		}

		private static void x_loadObjectFromRequest(string prefix, Type type, HttpRequest request, ref object o)
		{
			if ( !type.IsComplex() || Type.GetTypeCode(type) == TypeCode.DateTime ) {
				o = Convert.ChangeType(request[prefix], type);
			}
			else {
				foreach ( PropertyInfo pi in type.GetProperties() ) {
					if (pi.CanWrite)
					{
						string reqPropName = prefix + "." + pi.Name;
						if (pi.PropertyType.IsEnum && request[reqPropName] != null)
							type.SetProperty(o, pi.Name, Enum.Parse(pi.PropertyType, request[reqPropName]));
						else if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
						{
							if (request[reqPropName] != null)
							{
								type.SetProperty(o, pi.Name, Convert.ChangeType(request[reqPropName], Nullable.GetUnderlyingType(pi.PropertyType)));
							}
						}
						else if (pi.PropertyType.IsComplex())
							type.SetProperty(o, pi.Name, LoadObjectFromRequest(reqPropName, pi.PropertyType, request));
						else if (request[reqPropName] == null)
							type.SetProperty(o, pi.Name, pi.PropertyType.IsValueType ? Activator.CreateInstance(pi.PropertyType) : null);
						else
						{
							object v = Convert.ChangeType(request[reqPropName], pi.PropertyType);
							type.SetProperty(o, pi.Name, v);
						}
					}
				}
			}
		}

		public static T LoadObjectFromRequest<T>(string prefix, HttpRequest request)
		{
			return (T)LoadObjectFromRequest(prefix, typeof(T), request);
		}

		private static string getResourceAttribute(string name)
		{
			return resourceAttrs.ContainsKey(name) ? resourceAttrs[name] : null;
		}

		private static bool _getMethodInfo(MethodInfo mi, out string description, out string returnDescription)
		{
			description = getResourceAttribute("Method_" + mi.Name + "_Description");
			returnDescription = getResourceAttribute("Method_" + mi.Name + "_ReturnDescription");

			CSServiceAttribute su = (from a in mi.GetCustomAttributes(typeof(CSServiceAttribute), true) select a as CSServiceAttribute).FirstOrDefault();
			if (su != null)
			{
				description = description ?? su.Description;
				returnDescription = returnDescription ?? su.ReturnDescription;
			}

			return !string.IsNullOrEmpty(description) || !string.IsNullOrEmpty(returnDescription);
		}

		private static void _getParamInfo(MethodInfo mi, ParameterInfo pi, out string def_value, out bool mandatory, out string parameter, out string description, out string obsolete)
		{
			_getParamInfo(pi, out def_value, out mandatory, out parameter, out description, out obsolete);

			string resDescription = getResourceAttribute("Method_" + mi.Name + "_" + pi.Name + "_Description");
			if (!string.IsNullOrEmpty(resDescription)) description = resDescription;
		}

		private static void _getParamInfo(ParameterInfo pi, out string def_value, out bool mandatory, out string parameter, out string description, out string obsolete)
		{
			CSParameterAttribute[] pa = pi.GetCustomAttributes(typeof(CSParameterAttribute), true) as CSParameterAttribute[];
			if ( pa.Length > 0 ) {
				parameter = pa[0].Parameter ?? pi.Name;
				mandatory =
					pa[0].PresenceType == ParameterPresenceType.Mandatory ? true :
					pa[0].PresenceType == ParameterPresenceType.Discretionary ? false :
					pi.ParameterType.IsValueType;
				def_value = pa[0].DefaultValue;
				description = pa[0].Description;
				obsolete = pa[0].Obsolete;
			}
			else {
				parameter = pi.Name;
				mandatory = pi.ParameterType.IsValueType;
				def_value = null;
				description = null;
				obsolete = string.Empty;
			}
		}

		private static List<CSServiceUsageAttribute> _getUsageInfo(MethodInfo mi)
		{
			List<CSServiceUsageAttribute> list = new List<CSServiceUsageAttribute>();

			string description = getResourceAttribute("MethodUsage_" + mi.Name + "_Description");
			string example = getResourceAttribute("MethodUsage_" + mi.Name + "_Example");

			if (!string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(example))
				list.Add(new CSServiceUsageAttribute() { Description = description, Example = example });

			description = getResourceAttribute("MethodUsage2_" + mi.Name + "_Description");
			example = getResourceAttribute("MethodUsage2_" + mi.Name + "_Example");

			if (!string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(example))
				list.Add(new CSServiceUsageAttribute() { Description = description, Example = example });

			description = getResourceAttribute("MethodUsage3_" + mi.Name + "_Description");
			example = getResourceAttribute("MethodUsage3_" + mi.Name + "_Example");

			if (!string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(example))
				list.Add(new CSServiceUsageAttribute() { Description = description, Example = example });

			list.AddRange(from a in mi.GetCustomAttributes(typeof(CSServiceUsageAttribute), true) select a as CSServiceUsageAttribute);

			return list;
		}

		/// <summary>
		/// BadRequestException should be thrown when HTTP requests arguments do not make sense. It results in 400 response code.
		/// </summary>
		protected class BadRequestException : Exception
		{
			public BadRequestException(string format, params object[] args)
				: base(String.Format(format, args))
			{

			}
		}

		/// <summary>
		/// BadRequestException should be thrown when HTTP requests arguments do not make sense. It results in 400 response code.
		/// </summary>
		protected class RESTServiceException : Exception
		{
			int _httpCode;
			public int HttpCode { get { return _httpCode; } }

			public RESTServiceException(int httpCode, string format, params object[] args)
				: base(String.Format(format, args))
			{
				_httpCode = httpCode;
			}
		}

		protected ChemSpiderDB m_csdb = new ChemSpiderDB();
		protected ChemSpiderBlobsDB m_csbdb = new ChemSpiderBlobsDB();
		protected ChemUsersDB m_cudb = new ChemUsersDB();

		public virtual bool IsReusable
		{
			get { return true; }
		}

		protected bool IsMasterCurator
		{
			get { return HttpContext.Current.User.IsInRole("Master Curator"); }
		}

		protected bool IsAdmin
		{
			get { return HttpContext.Current.User.IsInRole("Administrator"); }
		}

		protected bool IsCurator
		{
			get { return HttpContext.Current.User.IsInRole("Curator"); }
		}

		protected bool IsDepositor
		{
			get { return HttpContext.Current.User.IsInRole("Depositor"); }
		}

		protected bool IsBetaTester
		{
			get { return HttpContext.Current.User.IsInRole("Beta Tester"); }
		}

		protected bool IsServiceSubscriber
		{
			get { return HttpContext.Current.User.IsInRole("Service Subscriber"); }
		}
	}
}
