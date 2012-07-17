using System;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace OpenRA.Autobot
{
	public static class Bot
	{
		static Color CONSOLE_COLOR = Color.Firebrick;
		static int THINK_TIME = 3000;

		private static Stopwatch sw;

		static Bot ()
		{
			sw = new Stopwatch();
			sw.Start();
		}

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

		public static void Tick ()
		{
			if(lua != null) {
				Game.RunAfterTick( () => { lua.CallFunc("OnThink"); });
			}

			Game.RunAfterDelay(THINK_TIME, Bot.Tick);
		}

		static Lua lua = null;
		private static void RunAutobot ()
		{
			lua = new Lua();
			lua.RunScript("autobot/autobot.lua");
			lua.CallFunc("OnInit");

			Game.RunAfterDelay(THINK_TIME, Bot.Tick);
		}


		public static void UnitDeployed (Actor a)
		{
			if (lua != null) {
				Game.RunAfterTick( () => { lua.CallFunc("OnUnitDeployed", a); });
			}
		}
	}
}

