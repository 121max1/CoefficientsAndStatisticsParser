using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class CoefficeintsTotalAndStatistics
    {
        public IList<CoefficientsTotal> BookmakerTotals { get; set; }

        public IList<TeamStatistics> TeamStatistics { get; set; }
    }
}
