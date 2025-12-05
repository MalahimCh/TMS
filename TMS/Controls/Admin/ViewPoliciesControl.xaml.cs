using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class ViewPoliciesControl : UserControl
    {
        private readonly CancellationPolicyBL _policyBL;

        public ViewPoliciesControl()
        {
            InitializeComponent();
            _policyBL = new CancellationPolicyBL(new DAL.CancellationPolicyDAL());

            LoadPolicies();
        }

        private async void LoadPolicies()
        {
            List<CancellationPolicyDTO> policies = await _policyBL.GetAllPoliciesAsync();
            if (policies != null && policies.Count > 0)
            {
                dgPolicies.ItemsSource = policies;
            }
            else
            {
                dgPolicies.ItemsSource = null;
                MessageBox.Show("No policies found.");
            }
        }
    }
}
