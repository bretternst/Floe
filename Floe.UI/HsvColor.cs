using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Floe.UI
{
	public struct HsvColor
	{
		public readonly float A;
		public readonly float H;
		public readonly float S;
		public readonly float V;

		public HsvColor(float a, float h, float s, float v)
		{
			this.A = a;
			this.H = h;
			this.S = s;
			this.V = v;
		}

		public static HsvColor FromColor(Color c)
		{
			float a = (float)c.A / 255f;
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

			return new HsvColor(a, h, s, v);
		}

		public Color ToColor()
		{
			float r, g, b;

			if (S == 0)
			{
				r = g = b = V;
			}
			else
			{
				float h = H / 60f;
				int i = (int)h;
				float f = h - (float)i;
				float p = V * (1 - S);
				float q = V * (1 - S * f);
				float t = V * (1 - S * (1 - f));

				switch (i)
				{
					case 0:
						r = V; g = t; b = p;
						break;
					case 1:
						r = q; g = V; b = p;
						break;
					case 2:
						r = p; g = V; b = t;
						break;
					case 3:
						r = p; g = q; b = V;
						break;
					case 4:
						r = t; g = p; b = V;
						break;
					default:
						r = V; g = p; b = q;
						break;
				}
			}
			return Color.FromArgb((byte)(A * 255f), (byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f));
		}
	}
}
