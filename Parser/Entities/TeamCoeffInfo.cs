using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class TeamCoeffInfo
    {
        public DateTime Date { get; set; }

        public string TeamNameHome { get; set; }

        public string TeamNameGuest { get; set; }

        public string MatchUrl { get; set; }
    }
}
