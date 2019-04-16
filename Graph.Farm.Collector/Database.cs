using Dapper;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Graph.Farm.Collector
{
    public class Database
    {
        string fileName;

        public Database(string saveFolder, bool fast = false)
        {
            fileName = Path.Combine(saveFolder, "stats.db");
            if (!File.Exists(fileName))
            {
                SQLiteConnection.CreateFile(fileName);
            }
            if (!fast)
            {
                Migrate();
            }
        }

        private SQLiteConnection Open()
        {
            return new SQLiteConnection($"Data Source = {fileName}; Version = 3;").OpenAndReturn();
        }


        private void Migrate()
        {
            using (var db = Open())
            {
                db.Execute("CREATE TABLE IF NOT EXISTS metrics (timestamp INTEGER, metric TEXT, value REAL, tag0 TEXT, tag1 TEXT);");
            }
        }

        public void ClearAfter(int ts)
        {
            using (var db = Open())
            {
                string q = "DELETE FROM metrics WHERE timestamp >= @ts ";
                db.Execute(q, new { ts });
            }
        }

        public void Insert(IList<Datapoint> batch)
        {
            using (var db = Open())
            {
                string q = "INSERT INTO metrics (timestamp, metric, value, tag0, tag1) VALUES (@Timestamp, @Metric, @Value, @Tag0, @Tag1)";
                db.Execute(q, batch);
            }
        }

        public IDictionary<string, TimeValArrays> GetTimeSeries(string metric)
        {
            using (var db = Open())
            {
                string q = "SELECT timestamp, value, tag0, tag1 FROM metrics WHERE metric = @metric";
                return db.Query<MetricRow>(q, new { metric })
                    .GroupBy(x => x.Tags())
                    .ToDictionary(x => x.Key,
                        x => new TimeValArrays
                        {
                            T = x.Select(y => y.timestamp).ToArray(),
                            V = x.Select(y => y.value).ToArray(),
                        });
            }
        }

        public class TimeValArrays
        {
            public int[] T;
            public double[] V;
        }

        private class MetricRow
        {
            public int timestamp;
            public double value;
            public string tag0;
            public string tag1;

            public string Tags()
            {
                if ((tag0 ?? "") == "")
                {
                    return "";
                }
                return (tag1 ?? "") == "" ? tag0 : tag0 + "," + tag1;
            }
        }
    }

}
