using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace DotAgent.Core.Util;

public static class JsonUtilities
{
    public static string GenerateSchema(Type type,string name, string description)
    {
        ArgumentNullException.ThrowIfNull(type);

        var schema = new JsonSchema
        {
            Name = name ?? type.Name,
            Description = description ?? $"Auto-generated schema for {type.Name}",
            Parameters = new Parameters
            {
                Type = "object",
                Properties = new Dictionary<string, Property>(),
                Required = []
            }
        };

        foreach (var property in type.GetProperties())
        {
            var propertySchema = GetPropertySchema(property);
            schema.Parameters.Properties.Add(property.Name, propertySchema);

            // Check if nullable:
            bool isNullable = (property.PropertyType.IsClass && property.PropertyType != typeof(string)) ||
                              (Nullable.GetUnderlyingType(property.PropertyType) != null);

            // Also, arrays (List<T>) are technically reference types 
            bool isArray = property.PropertyType.IsGenericType && 
                           property.PropertyType.GetGenericTypeDefinition() == typeof(List<>);

            if (!isNullable || isArray)
            {
                schema.Parameters.Required.Add(property.Name);
            }
        }

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    private static Property GetPropertySchema(PropertyInfo property)
    {
        var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
        var propertyType = property.PropertyType;

        // Handle nullable types
        if (Nullable.GetUnderlyingType(propertyType) != null)
        {
            propertyType = Nullable.GetUnderlyingType(propertyType);
        }

        var propertySchema = new Property
        {
            Description = description
        };

        if (propertyType == typeof(string))
        {
            propertySchema.Type = "string";
        }
        else if (propertyType == typeof(int) || propertyType == typeof(long) || 
                 propertyType == typeof(short) || propertyType == typeof(byte))
        {
            propertySchema.Type = "integer";
        }
        else if (propertyType == typeof(float) || propertyType == typeof(double) || 
                 propertyType == typeof(decimal))
        {
            propertySchema.Type = "number";
        }
        else if (propertyType == typeof(bool))
        {
            propertySchema.Type = "boolean";
        }
        else if (propertyType.IsEnum)
        {
            propertySchema.Type = "string";
            propertySchema.Enum = Enum.GetNames(propertyType).ToList();
        }
        else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
        {
            propertySchema.Type = "array";
            var itemType = propertyType.GetGenericArguments()[0];
            propertySchema.Items = new Items { Type = GetJsonType(itemType) };
        }
        else
        {
            propertySchema.Type = "object";
        }

        return propertySchema;
    }

    private static string GetJsonType(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte)) return "integer";
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return "number";
        if (type == typeof(bool)) return "boolean";
        if (type.IsEnum) return "string";
        return "object";
    }
    public static bool IsValidJsonForType<T>(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = JsonSerializer.Deserialize<T>(json, options);
            return result != null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static bool ValidateJsonAgainstType<T>(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = JsonSerializer.Deserialize<T>(json, options);
        
            if (result == null) return false;

            // Get all properties that aren't nullable
            var properties = typeof(T).GetProperties()
                .Where(p => !IsNullable(p.PropertyType));
        
            foreach (var prop in properties)
            {
                if (prop.GetValue(result) == null)
                {
                    return false;
                }
            }
        
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsNullable(Type type)
    {
        // Nullable value type
        if (Nullable.GetUnderlyingType(type) != null) return true;
    
        // Reference type
        if (!type.IsValueType) return true;
    
        return false;
    }
    
    // Supporting classes for JSON schema structure
    public class JsonSchema
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Parameters Parameters { get; set; }
    }

    public class Parameters
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    
        public Dictionary<string, Property> Properties { get; set; }
    
        public List<string> Required { get; set; }
    }

    public class Property
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    
        public string Description { get; set; }
    
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Enum { get; set; }
    
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Items Items { get; set; }
    }

    public class Items
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

}
