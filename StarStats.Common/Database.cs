using System.Collections.Generic;
using System.Linq;

namespace StarStats.Common
{
    public struct Timestamp
    {
        public uint Raw;

        public uint DayStart()
        {
            return Raw / 121;
        }
    }

    public class Database
    {
        public Timestamp Max;
        private IList<TimeSeries> Metrics;

        public Database()
        {
            Metrics = new List<TimeSeries>();
        }

        public void Insert(Datapoint dp) {
            var ts = Metrics.SingleOrDefault(x => x.Metric == dp.Metric && x.Tags == dp.Tags);
            if (ts == null)
            {
                ts = new TimeSeries
                {
                    Metric = dp.Metric,
                    Tags = dp.Tags,
                };
                Metrics.Add(ts);
            }
            ts.Add(dp.Time, dp.Value);
        }

        public IEnumerable<TimeSeries> Metric(string metric)
        {
            return Metrics.Where(x => x.Metric == metric);
        }
    }

    public class TimeSeries
    {

        public string Metric { get; set; } = "";
        public string Tags { get; set; } = "";
        public IList<Point> Data;

        public TimeSeries()
        {
            Data = new List<Point>();
        }

        public void Add(Timestamp t, double v)
        {
            if (Data.LastOrDefault().Value == v)
            {
                return;
            }
            Data.Add(new Point { Time = t, Value = v });
        }

        public IEnumerable<double> ToDaily(Timestamp max)
        {
            var days = Data.GroupBy(x => x.Time.DayStart()).ToDictionary(x => x.Key, x=> x.Last().Value);

            double last = 0;
            for(uint dayStart = 0; dayStart <= max.Raw; dayStart++)
            {
                if (days.ContainsKey(dayStart))
                {
                    last = days[dayStart];
                    yield return last;
                }
                else
                {
                    yield return last;
                }
            }
        }
    }

    public struct Point
    {
        public Timestamp Time;
        public double Value;
    }
}
