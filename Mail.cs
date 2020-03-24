using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace SM
{
    //Changed to follow wiki tutorial, much simpler but might not be able to give underwear this way
    public class Mail : IAssetEditor
    {
        public Mail() { }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\mail");
        }

        public void Edit<T>(IAssetData asset)
        {
            IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

            data["RegressionStart"] = "Dear @,^Welcome to town! Here are some veggies from the garden to tide you over while you move in! Also, sorry to be so forward, but living here might change you in ways you didn't expect. Just in case, I've also enclosed some... supplies. (You can buy more at Pierre's.) %item object 399 20 %% ^      <, Jodi";
        }

        //private static string nextLetterId;
        //private static string nextLetterText;
        //private static List<Item> nextLetterItems;
        //public static bool showingLetter;
        //private static string currentLetter;

        //public static void ShowLetter(Body b)
        //{
        //    if (Mail.nextLetterId != null && Mail.currentLetter == "robinWell")
        //    {
        //        Mail.showingLetter = true;
        //        if (Mail.nextLetterId == "jodi")
        //            b.lettersReceived.Add("jodi");
        //        Game1.activeClickableMenu = (IClickableMenu)new LetterViewerMenu(Strings.InsertVariables(Mail.nextLetterText, b, (Container)null), Mail.nextLetterId);
        //        Regression.events.Display.MenuChanged += Mail.OnMenuClosed;
        //    }
        //    else if (Game1.mailbox.Count > 0)
        //        Mail.currentLetter = Game1.mailbox[0];
        //    else
        //        Mail.currentLetter = "none";
        //}

        //private static void OnMenuClosed(object sender, MenuChangedEventArgs e)
        //{
        //    if (e.NewMenu == null)
        //    {
        //        Mail.nextLetterId = (string)null;
        //        Mail.currentLetter = Game1.mailbox.Count <= 0 ? "none" : Game1.mailbox[0];
        //        Regression.events.Display.MenuChanged -= Mail.OnMenuClosed;
        //        Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(Mail.nextLetterItems);
        //        Mail.showingLetter = false;
        //    }
        //}

        //public static void CheckMail(Body b)
        //{
        //    if (!b.lettersReceived.Contains("jodi"))
        //    {
        //        Mail.nextLetterId = "jodi";
        //        Mail.nextLetterText = "Dear @,^Welcome to town! Here are some veggies from the garden to tide you over while you move in! Also, sorry to be so forward, but living here might change you in ways you didn't expect. Just in case, I've also enclosed some... supplies. (You can buy more at Pierre's.)^      <, Jodi";
        //        if (Regression.config.Easymode)
        //            Mail.nextLetterItems = new List<Item>()
        //  {
        //    (Item) new StardewValley.Object(399, 20, false, -1, 0),
        //    (Item) new Underwear("pawprint diaper", 0.0f, 0.0f, 40)
        //  };
        //        else
        //            Mail.nextLetterItems = new List<Item>()
        //  {
        //    (Item) new StardewValley.Object(399, 20, false, -1, 0),
        //    (Item) new Underwear("lavender pullup", 0.0f, 0.0f, 40)
        //  };
        //    }
        //    else
        //        Mail.nextLetterId = (string)null;
        //    if (Mail.nextLetterId != null && !Game1.mailbox.Contains("robinWell"))
        //        Game1.mailbox.Add("robinWell");
        //    if (Game1.mailbox.Count > 0)
        //        Mail.currentLetter = Game1.mailbox[0];
        //    else
        //        Mail.currentLetter = "none";
        //}
    }
}