using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class CoefficientsPerBookmakercs
    {
        public Bookmaker Bookmaker { get; set; }

        public string Win { get; set; }

        public string Draw { get; set; }

        public string Lose { get; set; }
    }
}
