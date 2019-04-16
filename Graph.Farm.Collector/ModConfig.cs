using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph.Farm.Collector
{
    public class ModConfig
    {
        public string ListenAddress { get; set; } = "http://localhost:8989/";
        public string ViewRoot { get; set; } = "";
    }
}
