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

	public class DoubleToPercentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((double)value).ToString("P0");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return double.Parse(value.ToString());
		}
	}

	public class BrushAlphaConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var brush = (SolidColorBrush)values[0];
			double alpha = (double)values[1];
			return new SolidColorBrush(Color.FromArgb((byte)(alpha * 255.0), brush.Color.R, brush.Color.G, brush.Color.B));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	[ValueConversion(typeof(SolidColorBrush), typeof(SolidColorBrush))]
	public class BrushSaturationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			float factor = (float)(double)parameter;

			var c = ((SolidColorBrush)value).Color;
			byte a = c.A;
			float r = (float)c.R / 255f, g = (float)c.G / 255f, b = (float)c.B / 255f;
			float h = 0f, s = 0f, v = Math.Max(Math.Max(r, g), b);
			float delta = v - Math.Min(Math.Min(r, g), b);

			if (v > 0)
			{
				s = delta / v;
			}

			if (r == v)
			{
				h = (g - b) / delta;
			}
			else if (g == v)
			{
				h = 2f + (b - r) / delta;
			}
			else
			{
				h = 4f + (r - g) / delta;
			}

			h *= 60f;
			if (h < 0f)
			{
				h += 360f;
			}

			s *= factor;

			if (s == 0)
			{
				r = g = b = v;
			}
			else
			{
				h /= 60;
				int i = (int)h;
				float f = h - (float)i;
				float p = v * (1 - s);
				float q = v * (1 - s * f);
				float t = v * (1 - s * (1 - f));

				switch(i)
				{
					case 0:
						r = v; g = t; b = p;
						break;
					case 1:
						r = q; g = v; b = p;
						break;
					case 2:
						r = p; g = v; b = t;
						break;
					case 3:
						r = p; g = q; b = v;
						break;
					case 4:
						r = t; g = p; b = v;
						break;
					default:
						r = v; g = p; b = q;
						break;
				}
			}

			return new SolidColorBrush(Color.FromArgb(
				a, (byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f)));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
