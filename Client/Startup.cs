using RDR2DN;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Console = RDR2DN.Console;
using WinForms = System.Windows.Forms;
/*
namespace RDRNetwork
{
	abstract class Startup
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		static ScriptDomain Domain = null;
		static Console Console = null;

		private static void Init()
		{
			AllocConsole();
			
			if (Domain != null)
				ScriptDomain.Unload(Domain);

			Domain = ScriptDomain.Load();


			if (Console == null)
				

			if (Domain != null)
				Domain.Start();


		}

		private static void Tick()
		{
			if (Domain != null)
				Domain.DoTick();

			if (Console != null)
				Console.DoTick();
		}

		private static void KeyboardMessage(WinForms.Keys key, bool status, bool statusCtrl, bool statusShift, bool statusAlt)
		{
			if (Domain != null)
				Domain.DoKeyEvent(key, status);

			if (Console != null)
				Console.DoKeyEvent(key, status);
		}

		private static void DoD3DCall(IntPtr swapchain)
		{
			RDR2DN.Log.Message(Log.Level.Debug, "DoD3DCall");
		
		}
	}
}
*/