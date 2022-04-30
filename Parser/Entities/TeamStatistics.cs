using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public class TeamStatistics
    {
        public string Name { get; set; }

        public int? MatchesAtHome { get; set; }

        public int? GoalsScoredHome { get; set; }

        public int? ShotsOnTargetHome { get; set; }

        public int? MissedGoalsHome { get; set; }

        public int? MissedOnTargetHome { get; set; }

        public int? MatchesAway { get; set; }

        public int? GoalsScoredAway { get; set; }

        public int? ShotsOnTargetAway { get; set; }

        public int? MissedGoalsAway { get; set; }

        public int? MissedOnTargetAway { get; set; }
    }
}
