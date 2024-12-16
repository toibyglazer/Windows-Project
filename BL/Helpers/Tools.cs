using System.Linq;
using System.Reflection;

namespace Helpers;

internal static class Tools
{
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null)
            return "The provided object is null and has no properties.";

        Type type = t.GetType();
        PropertyInfo[] properties= type.GetProperties();

        List<string> propertyStrings = new List<string>();
        foreach (PropertyInfo property in properties)
        {
            object value = property.GetValue(t);

            if (value == null)
            {
                propertyStrings.Add($"{property.Name}: null");
            }
            else if (value is IEnumerable<object> enumerable)
            {
                propertyStrings.Add($"{property.Name}: [{string.Join(", ", enumerable)}]");
            }
            else
            {
                propertyStrings.Add($"{property.Name}: {value}");
            }
           
        }
        return $"{type.Name} Properties:\n {string.Join(", ", propertyStrings)}";
    }


}
