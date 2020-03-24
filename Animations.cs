using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SM
{
    internal static class Animations
    {
        private static Farmer player1 = Game1.player;
        private static Data t = RegressionMod.data;
        public static Texture2D sprites = Animations.LoadTexture("sprites.png");
        private static WateringCan waterCan = new WateringCan();

        public static void Warn(string msg, Body b = null)
        {
            Game1.addHUDMessage(new HUDMessage(Strings.InsertVariables(msg, b, (Container)null), 2));
        }

        public static void Warn(string[] msgs, Body b = null)
        {
            Animations.Warn(Strings.RandString(msgs), b);
        }

        public static void Say(string msg, Body b = null)
        {
            Game1.showGlobalMessage(Strings.InsertVariables(msg, b, (Container)null));
        }

        public static void Say(string[] msgs, Body b = null)
        {
            Animations.Say(Strings.RandString(msgs), b);
        }

        public static void Write(string msg, Body b = null)
        {
            Game1.drawObjectDialogue(Strings.InsertVariables(msg, b, (Container)null));
        }

        public static void Write(string[] msgs, Body b = null)
        {
            Animations.Write(Strings.RandString(msgs), b);
        }

        public static void CheckPants(Body b)
        {
            Animations.Say(Animations.t.LookPants[0] + " " + Strings.DescribeUnderwear(b.pants, (string)null) + ".", b);
        }

        public static void CheckUnderwear(Body b)
        {
            Animations.Say(Animations.t.PeekWaistband[0] + " " + Strings.DescribeUnderwear(b.underwear.container, (string)null) + ".", b);
        }

        public static void AnimateDrinking(bool waterSource = false)
        {
            if (Animations.player1.getFacingDirection() != 2)
                Animations.player1.faceDirection(2);
            Animations.player1.forceCanMove();
            Animations.player1.completelyStopAnimatingOrDoingAction();
            Animations.player1.FarmerSprite.animateOnce(294, 80f, 8, new AnimatedSprite.endOfAnimationBehavior(Animations.EndDrinking));
            Animations.player1.freezePause = 20000;
            Animations.player1.CanMove = false;
            if (!waterSource)
                return;
            Animations.Say(Animations.t.Drink_Water_Source, (Body)null);
        }

        private static void EndDrinking(Farmer who)
        {
            who.completelyStopAnimatingOrDoingAction();
            who.forceCanMove();
        }

        public static void AnimateWettingStart(Body b, bool voluntary, bool inUnderwear, bool inToilet)
        {
            Game1.playSound("wateringCan");
            if (b.sleeping || !voluntary && !RegressionMod.config.AlwaysNoticeAccidents && (double)b.bladderContinence + 0.200000002980232 <= RegressionMod.rnd.NextDouble())
                return;
            if (!inUnderwear)
            {
                if (inToilet)
                    Animations.Say(Animations.t.Pee_Toilet, b);
                else
                    Animations.Say(Animations.t.Pee_Voluntary, b);
            }
            else if (voluntary)
                Animations.Say(Animations.t.Wet_Voluntary, b);
            else
                Animations.Say(Animations.t.Wet_Accident, b);
            Animations.player1.forceCanMove();
            Animations.player1.completelyStopAnimatingOrDoingAction();
            Animations.player1.jitterStrength = 0.5f;
            Animations.player1.doEmote(28, false);
            if (!inUnderwear)
            {
                Animations.player1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(13, (Vector2)Game1.player.position, Microsoft.Xna.Framework.Color.White, 10, Game1.random.NextDouble() < 0.5, 70f, 0, Game1.tileSize, 0.05f, -1, 0));
                if ((Animations.player1.currentLocation.terrainFeatures).ContainsKey(Animations.player1.getTileLocation()) && (Animations.player1.currentLocation.terrainFeatures)[Animations.player1.getTileLocation()] is HoeDirt terrainFeature)
                    terrainFeature.performToolAction((Tool)waterCan, 0, player1.Position, player1.currentLocation);
            }
            Animations.player1.freezePause = 20000;
            Animations.player1.CanMove = false;
        }

        public static void AnimateWettingEnd(Body b)
        {
            if (b.wettingUnderwear && (double)b.pants.wetness > (double)b.pants.absorbency)
            {
                Animations.player1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(13, (Vector2)Game1.player.position, Microsoft.Xna.Framework.Color.White, 10, Game1.random.NextDouble() < 0.5, 70f, 0, Game1.tileSize, 0.05f, -1, 0));
                if ((Animations.player1.currentLocation.terrainFeatures).ContainsKey(Animations.player1.getTileLocation()) && (Animations.player1.currentLocation.terrainFeatures)[Animations.player1.getTileLocation()] is HoeDirt terrainFeature)
                    terrainFeature.performToolAction((Tool)waterCan, 0, player1.Position, player1.currentLocation); ;
            }
            Animations.player1.completelyStopAnimatingOrDoingAction();
            Animations.player1.forceCanMove();
        }

        public static void AnimateMessingStart(Body b, bool voluntary, bool inUnderwear, bool inToilet)
        {
            Game1.playSound("slosh");
            if (b.sleeping || !voluntary && !RegressionMod.config.AlwaysNoticeAccidents && (double)b.bowelContinence + 0.449999988079071 <= RegressionMod.rnd.NextDouble())
                return;
            if (!inUnderwear)
            {
                if (inToilet)
                    Animations.Say(Animations.t.Poop_Toilet, b);
                else
                    Animations.Say(Animations.t.Poop_Voluntary, b);
            }
            else if (voluntary)
                Animations.Say(Animations.t.Mess_Voluntary, b);
            else
                Animations.Say(Animations.t.Mess_Accident, b);
            Animations.player1.forceCanMove();
            Animations.player1.completelyStopAnimatingOrDoingAction();
            Animations.player1.jitterStrength = 1f;
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, Game1.tileSize, Game1.tileSize), 50f, 4, 0, (Vector2)Animations.player1.position - new Vector2(Animations.player1.FacingDirection == 1 ? 0.0f : (float)-Game1.tileSize, (float)(Game1.tileSize * 2)), false, Animations.player1.FacingDirection == 1, (float)Animations.player1.getStandingY() / 10000f, 0.01f, Microsoft.Xna.Framework.Color.White, 1f, 0.0f, 0.0f, 0.0f, false));
            Animations.player1.doEmote(12, false);
            Animations.player1.freezePause = 20000;
            Animations.player1.CanMove = false;
        }

        public static void AnimateMessingEnd()
        {
            Game1.playSound("coin");
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, Game1.tileSize, Game1.tileSize), 50f, 4, 0, (Vector2)Animations.player1.position - new Vector2(Animations.player1.FacingDirection == 1 ? 0.0f : (float)-Game1.tileSize, (float)(Game1.tileSize * 2)), false, Animations.player1.FacingDirection == 1, (float)Animations.player1.getStandingY() / 10000f, 0.01f, Microsoft.Xna.Framework.Color.White, 1f, 0.0f, 0.0f, 0.0f, false));
            Animations.player1.completelyStopAnimatingOrDoingAction();
            Animations.player1.forceCanMove();
        }

        public static void AnimatePeeAttempt(Body b, bool inUnderwear, bool inToilet)
        {
            if (inUnderwear)
                Animations.Say(Animations.t.Wet_Attempt, b);
            else if (inToilet)
                Animations.Say(Animations.t.Pee_Toilet_Attempt, b);
            else
                Animations.Say(Animations.t.Pee_Attempt, b);
        }

        public static void AnimatePoopAttempt(Body b, bool inUnderwear, bool inToilet)
        {
            if (inUnderwear)
                Animations.Say(Animations.t.Mess_Attempt, b);
            else if (inToilet)
                Animations.Say(Animations.t.Poop_Toilet_Attempt, b);
            else
                Animations.Say(Animations.t.Poop_Attempt, b);
        }

        public static void AnimateStillSoiled(Body b)
        {
            Animations.Say(Strings.ReplaceAndOr(Strings.RandString(Animations.t.Still_Soiled), (double)b.underwear.Wetness > 0.0, (double)b.underwear.Messiness > 0.0, "&"), b);
        }

        public static void AnimateNight(Body b)
        {
            bool first = b.peedToiletLastNight > 0;
            bool second = b.poopedToiletLastNight > 0;
            if (!(first | second) || !RegressionMod.config.Wetting && !RegressionMod.config.Messing)
                return;
            Animations.Write(Strings.ReplaceAndOr(Strings.RandString(Animations.t.Toilet_Night), first, second, "&"), b);
        }

        public static void AnimateMorning(Body b)
        {
            bool flag = (double)b.pants.wetness > 0.0;
            bool second = (double)b.pants.messiness > 0.0;
            string msg = "" + Strings.RandString(Animations.t.Wake_Up_Underwear_State);
            if (second)
            {
                msg = msg + " " + Strings.ReplaceOptional(Strings.RandString(Animations.t.Messed_Bed), flag);
                if (!RegressionMod.config.Easymode)
                    msg = msg + " " + Strings.ReplaceAndOr(Strings.RandString(Animations.t.Washing_Bedding), flag, second, "&");
            }
            else if (flag)
            {
                msg = msg + " " + Strings.RandString(Animations.t.Wet_Bed);
                if (!RegressionMod.config.Easymode)
                    msg = msg + " " + Strings.ReplaceAndOr(Strings.RandString(Animations.t.Spot_Washing_Bedding), flag, second, "&");
            }
            Animations.Write(msg, b);
        }

        public static void AnimateDryingBedding(Body b)
        {
            Animations.Write(Animations.t.Bedding_Still_Wet, b);
        }

        public static void AnimateWashingUnderwear(Container c)
        {
            Animations.Write(Strings.InsertVariables(Strings.RandString(Animations.t.Washing_Underwear), (Body)null, c), (Body)null);
        }

        public static Texture2D LoadTexture(string file)
        {
            return Animations.Bitmap2Texture(new Bitmap(Image.FromFile(Path.Combine(RegressionMod.helper.DirectoryPath, "Assets", file))));
        }

        public static Texture2D Bitmap2Texture(Bitmap bmp)
        {
            MemoryStream memoryStream = new MemoryStream();
            bmp.Save((Stream)memoryStream, ImageFormat.Png);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return Texture2D.FromStream(Game1.graphics.GraphicsDevice, (Stream)memoryStream);
        }

        public static Microsoft.Xna.Framework.Rectangle UnderwearRectangle(Container c, string type = null, int height = 16)
        {
            if (c.spriteIndex == -1)
                throw new Exception("Invalid sprite index.");
            int num = type != null ? (!(type == "drying") ? (!(type == "messy") ? (!(type == "wet") ? 0 : 16) : 32) : 48) : (!c.isDrying ? ((double)c.messiness <= 0.0 ? ((double)c.wetness <= 0.0 ? 0 : 16) : 32) : 48);
            return new Microsoft.Xna.Framework.Rectangle(c.spriteIndex * 16, num + (16 - height), 16, height);
        }

        public static void DrawUnderwearIcon(Container container, int x, int y)
        {
            if (container.isDrying)
            {
                Game1.spriteBatch.Draw(Animations.sprites, new Microsoft.Xna.Framework.Rectangle(x, y, 64, 64), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(container, "drying", 16)), Microsoft.Xna.Framework.Color.White);
            }
            else
            {
                Game1.spriteBatch.Draw(Animations.sprites, new Microsoft.Xna.Framework.Rectangle(x, y, 64, 64), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(container, "clean", 16)), Microsoft.Xna.Framework.Color.White);
                int height1 = Math.Min((int)((double)container.wetness / (double)container.absorbency * 16.0), 16);
                int height2 = Math.Min((int)((double)container.messiness / (double)container.containment * 16.0), 16);
                if (height1 > 0 && height1 >= height2)
                    Game1.spriteBatch.Draw(Animations.sprites, new Microsoft.Xna.Framework.Rectangle(x, y + (64 - height1 * 4), 64, height1 * 4), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(container, "wet", height1)), Microsoft.Xna.Framework.Color.White);
                if (height2 > 0)
                    Game1.spriteBatch.Draw(Animations.sprites, new Microsoft.Xna.Framework.Rectangle(x, y + (64 - height2 * 4), 64, height2 * 4), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(container, "messy", height2)), Microsoft.Xna.Framework.Color.White);
                if (height1 > 0 && height1 < height2)
                    Game1.spriteBatch.Draw(Animations.sprites, new Microsoft.Xna.Framework.Rectangle(x, y + (64 - height1 * 4), 64, height1 * 4), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(container, "wet", height1)), Microsoft.Xna.Framework.Color.White);
                if (Game1.getMouseX() >= x && Game1.getMouseX() <= x + 64 && Game1.getMouseY() >= y && Game1.getMouseY() <= y + 64)
                {
                    string source = Strings.DescribeUnderwear(container, (string)null);
                    string text = source.First<char>().ToString().ToUpper() + source.Substring(1);
                    int width = Game1.tileSize * 6 + Game1.tileSize / 6;
                    IClickableMenu.drawHoverText(Game1.spriteBatch, Game1.parseText(text, Game1.tinyFont, width), Game1.smallFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
                }
            }
        }

        public static void HandlePasserby()
        {
            List<string> stringList = new List<string>()
      {
        "Linus",
        "Krobus",
        "Dwarf"
      };
            if (!(Utility.isThereAFarmerOrCharacterWithinDistance(Animations.player1.getTileLocation(), 3, Game1.currentLocation) is NPC speaker) || stringList.Contains((string)speaker.name))
                return;
            speaker.CurrentDialogue.Push(new Dialogue("Oh wow, your diaper is all wet!", speaker));
        }

        public static bool HandleVillager(Body body, bool mess, bool inUnderwear, bool overflow, bool attempt = false, int baseFriendshipLoss = 20, int radius = 3)
        {
            //List of NPCs that don't care if they see you wet/mess
            //TODO: Make an ABDL NPCs list
            List<string> IgnoreNPC = new List<string>()
            {
              "Linus",
              "Krobus",
              "Dwarf"
            };

            string animalName = "";
            int friendshipLoss = -baseFriendshipLoss;

            //If it's a mess radius and friendship loss are doubled
            if (mess)
            {
                radius *= 2;
                friendshipLoss *= 2;
            }

            //If it's on the ground radius times 4, friendship loss doubled
            if (!inUnderwear)
            {
                radius *= 4;
                friendshipLoss *= 2;
            }

            //If it's just an attempt then radius is unchanged and friendship loss is halved
            if (attempt)
                friendshipLoss /= 2;

            //If it overflows the radius is doubled
            if (overflow)
                radius *= 2;

            //Max -> radius * 18, friendshipLoss * 4 for mess on the ground

            //If no one is around or they are from  who don't care then don't do anything
            if (!(Utility.isThereAFarmerOrCharacterWithinDistance(Animations.player1.getTileLocation(), radius, Game1.currentLocation) is NPC npc) || IgnoreNPC.Contains((string)npc.name))
                return false;

            //Current heart level with the npc that saw player
            int heartLevelForNpc = Animations.player1.getFriendshipHeartLevelForNPC(npc.getName());

            //TODO: test better bedmas
            int heartLossAmount = friendshipLoss + (heartLevelForNpc - 2) / 2 * baseFriendshipLoss;

            List<string> NPCString = new List<string>();

            //Is the npc an animal
            if (npc is Horse || npc is Cat || npc is Dog)
            {
                NPCString.Add("animal");
                animalName += string.Format("{0}: ", (object)npc.name);
            }
            else
            {
                //Add age then name
                switch (npc.age)
                {
                    case 0:
                        NPCString.Add("adult");
                        break;
                    case 1:
                        NPCString.Add("teen");
                        break;
                    case 2:
                        NPCString.Add("kid");
                        break;
                }
                NPCString.Add(npc.getName().ToLower());
            }

            string reactionType;
            if (!inUnderwear)
            {
                reactionType = attempt ? "ground_attempt" : "ground";
            }
            else
            {
                //Animals and villagers above 8 hearts don't lose hearts
                if (NPCString.Contains("animal"))
                {
                    reactionType = "soiled_nice";
                    heartLossAmount = 0;
                }
                else if (heartLevelForNpc >= 8)
                {
                    reactionType = "soiled_verynice";
                    heartLossAmount = 0;
                }
                else
                    reactionType = heartLossAmount < 0 ? "soiled_mean" : "soiled_nice";

                //ABDL NPCs don't lose hearts
                if (npc.getName() == "Abigail" || npc.getName() == "Jodi")
                    heartLossAmount = 0;
            }

            if (RegressionMod.config.Debug)
                Animations.Say(string.Format("{0} ({1}) changed friendship from {2} by {3}.", (object)npc.name, (object)(int)npc.age, (object)heartLevelForNpc, (object)heartLossAmount), (Body)null);
            
            if (heartLossAmount < 0 && !RegressionMod.config.NoFriendshipPenalty)
                Animations.player1.changeFriendship(heartLossAmount, npc);
            
            List<string> reactionStrings = new List<string>();
            
            foreach (string npcType in NPCString)
            {
                Dictionary<string, string[]> dictionary;
                string[] strArray;

                if (Animations.t.Villager_Reactions.TryGetValue(npcType, out dictionary) && dictionary.TryGetValue(reactionType, out strArray))
                    reactionStrings.AddRange(strArray);
            }
            
            string dialogueString = animalName + Strings.InsertVariables(
                Strings.ReplaceAndOr( Strings.RandString(reactionStrings.ToArray()), !mess, mess, "&"),
                body, (Container)null);
            
            npc.setNewDialogue(dialogueString, true, true);
            Game1.drawDialogue(npc);
            return true;
        }
    }
}