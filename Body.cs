using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace SM
{
    public class Body
    {
        public static float baseFoodDay = 100f;
        public static float baseWaterDay = 2500f;
        public static float glassOfWater = 240f;
        public static float baseMaxBladder = 400f;
        public static float baseMaxBowels = Body.baseFoodDay;
        public List<string> lettersReceived = new List<string>();
        public bool sleeping = false;
        public int bedtime = 0;
        public int beddingDryTime = 0;
        public int peedToiletLastNight = 0;
        public int poopedToiletLastNight = 0;
        public bool isWetting = false;
        public bool wettingVoluntarily = false;
        public bool wettingUnderwear = false;
        public bool isMessing = false;
        public bool messingVoluntarily = false;
        public bool messingUnderwear = false;
        public float foodDay = Body.baseFoodDay;
        public float maxFood = Body.baseFoodDay;
        public float food = Body.baseFoodDay;
        public float waterDay = Body.baseWaterDay;
        public float maxWater = Body.baseWaterDay;
        public float water = Body.baseWaterDay;
        public float maxBladder = Body.baseMaxBladder;
        public float bladder = 0.0f;
        public float bladderContinence = 1f;
        public float maxBowels = Body.baseMaxBowels;
        public float bowels = 0.0f;
        public float bowelContinence = 1f;
        public float[] stomach = new float[2];
        public float lastStamina;
        public Underwear underwear;
        public Underwear bottoms; //Need to make pants pull name/null from clothing

        //Constructor that starts with default underwear + pants
        public Body()
        {
            underwear = new Underwear((int)UnderwearType.DinosaurUndies);
            bottoms = new Underwear((int)UnderwearType.Pants);
        }

        //Public access to add food to body
        //TODO: Take in a food item and base amount added on stamina + health value
        public void Eat()
        {
            AddFood(Math.Max(maxFood / 2f, maxFood - food));
        }

        //Public access to add drink to body if you pres RMB with the watering can
        public void DrinkWateringCan()
        {
            Farmer player = Game1.player;
            WateringCan wateringCan = (WateringCan)player.CurrentTool;
            if (wateringCan.WaterLeft > 0)
            {
                float waterLeft = wateringCan.WaterLeft * 100;
                float fillAmount = Math.Max(maxWater - water, glassOfWater);
                if (waterLeft < fillAmount)
                {
                    AddWater(waterLeft, 0.65f);
                    wateringCan.WaterLeft = 0;
                }
                else
                {
                    wateringCan.WaterLeft -= (int)(fillAmount / 100.0);
                    AddWater(fillAmount, 0.65f);
                }
                Animations.AnimateDrinking(false);
            }
            else
            {
                player.doEmote(4);
                Game1.showRedMessage("Out of water");
            }
        }

        //Public access to add drink to body
        public void DrinkWaterSource()
        {
            AddWater(Math.Max(maxWater - water, Body.glassOfWater), 0.65f);
            Animations.AnimateDrinking(true);
        }

        //Public access to add drink to body
        //TODO: Take in a drink item and base amount added on stamina + health value
        public void DrinkBeverage()
        {
            AddWater(Body.glassOfWater * 2f, 0.65f);
        }

        //Add food to the stomach, handles negative and overflow values
        public void AddFood(float addedFoodAmount)
        {
            //How full is the player already
            float oldPercent = food / maxFood;

            float half = 0.5f;

            //If addedFoodAmount is negative, make it positive, then take the lesser of it or current food?(make sure you don't make food negative), times it by .5 then add to stomach
            //TODO: make sure that adding negative food/water works properly
            if (addedFoodAmount < 0.0)
            {
                AddStomach(Math.Min(-addedFoodAmount, food) * half, 0.0f);
            }

            food += addedFoodAmount;

            //If NoHungerAndThirst is enabled reset food when under 10%
            if (food < maxFood / 10.0 && RegressionMod.config.NoHungerAndThirst)
            {
                food = maxFood;
            }

            //If the current food is greater than max add half of the difference to stomach and set food to max
            if (food > maxFood)
            {
                AddStomach((food - maxFood) * half, 0.0f);
                food = maxFood;
            }
            //Else if it is less than 0 reduce stamina based on how far below 0 food is, storing the old stamina value and setting food to 0
            //TODO: understand what the math gives back
            else if (food < 0.0)
            {
                Game1.player.stamina = Math.Max(0.0f, Game1.player.stamina + (int)(food / maxFood * 200.0));
                lastStamina = Game1.player.stamina;
                food = 0.0f;
            }


            if (addedFoodAmount >= 0.0 || RegressionMod.config.NoHungerAndThirst)
                return;

            //Give the player a warning when food is at 25% and 0%
            Warn(oldPercent, (food / maxFood), new float[2]
            {
                0.0f,
                0.25f
            }, new string[2][]
            {
                RegressionMod.data.Food_None,
                RegressionMod.data.Food_Low
            }, false);
        }

        //Add water to the stomach, handles negative and overflow values
        public void AddWater(float addedWaterAmount, float conversionRatio = 0.65f)
        {
            //How full is the player already
            float oldPercent = water / maxWater;

            //If the amount of water is negative, invert and add half)
            if (addedWaterAmount < 0.0)
            {
                AddStomach(0.0f, Math.Min(-addedWaterAmount, water) * conversionRatio);
            }

            water += addedWaterAmount;


            //If NoHungerAndThirst is enabled reset water when under 10%
            if (water < maxWater / 10.0 && RegressionMod.config.NoHungerAndThirst)
            {
                water = maxWater;
            }

            //If current water is more than max then set too max
            //TODO: Check why this max check is different from food max check
            if (water > maxWater)
            {
                AddStomach(0.0f, water - maxWater);
                water = maxWater;
            }
            //Else if water is less than 0 then reduce health based on how far below 0 water is, then set water to 0
            else if (water < 0.0)
            {
                float waterToHealthMult = RegressionMod.config.Easymode ? 50f : 100f;
                Game1.player.health = Math.Max(0, Game1.player.health + (int)Math.Ceiling(water * waterToHealthMult / maxWater));
                water = 0.0f;
            }

            if (addedWaterAmount >= 0.0 || RegressionMod.config.NoHungerAndThirst)
                return;

            //Give the player a warning when water is at 25% and 0%
            Warn(oldPercent, water / maxWater, new float[2]
            {
                0.0f,
                0.25f
            }, new string[2][]
            {
                RegressionMod.data.Water_None,
                RegressionMod.data.Water_Low
            }, false);
        }

        public void AddStomach(float food, float water)
        {
            stomach[0] = Math.Max(stomach[0] + food, 0.0f);
            stomach[1] = Math.Max(stomach[1] + water, 0.0f);
        }

        //Move water from the stomach to the bladder
        public void AddBladder(float bladderAddAmount)
        {
            if (!RegressionMod.config.Wetting)
                return;

            //Percent full
            float oldPercent = (maxBladder - bladder) / maxBladder;

            bladder += bladderAddAmount;

            //Don't add more if already wetting
            if (isWetting)
                return;

            //If bladder is full and the player isn't wetting, messing, or in the fishing game then start wetting
            if (bladder >= maxBladder)
            {
                if (isWetting || isMessing || IsFishing())
                    return;
                //Involuntary and in underwear
                StartWetting(false, true);
            }
            else
            {
                //Make sure bladder is positive
                bladder = Math.Max(bladder, 0.0f);

                float newPercent = (maxBladder - bladder) / maxBladder;

                //As long as new fullness is not 0 there is a chance that the player isn't warned based on continence level
                if (newPercent > 0.0 && (bladderContinence / (4f * newPercent)) <= RegressionMod.rnd.NextDouble())
                    return;

                //Warn player when bladder is at 50%, 30%, and 10%
                Warn(oldPercent, newPercent, new float[3]
                {
                    0.1f,
                    0.3f,
                    0.5f
                }, new string[3][]
                {
                    RegressionMod.data.Bladder_Red,
                    RegressionMod.data.Bladder_Orange,
                    RegressionMod.data.Bladder_Yellow
                }, false);
            }
        }

        //Move food from the stomach to the bowels
        public void AddBowel(float value)
        {
            if (!RegressionMod.config.Messing)
                return;

            //Percent full
            float oldPercent = (maxBowels - bowels) / maxBowels;

            bowels += value;

            //Don't add if already messing
            if (isMessing)
                return;

            //If bowels is full and the player isn't wetting, messing, or in the fishing game then start messing
            if (bowels >= maxBowels)
            {
                if (isWetting || isMessing || IsFishing())
                    return;
                //Involuntary and in underwear
                StartMessing(false, true);
            }
            else
            {
                //Make sure bladder is positive
                bowels = Math.Max(bowels, 0.0f);

                float newPercent = (maxBowels - bowels) / maxBowels;

                //As long as new fullness is not 0 there is a chance that the player isn't warned based on continence level
                if (newPercent > 0.0 && (bowelContinence / (4f * newPercent)) <= RegressionMod.rnd.NextDouble())
                    return;

                //Warn player when bladder is at 30%, 10%, and 5%
                Warn(oldPercent, newPercent, new float[3]
                {
                    0.05f,
                    0.1f,
                    0.3f
                }, new string[3][]
                {
                    RegressionMod.data.Bowels_Red,
                    RegressionMod.data.Bowels_Orange,
                    RegressionMod.data.Bowels_Yellow
                }, false);
            }
        }

        //Warns the player when the new percentage goes below a threshold (if not sleeping)
        public void Warn(float oldPercent, float newPercent, float[] thresholds, string[][] msgs, bool write = false)
        {
            //Don't warn when sleeping
            if (sleeping)
                return;

            for (int i = 0; i < thresholds.Length; ++i)
            {
                //If the new percent brings the player below a threshold then warn them
                if (oldPercent > thresholds[i] && newPercent <= thresholds[i])
                {
                    if (write)
                    {
                        Animations.Write(msgs[i], this);
                        break;
                    }
                    Animations.Warn(msgs[i], this);
                    break;
                }
            }
        }

        //Starts the wetting proccess
        public void StartWetting(bool voluntary = false, bool inUnderwear = true)
        {
            if (!RegressionMod.config.Wetting)
                return;

            //If bladder is less than 10% then don't actually wet but play attempt animation
            if (bladder < maxBladder * 0.1f)
            {
                Animations.AnimatePeeAttempt(this, inUnderwear, Game1.currentLocation is FarmHouse);
                if (inUnderwear)
                    return;
                //Figure out
                Animations.HandleVillager(this, false, inUnderwear, false, true, 20, 3);
            }
            else
            {
                //If the wetting is non voluntary or bladder is at more than 50% lower continence
                if (!voluntary || bladder < maxBladder * 0.5)
                    ChangeBladderContinence(true, 0.01f);
                //Else raise continence
                else
                    ChangeBladderContinence(false, 0.01f);

                //Used for Wet function
                wettingUnderwear = inUnderwear;

                isWetting = true;

                Animations.AnimateWettingStart(this, voluntary, inUnderwear, Game1.currentLocation is FarmHouse);
            }
        }

        //Called by HandleTime while wetting until bladder empty
        public void Wet(float hours)
        {
            //Takes 2 min to empty a completely full bladder
            float amount = (maxBladder * hours * 30.0f);

            bladder -= amount;

            if (sleeping)
            {
                //With high continence you get up to pee in the night
                if (RegressionMod.rnd.NextDouble() < bladderContinence)
                {
                    peedToiletLastNight++;
                }
                else
                {
                    //float sleepOverflow = 
                    bottoms.AddPee(underwear.AddPee(amount));
                }
            }
            else if (wettingUnderwear)
            {
                //float overflow = 
                bottoms.AddPee(underwear.AddPee(amount));
            }

            //Keep wetting until bladder is empty
            if (bladder > 0.0)
                return;

            bladder = 0.0f;
            EndWetting();
        }

        //Ends wetting and checks to give overflow debuff
        public void EndWetting()
        {
            isWetting = false;

            Animations.AnimateWettingEnd(this);

            //If player is sleeping, ?villager?, didn't overflow into pants, or was on the toilet don't give overflow debuff
            if (sleeping 
                || Animations.HandleVillager(this, false, wettingUnderwear, bottoms.Wetness > 0.0, false, 20, 3) 
                || bottoms.Wetness <= 0.0 
                || !wettingUnderwear)
                return;

            HandlePeeOverflow(bottoms);
        }

        public void StartMessing(bool voluntary = false, bool inUnderwear = true)
        {
            if (!RegressionMod.config.Messing)
                return;
            if (bowels < maxBowels / 10.0)
            {
                Animations.AnimatePoopAttempt(this, inUnderwear, Game1.currentLocation is FarmHouse);
                if (inUnderwear)
                    return;
                Animations.HandleVillager(this, true, inUnderwear, false, true, 20, 3);
            }
            else
            {
                if (!voluntary || bowels < maxBowels * 0.5)
                    ChangeBowelContinence(true, 0.01f);
                else
                    ChangeBowelContinence(false, 0.01f);
                messingVoluntarily = voluntary;
                messingUnderwear = inUnderwear;
                isMessing = true;
                Animations.AnimateMessingStart(this, messingVoluntarily, messingUnderwear, Game1.currentLocation is FarmHouse);
            }
        }

        public void Mess(float hours)
        {
            float amount = (maxBowels * hours * 20.0f);
            bowels -= amount;
            if (sleeping)
            {
                //With high continence you get up to poo in the night
                messingVoluntarily = RegressionMod.rnd.NextDouble() < bowelContinence;
                if (messingVoluntarily)
                {
                    ++poopedToiletLastNight;
                }
                else
                {
                    //float sleepOverflow = 
                    bottoms.AddPoop(underwear.AddPoop(amount));
                }
            }
            else if (messingUnderwear)
            {
                //float overflow = 
                bottoms.AddPoop(underwear.AddPoop(amount));
            }
            if (bowels > 0.0)
                return;
            bowels = 0.0f;
            EndMessing();
        }

        public void EndMessing()
        {
            isMessing = false;
            Animations.AnimateMessingEnd();
            if (sleeping || (Animations.HandleVillager(this, true, messingUnderwear, bottoms.Messiness > 0.0, false, 20, 3) || bottoms.Messiness <= 0.0 || !messingUnderwear))
                return;
            HandlePoopOverflow(bottoms);
        }

        //Change to use +/- percents instead of extra bool
        public void ChangeBladderContinence(bool decrease = true, float percent = 0.01f)
        {
            if (decrease)
                percent = -percent;

            float oldBladderContinence = bladderContinence;

            bladderContinence += percent;
            bladderContinence = Math.Max(Math.Min(bladderContinence, 1f), 0.05f);
            maxBladder += percent * baseMaxBladder;


            maxBladder = Math.Max(Math.Min(maxBladder, baseMaxBladder), baseMaxBladder * 0.25f);
            if (!decrease)
                return;


            Warn(oldBladderContinence, bladderContinence, new float[4]
            {
                0.06f,
                0.2f,
                0.5f,
                0.8f
            }, new string[4][]
            {
                RegressionMod.data.Bladder_Continence_Min,
                RegressionMod.data.Bladder_Continence_Red,
                RegressionMod.data.Bladder_Continence_Orange,
                RegressionMod.data.Bladder_Continence_Yellow
            }, true);
        }

        public void ChangeBowelContinence(bool decrease = true, float percent = 0.01f)
        {
            if (decrease)
                percent = -percent;
            float bowelContinence = this.bowelContinence;
            this.bowelContinence += 2f * percent;
            this.bowelContinence = Math.Max(Math.Min(this.bowelContinence, 1f), 0.05f);
            maxBowels += percent * Body.baseMaxBowels;
            maxBowels = Math.Max(Math.Min(maxBowels, Body.baseMaxBowels), Body.baseMaxBowels * 0.25f);
            if (!decrease)
                return;
            Warn(bowelContinence, this.bowelContinence, new float[4]
            {
        0.06f,
        0.2f,
        0.5f,
        0.8f
            }, new string[4][]
            {
        RegressionMod.data.Bowel_Continence_Min,
        RegressionMod.data.Bowel_Continence_Red,
        RegressionMod.data.Bowel_Continence_Orange,
        RegressionMod.data.Bowel_Continence_Yellow
            }, true);
        }

        //Gives debuff based on overflow amount
        public void HandlePeeOverflow(Underwear bottoms)
        {
            Animations.Write(RegressionMod.data.Pee_Overflow, this);

            //Scale the defense debuff based on amount of overflow (1->10)
            float overflow = (bottoms.Wetness / bottoms.absorbency) * 10f;
            int defense = -Math.Max(
                Math.Min((int)overflow, 10) //Percentage of overflow times 10 (wetness at double absorbancy is 10)
                , 1);

            Buff buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, defense, 0, 15, "", "")
            {
                description = string.Format("{0} {1} Defense.", (object)Strings.RandString(RegressionMod.data.Debuff_Wet_Pants), (object)defense),
                millisecondsDuration = 1080000, // 18 minutes
                glow = bottoms.Messiness > 0.0 ? Color.Brown : Color.Yellow,
                sheetIndex = -1,
                which = 111
            };

            //If player already has wet overflow debuff remove and re-apply it
            if (Game1.buffsDisplay.hasBuff(buff.which))
                RemoveBuff(buff.which);

            Game1.buffsDisplay.addOtherBuff(buff);
        }

        public void HandlePoopOverflow(Underwear underwear)
        {
            Animations.Write(RegressionMod.data.Poop_Overflow, this);

            float overflow = underwear.Messiness / underwear.containment;
            //100% -> -3, 50% -> -2, 0% -> -1
            int speed = overflow >= 0.5 ? (overflow > 1.0 ? -3 : -2) : -1;

            Buff buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, speed, 0, 0, 15, "", "")
            {
                description = string.Format("{0} {1} Speed.", (object)Strings.RandString(RegressionMod.data.Debuff_Messy_Pants), (object)speed),
                millisecondsDuration = 1080000, // 18 minutes
                glow = Color.Brown,
                sheetIndex = -1,
                which = 222
            };
            
            //If player already has wet overflow debuff remove and re-apply it
            if (Game1.buffsDisplay.hasBuff(buff.which))
                RemoveBuff(buff.which);

            Game1.buffsDisplay.addOtherBuff(buff);
        }

        public void RemoveBuff(int which)
        {
            BuffsDisplay buffsDisplay = Game1.buffsDisplay;
            for (int index = buffsDisplay.otherBuffs.Count - 1; index >= 0; --index)
            {
                if (buffsDisplay.otherBuffs[index].which == which)
                {
                    buffsDisplay.otherBuffs[index].removeBuff();
                    buffsDisplay.otherBuffs.RemoveAt(index);
                    buffsDisplay.syncIcons();
                }
            }
        }

        //Called in the morning to make up for time passed over night
        public void HandleNight()
        {
            lastStamina = Game1.player.stamina;
            bottoms = new Underwear((int)UnderwearType.Bed);
            sleeping = true;
            if (bedtime <= 0)
                return;
            
            //Gets time slept in  hours, at least 4
            //time of day is in the morning ~600, bedtime is up to 2600 (2am)
            HandleTime((float)Math.Max(4, (Game1.timeOfDay + 2400 - bedtime) / 100));
        }

        public void HandleMorning()
        {
            //Refresh food and water in easymode
            if (RegressionMod.config.Easymode)
            {
                food = maxFood;
                water = maxWater;
            }

            //If no wetting or messing don't wet pants at night
            if (!RegressionMod.config.Wetting && !RegressionMod.config.Messing)
            {
                peedToiletLastNight = 0;
                poopedToiletLastNight = 0;
                sleeping = false;
                bottoms = new Underwear((int)UnderwearType.Pants);
            }
            else
            {
                if (!RegressionMod.config.Easymode)
                {
                    //Lower stamina regen if pants are soiled
                    if (bottoms.Messiness > 0.0 || bottoms.Wetness > Body.glassOfWater)
                    {
                        beddingDryTime = Game1.timeOfDay + 1500;
                        Game1.player.Stamina -= 100f;
                    }
                    else if (bottoms.Wetness > 0.0)
                    {
                        beddingDryTime = Game1.timeOfDay + 900;
                        Game1.player.Stamina -= 50f;
                    }
                    else
                        beddingDryTime = 0;
                }

                Animations.AnimateMorning(this);

                peedToiletLastNight = 0;
                poopedToiletLastNight = 0;

                sleeping = false;
                bottoms = new Underwear((int)UnderwearType.Pants);
            }
        }

        //Updates the body systems based on the amount of hours passed
        public void HandleTime(float hours)
        {
            HandleStamina();
            AddWater((float)(waterDay * hours / -24.0), 0.65f);
            AddFood((float)(foodDay * hours / -24.0));
            HandleStomach(hours);

            if (isWetting)
                Wet(hours);

            if (!isMessing)
                return;

            Mess(hours);
        }

        //Handles stamina, missing food/water lowers stamina
        public void HandleStamina()
        {
            //Difference between current stamina andd last stored stamina
            float staminaChange = Game1.player.stamina - lastStamina;

            if (staminaChange == 0.0)
                return;

            //If stamina has decreased since last time remove food and water
            //TODO: what are these numbers based on
            if (staminaChange < 0.0)
            {
                AddFood(staminaChange / 300f * maxFood);
                AddWater(staminaChange / 100f * maxWater, 0.05f);
            }

            //Update stored stamina amount
            lastStamina = Game1.player.stamina;
        }

        //Moves food and water from stomach to bowels/bladder based on hours
        public void HandleStomach(float hours)
        {
            float foodRate = (float)(foodDay / 3.0 * 0.5) * hours;
            float waterRate = Body.glassOfWater * 2f * hours;

            float foodToBowel = Math.Min(stomach[0], foodRate);
            float waterToBladder = Math.Min(stomach[1], waterRate);

            AddBowel(foodToBowel);
            AddBladder(waterToBladder);
            AddStomach(-foodToBowel, -waterToBladder);
        }

        //Switches to new underwear and returns old underwear
        public Underwear ChangeUnderwear(Underwear underwear)
        {
            Underwear oldUnderwear = this.underwear;
            this.underwear = underwear;

            //TODO: Make this take from clothes
            bottoms = new Underwear((int)UnderwearType.Pants);
           
            CleanPants();

            //RegressionMod.monitor.Log(underwear.name, StardewModdingAPI.LogLevel.Debug);
            Animations.Say(RegressionMod.data.Change, this);
            return oldUnderwear;
        }

        //Removes wet/messy debuffs
        public void CleanPants()
        {
            RemoveBuff(111);
            RemoveBuff(222);
        }

        //Is the player fishing (lots of bool checks)
        public bool IsFishing()
        {
            return Game1.player.CurrentTool is FishingRod currentTool && (currentTool.isCasting || currentTool.isTimingCast || (currentTool.isNibbling || currentTool.isReeling) || currentTool.castedButBobberStillInAir || currentTool.pullingOutOfWater);
        }

        //Debug Mode
        public void DecreaseFoodAndWater()
        {
            AddWater(maxWater / -10f, 0.65f);
            AddFood(maxFood / -10f);
            AddBladder(maxBladder / -10f);
            AddBowel(maxBowels / -10f);
        }

        //Debug Mode
        public void IncreaseEverything()
        {
            AddWater(maxWater - water, 0.65f);
            AddFood(maxFood - food);
            AddBladder(maxBladder / 4f);
            AddBowel(maxBowels / 4f);
        }
    }
}