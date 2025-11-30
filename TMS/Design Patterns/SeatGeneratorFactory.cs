using TMS.Design_Patterns;
namespace TMS.Design_Patterns
{
    public static class SeatGeneratorFactory
    {
        public static ISeatGenerator GetGenerator(string busType)
        {
            return busType.ToLower() switch
            {
                "economy" => new EconomySeatGenerator(),
                "luxury" => new LuxurySeatGenerator(),
                "sleeper" => new SleeperSeatGenerator(),
                _ => throw new ArgumentException("Unknown bus type")
            };
        }
    }
}