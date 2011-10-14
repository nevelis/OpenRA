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
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.RA.Buildings;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets
{
	public class PowerBinWidget : Widget
	{
		float? lastPowerProvidedPos;
		float? lastPowerDrainedPos;
		string powerCollection;

		readonly PowerManager power;
		readonly World world;

		[ObjectCreator.UseCtor]
		public PowerBinWidget(World world)
		{
			this.world = world;

			if (world.LocalPlayer != null)
				power = world.LocalPlayer.PlayerActor.Trait<PowerManager>();
		}

		static Color GetPowerColor(PowerManager pm)
		{
			if (pm.PowerState == PowerState.Critical) return Color.Red;
			if (pm.PowerState == PowerState.Low) return Color.Orange;
			return Color.LimeGreen;
		}

		const float PowerBarLerpFactor = .2f;

		public override void Draw()
		{
			if( world.LocalPlayer == null ) return;

			powerCollection = "power-" + world.LocalPlayer.Country.Race;

			// Nothing to draw
			if (power.PowerProvided == 0 && power.PowerDrained == 0)
				return;

			var rb = RenderBounds;

			// Draw bar horizontally
			var barStart = rb.Left;
			var barEnd = rb.Right;

			float powerScaleBy = 100;
			var maxPower = Math.Max(power.PowerProvided, power.PowerDrained);

			while (maxPower >= powerScaleBy) powerScaleBy *= 2;

			// Current power supply
			var powerLevelTemp = barStart + (barEnd - barStart) * (power.PowerProvided / powerScaleBy);
			lastPowerProvidedPos = float2.Lerp(lastPowerProvidedPos.GetValueOrDefault(powerLevelTemp), powerLevelTemp, PowerBarLerpFactor);
			var powerLevel = new float2(lastPowerProvidedPos.Value, rb.Top);

			var color = GetPowerColor(power);

			var barHeight = 4;

			var colorDark = Graphics.Util.Lerp(0.25f, color, Color.Black);
			var ro = RenderOrigin;

			for (int i = 0; i < barHeight; i++)
			{
				var actualColor = (i - 1 < barHeight / 2) ? color : colorDark;
				var leftOffset = new float2(0, i);
				var rightOffset = new float2(0, i);

				// Indent corners
				if ((i == 0 || i == barHeight - 1) && powerLevel.X - barStart > 1)
				{
					leftOffset.X += 1;
					rightOffset.X -= 1;
				}

				Game.Renderer.LineRenderer.DrawLine(ro + leftOffset, powerLevel + rightOffset, actualColor, actualColor);
			}

			// Power usage indicator
			var indicator = ChromeProvider.GetImage( powerCollection, "power-indicator" );
			var powerDrainedTemp = barStart + (barEnd - barStart) * (power.PowerDrained / powerScaleBy);
			lastPowerDrainedPos = float2.Lerp(lastPowerDrainedPos.GetValueOrDefault(powerDrainedTemp), powerDrainedTemp, PowerBarLerpFactor);
			var powerDrainLevel = new float2(lastPowerDrainedPos.Value - indicator.size.X / 2, rb.Top - 1);

			Game.Renderer.RgbaSpriteRenderer.DrawSprite(indicator, powerDrainLevel);
		}
	}
}
