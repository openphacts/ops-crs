using Newtonsoft.Json;
using RSC.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RSC.CVSP
{
	public static class RecordExtensions
	{
		private static void LoadProperties(this Record record)
		{
			record.Properties = PropertyManager.Current.GetProperties(record.PropertyIDs).ToList();
		}

		public static bool HasProperty(this Record record, string name)
		{
			if (record.Properties == null)
			{
				if(record.PropertyIDs == null || !record.PropertyIDs.Any())
					return false;

				record.LoadProperties();
			}

			return record.Properties.Where(p => p.Name == name).Any();
		}

		public static object GetProperty(this Record record, string name)
		{
			if (record.Properties == null)
			{
				if (record.PropertyIDs == null || !record.PropertyIDs.Any())
					return null;

				record.LoadProperties();
			}

			var property = record.Properties.Where(p => p.Name == name).FirstOrDefault();

			if (property != null)
				return property.Value;

			return null;
		}

		public static IEnumerable<Property> GetProperties(this Record record)
		{
			if (record.Properties == null)
			{
				if (record.PropertyIDs == null || !record.PropertyIDs.Any())
					return null;

				record.LoadProperties();
			}

			return record.Properties.ToList();
		}

		public static string GetStringProperty(this Record record, string name)
		{
			var property = record.GetProperty(name);

			if (property != null)
				return property.ToString();

			return null;
		}

		public static int? GetIntProperty(this Record record, string name)
		{
			var property = record.GetProperty(name);

			if (property != null)
				return (int)property;

			return null;
		}

		public static void AddProperty(this Record record, string name, object value)
		{
			record.AddProperty(new Property() { Name = name, Value = value });
		}

		public static void AddProperty(this Record record, string name, object value, OriginValueType originType, Guid? provenanceId, Guid? originalUnitId = null, Condition condition = null)
		{
			record.AddProperty(new Property()
			{
				Name = name,
				Value = value,
				OriginType = originType,
				ProvenanceId = provenanceId,
				OriginalUnitId = originalUnitId,
				Conditions = condition == null ? null : new List<Condition>() { condition }
			});
		}

		public static void AddProperty(this Record record, string name, double value, double? error, OriginValueType originType, Guid? provenanceId, Guid? originalUnitId = null)
		{
			record.AddProperty(new Property()
			{
				Name = name,
				Value = value,
				Error = error,
				OriginType = originType,
				ProvenanceId = provenanceId,
				OriginalUnitId = originalUnitId
			});
		}

		public static void AddProperty(this Record record, Property property)
		{
			if (record.Properties == null)
				record.Properties = new List<Property>();

			(record.Properties as List<Property>).Add(property);
		}

		public static string ToJson(this Record record)
		{
			return JsonConvert.SerializeObject(record, Newtonsoft.Json.Formatting.Indented);
		}
	}
}
