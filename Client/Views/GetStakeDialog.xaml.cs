using System.Globalization;
using System.Windows;

namespace Project.Views
{
    /// <summary>
    /// Interaction logic for GetStakeDialog.xaml
    /// </summary>
    public partial class GetStakeDialog : Window
    {
        public double Result_Percentage = 0.8;
        public GetStakeDialog()
        {
            InitializeComponent();

            PercentBox.Text = Setting.Instance.percentageGetStake.ToString();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            PercentBox.Text = PercentBox.Text.Replace(",", ".");
            if (!double.TryParse(PercentBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out Result_Percentage))
            {
                MessageBox.Show("Please input correct percentage");
                return;
            }

            if (Result_Percentage <= 0 || Result_Percentage > 5)
            {
                MessageBox.Show("Percentage should be lower than 5, larger than 0!");
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
