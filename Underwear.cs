using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SM
{
    //Stardew item that controls a Regression Container
    public class Underwear : StardewValley.Object
    {
        public static Color color;
        public Container container;
        public string id;

        //Empty constructor
        public Underwear()
        {
        }

        //Constructor from values
        public Underwear(string type, float wetness = 0.0f, float messiness = 0.0f, int count = 1)
        {
            this.Initialize(type, wetness, messiness, count);
        }

        //Constructor from container object
        public Underwear(Container container, int count = 1)
        {
            Initialize(container, count);
        }

        //Name getter
        public override string Name
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Status + this.id);
            }
        }

        //Display name getter/setter
        public override string DisplayName
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Status + this.id);
            }
            set
            {
                this.displayName = value;
            }
        }

        //Get status of underwear as string
        //TODO: make more options
        public string Status
        {
            get
            {
                if ((double)this.container.messiness > 0.0)
                    return "messy ";
                if ((double)this.container.wetness > 0.0)
                    return "wet ";
                return this.container.isDrying ? "drying " : "";
            }
        }

        public float Wetness { get => container.wetness; /*set => wetness = value;*/ }
        public float Messiness { get => container.messiness; /*set => messiness = value;*/ }

        public float AddPee(float peeAmount)
        {
            return container.AddPee(peeAmount);
        }

        public float AddPoop(float poopAmount)
        {
            return container.AddPoop(poopAmount);
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
            if ((double)this.container.messiness > 0.0 || (double)this.container.wetness > 0.0 || this.container.isDrying)
                return 1;
            return base.maximumStackSize();
        }

        //Initialize from values
        public void Initialize(string type, float wetness, float messiness, int count = 1)
        {
            this.container = new Container(type, wetness, messiness);
            if (count > 1)
                this.Stack = count;
            this.id = type;
            this.name = this.container.name;
            this.Price = 2;
        }

        //Initialize based on Container
        //TODO: Merge initializes so the don't diverge later
        public void Initialize(Container container, int count = 1)
        {
            this.container = container;
            if (count > 1)
                this.Stack = count;
            this.id = type;
            this.name = this.container.name;
            this.Price = 2;
        }

        //Don't know
        public object GetReplacement()
        {
            return (object)new StardewValley.Object(685, 1, false, -1, 0);
        }

        //Get a new clean copy of this underwear
        public override Item getOne()
        {
            return (Item)new Underwear(this.container.name, 0.0f, 0.0f, 1);
        }

        //From take the status of the underwear and give it to Strings to pull from Data
        //TODO: comment strings class
        public override string getDescription()
        {
            string source = Strings.DescribeUnderwear(this.container);
            return Game1.parseText(source.First<char>().ToString().ToUpper() + source.Substring(1), Game1.smallFont, Game1.tileSize * 6 + Game1.tileSize / 6);
        }

        //Get the extra dat that this item has that needs to be saved
        public Dictionary<string, string> GetAdditionalSaveData()
        {
            return new Dictionary<string, string>()
      {
        {
          "type",
          this.container.name
        },
        {
          "wetness",
          string.Format("{0}", (object) this.container.wetness)
        },
        {
          "messiness",
          string.Format("{0}", (object) this.container.messiness)
        },
        {
          "stack",
          string.Format("{0}", (object) (int) this.stack)
        }
      };
        }

        //Rebuild this underwear from it's additional save data
        public void Rebuild(Dictionary<string, string> data, object replacement)
        {
            this.Initialize(data["type"], float.Parse(data["wetness"]), float.Parse(data["messiness"]), int.Parse(data["stack"]));
        }

        //Draw the underwear sprite when in the menu
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            bool stackFlag = (drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1 || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;
            Rectangle rectangle = Animations.UnderwearRectangle(this.container, (string)null, 16);
            spriteBatch.Draw(Animations.sprites, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Rectangle?(rectangle), Color.White * transparency, 0.0f, new Vector2(8f, 8f), (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);
            
            if (stackFlag)
                Utility.drawTinyDigits((int)this.stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString((int)this.stack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);

            }

        //Draw the underwear sprite when in your hand
        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            Rectangle rectangle = Animations.UnderwearRectangle(this.container, (string)null, 16);
            spriteBatch.Draw(Animations.sprites, objectPosition, new Rectangle?(rectangle), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
        }
    }
}