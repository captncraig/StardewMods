using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data/mail")) return true;
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/mail"))
            {
                var dict = asset.AsDictionary<string, string>().Data;
                var pre = Helper.Translation.Get("rewardLetterPrefix").ToString();
                var post = Helper.Translation.Get("rewardLetterSuffix").ToString();
                foreach (var id in Game1.netWorldState.Value.BundleRewards.Keys)
                {
                    var key = $"rewardLetter{id}";
                    dict[key] = pre + Helper.Translation.Get(key).ToString()+post;
                }
            }
        }
    }
}
