using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.Design_Patterns
{
    public interface ISeatLayoutStrategy
    {
        List<List<SeatModel>> GenerateLayout(List<SeatModel> seats);
    }

}
