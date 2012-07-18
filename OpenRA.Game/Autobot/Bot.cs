using System;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Linq;

namespace OpenRA.Autobot
{
	public static partial class Bot
	{
		static Color CONSOLE_COLOR = Color.Firebrick;
		static int THINK_TIME = 1000;

		static Lua lua = null;

		static Bot ()
		{
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

		private static void Run (Action a)
		{
			try {
				if(lua != null) {
					a();
				}
			} catch (Exception ex) {
				Log("Exception caught:" + ex.Message);
				foreach(var line in ex.StackTrace.Split(new char[] {'\n'}).Take(4)) {
					Log(line);
				}
				lua.Dispose();
				lua = null;
			}
		}

		public static void Tick ()
		{
			Game.RunAfterTick (() => { 
				Run(delegate() {
					lua.CallFunc ("OnThink");

					if(lua != null) {
						Game.RunAfterDelay (THINK_TIME, Bot.Tick);
					}
				});
			});
		}

		private static void RunAutobot ()
		{
			Game.RunAfterTick( delegate() {
				lua = new Lua();

				Run(delegate() {
					lua.RunScript("autobot/autobot.lua");
					lua.CallFunc("OnInit");
					Game.RunAfterDelay(THINK_TIME, Bot.Tick);
				});
			});
		}


		public static void UnitDeployed (Actor a)
		{
			Run(delegate() {
				Game.RunAfterTick( () => { lua.CallFunc("OnUnitDeployed", a); });
			});
		}
	}
}

