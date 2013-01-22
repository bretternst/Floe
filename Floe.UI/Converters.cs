using System;
using System.Globalization;
using System.Text;
using System.Windows;
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

	public class SecondsToFriendlyTimeConverter : IValueConverter
	{
		private string _format = "{0}";

		public string Format { get { return _format; } set { _format = value; } }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var seconds = System.Convert.ToInt32(value);
			if (seconds > 0)
			{
				var sb = new StringBuilder();
				var span = TimeSpan.FromSeconds(System.Convert.ToInt32(value));
				if ((int)span.TotalDays > 0)
				{
					sb.Append(((int)(span.TotalDays)).ToString()).Append(" day").Append(span.TotalDays > 1 ? "s" : "");
				}
				if (span.Hours > 0)
				{
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}
					sb.Append(span.Hours.ToString()).Append(" hour").Append(span.Hours > 1 ? "s" : "");
				}
				if (span.Minutes > 0)
				{
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}
					sb.Append(span.Minutes.ToString()).Append(" minute").Append(span.Minutes > 1 ? "s" : "");
				}

				if ((int)span.TotalDays == 0 && span.Hours == 0)
				{
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}
					sb.Append(span.Seconds.ToString()).Append(" second").Append(span.Seconds > 1 ? "s" : "");
				}
				return string.Format(_format, sb.ToString());
			}
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class BytesToFriendlyStringConverter : IValueConverter
	{
		private static readonly string[] _suffixes = new[] { "B", "KB", "MB", "GB", "TB" };
		private static readonly string[] _formats = new[] { "F0", "F0", "F1", "F2", "F2" };

		private string _format = "{0} {1}";

		public string Format { get { return _format; } set { _format = value; } }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var bytes = System.Convert.ToDouble(value);
			int i = 0;
			while (bytes > 1024 && i < _suffixes.Length - 1)
			{
				bytes /= 1024.0;
				i++;
			}
			return string.Format(_format, bytes.ToString(_formats[i]), _suffixes[i]);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
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
			var hsv = HsvColor.FromColor(((SolidColorBrush)value).Color);
			hsv = new HsvColor(hsv.A, hsv.H, hsv.S * (float)(double)parameter, hsv.V);
			return new SolidColorBrush(hsv.ToColor());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class MultiplyConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			return new System.Windows.GridLength(System.Convert.ToDouble(values[0]) * System.Convert.ToDouble(values[1]));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class VisibilityConverter : IValueConverter
	{
		public Visibility TrueValue { get; set; }
		public Visibility FalseValue { get; set; }

		public VisibilityConverter()
		{
			this.TrueValue = Visibility.Visible;
			this.FalseValue = Visibility.Collapsed;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? this.TrueValue : this.FalseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Visibility)value == this.TrueValue;
		}
	}
}
