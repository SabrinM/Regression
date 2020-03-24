using Microsoft.Xna.Framework;
using Regression;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Quests;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SM
{
    public class RegressionMod : Mod
    {
        public static bool started = false;
        public static bool morningHandled = true;
        public static int lastTimeOfDay = 0;
        public static Random rnd = new Random();
        public static string foodToHandle = (string)null;
        public Body body;
        public static Data data;
        public static Config config;
        public static Farmer player1;
        public static IModHelper helper;
        public static IMonitor monitor;
        public static IModEvents events;

        public override void Entry(IModHelper h)
        {
            RegressionMod.helper = h;
            RegressionMod.monitor = this.Monitor;
            RegressionMod.events = helper.Events;
            RegressionMod.config = this.Helper.ReadConfig<Config>();
            RegressionMod.data = this.Helper.Data.ReadJsonFile<Data>(string.Format("{0}.json", (object)SM.RegressionMod.config.Lang)) ?? this.Helper.Data.ReadJsonFile<Data>("en.json");
            events.GameLoop.SaveLoaded += RecieveSaveLoaded;
            events.GameLoop.Saving += this.ReceiveBeforeSave;
            events.GameLoop.DayStarted += this.ReceiveAfterDayStarted;
            events.GameLoop.UpdateTicked += this.UpdateTicked;
            events.GameLoop.TimeChanged += this.ReceiveTimeOfDayChanged;
            events.Input.ButtonPressed += this.ReceiveKeyPress;
            events.Input.ButtonReleased += this.ReceiveKeyReleased;
            events.Input.CursorMoved += this.ReceiveMouseMoved;
            events.Display.MenuChanged += this.ReceiveMenuChanged;
            events.Display.RenderingHud += this.ReceivePreRenderHudEvent;
        }

        private void RecieveSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.body = this.Helper.Data.ReadJsonFile<Body>(string.Format("{0}/RegressionSave.json", (object)Constants.SaveFolderName)) ?? new Body();
            body.underwear.container = body.underwearContainer;
            SM.RegressionMod.started = true;
            SM.RegressionMod.player1 = Game1.player;

            if (Game1.dayOfMonth == 1 && Game1.year == 1)
            {
                Game1.player.mailbox.Add("RegressionStart");
                Game1.player.mailReceived.Add("RegressionStart");
            }
        }

        private void ReceiveMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!RegressionMod.started)
                return;
            if (Game1.currentLocation is FarmHouse && e.NewMenu is DialogueBox newMenu && Game1.currentLocation.lastQuestionKey == "Sleep" && !SM.RegressionMod.config.Easymode)
            {
                if (this.body.beddingDryTime > Game1.timeOfDay)
                {
                    List<Response> privateValue = (List<Response>)this.Helper.Reflection.GetField<List<Response>>((object)newMenu, "responses", true);
                    if (privateValue.Count == 2)
                    {
                        Response answer = privateValue[1];
                        Game1.currentLocation.answerDialogue(answer);
                        Game1.currentLocation.lastQuestionKey = (string)null;
                        newMenu.closeDialogue();
                        Animations.AnimateDryingBedding(this.body);
                    }
                }
            }
            else if (Game1.currentLocation is SeedShop && e.NewMenu is ShopMenu seedShop)
            {
                string fieldName1 = "forSale";
                List<Item> field1 = seedShop.GetField<List<Item>>(fieldName1);
                string fieldName2 = "itemPriceAndStock";
                Dictionary<Item, int[]> field2 = seedShop.GetField<Dictionary<Item, int[]>>(fieldName2);
                List<string> stringList = Strings.ValidUnderwearTypes();
                stringList.Remove("Joja diaper");
                foreach (string type in stringList)
                {
                    Underwear underwear = new Underwear(type, 0.0f, 0.0f, 1);
                    field1.Add((Item)underwear);
                    field2.Add((Item)underwear, new int[2]
                    {
            underwear.container.price,
            999
                    });
                }
            }
            else if (Game1.currentLocation is JojaMart && e.NewMenu is ShopMenu jojaShop)
            {
                string fieldName1 = "forSale";
                List<Item> field1 = jojaShop.GetField<List<Item>>(fieldName1);
                string fieldName2 = "itemPriceAndStock";
                Dictionary<Item, int[]> field2 = jojaShop.GetField<Dictionary<Item, int[]>>(fieldName2);
                string[] strArray = new string[2]
                {
          "Joja diaper",
          "cloth diaper"
                };
                foreach (string type in strArray)
                {
                    Underwear underwear = new Underwear(type, 0.0f, 0.0f, 1);
                    field1.Add((Item)underwear);
                    field2.Add((Item)underwear, new int[2]
                    {
            (int) ((double) underwear.container.price * 1.20000004768372),
            999
                    });
                }
            }
        }

        private void ReceiveTimeOfDayChanged(object sender, TimeChangedEventArgs e)
        {
            SM.RegressionMod.lastTimeOfDay = Game1.timeOfDay;

            //If it's before 6:30, wetness + messiness is at 0, or 5% chance - don't animate soiled
            if (SM.RegressionMod.rnd.NextDouble() >= 0.0555555559694767
                || body.underwear.Wetness + body.underwear.Messiness <= 0.0
                || Game1.timeOfDay < 630)
                return;

            Animations.AnimateStillSoiled(this.body);
        }

        private void ReceiveBeforeSave(object Sender, EventArgs e)
        {
            this.body.bedtime = SM.RegressionMod.lastTimeOfDay;

            //Don't handle night on the first day of year one
            if (Game1.dayOfMonth > 1 || Game1.currentSeason != "spring" || Game1.year > 1)
            {
                this.body.HandleNight();
            }

            if (string.IsNullOrWhiteSpace(Constants.SaveFolderName))
                return;
            this.body.underwearContainer = this.body.underwear.container;
            this.Helper.Data.WriteJsonFile<Body>(string.Format("{0}/RegressionSave.json", (object)Constants.SaveFolderName), this.body);
        }

        private void ReceiveAfterDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.dayOfMonth > 1 || Game1.currentSeason != "spring" || Game1.year > 1)
            {
                SM.RegressionMod.morningHandled = false;
                Animations.AnimateNight(this.body);
            }
        }

        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            //Has 8 ticks passed
            if (e.IsMultipleOf(8))
            {

                if (!SM.RegressionMod.started)
                    return;

                if (!SM.RegressionMod.morningHandled && !Game1.fadeToBlack && SM.RegressionMod.player1.CanMove)
                {
                    this.body.HandleMorning();
                    SM.RegressionMod.morningHandled = true;
                }
                //Game is in focus or doesn't pause when not in focus
                if ((((Game)typeof(Game1).Assembly.GetType("StardewValley.Program", true).GetField("gamePtr").GetValue((object)null)).IsActive || !Game1.options.pauseWhenOutOfFocus)
                     //Not paused or with dialogue up
                     && !(Game1.paused || Game1.dialogueUp)
                     //Not in a minigame, event or menu
                     && (Game1.currentMinigame == null && !Game1.eventUp && (Game1.activeClickableMenu == null && !Game1.menuUp))
                     //Not in fade to black
                     && !Game1.fadeToBlack)
                {
                    // 2/645 -> 8/2580, there are ~2580 ticks per hour
                    this.body.HandleTime(2f / 645f);
                }

                //If the player is eating, not in a menu and the food to handle hasn't been set
                if (Game1.player.isEating && Game1.activeClickableMenu == null && SM.RegressionMod.foodToHandle == null)
                {
                    //Get item being eaten from SDV Farmer class
                    SM.RegressionMod.foodToHandle = SM.RegressionMod.player1.itemToEat.Name.ToLower();
                }
                //If the player has an item to eat and isn't eating then process the item then null the value
                else if (SM.RegressionMod.foodToHandle != null && !Game1.player.isEating)
                {
                    if (new Regex("(beer|ale|wine|juice|mead|coffee|milk)").IsMatch(SM.RegressionMod.foodToHandle))
                        this.body.DrinkBeverage();
                    else
                        this.body.Eat();
                    SM.RegressionMod.foodToHandle = (string)null;
                }
            }
        }

        private void ReceiveKeyReleased(object sender, ButtonReleasedEventArgs e)
        {
        }

        private void ReceiveKeyPress(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseRight)
            {
                //Don't take action when in menus and events
                if ((Game1.dialogueUp || Game1.currentMinigame != null || (Game1.eventUp || Game1.activeClickableMenu != null) || Game1.menuUp || Game1.fadeToBlack) || (SM.RegressionMod.player1.isRidingHorse() || !SM.RegressionMod.player1.canMove || (Game1.player.isEating || SM.RegressionMod.player1.canOnlyWalk) || SM.RegressionMod.player1.FarmerSprite.pauseForSingleAnimation))
                    return;
                //If holding a watering can drink from it
                if (SM.RegressionMod.player1.CurrentTool != null && SM.RegressionMod.player1.CurrentTool is WateringCan)
                    this.body.DrinkWateringCan();
                else if (SM.RegressionMod.player1.ActiveObject is Underwear activeObject)
                {
                    //If the held underwear is not wet/messy and not drying then swap it with the current underwear
                    if (activeObject.container.wetness + activeObject.container.messiness == 0.0 && !activeObject.container.isDrying)
                    {
                        SM.RegressionMod.player1.reduceActiveItemByOne();
                        Underwear underwear = body.ChangeUnderwear(activeObject);
                        if (!SM.RegressionMod.player1.addItemToInventoryBool((Item)underwear, false))
                            Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(new List<Item>() { (Item)underwear });
                    }
                    else if (activeObject.container.washable && !activeObject.container.isDrying)
                    {
                        GameLocation currentLocation = Game1.currentLocation;
                        Vector2 toolLocation = SM.RegressionMod.player1.GetToolLocation(false);
                        int x = (int)toolLocation.X;
                        int y = (int)toolLocation.Y;

                        //Is the player next to water with dirty, dry underwear, then clean it; better method? - https://github.com/Platonymous/Stardew-Valley-Mods/blob/master/SwimSuit/SwimSuitMod.cs
                        if (currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "Water", "Back") != null || currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "WaterSource", "Back") != null)
                        {
                            Animations.AnimateWashingUnderwear(activeObject.container);
                            activeObject.container.wetness = 0.0f;
                            activeObject.container.messiness = 0.0f;
                            activeObject.container.isDrying = true;
                        }
                    }
                }
                else
                {
                    GameLocation currentLocation = Game1.currentLocation;
                    Vector2 toolLocation = SM.RegressionMod.player1.GetToolLocation(false);
                    int x = (int)toolLocation.X;
                    int y = (int)toolLocation.Y;
                    Vector2 tile = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
                    //Is the player next to water, then drink
                    //TODO: Need to fix as this currently blocks you from building the bridges on the beach
                    if (currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "Water", "Back") != null || currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "WaterSource", "Back") != null || currentLocation is BuildableGameLocation && (currentLocation as BuildableGameLocation).getBuildingAt(tile) != null && (((string)(currentLocation as BuildableGameLocation).getBuildingAt(tile).buildingType).Equals("Well") && (currentLocation as BuildableGameLocation).getBuildingAt(tile).daysOfConstructionLeft <= 0))
                        this.body.DrinkWaterSource();
                }
            }
            else
            {
                if (!started)
                    return;
                bool shiftHeld = e.IsDown(SButton.LeftShift);
                //Mostly debug, move the wet/mess buttons to different keys?
                switch (e.Button)
                {
                    case SButton.L:
                        if (config.Debug && shiftHeld)
                        {
                            this.body.DecreaseFoodAndWater();
                            break;
                        }
                        break;
                    case SButton.S:
                        if (config.Debug && shiftHeld)
                        {
                            this.body.IncreaseEverything();
                            break;
                        }
                        break;
                    case SButton.F1:
                        if (!this.body.isWetting && !this.body.isMessing && !this.body.IsFishing())
                        {
                            this.body.StartWetting(true, !shiftHeld);
                            break;
                        }
                        break;
                    case SButton.F2:
                        if (!this.body.isWetting && !this.body.isMessing && !this.body.IsFishing())
                        {
                            this.body.StartMessing(true, !shiftHeld);
                            break;
                        }
                        break;
                    case SButton.F3:
                        if (config.Debug)
                        {
                            this.GiveUnderwear();
                            break;
                        }
                        break;
                    case SButton.F5:
                        Animations.CheckUnderwear(this.body);
                        break;
                    case SButton.F6:
                        Animations.CheckPants(this.body);
                        break;
                    case SButton.F7:
                        if (config.Debug)
                        {
                            //TimeMagic.DoMagic();
                            break;
                        }
                        break;
                    case SButton.F8:
                        //Toggle wetting / messing
                        config.Wetting = !config.Wetting;
                        config.Messing = !config.Messing;
                        Monitor.Log(String.Format("Weeting and messing {0}", config.Debug ? "Enabled" : "Disabled"));
                        break;
                    case SButton.F9:
                        RegressionMod.config.Debug = !SM.RegressionMod.config.Debug;
                        Monitor.Log(String.Format("Debug {0}", config.Debug ? "Enabled" : "Disabled"));
                        Game1.addHUDMessage(new HUDMessage(String.Format("Debug {0}", config.Debug ? "Enabled" : "Disabled"), 2));
                        break;
                }
            }
        }

        private void ReceiveMouseMoved(object sender, CursorMovedEventArgs e)
        {

        }

        public void ReceivePreRenderHudEvent(object sender, EventArgs args)
        {
            if (!SM.RegressionMod.started || Game1.currentMinigame != null || Game1.eventUp || Game1.globalFade)
                return;
            this.DrawStatusBars();
        }

        public void DrawStatusBars()
        {
            int x1 = Game1.viewport.Width - (65 + StatusBars.barWidth);
            int y1 = Game1.viewport.Height - (25 + StatusBars.barHeight);
            if (Game1.currentLocation is MineShaft || Game1.currentLocation is Woods || Game1.currentLocation is SlimeHutch || SM.RegressionMod.player1.health < SM.RegressionMod.player1.maxHealth)
                x1 -= 58;

            if (config.Debug)
            {
                //Hunger and thirst
                if (!SM.RegressionMod.config.NoHungerAndThirst)
                {
                    float percentage1 = this.body.food / this.body.maxFood;
                    StatusBars.DrawStatusBar(x1, y1, percentage1, new Color(115, (int)byte.MaxValue, 56));
                    int x2 = x1 - (10 + StatusBars.barWidth);
                    float percentage2 = this.body.water / this.body.maxWater;
                    StatusBars.DrawStatusBar(x2, y1, percentage2, new Color(117, 225, (int)byte.MaxValue));
                    x1 = x2 - (10 + StatusBars.barWidth);
                }

                //Draw the bowels and bladder meters
                if (SM.RegressionMod.config.Messing)
                {
                    float percentage = this.body.bowels / this.body.maxBowels;
                    StatusBars.DrawStatusBar(x1, y1, percentage, new Color(146, 111, 91));
                    x1 -= 10 + StatusBars.barWidth;
                }
                if (SM.RegressionMod.config.Wetting)
                {
                    float percentage = this.body.bladder / this.body.maxBladder;
                    StatusBars.DrawStatusBar(x1, y1, percentage, new Color((int)byte.MaxValue, 225, 56));
                }
            }

            if (!SM.RegressionMod.config.Wetting && !SM.RegressionMod.config.Messing)
                return;

            //Draw underwear icon
            //TODO: move to be in menu?
            int y2 = ((Netcode.NetObjectList<Quest>)Game1.player.questLog).Count == 0 ? 250 : 310;
            Animations.DrawUnderwearIcon(this.body.underwear.container, Game1.viewport.Width - 94, y2);
        }

        private void GiveUnderwear()
        {
            List<Item> objList = new List<Item>();
            foreach (string validUnderwearType in Strings.ValidUnderwearTypes())
                objList.Add((Item)new Underwear(validUnderwearType, 0.0f, 0.0f, 20));
            objList.Add((Item)new StardewValley.Object(399, 99, false, -1, 0));
            objList.Add((Item)new StardewValley.Object(348, 99, false, -1, 0));
            Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(objList);
        }
    }
}