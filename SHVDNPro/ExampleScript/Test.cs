using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExampleScript
{
	public class Test : Script
	{
		internal static StreamWriter m_log;

		public override void OnInit()
		{
			m_log = new StreamWriter("ExampleScript.log", true);
			m_log.AutoFlush = true;

			DateTime now = DateTime.Now;
			for(int a = 0; a < 100000; a++)
			{
				Function.Call(Hash.DRAW_RECT, 0.1f, 0.2f, 0.1f, 0.1f, 255, 0, 0, 255);
				Function.Call(Hash.PLAYER_ID);
			}

			m_log.WriteLine("Native perfs {0}", (DateTime.Now - now).TotalMilliseconds);
		}

		public override void OnTick()
		{
			
		}
	}
}
