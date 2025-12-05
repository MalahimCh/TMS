using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class AddPolicyControl : UserControl
    {
        private readonly CancellationPolicyBL _policyBL;

        public AddPolicyControl()
        {
            InitializeComponent();
            _policyBL = new CancellationPolicyBL(new DAL.CancellationPolicyDAL());
        }

        private async void AddPolicy_Click(object sender, RoutedEventArgs e)
        {
            // Validate refund percentage
            if (!int.TryParse(txtRefund.Text, out int refund) || refund < 0 || refund > 100)
            {
                MessageBox.Show("Enter a valid refund percentage (0-100).");
                return;
            }

            // Validate cutoff hours
            if (!int.TryParse(txtCutoffHours.Text, out int cutoff) || cutoff < 0)
            {
                MessageBox.Show("Enter a valid cutoff hours value.");
                return;
            }

            bool isActive = chkIsActive.IsChecked ?? false;

            try
            {

                bool added = await _policyBL.AddPolicyAsync(refund,cutoff,isActive);
                if (added)
                {
                    MessageBox.Show("Policy added successfully!");
                    txtRefund.Text = "";
                    txtCutoffHours.Text = "";
                    chkIsActive.IsChecked = true;
                }
                else
                {
                    MessageBox.Show("Failed to add policy.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
