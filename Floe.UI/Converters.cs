using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Floe.UI
{
	[ValueConversion(typeof(SolidColorBrush), typeof(Color))]
	public class BrushToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var scb = value as SolidColorBrush;
			if (scb != null && targetType == typeof(Color))
			{
				var c = scb.Color;
				c.A = (byte)((double)scb.Color.A * scb.Opacity);
				return c;
			}
			else
			{
				throw new ArgumentException();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	[ValueConversion(typeof(double), typeof(bool))]
	public class CanScrollLeftConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is double && (double)value > 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class CanScrollRightConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values == null || values.Length < 3 || values.Any((v) => v == null || v.Equals(double.NaN)))
			{
				return false;
			}

			double offset = (double)values[0], viewport = (double)values[1], extent = (double)values[2];
			return Math.Round(offset + viewport, 2) < Math.Round(extent, 2);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
