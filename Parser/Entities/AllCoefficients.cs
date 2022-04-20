using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class AllCoefficients
    {
        public IList<CoefficientsWinDrawLose> CoefficientsWinDrawLose { get; set; }

        public IList<CoefficientsHandicap> CoefficientsHandicap { get; set; }

        public IList<CoefficientsTotal> CoefficientsTotal { get; set; }

        public List<CoefficientsWinDrawLose> CoefficientsWinDrawLoseBetBrain { get; set; }

        public List<CoefficientsHandicap> CoefficientsHandicapBetBrain { get; set; }

        public List<CoefficientsTotal> CoefficientsTotalBetBrain { get; set; }
    }
}
