using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;

namespace MouseBundles
{
    public class ModEntry : Mod, IAssetEditor
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += FixBundles;
            helper.Events.Player.Warped += Warped;
            helper.Events.Input.ButtonPressed += ButtonPressed;
        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var cc = Game1.currentLocation as CommunityCenter;
            if (cc == null) return;
            if (e.Cursor.GrabTile.X == buyUpgradesPos.X && e.Cursor.GrabTile.Y == buyUpgradesPos.Y && e.Button.IsActionButton())
            {
                Helper.Input.Suppress(e.Button);
            }
            else
            {
                return;
            }
            Monitor.Log("DO IT!");
        }

        private void FixBundles(object sender, SaveLoadedEventArgs e)
        {
            // todo: smarter merging on upgrades
            var bundles = Game1.netWorldState.Value.Bundles;
            var rewards = Game1.netWorldState.Value.BundleRewards;
            if (bundles.Count() != mydata.Count)
            {
                bundles.Clear();
                rewards.Clear();
                foreach (var kvp in mydata)
                {
                    var id = int.Parse(kvp.Key.Split('/')[1]);
                    var ings = kvp.Value.Split('/')[2].Split(' ');
                    var count = ings.Count() / 3;
                    bundles[id] = new bool[count];
                    if (count > 1)
                    {
                        bundles[id][0] = true;
                    }
                    rewards[id] = false;
                }
            }
            var newCount = bundles.Count();
        }
        Point buyUpgradesPos = new Point(32, 15);

        private void Warped(object sender, WarpedEventArgs e)
        {
            // force add all junimo notes
            var cc = e.NewLocation as CommunityCenter;
            if (cc == null || !e.Player.IsMainPlayer) return;
            if (Game1.MasterPlayer.mailReceived.Contains("JojaMember") || cc.areAllAreasComplete() || !Game1.MasterPlayer.hasOrWillReceiveMail("canReadJunimoText"))
            {
                return;
            }
            for (int i = 0; i < cc.areasComplete.Count; i++)
            {
                if (!cc.isJunimoNoteAtArea(i) && !cc.areasComplete[i])
                {
                    cc.addJunimoNote(i);
                }
            }
            // add master note
            var position = buyUpgradesPos;
            var layer = cc.map.GetLayer("Buildings");
            var frames = new int[] { 1825, 1826, 1827, 1828, 1829, 1830, 1831, 1832, 1833, 1833, 1833, 1833, 1833, 1833, 1833, 1833, 1833, 1833, 1832, 1824 };
            StaticTile[] tileFrames = frames.Select(x => new StaticTile(layer, cc.map.TileSheets[0], BlendMode.Alpha, x)).ToArray();
            layer.Tiles[position.X, position.Y] = new AnimatedTile(layer, tileFrames, 70);
            Game1.currentLightSources.Add(new LightSource(4, new Vector2(position.X * 64, position.Y * 64), 1f));
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
{"Crafts Room/0", "Basic Forage/O 434 1/16 50 4 18 50 4 20 50 4 22 50 4 396 50 4 398 50 4 402 50 4 406 50 4 408 50 4 410 50 4 412 50 4 414 50 4/0"},
{"Crafts Room/1", "Rare Forage/O 434 1/399 50 4 296 50 4 259 50 4 416 50 4 418 50 4 283 50 4 88 50 4 90 50 4/0"},
{"Crafts Room/2", "Construction/O 434 1/388 999 0 388 999 0 388 999 0 709 500 0 390 999 0 390 999 0 771 999 0 92 999 0 330 300 0 334 50 0/0"},
{"Crafts Room/3", "Dank Forage/O 434 1/257 50 4 404 50 4 420 50 4 281 50 4 422 50 4 78 150 0/0"},
{"Crafts Room/4", "Achiever's/O 434 1/789 1 4 458 1 4 204 1 4 113 1 4 709 1 4 580 1 4 93 1 4 167 1 4 802 1 4 243 1 4 125 1 4/0"},
{"Crafts Room/5", "TBD/O 434 1/388 999 0/0"},
{"Pantry/6", "Spring Crops/O 434 1/190 100 2 433 100 2 248 100 2 188 100 2 250 100 2 24 200 2 192 100 2 252 100 2 400 100 2 454 100 2 240 15 0/0"},
{"Pantry/7", "Summer Crops/O 434 1/258 150 2 270 150 2 304 200 2 260 100 2 254 100 2 264 100 2 266 100 2 268 100 2 256 100 2 262 150 2/0"},
{"Pantry/8", "Fall Crops/O 434 1/300 100 2 274 100 2 284 150 2 278 100 2 282 200 2 272 200 2 398 150 2 276 100 2 280 150 2 417 100 2/0"},
{"Pantry/9", "Orchard/O 434 1/634 60 4 613 60 4 635 60 4 636 60 4 637 60 4 638 60 4 726 50 0 725 50 0 724 50 0 309 50 0 310 50 0 311 50 0/0"},
{"Pantry/10", "Animal/O 434 1/174 50 4 182 50 4 186 50 4 438 50 4 430 50 4 440 50 4 442 50 4 305 50 4 444 30 4 446 15 4/0"},
{"Pantry/11", "Artisan/O 434 1/424 50 4 426 50 4 306 50 2 307 50 2 308 50 2 428 50 0 340 100 0 342 100 0 344 100 0/0"},
{"Bulletin Board/12", "Chef/O 434 1/388 999 0/0"},
{"Bulletin Board/13", "Baker/O 434 1/234 25 0 220 25 0 221 25 0 608 25 0 222 25 0 203 25 0 216 25 0 223 25 0 611 25 0 651 25 0 731 25 0 206 25 0/0"},
{"Bulletin Board/14", "Florist/O 434 1/597 100 2 591 100 2 376 100 2 593 100 2 421 100 2 595 100 2 458 20 0/0"},
{"Bulletin Board/15", "Wizard/O 434 1/388 999 0/0"},
{"Bulletin Board/16", "Colors/O 434 1/388 999 0/0"},
{"Bulletin Board/34", "Color2s/O 434 1/388 999 0/0"},
{"Bulletin Board/17", "Liquor Cabinet/O 434 1/348 100 4 303 100 4 346 100 4 459 100 4 350 100 0 395 100 0 432 50 0 247 50 0 772 50 0 773 15 0 349 40 0 351 40 0/0"},
{"Boiler Room/18", "Metals/O 434 1/388 999 0/0"},
{"Boiler Room/19", "Monsters/O 434 1/684 200 0 766 200 0 767 200 0 768 100 0 769 100 0 437 1 0 413 1 0 680 1 0 439 1 0/0"},
{"Boiler Room/20", "Gems/O 434 1/388 999 0/0"},
{"Boiler Room/21", "Crafter/O 434 1/388 999 0/0"},
{"Boiler Room/22", "TBD2/O 434 1/388 999 0/0"},
{"Boiler Room/23", "Archaeologist/O 434 1/388 999 0/0"},
{"Fish Tank/24", "Easy/O 434 1/142 15 2 147 15 2 137 15 2 129 15 2 131 15 2 145 15 2 702 15 2 141 15 2 132 15 2 150 15 2 154 15 2 138 15 2/0"},
{"Fish Tank/25", "Medium/O 434 1/140 15 2 706 15 2 700 15 2 136 15 2 139 15 2 156 15 2 701 15 2 734 15 1 708 15 2 796 15 2 146 15 2 144 15 2/0"},
{"Fish Tank/26", "Difficult/O 434 1/699 15 2 705 15 2 164 15 1 158 15 2 130 15 2 148 15 2 143 15 2 151 15 2 698 15 2 704 15 2 128 15 2 795 15 2/0"},
{"Fish Tank/27", "Crab/O 434 1/715 15 0 372 15 4 716 15 0 717 15 0 718 15 4 719 15 4 720 15 0 721 15 0 722 15 0 723 15 4 152 25 0 153 25 0/0"},
{"Fish Tank/28", "Evasive/O 434 1/155 10 2 161 10 2 707 10 2 165 10 2 162 10 2 149 10 2/0"},
{"Fish Tank/29", "Legends/O 434 1/159 1 0 160 1 0 163 1 0 775 1 0 682 1 0 798 10 1 799 10 1 800 10 1/0"},
{"Vault/30", "2,500g/O 434 1/-1 25000 25000/0"},
{"Vault/31", "5,000g/O 434 1/-1 25000 25001/0"},
{"Vault/32", "10,000g/O 434 1/-1 25000 25002/0"},
{"Vault/33", "25,000g/O 434 1/-1 25000 25003/0"},
        };



        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/Bundles"))
            {
                asset.AsDictionary<string, string>().ReplaceWith(mydata);
            }
        }
    }

}
