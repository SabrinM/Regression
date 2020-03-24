using StardewValley;
using System.Collections.Generic;

namespace Regression
{
    internal static class TimeMagic
    {
        private static void MoveTimeForward()
        {
            Game1.playSound("parry");
            Game1.performTenMinuteClockUpdate();
        }

        private static void SlowDown()
        {
            foreach (GameLocation location in (List<GameLocation>)Game1.locations)
            {
                foreach (NPC character in location.characters)
                {
                    if (character.isVillager())
                        character.addedSpeed = 0;
                }
            }
        }

        //Broken
        public static void DoMagic()
        {
            Game1.player.forceTimePass = true;
            Game1.playSound("stardrop");
            foreach (GameLocation location in (List<GameLocation>)Game1.locations)
            {
                foreach (NPC character in location.characters)
                {
                    if (character.isVillager())
                        character.addedSpeed = 10;
                }
            }
            for (int index = 0; index < 12; ++index)
                ((List<DelayedAction>)Game1.delayedActions).Add(new DelayedAction((index + 1) * 1000 / 2)
                {
                    behavior = new DelayedAction.delayedBehavior(TimeMagic.MoveTimeForward)
                });
            ((List<DelayedAction>)Game1.delayedActions).Add(new DelayedAction(7000)
            {
                behavior = new DelayedAction.delayedBehavior(TimeMagic.SlowDown)
            });
        }
    }
}