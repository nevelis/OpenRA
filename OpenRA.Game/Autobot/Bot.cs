using System;
using System.Drawing;
using System.IO;

namespace OpenRA.Autobot
{
	public static class Bot
	{
		static Bot ()
		{
		}

		public static Color CONSOLE_COLOR = Color.Azure;

		public static void Log (string s)
		{
			Game.AddChatLine(CONSOLE_COLOR, "autobot", s);
		}

		public static void ParseCommand (string cmd)
		{
			if (cmd == "/help") {
				Log ("/help    This help text");
				Log ("/run     Starts the autobot");
			} else if (cmd == "/run") {
				Log ("Starting...");

				RunAutobot();
			} else {
				Log("Unknown command: " + cmd);
			}
		}

		static Lua lua = null;
		private static void RunAutobot ()
		{
			lua = new Lua();
			lua.RunScript("autobot/autobot.lua");
			lua.CallFunc("OnInit");
		}
	}
}

