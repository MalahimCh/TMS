using System;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class UpdatePolicyControl : UserControl
    {
        private readonly CancellationPolicyBL _policyBL;
        private CancellationPolicyDTO _selectedPolicy;

        public UpdatePolicyControl()
        {
            InitializeComponent();
            _policyBL = new CancellationPolicyBL(new DAL.CancellationPolicyDAL());
            LoadPolicies();
        }

        private async void LoadPolicies()
        {
            var policies = await _policyBL.GetAllPoliciesAsync();
            cmbPolicies.ItemsSource = policies;
            cmbPolicies.DisplayMemberPath = "Id";
        }

        private void CmbPolicies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPolicy = cmbPolicies.SelectedItem as CancellationPolicyDTO;
            if (_selectedPolicy != null)
            {
                txtRefund.Text = _selectedPolicy.RefundPercentage.ToString();
                txtCutoffHours.Text = _selectedPolicy.CutoffHoursBeforeDeparture.ToString();
                chkIsActive.IsChecked = _selectedPolicy.IsActive;
            }
        }

        private async void UpdatePolicy_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPolicy == null)
            {
                MessageBox.Show("Please select a policy.");
                return;
            }

            if (!int.TryParse(txtRefund.Text, out int refund) || refund < 0 || refund > 100)
            {
                MessageBox.Show("Enter a valid refund percentage (0-100).");
                return;
            }

            if (!int.TryParse(txtCutoffHours.Text, out int cutoff) || cutoff < 0)
            {
                MessageBox.Show("Enter a valid cutoff hours value.");
                return;
            }

            _selectedPolicy.RefundPercentage = refund;
            _selectedPolicy.CutoffHoursBeforeDeparture = cutoff;
            _selectedPolicy.IsActive = chkIsActive.IsChecked ?? false;

            bool updated = await _policyBL.UpdatePolicyAsync(_selectedPolicy);
            MessageBox.Show(updated ? "Policy updated successfully!" : "Failed to update policy.");
        }
    }
}
