using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class CoefficientsHandicap : TeamCoeffInfo
    {
        public IList<BookmakerHandicap> BookmakerHandicapCoeffs { get; set; }
    }
}
