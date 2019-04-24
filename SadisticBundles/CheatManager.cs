using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace SadisticBundles
{
    class CheatManager : IAssetEditor
    {

        private readonly IModHelper Helper;
        private readonly IMonitor Monitor;

        public CheatManager(IModHelper helper, IMonitor monitor)
        {
            Helper = helper;
            Monitor = monitor;
            Helper.Events.GameLoop.DayEnding += DayEnding;
            Helper.Events.Display.MenuChanged += MenuChanged;
        }

        private void MenuChanged(object sender, MenuChangedEventArgs e)
        {
            // robin buildings all half price
            // todo: make sure robin not wizard
            var carpenter = e.NewMenu as CarpenterMenu;
            if (carpenter != null && bundleDone(BRobinHalf))
            {
                var info = Helper.Reflection.GetField<List<BluePrint>>(carpenter, "blueprints");
                foreach(var print in info.GetValue())
                {
                    foreach(var ing in print.itemsRequired.ToList())
                    {
                        print.itemsRequired[ing.Key] /= 2;
                    }
                }
                carpenter.setNewActiveBlueprint();
            }
        }

        private bool bundleDone(int id)
        {
            return Game1.netWorldState.Value.Bundles[id].All(x => x);
        }

        private string rewardMail(int id)
        {
            return $"rewardLetter{id}";
        }

        const int BForage5Perk = 0;
        const int BForage10Perk = 1;
        const int BRobinHalf = 2;

        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            var p = Game1.MasterPlayer;
            // todo: find a way to make level 5 choice still govern level 10 choices availible if 
            // both perks enabled before player hits 10. Probably remember initial choice and remove the opposite one
            // right before the level up menu shows, and put it back right after.
            checkProfession(p, BForage5Perk, p.ForagingLevel,5, Farmer.gatherer, Farmer.forester);
            checkProfession(p, BForage10Perk, p.ForagingLevel, 10, Farmer.lumberjack, Farmer.botanist, Farmer.tapper, Farmer.tracker);

            foreach(var b in new int[]
            {
                BRobinHalf,
            })
            {
                if (!p.hasOrWillReceiveMail(rewardMail(b)))
                {
                    p.mailForTomorrow.Add(rewardMail(b));
                }
            }
        }

        private void checkProfession(Farmer p, int bundle, int level,int required, params int[] choices)
        {
            var mail = rewardMail(bundle);
            if (bundleDone(bundle) && level >= required && !p.hasOrWillReceiveMail(mail))
            {
                foreach(var choice in choices)
                {
                    if (!p.professions.Contains(choice))
                    {
                        p.professions.Add(choice);
                    }
                }
                p.mailForTomorrow.Add(mail);
            }
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            // mail is always safe, since we are just adding new fields.
            if (asset.AssetNameEquals("Data/mail")) return true;
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/mail"))
            {
                var dict = asset.AsDictionary<string, string>().Data;
                // all reward mails
                var pre = Helper.Translation.Get("rewardLetterPrefix").ToString();
                var post = Helper.Translation.Get("rewardLetterSuffix").ToString();
                foreach (var id in Game1.netWorldState.Value.BundleRewards.Keys)
                {
                    var key = rewardMail(id);
                    dict[key] = pre + Helper.Translation.Get(key).ToString() + post;
                }
            }
        }
    }
}
