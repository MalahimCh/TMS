using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Design_Patterns;
using static TMS.Design_Patterns.EconomySeatLayoutStrategy;

namespace TMS.Design_Patterns
{
    public static class SeatLayoutFactory
    {
        public static ISeatLayoutStrategy GetStrategy(string busType)
        {
            return busType switch
            {
                "Economy" => new EconomySeatLayoutStrategy(),
                "Luxury" => new LuxurySeatLayoutStrategy(),
                "Sleeper" => new SleeperSeatLayoutStrategy(),
                _ => throw new Exception("Unknown bus type")
            };
        }
    }
}

