using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.DTO;

namespace TMS.Design_Patterns
{
    // --------------------- ECONOMY ---------------------
    public class EconomySeatGenerator : ISeatGenerator
    {
        public List<SeatModel> GenerateSeats()
        {
            var seats = new List<SeatModel>();
            int totalRows = 11;
            int backRowSeats = 5;
            int seatNumber = 1;

            bool firstSeatReserved = false;

            // Main 11 rows, 2+2 layout
            for (int row = 0; row < totalRows; row++)
            {
                for (int col = 0; col < 4; col++) // 2+2
                {
                    seats.Add(new SeatModel
                    {
                        SeatNumber = seatNumber++,
                        IsSide = false,       // economy, no side premium
                        BunkType = null,      // not sleeper
                        Status = !firstSeatReserved ? "Reserved" : "Available"
                    });

                    firstSeatReserved = true; // mark only first seat reserved
                }
            }

            // Back row 5 seats
            for (int i = 0; i < backRowSeats; i++)
            {
                seats.Add(new SeatModel
                {
                    SeatNumber = seatNumber++,
                    IsSide = false,
                    BunkType = null,
                    Status = "Available"
                });
            }

            return seats; // total 49
        }
    }

    // --------------------- LUXURY ---------------------
    public class LuxurySeatGenerator : ISeatGenerator
    {
        public List<SeatModel> GenerateSeats()
        {
            var seats = new List<SeatModel>();
            int totalRows = 11;
            int seatNumber = 1;

            for (int row = 0; row < totalRows; row++)
            {
                for (int col = 0; col < 3; col++) // 1+2 layout
                {
                    seats.Add(new SeatModel
                    {
                        Status = (seatNumber == 1) ? "Reserved" : "Available",
                        SeatNumber = seatNumber++,
                        IsSide = (col == 0),
                        BunkType = null,
                    });
                }
            }



            return seats;
        }
    }

    // --------------------- SLEEPER ---------------------
    public class SleeperSeatGenerator : ISeatGenerator
    {
        public List<SeatModel> GenerateSeats()
        {
            var seats = new List<SeatModel>();
            int totalRows = 6;
            int seatNumber = 1;

            for (int row = 0; row < totalRows; row++)
            {
                // 2+1 layout, each seat has Lower and Upper bunks
                for (int col = 0; col < 3; col++)
                {
                    seats.Add(new SeatModel
                    {
                        SeatNumber = seatNumber++,
                        IsSide = (col == 2), // single side seat is costly
                        BunkType = "Lower",
                        Status = "Available",
                    });
                    seats.Add(new SeatModel
                    {
                        SeatNumber = seatNumber++,
                        IsSide = (col == 2),
                        BunkType = "Upper",
                        Status = "Available",
                    });
                }
            }

            return seats;
        }
    }

}
