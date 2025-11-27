using Microsoft.Data.SqlClient;
using System.Configuration;

namespace TMS.DAL
{
    public class DBConnection
    {
        public string ConnectionString { get; }

        public DBConnection()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["TMS"].ConnectionString;
        }
    }
}