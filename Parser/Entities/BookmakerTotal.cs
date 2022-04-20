using System.Collections.Generic;

namespace Parser.Entities
{
    public class BookmakerTotal
    {
        public Bookmaker Bookmaker { get; set; }

        public IList<TotalCoeff> TotalCoeffs { get; set; }
    }
}
