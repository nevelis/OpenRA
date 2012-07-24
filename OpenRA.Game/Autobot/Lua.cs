using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using OpenRA.Traits;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

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

		static bool lua_isboolean(IntPtr l, int index)
		{
			return (LuaType)lua_type (l, index) == LuaType.LUA_TBOOLEAN;
		}

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern int lua_toboolean(IntPtr l, int index);

		[DllImport(LUA_DLL, CallingConvention=CallingConvention.Cdecl)]
		static extern void lua_pushboolean(IntPtr l, int i);

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

			LoadFunctionsFromAssembly(Assembly.GetExecutingAssembly());

			// Hack - do this when mods load
			LoadFunctionsFromAssembly(Assembly.Load("OpenRA.Mods.RA"));
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
					push_actor(lua, arg as Actor);
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


		// For some reason 'm' goes missing but 'fat.Name' stays in scope, so we save the MethodInfo things in here
		static Dictionary<string, MethodInfo> function_dict = new Dictionary<string, MethodInfo>();

		private void LoadFunctionsFromAssembly (Assembly ass)
		{
			var types = ass.GetTypes();
			var methods = types.SelectMany(t => t.GetMethods())
				.Where(m => m.GetCustomAttributes(false).OfType<LuaFunctionAttribute>().Count() > 0)
				.ToArray();
				
			foreach (var m in methods) {
				LuaFunctionAttribute fat = m.GetCustomAttributes(false).OfType<LuaFunctionAttribute>().FirstOrDefault();
				function_dict[fat.Name] = m;
				lua_register(lua, fat.Name, delegate(IntPtr l) {
					var func = function_dict[fat.Name];
					var ret = func.Invoke(null, new object[] { new LuaFunctionParams(lua) });
					return (int)ret;
				});
			}

			Bot.Log("Loaded " + methods.Length + " functions from assembly " + ass.GetName());
			Bot.Log(string.Join (", ", methods.Select( m => m.Name ).ToArray()));
		}

		private static void push_actor (IntPtr lua, Actor a)
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

		public class LuaFunctionParams
		{
			IntPtr lua;

			public LuaFunctionParams(IntPtr l)
			{
				lua = l;
			}

			public int Arguments { get { return lua_gettop(lua); } }

			public bool IsNumber (int index)
			{
				return lua_isnumber (lua, index);
			}

			public bool IsTable(int index)
			{
				return lua_istable (lua, index);
			}

			public bool IsBoolean (int index)
			{
				return lua_isboolean(lua, index);
			}

			public bool ToBoolean (int index)
			{
				return lua_toboolean(lua, index) == 1;
			}

			public int ToInteger (int index)
			{
				return lua_tointeger (lua, index);
			}

			public string ToString(int index)
			{
				return lua_tostring(lua, index);
			}

			public void PushActor(Actor a)
			{
				push_actor(lua, a);
			}

			public void PushNil()
			{
				lua_pushnil(lua);
			}

			public void PushString(string s)
			{
				lua_pushstring(lua, s);
			}

			public void PushInt (int i)
			{
				lua_pushinteger(lua, i);
			}

			public void PushBoolean (bool b)
			{
				lua_pushboolean(lua, b ? 1 : 0);
			}

			public void Pop(int amount)
			{
				lua_pop (lua, amount);
			}

			public void GetField(int index, string key)
			{
				lua_getfield(lua, index, key);
			}
		}
	}
}

