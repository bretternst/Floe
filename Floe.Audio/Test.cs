using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Floe.Audio.Interop;

namespace Floe.Audio
{
	public static class Test
	{
		public static void Run()
		{
			var fg = FilterGraph.Create();
			var mc = (IMediaControl)fg;
			var me = (IMediaEvent)fg;
			fg.RenderFile("c:\\test.mp3", IntPtr.Zero);

			IEnumFilters ief;
			var filters = new IBaseFilter[8];
			fg.EnumFilters(out ief);
			int fetched;
			ief.Next(8, filters, out fetched);

			for (int i = 0; i < fetched; i++)
			{
				var ibf = filters[i];
				FilterInfo fi;
				ibf.QueryFilterInfo(out fi);
				string vendorInfo = "";
				try
				{
					ibf.QueryVendorInfo(out vendorInfo);
				}
				catch (Exception)
				{
				}
				Console.WriteLine(fi.Name + " " + vendorInfo);
			}

			Console.ReadLine();
		}
	}
}
