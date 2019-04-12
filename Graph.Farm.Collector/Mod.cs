using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Graph.Farm.Collector
{
    public class GlobalConfig
    {
        public const string defaultHost = "https://graph.farm";

        public string ApiToken { get; set; }
        public string AccountID { get; set; }

        public string Host { get; set; } = defaultHost;
     }

    public class ModEntry : Mod
    {
        private GlobalConfig config;
        private ApiClient api;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<GlobalConfig>();
            api = new ApiClient(config.Host);

            if(!setupAccount()) return;

            helper.Events.GameLoop.SaveLoaded += SaveLoaded;
            helper.Events.GameLoop.TimeChanged += (o, e) => TimeChanged();
            helper.Events.GameLoop.DayStarted += (o, e) => DayStart();
        }

        private bool setupAccount()
        {
            Monitor.Log($"Checking account status with {config.Host}");
            if (string.IsNullOrEmpty(config.ApiToken) != string.IsNullOrEmpty(config.AccountID))
            {
                Monitor.Log($"BAD CONFIG: ApiToken and AccountID must both be specified. No stats will be tracked this session.", LogLevel.Error);
                return false;
            } 
            if (string.IsNullOrEmpty(config.ApiToken))
            {
                Monitor.Log("No graph.farm account found. Creating one now.");

                try
                {
                    var acct = api.CreateAccount().Result;
                    Monitor.Log($"Account ID {acct.ID}, api key {acct.SecretToken}", LogLevel.Debug);
                }catch(Exception e)
                {
                    while (e.InnerException != null)
                    {
                        e = e.InnerException;
                    }
                    Monitor.Log($"ERROR REGISTERING ACCOUNT: {e.Message}", LogLevel.Error);
                    return false;
                }
            }

            return true;
        }

        IDictionary<string, double> lastValues;
        int lastTimeChange = -1;
        IList<Datapoint> toSend;

        private void SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // TODO: Register Game
            // TODO: Clear data from current time on
            lastValues = new Dictionary<string, double>();
            toSend = new List<Datapoint>();
            lastTimeChange = -1;
        }

        private void DayStart()
        {
            TimeChanged();
        }

        private void TimeChanged()
        {
            var ts = TimeStamp();
            if (ts == lastTimeChange)
            {
                return;
            }
            lastTimeChange = ts;
            Monitor.Log($"Time Changed to {SDate.Now()} {Game1.timeOfDay} {TimeStamp()}");
            var p = Game1.player;

            Add(ts, p.stats.stepsTaken, "stepstaken");
            Add(ts, p.Money, "money");
            Add(ts, p.totalMoneyEarned, "earnings");
            Add(ts, p.stamina, "stamina");
            Add(ts, p.health, "health");
            Add(ts, (float)Game1.dailyLuck, "luck");
            Add(ts, p.experiencePoints[0], "skill", "farming");
            Add(ts, p.experiencePoints[1], "skill", "fishing");
            Add(ts, p.experiencePoints[2], "skill", "foraging");
            Add(ts, p.experiencePoints[3], "skill", "mining");
            Add(ts, p.experiencePoints[4], "skill", "combat");
            Add(ts, (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, "realtime");
            Add(ts, ts, "time");

            foreach (var kvp in p.friendshipData.Pairs)
            {
                Add(ts, kvp.Value.Points, "friendship", kvp.Key);
            }
            foreach (var kvp in p.basicShipped)
            {
                var obj = new StardewValley.Object(kvp.Key, 1);
                Add(ts, kvp.Value, "shipped", obj.Name);
            }

            foreach (var loc in Game1.locations)
            {
                locationStats(loc, ts);
            }

            // TODO: do this at end of day
            // foreach (var grp in Game1.getFarm().shippingBin.GroupBy(x => x.Name))
            // {
            //     // TODO: account for quality
            //     var total = grp.Sum(x => x.Stack);
            //     Add(ts, total, "shippingBin", grp.Key);
            // }
            Send();
        }

        public static int TimeStamp()
        {
            var date = SDate.Now();
            var datePart = 121 * (date.DaysSinceStart - 1);
            // 10 minute increments starting from 600. So Subtract 600 and divide by 10.
            var timePart = (Game1.timeOfDay - 600) / 10;
            // Now its 0 1 2 3 4 5 10 11 12 13 14 15 20 21 ....
            var tens = (int)Math.Floor((double)timePart / 10);
            timePart -= 4 * tens;
            return datePart + timePart;
        }

        private void Add(int ts, double val, string metric, params string[] tags)
        {
            var key = metric + string.Join(",", tags);
            if (!lastValues.ContainsKey(key) || lastValues[key] != val)
            {
                lastValues[key] = val;
                AddRaw(ts, val, metric, tags);
            }
        }

        private void AddSkipZero(int ts, double val, string metric, params string[] tags)
        {
            var key = metric + string.Join(",", tags);
            if (val == 0 && !lastValues.ContainsKey(key)) return;
            Add(ts, val, metric, tags);
        }

        private void AddRaw(int ts, double val, string metric, params string[] tags)
        {
            var dp = new Datapoint
            {
                Timestamp = ts,
                Value = val,
                Metric = metric,
                Tags = tags,
            };
            toSend.Add(dp);
        }

        private void Send()
        {
            var thisBatch = toSend;
            toSend = new List<Datapoint>();
            foreach (var dp in thisBatch)
            {
                Monitor.Log($"{dp.Timestamp} {dp.Metric} {string.Join(",", dp.Tags)} {dp.Value}", LogLevel.Warn);
            }

        }

        private void locationStats(GameLocation loc, int ts)
        {
            var watered = 0;
            var grass = 0;
            var stumps = 0;
            var trees = 0;
            var saplings = 0;
            var hoedirt = 0;
            var crops = 0;
            var dead = 0;
            var weeds = 0;
            var stone = 0;
            var artifacts = 0;
            var twigs = 0;
            var forage = 0;

            if (!loc.IsOutdoors)
            {
                return;
            }
            foreach (var tf in loc.terrainFeatures.Values)
            {
                if (tf is Grass)
                {
                    grass++;
                    continue;
                }
                if (tf is Tree)
                {
                    var tree = tf as Tree;
                    if (tree.stump.Value)
                    {
                        stumps++;
                        continue;
                    }
                    if (tree.growthStage.Value >= 5)
                    {
                        trees++;
                        continue;
                    }
                    saplings++;
                    continue;
                }
                if (tf is HoeDirt)
                {
                    var hd = tf as HoeDirt;
                    if (hd.state.Value == 1)
                    {
                        watered++;
                    }
                    if (hd.crop == null)
                    {
                        hoedirt++;
                        continue;
                    }
                    if (hd.crop.dead.Value)
                    {
                        dead++;
                        continue;
                    }
                    crops++;
                    continue;
                }
            }
            foreach (var obj in loc.Objects.Values)
            {
                if (obj.Name == "Weeds") {
                    weeds++;
                    continue;
                }
                if (obj.Name == "Stone")
                {
                    stone++;
                    continue;
                }
                if (obj.Name == "Twig")
                {
                    twigs++;
                    continue;
                }
                if (obj.Name == "Artifact Spot")
                {
                    artifacts++;
                    continue;
                }
                else if (obj.isForage(loc))
                {
                    forage++;
                    continue;
                }
            }
            if (loc is StardewValley.Farm)
            {
                // todo: add greenhouse
                Add(ts, watered, "location_watered", loc.Name);
            }
            // todo: big crops, big stumps, boulders, meteors
            AddSkipZero(ts, grass, "objects", loc.Name, "grass");
            AddSkipZero(ts,stumps, "objects", loc.Name, "stumps");
            AddSkipZero(ts,trees, "objects", loc.Name, "trees");
            AddSkipZero(ts,saplings, "objects", loc.Name, "saplings");
            AddSkipZero(ts,hoedirt, "objects", loc.Name,"hoedirt");
            AddSkipZero(ts,crops, "objects", loc.Name, "crops");
            AddSkipZero(ts,dead, "objects", loc.Name, "deadcrops");
            AddSkipZero(ts,weeds, "objects", loc.Name, "weeds");
            AddSkipZero(ts,stone, "objects", loc.Name, "stone");
            AddSkipZero(ts,artifacts, "objects", loc.Name, "artifacts");
            AddSkipZero(ts,twigs, "objects", loc.Name, "twigs");
            AddSkipZero(ts,forage, "objects", loc.Name, "forage");
            AddSkipZero(ts, loc.debris.Count, "objects", loc.Name, "debris");
        }
    }

    internal class Datapoint
    {
        public int Timestamp { get; set; }
        public string Metric { get; set; }
        public string[] Tags { get; set; }
        public double Value { get; set; }
    }
}
