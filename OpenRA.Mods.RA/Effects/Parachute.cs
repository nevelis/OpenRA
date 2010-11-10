#region Copyright & License Information
/*
 * Copyright 2007-2010 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see LICENSE.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Effects;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.RA.Effects
{
	public class Parachute : IEffect
	{
		readonly Animation anim;
		readonly Animation paraAnim;
		readonly float2 location;
		
		readonly Actor cargo;
		readonly Player owner;

		float altitude;
		const float fallRate = .3f;

		public Parachute(Player owner, string image, float2 location, int altitude, Actor cargo)
		{
			this.location = location;
			this.altitude = altitude;
			this.cargo = cargo;
			this.owner = owner;

			anim = new Animation(image);
			if (anim.HasSequence("idle"))
				anim.PlayFetchIndex("idle", () => 0);
			else
				anim.PlayFetchIndex("stand", () => 0);
			anim.Tick();

			paraAnim = new Animation("parach");
			paraAnim.PlayThen("open", () => paraAnim.PlayRepeating("idle"));
		}

		public void Tick(World world)
		{ 
			paraAnim.Tick();

			altitude -= fallRate;

			if (altitude <= 0)
				world.AddFrameEndTask(w =>
					{
						w.Remove(this);
						var loc = Traits.Util.CellContaining(location);
						cargo.CancelActivity();
						cargo.Trait<ITeleportable>().SetPosition(cargo, loc);
						w.Add(cargo);
					});
		}

		public IEnumerable<Renderable> Render()
		{
			var pos = location - new float2(0, altitude);
			yield return Renderable.Centered(anim.Image, location, PaletteRef.Shadow, null, 0);
			yield return Renderable.Centered(anim.Image, pos, null, owner.Index, 2);
			yield return Renderable.Centered(paraAnim.Image, pos, null, owner.Index, 3);
		}
	}
}
