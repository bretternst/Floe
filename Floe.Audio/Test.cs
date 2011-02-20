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
			fg.RenderFile("\\test.wav", IntPtr.Zero);
			IBaseFilter filter;
//			fg.AddSourceFilter("\test.wav", "Source1", out filter);
			IEnumFilters ief;
			var filters = new IBaseFilter[8];
			fg.EnumFilters(out ief);
			int fetched;
			ief.Next(8, filters, out fetched);

			mc.Run();
			int eventCode;
			int ret = me.WaitForCompletion(40000, out eventCode);
			Console.WriteLine(ret + " " + eventCode);
			Console.ReadLine();
		}
	}
}
