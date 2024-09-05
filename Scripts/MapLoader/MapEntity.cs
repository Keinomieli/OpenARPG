using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenARPG.MapLoader
{
	public interface MapEntity
	{
		public void AfterMapLoad();
		public bool AutoReflectProperties {get;}
		public void OnReflectProperties(Dictionary<string,string> properties);
		
		public void ReflectProperties(Dictionary<string,string> properties) 
		{
			const BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			Type type = GetType();

			foreach (KeyValuePair<string, string> property in properties)
			{
				FieldInfo fieldInfo = type.GetField(property.Key, binding);
				if (fieldInfo != null)
				{
					object value = Convert.ChangeType(property.Value, fieldInfo.FieldType);
					fieldInfo.SetValue(this, value);
				}

				PropertyInfo propertyInfo = type.GetProperty(property.Key, binding);
				if (propertyInfo != null && propertyInfo.CanWrite)
				{
					object value = Convert.ChangeType(property.Value, propertyInfo.PropertyType);
					propertyInfo.SetValue(this, value);
				}
			}
		}
	}
}