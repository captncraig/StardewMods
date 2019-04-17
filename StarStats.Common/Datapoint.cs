using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarStats.Common
{
    public class Datapoint
    {
        public Timestamp Time;
        public string Metric;
        public string Tags; // key=value,key2=value pairs
        public double Value;
    }
}
