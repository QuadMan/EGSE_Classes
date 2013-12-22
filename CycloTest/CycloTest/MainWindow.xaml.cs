using EGSE.Cyclogram;
using EGSE.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace CycloTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //cCmd = new CycComands();
            //
            //cycCommandsAvailable = new CyclogramCommands();
            CycloGrid.cycCommandsAvailable.AddCommand("TEST", new CyclogramLine("TEST", TestTest, TestExec, ""));
            CycloGrid.cycCommandsAvailable.AddCommand("FIDERS", new CyclogramLine("FIDERS", StopTest, StopExec, ""));
            CycloGrid.cycCommandsAvailable.AddCommand("HZ", new CyclogramLine("HZ", StopTest, StopExec, ""));
            CycloGrid.cycCommandsAvailable.AddCommand("MKO", new CyclogramLine("MKO", StopTest, StopExec, ""));

            CycloGrid.cycCommandsAvailable.AddCommand("HS_IMIT_SETUP", new CyclogramLine("HS_IMIT_SETUP", StopTest, StopExec, ""));
            CycloGrid.cycCommandsAvailable.AddCommand("HS_INT", new CyclogramLine("HS_INT", StopTest, StopExec, ""));
            CycloGrid.cycCommandsAvailable.AddCommand("HS_IMIT_DATA", new CyclogramLine("HS_IMIT_DATA", StopTest, StopExec, ""));
            CycloGrid.cycCommandsAvailable.AddCommand("HS_IMIT", new CyclogramLine("HS_IMIT", StopTest, StopExec, ""));

            CycloGrid.cycCommandsAvailable.AddCommand("LS_INT", new CyclogramLine("LS_INT", StopTest, StopExec, ""));
            CycloGrid.cycCommandsAvailable.AddCommand("LS_IMIT", new CyclogramLine("LS_IMIT", StopTest, StopExec, ""));
            CycloGrid.cycCommandsAvailable.AddCommand("LS_IMIT_DATA", new CyclogramLine("LS_IMIT_DATA", StopTest, StopExec, ""));

            CycloGrid.cycCommandsAvailable.AddCommand("BRK_IMIT", new CyclogramLine("BRK_IMIT", StopTest, StopExec, ""));
            CycloGrid.cycCommandsAvailable.AddCommand("BRK", new CyclogramLine("BRK", StopTest, StopExec, ""));

            //CycloGrid.cycCommandsAvailable.AddCommand(); = cycCommandsAvailable;
        }

        public bool TestTest(string[] Params, out string errString)
        {
            errString = "";
            return true;
        }

        public bool TestExec(string[] Params)
        {
            return true;
        }

        public bool StopTest(string[] Params, out string errString)
        {
            errString = "";
            return true;
        }

        public bool StopExec(string[] Params)
        {
            return true;
        }
    }
}
