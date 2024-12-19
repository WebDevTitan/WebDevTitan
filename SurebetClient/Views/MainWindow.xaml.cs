using Newtonsoft.Json;
using Project.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class Data
    {
        public double temp;
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (DataContext is ICloseable)
                {
                    (DataContext as ICloseable).RequestClose += (sender, args) => Close();
                    (DataContext as ICloseable).RequestMinimize += (sender, args) => { this.WindowState = WindowState.Minimized; };
                    (DataContext as ICloseable).RequestRestore += (sender, args) => { if (this.WindowState == WindowState.Normal) this.WindowState = WindowState.Maximized; else this.WindowState = WindowState.Normal; };
                }
            };

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
#if OXYLABS
            File.WriteAllText("proxySession", Global.ProxySessionID);
#endif
        }
    }
}
