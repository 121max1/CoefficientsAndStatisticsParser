using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class BetonMatchJsonEntity
    {
        public string @Content { get; set; }

        public string @Type { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public List<BetonMatchJsonEntityTeam> Competitor { get; set; }

        public DateTime StartDate { get; set; }

        public BetonMatchJsonEntityLocation Location { get; set; }

    }
}
