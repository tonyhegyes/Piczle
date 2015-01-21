using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Globalization;


namespace puzzle.resources
{
    public static class ThemeProperties
    {
        public static readonly DependencyProperty RightTextProperty =
            DependencyProperty.RegisterAttached("RightText", typeof(string), typeof(ThemeProperties), new FrameworkPropertyMetadata("ON", FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty LeftTextProperty =
           DependencyProperty.RegisterAttached("LeftText", typeof(string), typeof(ThemeProperties), new FrameworkPropertyMetadata("OFF", FrameworkPropertyMetadataOptions.Inherits));

        public static string GetRightText(DependencyObject obj)
        {
            return (string)obj.GetValue(RightTextProperty);
        }
        public static string GetLeftText(DependencyObject obj)
        {
            return (string)obj.GetValue(RightTextProperty);
        }
      
        public static void SetRightText(DependencyObject obj, string text)
        {
            obj.SetValue(RightTextProperty, text);
        }
        public static void SetLeftText(DependencyObject obj, string text)
        {
            obj.SetValue(LeftTextProperty, text);
        }
    }

    public class ToggleElementsSizeAdapter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double x = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            
            if (Math.Abs(x) > 1)
                return ((double)value + x);

            return ((double)value * x);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value / (double)parameter);
        }
    }
}
