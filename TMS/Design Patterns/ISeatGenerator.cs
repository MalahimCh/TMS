using Microsoft.Data.SqlClient;
using TMS.DTO;

namespace TMS.Design_Patterns
{
    public interface ISeatGenerator
    {
        List<SeatModel> GenerateSeats();
    }


  
}
