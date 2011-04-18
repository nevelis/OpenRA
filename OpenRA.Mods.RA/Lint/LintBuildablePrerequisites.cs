#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
    class LintBuildablePrerequisites : ILintPass
    {
        public void Run(Action<string> emitError)
        {
			/* do something intelligent here. */
        }
    }
	
	class CheckAutotargetWiring : ILintPass
	{
		public void Run(Action<string> emitError)
        {
            foreach( var i in Rules.Info )
			{
				if (i.Key.StartsWith("^"))
					continue;
				var attackMove = i.Value.Traits.GetOrDefault<AttackMoveInfo>();
				if (attackMove != null && !attackMove.JustMove &&
				    !i.Value.Traits.Contains<AutoTargetInfo>())
					emitError( "{0} has AttackMove setup without AutoTarget, and will crash when resolving that order.".F(i.Key) );
			}
        }
	}
	
	class LintBuildActorSizeHistogram : ILintPass
	{
		public void Run(Action<string> emitError)
		{
			var sizes = new Dictionary<int,int>();
			
			foreach( var i in Rules.Info )
			{
				if (i.Key.StartsWith("^"))
					continue;
			
				var size = i.Value.Traits.WithInterface<object>().Count();
				if (!sizes.ContainsKey(size))
					sizes[size] = 1;
				else
					sizes[size]++;
			}
			
			foreach( var s in sizes.OrderByDescending( kv => kv.Key ) )
			{
				Console.WriteLine ("{0} {1}", s.Key.ToString().PadLeft(4), new string('+',s.Value) );
			}
		}
	}
	
	class LintPopularTraitsHistogram : ILintPass
	{
		public void Run(Action<string> emitError)
		{
			var sizes = new Dictionary<string,int>();
			
			foreach( var i in Rules.Info )
			{
				if (i.Key.StartsWith("^"))
					continue;
			
				foreach( var t in i.Value.Traits.WithInterface<object>().Select( a => a.GetType().Name.Replace("Info","") ) )
					if (!sizes.ContainsKey(t))
						sizes[t] = 1;
					else
						sizes[t]++;
			}
			
			var w = sizes.Keys.Max(k => k.Length);
			var q = sizes.Values.Max();
			var r = sizes.Values.Max(k => k.ToString().Length);
			
			var z = 80 - 4 - w - r;	/* roughly: space left in term */
			
			foreach( var s in sizes.OrderByDescending( kv => kv.Value ) )
				Console.WriteLine ("{0}:{2} {1}", s.Key.ToString().PadLeft(w), new string('+', (s.Value * z + q - 1) / q), s.Value.ToString().PadLeft(r) );
		}
	}
}
