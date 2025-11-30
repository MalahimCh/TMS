using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.Design_Patterns
{
    public class EconomySeatLayoutStrategy : ISeatLayoutStrategy
    {
        public List<List<SeatModel>> GenerateLayout(List<SeatModel> seats)
        {
            var layout = new List<List<SeatModel>>();
            int index = 0;

            // 11 rows of 2 + 2
            for (int i = 0; i < 11; i++)
            {
                var row = new List<SeatModel>();

                if (index < seats.Count) row.Add(seats[index++]);
                if (index < seats.Count) row.Add(seats[index++]);

                row.Add(null); // aisle

                if (index < seats.Count) row.Add(seats[index++]);
                if (index < seats.Count) row.Add(seats[index++]);

                layout.Add(row);
            }

            // Last row (5 seats)
            var lastRow = new List<SeatModel>();
            for (int i = 0; i < 5; i++)
                if (index < seats.Count) lastRow.Add(seats[index++]);

            layout.Add(lastRow);

            return layout;
        }

    }

    public class LuxurySeatLayoutStrategy : ISeatLayoutStrategy
    {
        public List<List<SeatModel>> GenerateLayout(List<SeatModel> seats)
        {
            var layout = new List<List<SeatModel>>();
            int index = 0;

            for (int i = 0; i < 11; i++)
            {
                var row = new List<SeatModel>();

                // left side (single seat)
                if (index < seats.Count)
                {
                    seats[index].IsSide = true;
                    row.Add(seats[index++]);
                }

                row.Add(null); // aisle gap

                // right side (two seats)
                if (index < seats.Count) row.Add(seats[index++]);
                if (index < seats.Count) row.Add(seats[index++]);

                layout.Add(row);
            }

            return layout;
        }
    }

    public class SleeperSeatLayoutStrategy : ISeatLayoutStrategy
    {
        public List<List<SeatModel>> GenerateLayout(List<SeatModel> seats)
        {
            var layout = new List<List<SeatModel>>();
            int index = 0;

            // Each "block" is 6 seats: L,U, L,U, L(side), U(side)
            while (index < seats.Count)
            {
                var lowerRow = new List<SeatModel>();
                var upperRow = new List<SeatModel>();

                // LEFT BLOCK (2 seats)
                if (index < seats.Count) lowerRow.Add(seats[index++]); // L
                if (index < seats.Count) upperRow.Add(seats[index++]); // U

                // MIDDLE BLOCK (2 seats)
                if (index < seats.Count) lowerRow.Add(seats[index++]); // L
                if (index < seats.Count) upperRow.Add(seats[index++]); // U

                // Add aisle
                lowerRow.Add(null);
                upperRow.Add(null);

                // SIDE BLOCK (2 seats)
                if (index < seats.Count) lowerRow.Add(seats[index++]); // L(side)
                if (index < seats.Count) upperRow.Add(seats[index++]); // U(side)

                layout.Add(lowerRow);
                layout.Add(upperRow);
            }

            return layout;
        }
    }

}
