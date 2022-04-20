using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class BookmakerHandicap
    {
        public Bookmaker Bookmaker { get; set; }

        public IList<HandicapCoeff> HandicapCoeffs { get; set; }

        public IList<HandicapCoeff> HandicapCoeffsInvert { get; set; }
    }
}
