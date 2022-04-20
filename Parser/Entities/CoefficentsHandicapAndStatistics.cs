using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class CoefficentsHandicapAndStatistics
    {
        public IList<CoefficientsHandicap> CoefficientsHandicaps { get; set; }

        public IList<TeamStatistics> TeamStatistics { get; set; }

    }
}
