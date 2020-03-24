using System;

namespace SM
{
    //Regression class that handle most 
    public class Container
    {
        public float wetness = 0.0f;
        public float messiness = 0.0f;
        public bool isDrying = false;
        public string name;
        public string prefix;
        public string description;
        public float absorbency;
        public float containment;
        public int spriteIndex;
        public int price;
        public bool washable;
        public bool plural;

        //Empty constructor
        public Container()
        {
        }

        //Constructor from values
        public Container(string type, float wetness = 0.0f, float messiness = 0.0f)
        {
            this.Initialize(type, wetness, messiness);
        }

        //Initialize from values
        public void Initialize(string type, float wetness = 0.0f, float messiness = 0.0f)
        {
            Container container = new Container();
            if (!RegressionMod.data.Underwear_Options.TryGetValue(type, out container))
                throw new Exception(string.Format("Invalid underwear choice: {0}", (object)type));
            this.Initialize(container, wetness, messiness);
        }

        //Initialize from another container
        private void Initialize(Container c, float wetness = 0.0f, float messiness = 0.0f)
        {
            this.name = c.name;
            this.prefix = c.prefix;
            this.description = c.description;
            this.absorbency = c.absorbency;
            this.containment = c.containment;
            this.spriteIndex = c.spriteIndex;
            this.price = c.price;
            this.washable = c.washable;
            this.plural = c.plural;
            this.wetness = wetness;
            this.messiness = messiness;
            RegressionMod.events.GameLoop.DayStarted += this.DayStarted;
        }

        //Destructor, removes DatStarted listener
        ~Container()
        {
            RegressionMod.events.GameLoop.DayStarted -= this.DayStarted;
        }

        //Dries all underwear at the start of the day
        private void DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            isDrying = false;
        }

        //Adds pee to underwear and returns the amount that overflows
        public float AddPee(float peeAmount)
        {
            this.wetness += peeAmount;
            if (this.wetness > this.absorbency)
            {
                //Wetness > absorbancy = overflow, so the amount overflow is returned. If it has already overflowed then just pass through the amount
                return Math.Max(peeAmount, this.wetness - this.absorbency);
            }
            return 0.0f;
        }

        //Adds poo to underwear and returns the amount that overflows
        public float AddPoop(float poopAmount)
        {
            this.messiness += poopAmount;
            if (this.messiness > this.containment)
            {
                //Messiness > containment = overflow, so the amount overflow is returned. If it has already overflowed then just pass through the amount
                return Math.Max(poopAmount, this.messiness - this.containment);
            }
            return 0.0f;
        }
    }
}