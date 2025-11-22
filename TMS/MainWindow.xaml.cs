using System;
using System.Configuration;
using Microsoft.Data.SqlClient; // updated namespace
using System.Windows;

namespace TMS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TestDatabaseConnection();
        }

        private void TestDatabaseConnection()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["TMS_DB"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open(); // Try to connect
                    MessageBox.Show("Connection successful!", "DB Test", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message, "DB Test", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
