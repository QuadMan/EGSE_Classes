using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfEgseBuk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HSIWindow winHSI = new HSIWindow();
        private SpaceWireWindow winSW = new SpaceWireWindow();
        private SDWindow winSD = new SDWindow();
        private SimRouterWindow winSimRouter = new SimRouterWindow();

        public MainWindow()
        {
            InitializeComponent();            
        }

        private void ControlSpaceWire_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)ControlSpaceWire.IsChecked)
            {
                winSW.Show();
            }
            else
            {
                winSW.Hide();
            }

        }

        private void ControlHSI_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)ControlHSI.IsChecked)
            {
                winHSI.Show();
            }
            else
            {
                winHSI.Hide();
            }

        }

        private void ControlSD_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)ControlSD.IsChecked)
            {
                winSD.Show();
            }
            else
            {
                winSD.Hide();
            }

        }

        private void ControlSimRouter_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ControlSimRouter.IsChecked)
            {
                winSimRouter.Show();
            }
            else
            {
                winSimRouter.Hide();
            }

        }
    }
}
