using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class WinDrawLoseAndStatistics
    {
        public IList<CoefficientsWinDrawLose> CoefficientsWinDrawLose { get; set; }

        public IList<TeamStatistics> TeamStatistics { get; set; }
    }
}
