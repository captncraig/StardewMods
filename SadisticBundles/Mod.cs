using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseBundles
{
    public class ModEntry : Mod, IAssetEditor
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Player.Warped += Warped;
        }

        private void Warped(object sender, WarpedEventArgs e)
        {
            // force add all junimo notes
            var cc = e.NewLocation as CommunityCenter;
            if (cc == null || !e.Player.IsMainPlayer) return;
            if (Game1.MasterPlayer.mailReceived.Contains("JojaMember") || cc.areAllAreasComplete())
            {
                return;
            }
            for (int i = 0; i < cc.areasComplete.Count; i++)
            {
                if (!cc.isJunimoNoteAtArea(i) && !cc.areasComplete[i])
                {
                    cc.addJunimoNoteViewportTarget(i);
                }
            }
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data/Bundles"))
            {
                return true;
            }

            return false;
        }
        // generated from spreadsheet https://docs.google.com/spreadsheets/d/1dLXiNOHzCFJL-q9IbMIO2Fv2Hk-PC5zklqHFYqOiUDA/edit?usp=sharing
        private Dictionary<string, string> mydata = new Dictionary<string, string>
        {
                {"Crafts Room/0", "Forage 1/O 789 1/16 50 4 18 50 4 20 50 4 22 50 4 396 50 4 398 50 4 402 50 4 406 50 4 408 50 4 410 50 4 412 50 4/0"},
                {"Crafts Room/1", "Forage 2/O 789 1/399 50 4 296 50 4 259 50 4 416 50 4 418 50 4 282 50 4 88 50 4 90 50 4/0"},
                {"Crafts Room/2", "Construction/O 789 1/388 999 0 388 999 0 388 999 0 709 500 0 390 999 0 92 999 0 771 500 0 334 40 0 330 200 0/0"},
                {"Crafts Room/3", "Forage 4/O 789 1/257 50 4 404 50 4 420 50 4 281 50 4 422 50 4 78 150 0/0"},
                {"Crafts Room/4", "TBD/O 789 1/472 1 0/0"},
                {"Crafts Room/5", "TBD/O 789 1/472 1 0/0"},
                {"Pantry/6", "Spring Crops/O 789 1/190 100 2 248 100 2 188 100 2 250 100 2 24 100 2 192 100 2 252 100 2 400 100 2 454 100 1 240 10 0/0"},
                {"Pantry/7", "Summer Crops/O 789 1/258 200 2 270 100 2/0"},
                {"Pantry/8", "Fall Crops/O 789 1/300 100 2 274 100 2/0"},
                {"Pantry/9", "Orchard/O 789 1/634 40 4 638 40 4/0"},
                {"Pantry/10", "Animal/O 789 1/174 50 4 182 50 4/0"},
                {"Pantry/11", "Artisan/O 789 1/424 50 4 426 50 4/0"},
                {"Bulletin Board/12", "Chef/O 789 1/730 10 1/0"},
                {"Bulletin Board/13", "Baker/O 789 1/234 100 0 220 100 0/0"},
                {"Bulletin Board/14", "Florist/O 789 1/597 75 2 591 75 2/0"},
                {"Bulletin Board/15", "Wizard/O 789 1/578 15 0 373 4 0/0"},
                {"Bulletin Board/16", "Colors/O 789 1/74 2 0 562 25 0/0"},
                {"Bulletin Board/17", "Brewer/O 789 1/395 200 0 348 100 4/0"},
                {"Boiler Room/18", "Metals/O 789 1/334 100 0 335 100 0/0"},
                {"Boiler Room/19", "Monsters/O 789 1/766 500 0 684 500 0/0"},
                {"Boiler Room/20", "Gems/O 789 1/80 25 0 86 25 0/0"},
                {"Boiler Room/21", "Crafter/O 789 1/599 25 0 621 25 0/0"},
                {"Boiler Room/22", "Friendship/O 789 1/789 5 4/0"},
                {"Boiler Room/23", "Archaeologist/O 789 1/99 1 0 104 1 0/0"},
                {"Fish Tank/24", "Easy/O 789 1/142 10 2/0"},
                //{"Fish Tank/25", "Medium/O 789 1/140 10 2/0"},
                //{"Fish Tank/26", "Difficult/O 789 1/699 10 2/0"},
                //{"Fish Tank/27", "Crab/O 789 1/715 10 0/0"},
                //{"Fish Tank/28", "Evasive/O 789 1/155 10 2/0"},
                //{"Fish Tank/29", "Legends/O 789 1/159 1 0/0"},
                {"Vault/30", "2,500g/O 220 3/-1 2500 2500/4"},
                {"Vault/31", "2,501g/O 220 3/-1 2500 2500/4"},
                {"Vault/32", "2,502g/O 220 3/-1 2500 2500/4"},
                {"Vault/33", "2,503g/O 220 3/-1 2500 2500/4"},
        };
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/Bundles"))
            {
                asset.AsDictionary<string, string>().ReplaceWith(mydata) ;
            }
        }
    }
}
