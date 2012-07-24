using System;
using System.Text;
using OpenRA.Traits;

namespace OpenRA.Autobot
{
	public partial class Bot
	{
		[LuaFunction(Name="log")]
		public static int Lua_Log(Lua.LuaFunctionParams fun)
		{
			int args = fun.Arguments;

			StringBuilder sb = new StringBuilder();
			for(int i = 1; i <= args; ++i) {
				if(fun.IsNumber(i)) {
					sb.Append(fun.ToInteger(i));
				} else if(fun.IsBoolean(i)) {
					sb.Append (fun.ToBoolean(i).ToString());
				} else {
					sb.Append(fun.ToString(i));
				}
			}

			sb.AppendLine();

			Bot.Log(sb.ToString());
			return 0;
		}

		[LuaFunction(Name="FindUnitByName")]
		public static int Lua_FindUnitByName(Lua.LuaFunctionParams fun)
		{
			var world = Game.orderManager.world;
			int args = fun.Arguments;

			if(args != 1) {
				Bot.Log("FindUnitByName: incorrect number of parameters");
				return 0;
			}

			string name = fun.ToString(1);

			// Find the first unit of the given type & return it
			foreach(var actor in world.ActorsWithTrait<Selectable>()) {
				Actor a = actor.Actor;

				if(a.Owner != world.LocalPlayer) {
					// Not our unit
					continue;
				}

				if(a.Info.Name.Equals( name, StringComparison.OrdinalIgnoreCase)) {
					fun.PushActor(a);
					return 1;
				}
			}

			fun.PushNil();
			return 1;
		}

		[LuaFunction(Name="DeployUnit")]
		public static int Lua_DeployUnit(Lua.LuaFunctionParams fun)
		{
			int args = fun.Arguments;
			if(args != 1 ) {
				Bot.Log("invalid parameter count");
				return 0;
			}

			if(!fun.IsTable(1)) {
				Bot.Log("invalid argument type");
				return 0;
			}

			// lua_getfield(l, 1, "id");
			fun.GetField(1, "id");
			// uint i = (uint)lua_tointeger(l, -1);
			uint i = (uint)fun.ToInteger(-1);
			fun.Pop(1);

			Bot.Log("Deploying unit " + i);

			var world = Game.orderManager.world;
			world.IssueOrder(new Order("DeployTransform", world.GetActorById(i), false));

			return 0;
		}

		[LuaFunction(Name="Team")]
		public static int Lua_Team(Lua.LuaFunctionParams fun)
		{
			fun.PushString(Game.orderManager.world.LocalPlayer.Country.Name.ToLower());
			return 1;
		}
	}
}

