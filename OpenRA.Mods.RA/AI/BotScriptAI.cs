using System;
using OpenRA.Traits;

namespace OpenRA.Mods.RA.AI
{
	class BotScriptAIInfo : IBotInfo, ITraitInfo
	{
		public string Name { get { return "BotScript AI"; } }

		public object Create(ActorInitializer init)
		{
			return new BotScriptAI(this);
		}
	}

	class BotScriptAI : ITick, IBot, INotifyDamage
	{
		public BotScriptAI(BotScriptAIInfo info)
		{
			Info = info;
		}

		IBotInfo IBot.Info { get { return this.Info; } }

		internal readonly BotScriptAIInfo Info;

		private Player player;

		public void Tick (Actor self)
		{
			if (player != null) {
				Console.WriteLine("Tick: " + this.GetHashCode() );
			}
		}

		public void Activate (Player p)
		{
			Console.WriteLine("BotScriptAI::Activate(): " + this.GetHashCode() );
			player = p;
		}

		public void Damaged (Actor self, AttackInfo e)
		{
			Console.WriteLine("BotScriptAI::Damaged()");
		}
	}
}
