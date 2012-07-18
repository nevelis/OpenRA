using System;

namespace OpenRA.Autobot
{
	public class LuaFunctionAttribute : Attribute
	{
		public string Name { get; set; }

		public LuaFunctionAttribute()
		{
		}
	}
}

