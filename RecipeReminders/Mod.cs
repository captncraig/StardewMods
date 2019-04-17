using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace RecipeReminders
{
    public class ModConfig
    {
        public int ExtraChancesIfUnknown { get; set; } = 3;
    }
    public class ModEntry : Mod
    {
        static ModConfig config;
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            var harmony = HarmonyInstance.Create("captncraig.stardew.mod.recipes");
            harmony.Patch(
                original: AccessTools.Method(typeof(TV), "getWeeklyRecipe"),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.getWeeklyRecipe))
            );
            helper.Events.GameLoop.DayStarted += DayStart;
        }

        private void DayStart(object sender, DayStartedEventArgs e)
        {
            var day = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
            if(Game1.stats.DaysPlayed < 5)
            {
                return;
            }
            if (day != "Wed" && day != "Sun")
            {
                return;
            }
            var recipe = getRecipe();
            if (!Game1.player.cookingRecipes.ContainsKey(recipe))
            {
                Game1.addHUDMessage(new HUDMessage($"Watch TV today to learn {recipe}", 1));
            }
        }

        private static int getWhichWeek()
        {
            int whichWeek = (int)(Game1.stats.DaysPlayed % 224 / 7);
            Dictionary<string, string> cookingRecipeChannel = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
            if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed"))
            {
                var candidates = new List<int>();
                for (var i = 1; i<=whichWeek; i++)
                {
                    var recipeName = cookingRecipeChannel[string.Concat(i)].Split(new char[] { '/' })[0];
                    candidates.Add(i);
                    if (!Game1.player.cookingRecipes.ContainsKey(recipeName))
                    {
                        for (var j = 0; j < config.ExtraChancesIfUnknown; j++)
                        {
                            candidates.Add(i);
                        }
                    }
                }
                Random r = new Random((int)(Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2));
                whichWeek = candidates[r.Next(candidates.Count)];
            }
            return whichWeek;
        }

        private static string getRecipe()
        {
            var ww = getWhichWeek();
            Dictionary<string, string> cookingRecipeChannel = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
            return cookingRecipeChannel[string.Concat(ww)].Split(new char[] { '/' })[0];
        }

        // copied directly from source. Just uses my getWhichWeek function.
        public static bool getWeeklyRecipe(ref string[] __result)
        {
            string str;
            string str1;
            string str2;
            string[] text = new string[2];
            int whichWeek = getWhichWeek();
            Dictionary<string, string> cookingRecipeChannel = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
            try
            {
                string recipeName = cookingRecipeChannel[string.Concat(whichWeek)].Split(new char[] { '/' })[0];
                text[0] = cookingRecipeChannel[string.Concat(whichWeek)].Split(new char[] { '/' })[1];
                if (!CraftingRecipe.cookingRecipes.ContainsKey(recipeName))
                {
                    string[] strArrays = text;
                    if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
                    {
                        str = (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel[string.Concat(whichWeek)].Split(new char[] { '/' })[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", recipeName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", recipeName));
                    }
                    else
                    {
                        str = (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel[string.Concat(whichWeek)].Split(new char[] { '/' })[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", cookingRecipeChannel[string.Concat(whichWeek)].Split(new char[] { '/' }).Last<string>()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", cookingRecipeChannel[string.Concat(whichWeek)].Split(new char[] { '/' }).Last<string>()));
                    }
                    strArrays[1] = str;
                }
                else
                {
                    string[] split = CraftingRecipe.cookingRecipes[recipeName].Split(new char[] { '/' });
                    string[] strArrays1 = text;
                    if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
                    {
                        str1 = (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel[string.Concat(whichWeek)].Split(new char[] { '/' })[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", recipeName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", recipeName));
                    }
                    else
                    {
                        str1 = (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel[string.Concat(whichWeek)].Split(new char[] { '/' })[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", split[(int)split.Length - 1]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", split[(int)split.Length - 1]));
                    }
                    strArrays1[1] = str1;
                }
                if (!Game1.player.cookingRecipes.ContainsKey(recipeName))
                {
                    Game1.player.cookingRecipes.Add(recipeName, 0);
                }
            }
            catch (Exception exception)
            {
                string recipeName = cookingRecipeChannel["1"].Split(new char[] { '/' })[0];
                text[0] = cookingRecipeChannel["1"].Split(new char[] { '/' })[1];
                string[] strArrays2 = text;
                if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
                {
                    str2 = (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel["1"].Split(new char[] { '/' })[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", recipeName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", recipeName));
                }
                else
                {
                    str2 = (Game1.player.cookingRecipes.ContainsKey(cookingRecipeChannel["1"].Split(new char[] { '/' })[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", cookingRecipeChannel["1"].Split(new char[] { '/' }).Last<string>()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", cookingRecipeChannel["1"].Split(new char[] { '/' }).Last<string>()));
                }
                strArrays2[1] = str2;
                if (!Game1.player.cookingRecipes.ContainsKey(recipeName))
                {
                    Game1.player.cookingRecipes.Add(recipeName, 0);
                }
            }
            __result = text;
            return false;
        }
    }

}
