using System.Windows;

namespace TMS.Controls.Customer
{
    public partial class GenderSelectionWindow : Window
    {
        public string? SelectedGender { get; private set; }

        public GenderSelectionWindow()
        {
            InitializeComponent();

            MaleButton.Click += (s, e) => { SelectedGender = "Male"; DialogResult = true; };
            FemaleButton.Click += (s, e) => { SelectedGender = "Female"; DialogResult = true; };
        }
    }
}
