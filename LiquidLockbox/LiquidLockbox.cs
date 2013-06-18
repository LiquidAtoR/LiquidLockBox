/*
 * LiquidLockbox v3.0.0.0 by LiquidAtoR
 * 
 * This is a little addon to open lockboxes in the char's inventory.
 * The skill of his lockpicking is compared to the skill needed for the box.
 * There should be no event where it tries to open a lockbox without skillz.
 * I borrowed back from Bengal what he borrowed from me in the first place.
 * Bits of the code he changed I integrated here for ease of use.
 * 
 * 2013/18/06   v3.0.0.1
 *               Streamlining for testing.
 *
 * 2012/10/08   v3.0.0.0
 *				Updated to new HB API.
 *				Added the 2 new MoP lock and junkboxes.
 *
 * 2011/11/23   v2.0.0.0
 *				Credits to Smarter for well, being smarter :D
 *				Also thanks to no1knowsy for thinking along ;)
 *
 * 2011/11/22   v1.0.0.0
 *              First writeup of the plugin
 * 
 */
namespace LiquidLockbox
{
    using Styx;
    using Styx.Common;
    using Styx.Common.Helpers;
	using Styx.CommonBot;
    using Styx.CommonBot.Frames;
    using Styx.CommonBot.Inventory;
    using Styx.CommonBot.Profiles;
    using Styx.Helpers;
    using Styx.Pathing;
    using Styx.Plugins;
    using Styx.WoWInternals;
	using Styx.WoWInternals.Misc;
	using Styx.WoWInternals.World;
    using Styx.WoWInternals.WoWObjects;

    using System;
    using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
	using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Xml.Linq;

    public class LiquidLockbox : HBPlugin
    {
        public override string Name { get { return "LiquidLockbox"; } }
        public override string Author { get { return "LiquidAtoR"; } }
        public override Version Version { get { return new Version(3,0,0,1); } }

		private bool _init;
        public override void Initialize()
        {
            if (_init) return;
            base.Initialize();
            Logging.Write(LogLevel.Normal, Colors.DarkRed, "LiquidLockbox 3.0 ready for use...");
            _init = true;
        }
		
		struct Lockbox
			{
				public Lockbox(uint ID, int Skill)
					{
						this.ID = ID;
						this.Skill = Skill;
					}
				public uint ID;
				public int Skill;
			}
		
        private List<Lockbox> _lockBox = new List<Lockbox>()
			{
			// The following items are locked and in your inventory (looted).
			// These items can be opened by a rogue/bs with the proper skill/keys.
 
			// The boxes you can loot from anything
 
				new Lockbox(4632, 1),    //Ornate Bronze Lockbox (Skill 1)
				new Lockbox(4633, 25),   //Heavy Bronze Lockbox (Skill 25)
				new Lockbox(4634, 70),   //Iron Lockbox (Skill 70)
				new Lockbox(4636, 125),  //Strong Iron Lockbox (Skill 125)
				new Lockbox(4637, 175),  //Steel Lockbox (Skill 175)
				new Lockbox(4638, 225),  //Reinforced Steel Lockbox (Skill 225)
				new Lockbox(5758, 225),  //Mithril Lockbox (Skill 225)
				new Lockbox(5759, 225),  //Thorium Lockbox (Skill 225)
				new Lockbox(5760, 225),  //Eternium Lockbox (Skill 225)
				new Lockbox(31952, 325), //Khorium Lockbox (Skill 325)
				new Lockbox(43622, 375), //Froststeel Lockbox (Skill 375)
				new Lockbox(43624, 400), //Titanium Lockbox (Skill 400)
				new Lockbox(45986, 400), //Tiny Titanium Lockbox (Skill 400)
				new Lockbox(68729, 425), //Elementium Lockbox (Skill 425)
				new Lockbox(88567, 450), //Ghost-Iron Lockbox (Skill 450)
 
			// The boxes you can pickpocket from mobs
 
				new Lockbox(16882, 1),   //Battered Junkbox (Skill 1)
				new Lockbox(16883, 70),  //Worn Junkbox (Skill 70)
				new Lockbox(16884, 175), //Sturdy Junkbox (Skill 175)
				new Lockbox(16885, 250), //Heavy Junkbox (Skill 250)
				new Lockbox(29569, 300), //Strong Junkbox (Skill 300)
				new Lockbox(43575, 350), //Reinforced Junkbox (Skill 350)
				new Lockbox(63349, 400), //Flame-Scarred Junkbox (Skill 400)
				new Lockbox(88165, 450), //Vine-Cracked Junkbox (Skill 450)
			};

        private static Stopwatch sw = new Stopwatch();

        public override void Pulse()
        {
		if (_init)
			{
                if (Battlegrounds.IsInsideBattleground ||
                    StyxWoW.Me.HasAura(1784) ||
                    StyxWoW.Me.HasAura("Drink") ||
                    StyxWoW.Me.HasAura("Food") ||
					StyxWoW.Me.IsActuallyInCombat ||
                    StyxWoW.Me.IsCasting ||
					StyxWoW.Me.IsDead ||
					StyxWoW.Me.IsGhost ||
                    StyxWoW.Me.IsMoving ||
                    StyxWoW.Me.Mounted) {
                return;
            }

            if (!sw.IsRunning)
                sw.Start();

            if (sw.Elapsed.TotalSeconds > 1)
            {
				var lockpickSkill = StyxWoW.Me.Level*5;
                foreach (WoWItem item in ObjectManager.GetObjectsOfType<WoWItem>()) 
                {
					foreach (Lockbox l in _lockBox)
					{
						if (l.ID == item.Entry)
						{
							if (item != null)
							{
								if (item.BagSlot != -1)
								{
									if (StyxWoW.Me.FreeNormalBagSlots >= 2)
									{
										if ((lockpickSkill > l.Skill) && (item.StackCount >= 1))
										{
											SpellManager.Cast(1804);
											Lua.DoString("UseItemByName(\"" + item.Name + "\")");
											StyxWoW.SleepForLagDuration();
												while (StyxWoW.Me.IsCasting)
												Thread.Sleep(50);
											Logging.Write(LogLevel.Normal, Colors.DarkRed, "[LiquidLockbox]: Unlocking and opening a {0}.", item.Name);
											Lua.DoString("UseItemByName(\"" + item.Name + "\")");
											StyxWoW.SleepForLagDuration();
											Lua.DoString("RunMacroText(\"/click StaticPopup1Button1\");");
											StyxWoW.SleepForLagDuration();
										}
									}
									else
									{
										Logging.Write(LogLevel.Normal, Colors.DarkRed, "[LiquidLockbox]: No Lockpicking because our free bag slots are less then 3.");
									}
								}
							}
						}
					}	
                    sw.Reset();
                    sw.Start();
					}
                }
            }
        }
    }
}