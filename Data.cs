using System.Collections.Generic;

namespace SM
{
    public class Data
    {
        public string[] Food_Low;
        public string[] Food_None;
        public string[] Water_Low;
        public string[] Water_None;
        public string[] Drink_Water_Source;
        public string[] Bladder_Yellow;
        public string[] Bladder_Orange;
        public string[] Bladder_Red;
        public string[] Bowels_Yellow;
        public string[] Bowels_Orange;
        public string[] Bowels_Red;
        public string[] Bladder_Continence_Min;
        public string[] Bladder_Continence_Red;
        public string[] Bladder_Continence_Orange;
        public string[] Bladder_Continence_Yellow;
        public string[] Bowel_Continence_Min;
        public string[] Bowel_Continence_Red;
        public string[] Bowel_Continence_Orange;
        public string[] Bowel_Continence_Yellow;
        public string[] Pee_Voluntary;
        public string[] Wet_Voluntary;
        public string[] Wet_Accident;
        public string[] Poop_Voluntary;
        public string[] Mess_Voluntary;
        public string[] Mess_Accident;
        public string[] Pee_Toilet;
        public string[] Poop_Toilet;
        public string[] Pee_Toilet_Attempt;
        public string[] Poop_Toilet_Attempt;
        public string[] Pee_Attempt;
        public string[] Poop_Attempt;
        public string[] Wet_Attempt;
        public string[] Mess_Attempt;
        public string[] Still_Soiled;
        public string[] Toilet_Night;
        public string[] Wake_Up_Underwear_State;
        public string[] Wet_Bed;
        public string[] Messed_Bed;
        public string[] Spot_Washing_Bedding;
        public string[] Washing_Bedding;
        public string[] Washing_Underwear;
        public string[] Bedding_Still_Wet;
        public string[] Change; //***"You take off your soiled <OLD_UNDERWEAR_DESC> and put on $UNDERWEAR_PREFIX$ clean $UNDERWEAR_NAME$."
        public string[] PeekWaistband;
        public string[] LookPants;
        public string Underwear_Clean;
        public string Underwear_Drying;
        public string[] Underwear_Wet; //Chosen based on diaper wetness, applied after Underwear_Messy. Words in brackets only show if underwear is also messy.
        public string[] Underwear_Messy; //Chosen based on diaper messiness, applied before Underwear_Wet. Words in brackets are only used if underwear is also wet.
        public string[] Pee_Overflow;
        public string[] Poop_Overflow;
        public string[] Debuff_Wet_Pants;
        public string[] Debuff_Messy_Pants;
        public Dictionary<string, Dictionary<string, string[]>> Villager_Reactions;
        public Dictionary<string, Container> Underwear_Options;
    }
}
