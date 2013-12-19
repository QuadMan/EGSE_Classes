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
        private CycComands1 cCmd;
        private CyclogramCommands cycCommandsAvailable;
        public Window1()
        {
            InitializeComponent();

            cCmd = new CycComands1();
            //
            cycCommandsAvailable = new CyclogramCommands();
            cycCommandsAvailable.AddCommand("TEST", new CyclogramLine("TEST", cCmd.TestTest, cCmd.TestExec, ""));
            cycCommandsAvailable.AddCommand("STOP", new CyclogramLine("STOP", cCmd.StopTest, cCmd.StopExec, ""));
            cycCommandsAvailable.AddCommand("FIDERS", new CyclogramLine("FIDERS", cCmd.StopTest, cCmd.StopExec, ""));
            cycCommandsAvailable.AddCommand("HZ", new CyclogramLine("HZ", cCmd.StopTest, cCmd.StopExec, ""));
            cycCommandsAvailable.AddCommand("MKO", new CyclogramLine("MKO", cCmd.StopTest, cCmd.StopExec, ""));


            cycCommandsAvailable.AddCommand("HS_IMIT_SETUP", new CyclogramLine("HS_IMIT_SETUP", cCmd.StopTest, cCmd.StopExec, ""));
            cycCommandsAvailable.AddCommand("HS_INT", new CyclogramLine("HS_INT", cCmd.StopTest, cCmd.StopExec, ""));
            cycCommandsAvailable.AddCommand("HS_IMIT_DATA", new CyclogramLine("HS_IMIT_DATA", cCmd.StopTest, cCmd.StopExec, ""));
            cycCommandsAvailable.AddCommand("HS_IMIT", new CyclogramLine("HS_IMIT", cCmd.StopTest, cCmd.StopExec, ""));

            cycCommandsAvailable.AddCommand("LS_INT", new CyclogramLine("LS_INT", cCmd.StopTest, cCmd.StopExec, ""));
            cycCommandsAvailable.AddCommand("LS_IMIT", new CyclogramLine("LS_IMIT", cCmd.StopTest, cCmd.StopExec, ""));
            cycCommandsAvailable.AddCommand("LS_IMIT_DATA", new CyclogramLine("LS_IMIT_DATA", cCmd.StopTest, cCmd.StopExec, ""));

            cycCommandsAvailable.AddCommand("BRK_IMIT", new CyclogramLine("BRK_IMIT", cCmd.StopTest, cCmd.StopExec, ""));
            cycCommandsAvailable.AddCommand("BRK", new CyclogramLine("BRK", cCmd.StopTest, cCmd.StopExec, ""));

            CycloGrid.cycCommandsAvailable = cycCommandsAvailable;
        }
    }
}
