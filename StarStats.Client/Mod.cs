﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;


namespace StarStats.Client
{

    public class ModEntry : Mod
    {
        public static ModEntry Instance;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            helper.Events.GameLoop.SaveLoaded += SaveLoaded;
            helper.Events.GameLoop.TimeChanged += (o, e) => TimeChanged();
            helper.Events.GameLoop.DayStarted += (o, e) => DayStart();
            helper.Events.GameLoop.DayEnding += (o, e) => DayEnding();
        }

        private void DayEnding()
        {
            AddRaw(TimeStamp(), Game1.timeOfDay, "bedtime");
        }

   
        int lastTimeChange = -1;

        private void SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            db = new Database(Constants.CurrentSavePath);
            var ts = TimeStamp();
            db.ClearAfter(ts);
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
            Monitor.Log($"Time Changed to {SDate.Now()} {Game1.timeOfDay} {TimeStamp()}");
            if (ts == lastTimeChange)
            {
                return;
            }
            lastTimeChange = ts;
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
            Add(ts, (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, "time");
            Add(ts, p.deepestMineLevel, "minelevel");
            Add(ts, Game1.weatherIcon, "weather");
            foreach (var kvp in p.friendshipData.Pairs)
            {
                Add(ts, kvp.Value.Points, "friendship", kvp.Key);
                Add(ts, kvp.Value.GiftsThisWeek, "giftsweek", kvp.Key);
                Add(ts, kvp.Value.GiftsToday, "giftstoday", kvp.Key);
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

        private void Add(int ts, double val, string metric, string tag0 = null, string tag1 = null)
        {
            var key = metric + string.Join(",", tag0, tag1);
            if (!lastValues.ContainsKey(key) || lastValues[key] != val)
            {
                lastValues[key] = val;
                AddRaw(ts, val, metric, tag0, tag1);
            }
        }

        private void AddSkipZero(int ts, double val, string metric, string tag0 = null, string tag1 = null)
        {
            var key = metric + string.Join(",", tag0, tag1);
            if (val == 0 && !lastValues.ContainsKey(key)) return;
            Add(ts, val, metric, tag0, tag1);
        }

        private void AddRaw(int ts, double val, string metric, string tag0 = null, string tag1 = null)
        {
            var dp = new Datapoint
            {
                Timestamp = ts,
                Value = val,
                Metric = metric,
                Tag0 = tag0,
                Tag1 = tag1,
            };
            toSend.Add(dp);
        }

        private void Send()
        {
            var thisBatch = toSend;
            toSend = new List<Datapoint>();
            foreach (var dp in thisBatch)
            {
                Monitor.Log($"{dp.Timestamp} {dp.Metric} {dp.Tag0 ?? ""} {dp.Tag1 ?? ""} {dp.Value}", LogLevel.Warn);
            }
            Task.Run(() => db.Insert(thisBatch));
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
                    if (hd.state.Value == 1)
                    {
                        watered++;
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
            AddSkipZero(ts, watered, "objects", loc.Name, "watered");
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

    public class Datapoint
    {
        public int Timestamp { get; set; }
        public string Metric { get; set; }
        public string Tag0 { get; set; }
        public string Tag1 { get; set; }
        public double Value { get; set; }
    }
}
