using EGSE.Cyclogram;
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
using System.Windows.Shapes;

namespace CycloTest
{

    public class CycComands1
    {
        public bool TestTest(string[] Params, string errString)
        {
            return true;
        }

        public bool TestExec(string[] Params)
        {
            return true;
        }

        public bool StopTest(string[] Params, string errString)
        {
            return true;
        }

        public bool StopExec(string[] Params)
        {
            return true;
        }
    }

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        public Window1()
        {
            InitializeComponent();


        }
    }
}
