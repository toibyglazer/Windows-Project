using System;
using System.Globalization;
using System.Windows.Data;

namespace PL.Volunteer;

public class AddUpdateToReadOnlyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && value.ToString() == "Update")
        {
            return true; // Disable editing (readonly)
        }

        return false; // Allow editing in "Add" mode
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value; // לא נדרש להמיר חזרה
    }
}