using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Netcode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace SM
{
    enum CleanStatus
    {
        Clean = 0,
        Drying,
        Dirty
    }
    enum UnderwearType
    {
        Bed = -2,
        Pants = -1,
        Thong = 100,
        PolkaDotPanties,
        BigKidUndies,
        DinosaurUndies,
        LavenderPullup,
        TrainingPants,
        JojaDiaper,
        BabyPrintDiaper,
        ClothDiaper,
        SpacePrintDiaper,
        PawPrintDiaper,
        MassiveDiaper
    }
    //Stardew item that controls a Regression Container
    public class Underwear : Item
    {
        #region variables
        [XmlElement("absorbency")]
        public readonly NetInt absorbency = new NetInt();
        [XmlElement("containment")]
        public readonly NetInt containment = new NetInt();
        [XmlElement("price")]
        public readonly NetInt price = new NetInt();
        [XmlElement("spriteIndex")]
        public readonly NetInt spriteIndex = new NetInt();
        [XmlElement("dyeable")]
        public readonly NetBool dyeable = new NetBool();
        [XmlElement("washable")]
        public readonly NetBool washable = new NetBool();
        [XmlElement("isDiaper")]
        public readonly NetBool isDiaper = new NetBool();
        [XmlElement("clothesColor")]
        public readonly NetColor clothesColor = new NetColor(new Color((int)byte.MaxValue, (int)byte.MaxValue, (int)byte.MaxValue));
        [XmlElement("wetness")]
        public readonly NetFloat wetness = new NetFloat(0);
        [XmlElement("messiness")]
        public readonly NetFloat messiness = new NetFloat(0);
        [XmlElement("cleanStatus")]
        public readonly NetInt cleanStatus = new NetInt(0);
        [XmlIgnore]
        public string name;
        [XmlIgnore]
        public string description;
        [XmlIgnore]
        public string prefix;
        [XmlIgnore]
        public bool _loadedData;
        #endregion

        //Empty constructor
        public Underwear()
        {
            this.Category = -100;
            this.NetFields.AddFields((INetSerializable)this.absorbency, (INetSerializable)this.containment, (INetSerializable)this.price, (INetSerializable)this.spriteIndex, (INetSerializable)this.dyeable, (INetSerializable)this.clothesColor);
            RegressionMod.events.GameLoop.DayStarted += this.DayStarted;
        }

        public Underwear(int item_index, float wetness = 0.0f, float messiness = 0.0f, int count = 1) : this()
        {
            this.ParentSheetIndex = item_index;
            this.LoadData(true);
            AddPee(wetness);
            AddPoop(messiness);
            this.Stack = count;
        }
        
        //Destructor, removes DatStarted listener
        ~Underwear()
        {
            RegressionMod.events.GameLoop.DayStarted -= this.DayStarted;
        }

        //Display name getter/setter
        [XmlIgnore]
        public override string DisplayName
        {
            get
            {
                if (!this._loadedData)
                    this.LoadData(false);
                return this.Status.ToUpper() + this.name;
                //return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Status + this.id);
            }
            set
            {
                this.name = value;
            }
        }

        [XmlIgnore]
        public override string Name { get => DisplayName; }

        //Get status of underwear as string
        //TODO: make more options
        [XmlIgnore]
        public string Status
        {
            get
            {
                switch (cleanStatus)
                {
                    case 1:
                        return "drying ";
                    case 2:
                        if ((double)this.Messiness > 0.0)
                            return "messy ";
                        return "wet ";
                    default:
                        return "";
                }
            }
        }

        [XmlIgnore]
        public int CleanStatus { get => cleanStatus; }
        [XmlIgnore]
        public float Wetness { get => wetness; set => wetness.Set(value); }
        [XmlIgnore]
        public float Messiness { get => messiness; set => messiness.Set(value); }

        //Dries all underwear at the start of the day
        private void DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (cleanStatus == (int)SM.CleanStatus.Drying)
                cleanStatus.Set(0);
        }

        //Adds pee to underwear and returns the amount that overflows
        public float AddPee(float peeAmount)
        {
            if (cleanStatus < 2 && peeAmount > 0f)
                cleanStatus.Set(2);

            this.Wetness += peeAmount;
            if (this.Wetness > this.absorbency)
            {
                //Wetness > absorbancy = overflow, so the amount overflow is returned. If it has already overflowed then just pass through the amount
                return Math.Max(peeAmount, this.Wetness - this.absorbency);
            }
            return 0.0f;
        }

        //Adds poo to underwear and returns the amount that overflows
        public float AddPoop(float poopAmount)
        {
            if (cleanStatus < 2 && poopAmount > 0f)
                cleanStatus.Set(2);

            this.Messiness += poopAmount;
            if (this.Messiness > this.containment)
            {
                //Messiness > containment = overflow, so the amount overflow is returned. If it has already overflowed then just pass through the amount
                return Math.Max(poopAmount, this.Messiness - this.containment);
            }
            return 0.0f;
        }

        //SDV override that you can't give underwear as a gift
        //TODO: add to gift list of ABDL NPCs
        public override bool canBeGivenAsGift()
        {
            return false;
        }

        //SDV override that you can't drop underwear on the ground
        public override bool canBeDropped()
        {
            return false;
        }

        //Get max stack size: if it has a status (wet, messy, drying) stack size is one, else SDV max
        public override int maximumStackSize()
        {
            if ((double)this.Messiness > 0.0 || (double)this.Wetness > 0.0 || this.CleanStatus > 0)
                return 1;
            //Max size when clean
            return 16;
        }
        
        //Don't know
        public object GetReplacement()
        {
            return (object)new StardewValley.Object(685, 1, false, -1, 0);
        }

        //Get a new clean copy of this underwear
        public override Item getOne()
        {
            return (Item)new Underwear(this.ParentSheetIndex);
        }

        //From take the status of the underwear and give it to Strings to pull from Data
        //TODO: comment strings class
        //public override string getDescription()
        //{
        //    string source = Strings.DescribeUnderwear(this.container);
        //    return Game1.parseText(source.First<char>().ToString().ToUpper() + source.Substring(1), Game1.smallFont, Game1.tileSize * 6 + Game1.tileSize / 6);
        //}

      //  //Get the extra dat that this item has that needs to be saved
      //  public Dictionary<string, string> GetAdditionalSaveData()
      //  {
      //      return new Dictionary<string, string>()
      //{
      //  {
      //    "type",
      //    this.container.name
      //  },
      //  {
      //    "wetness",
      //    string.Format("{0}", (object) this.container.wetness)
      //  },
      //  {
      //    "messiness",
      //    string.Format("{0}", (object) this.container.messiness)
      //  },
      //  {
      //    "stack",
      //    string.Format("{0}", (object) (int) this.Stack)
      //  }
      //};
      //  }

      //  //Rebuild this underwear from it's additional save data
      //  public void Rebuild(Dictionary<string, string> data, object replacement)
      //  {
      //      this.Initialize(data["type"], float.Parse(data["wetness"]), float.Parse(data["messiness"]), int.Parse(data["stack"]));
      //  }

        //Draw the underwear sprite when in the menu
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            bool stackFlag = (drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1 || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;
            Rectangle rectangle = Animations.UnderwearRectangle(this, (string)null, 16);
            spriteBatch.Draw(Animations.sprites, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Rectangle?(rectangle), Color.White * transparency, 0.0f, new Vector2(8f, 8f), (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);
            
            if (stackFlag)
                Utility.drawTinyDigits((int)this.Stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString((int)this.Stack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);

            }

        ////Draw the underwear sprite when in your hand
        //public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        //{
        //    Rectangle rectangle = Animations.UnderwearRectangle(this.container, (string)null, 16);
        //    spriteBatch.Draw(Animations.sprites, objectPosition, new Rectangle?(rectangle), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
        //}

        public override bool isPlaceable()
        {
            return false;
        }

        public override int addToStack(Item stack)
        {
            return 1;
        }

        [XmlIgnore]
        public int Price
        {
            set
            {
                this.price.Value = value;
            }
            get
            {
                return this.price.Value;
            }
        }

        [XmlIgnore]
        public override int Stack
        {
            get
            { return 1; }
            set { }
        }

        public void LoadData(bool initialize_color = false)
        {
            if (this._loadedData)
                return;
            int parentSheetIndex = this.ParentSheetIndex;
            this.Category = -100;

            RegressionMod.monitor.Log("Loading underwear " + parentSheetIndex, StardewModdingAPI.LogLevel.Debug);

            string[] strArray;

            if (RegressionMod.data.underwearInformation.ContainsKey(parentSheetIndex))
            {
                strArray = RegressionMod.data.underwearInformation[parentSheetIndex].Split('/');
            }
            else
            {
                strArray = RegressionMod.data.underwearInformation[-3].Split('/');
            }

            this.name = strArray[0];
            this.prefix = strArray[1];
            this.description = strArray[2];
            this.absorbency.Value = Convert.ToInt32(strArray[3]);
            this.containment.Value = Convert.ToInt32(strArray[4]);
            this.spriteIndex.Value = Convert.ToInt32(strArray[5]);
            this.price.Value = Convert.ToInt32(strArray[6]);
            if (initialize_color)
            {
                string[] colorArray = strArray[7].Split(' ');
                this.clothesColor.Value = new Color(Convert.ToInt32(colorArray[0]), Convert.ToInt32(colorArray[1]), Convert.ToInt32(colorArray[2]));
            }
            this.dyeable.Value = Convert.ToBoolean(strArray[8]);
            this.washable.Value = Convert.ToBoolean(strArray[9]);
            this.isDiaper.Value = Convert.ToBoolean(strArray[10]);


            if (this.dyeable.Value)
                this.description = this.description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Clothes_Dyeable");
            this._loadedData = true;
        }

        public override string getDescription()
        {
            if (!this._loadedData)
                this.LoadData(false);
            return Game1.parseText(Strings.DescribeUnderwear(this), Game1.smallFont, this.getDescriptionWidth());
        }

        public void CleanUnderwear()
        {
            cleanStatus.Set(1);
            Wetness = 0f;
            Messiness = 0f;
        }

        //TODO: Add logic
        public override bool canStackWith(ISalable other)
        {
            return false;
        }
    }
}