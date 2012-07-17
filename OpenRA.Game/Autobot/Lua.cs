using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using OpenRA.Traits;

namespace OpenRA.Autobot
{
	public class Lua : IDisposable
	{
		#region Constants
		public const string LUA_DLL = "/Users/aaron/liblua.dylib";

		enum LuaType {
			LUA_TNONE = -1,
			LUA_TNIL = 0,
			LUA_TBOOLEAN,
			LUA_TLIGHTUSERDATA,
			LUA_TNUMBER,
			LUA_TSTRING,
			LUA_TTABLE,
			LUA_TFUNCTION,
			LUA_TUSERDATA,
			LUA_TTHREAD,
			LUA_TNUMTAGS
		}
		#endregion

		#region Lua Interface
		delegate IntPtr lua_Alloc(IntPtr ud, IntPtr ptr, int osize, int nsize);
		delegate int lua_CFunction(IntPtr l);

		[DllImport(LUA_DLL, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr lua_newstate(lua_Alloc f, IntPtr ud); 

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_close(IntPtr L);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern int lua_type(IntPtr l, int index);

		static bool lua_istable(IntPtr l, int index)
		{
			return (LuaType)lua_type(l, index) == LuaType.LUA_TTABLE;
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern int luaL_loadstring(IntPtr l, string s);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern int lua_pcallk(IntPtr l, int num_args, int num_results, int msgh, int ctx, IntPtr k);

		static int lua_pcall (IntPtr l, int num_args, int num_results, int msgh)
		{
			return lua_pcallk(l, num_args, num_results, msgh, 0, IntPtr.Zero);
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl, EntryPoint="lua_tolstring")]
		static extern IntPtr lua_tolstring_(IntPtr l, int index, IntPtr len);

		static string lua_tolstring (IntPtr l, int index, IntPtr len)
		{
			IntPtr ptr = lua_tolstring_(l, index, len);
			return Marshal.PtrToStringAuto(ptr);
		}

		static string lua_tostring (IntPtr l, int index)
		{
			return lua_tolstring(l, index, IntPtr.Zero);
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_pushcclosure(IntPtr l, lua_CFunction func, int n);

		static void lua_pushcfunction(IntPtr l, lua_CFunction func)
		{
			lua_pushcclosure(l, func, 0);
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_setglobal(IntPtr l, string name);

		static void lua_register(IntPtr l,string name, lua_CFunction func)
		{
			lua_pushcfunction(l, func);
			lua_setglobal(l, name);
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern int lua_gettop(IntPtr l);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern bool lua_isnumber(IntPtr l, int index);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern int lua_tointegerx(IntPtr l, int index, IntPtr isnum);

		static int lua_tointeger(IntPtr l, int index)
		{
			return lua_tointegerx(l, index, IntPtr.Zero);
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_getglobal(IntPtr l, string name);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_callk(IntPtr l, int nargs, int nresults, int ctx, lua_CFunction k);

		static void lua_call (IntPtr l, int nargs, int nresults)
		{
			lua_callk(l, nargs, nresults, 0, null);
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_pushinteger(IntPtr l, int i);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr lua_pushstring(IntPtr l, string s);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_pushnil(IntPtr l);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_createtable(IntPtr l, int narr, int nrec);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_getfield(IntPtr l, int index, string key);

		static void lua_newtable (IntPtr l)
		{
			lua_createtable(l, 0, 0);
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_settop(IntPtr l, int i);

		static void lua_pop(IntPtr l, int num)
		{
			lua_settop(l, -num - 1);
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr lua_atpanic(IntPtr l, lua_CFunction func);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_setfield(IntPtr l, int index, string key);

		#endregion

		private IntPtr lua = IntPtr.Zero;
	
		public Lua()
		{
			lua = lua_newstate( delegate(IntPtr u, IntPtr p, int o, int n)
			{
				if( n == 0 ) {
					Marshal.FreeHGlobal( p );
					return IntPtr.Zero;
				} 

				if( p == IntPtr.Zero) {
					p = Marshal.AllocHGlobal( n );
				} else {
					p = Marshal.ReAllocHGlobal( p, new IntPtr( n ) );
				}
			
				return p;
			} , IntPtr.Zero);

			if(lua == IntPtr.Zero)
			{
				throw new Exception("Failed to create lua state");
			}

			lua_atpanic(lua, delegate(IntPtr l) {
				throw new Exception("Fatal error in lua library");
			});

			RegisterAPI();
		}

		public void RunScript (string filename)
		{
			FileInfo fi = new FileInfo (filename);
			if (!fi.Exists) {
				throw new FileNotFoundException ("Couldn't open file", filename);
			}

			using (TextReader reader = fi.OpenText()) {
				string s = reader.ReadToEnd();
				int ret = luaL_loadstring( lua, s );
				if( ret != 0 ) {
					ThrowError();
				}
			}

			int result = lua_pcall( lua, 0, 0, 0 );
			if( result != 0 ) {
				ThrowError();
			}
		}

		public void CallFunc(string function, params object[] args)
		{
			// Push function name
			lua_getglobal (lua, function);

			// Push args
			foreach (var arg in args) {
				if (arg is string) {
					lua_pushstring (lua, arg as String);
				} else if (arg is Int32) {
					lua_pushinteger (lua, (int)arg);
				} else if (arg is Actor) {
					PushActor(lua, arg as Actor);
				} else {
					throw new NotImplementedException ("Unsupported argument type: " + arg.GetType ().ToString ());
				}
			}

			if (lua_pcall (lua, args.Length, 0, 0) != 0) {
				ThrowError();
			}
		}

		private void ThrowError ()
		{
			throw new Exception("Error: " + lua_tostring(lua,-1));
		}

		private void RegisterAPI ()
		{
			var world = Game.orderManager.world;

			#region void log(...)
			lua_register(lua, "log", delegate(IntPtr l) {
				int args = lua_gettop(l);

				StringBuilder sb = new StringBuilder();
				for(int i = 1; i <= args; ++i) {
					if(lua_isnumber(l, i)) {
						sb.Append(lua_tointeger(l, i));
					} else {
						sb.Append(lua_tostring(l, i));
					}
				}

				sb.AppendLine();

				Bot.Log(sb.ToString());
				return 0;
			});
			#endregion

			#region array FindUnitByName(name)
			lua_register(lua, "FindUnitByName", delegate(IntPtr l) {
				int args = lua_gettop(l);

				if(args != 1) {
					Bot.Log("FindUnitByName: incorrect number of parameters");
					return 0;
				}

				string name = lua_tostring(l, 1);

				// Find the first unit of the given type & return it
				foreach(var actor in world.ActorsWithTrait<Selectable>()) {
					Actor a = actor.Actor;

					if(a.Owner != world.LocalPlayer) {
						// Not our unit
						continue;
					}

					if(a.Info.Name.Equals( name, StringComparison.OrdinalIgnoreCase)) {
						PushActor(l, a);
						return 1;
					}
				}

				lua_pushnil(l);
				return 1;
			});
			#endregion

			#region void DeployUnit(unit)
			lua_register(lua, "DeployUnit", delegate(IntPtr l) {
				int args = lua_gettop(l);
				if(args != 1 ) {
					Bot.Log("invalid parameter count");
					return 0;
				}

				if(!lua_istable(l, 1)) {
					Bot.Log("invalid argument type");
					return 0;
				}

				lua_getfield(l, 1, "id");
				uint i = (uint)lua_tointeger(l, -1);
				lua_pop(l, 1);

				Bot.Log("Deploying unit " + i);
				world.IssueOrder(new Order("DeployTransform", world.GetActorById(i), false));

				return 0;
			});
			#endregion
		}

		/// <summary>
		/// Pushes an actor into a table on the stack.
		/// </summary>
		private static void PushActor (IntPtr lua, Actor a)
		{
			lua_newtable(lua);
			lua_pushstring(lua, a.Info.Name);
			lua_setfield(lua, -2, "name");
			lua_pushinteger(lua, (int)a.ActorID);
			lua_setfield(lua, -2, "id");
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			lua_close(lua);
		}
		#endregion
	}
}

