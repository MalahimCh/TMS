using System;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class DeletePolicyControl : UserControl
    {
        private readonly CancellationPolicyBL _policyBL;
        private CancellationPolicyDTO _selectedPolicy;

        public DeletePolicyControl()
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

        private async void DeletePolicy_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPolicy == null)
            {
                MessageBox.Show("Please select a policy.");
                return;
            }

            bool deleted = await _policyBL.DeletePolicyAsync(_selectedPolicy.Id);
            MessageBox.Show(deleted ? "Policy deleted successfully!" : "Failed to delete policy.");
        }
    }
}
