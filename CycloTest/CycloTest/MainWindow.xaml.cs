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
    public class CycComands
    {
        public bool TestTest(string[] Params, string errString) {
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CycComands cCmd;
        private CyclogramThread cThread;
        private CyclogramCommands cycCommandsAvailable;
        //private ObservableCollection<CyclogramLine> cCmds;
        private string statusText;

        public MainWindow()
        {
            InitializeComponent();
            //
            cCmd = new CycComands();
            //
            //cCmds = new ObservableCollection<CyclogramCommand>();
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
            //
            cThread = new CyclogramThread(cycCommandsAvailable);
            cThread.onGetCmd = onNewCmd;
            cThread.onCmdStep = onNewCmd;
            cThread.onChangeState = onCycStateChange;
            cThread.onFinished = onCycFinished;
            //
        }

        private void onCycFinished()
        {
            MessageBox.Show("Циклограмма завершена!");
        }

        private void onCycStateChange(CurState cState)
        {
            DG.Dispatcher.Invoke(new Action(delegate
            {
                switch (cState)
                {
                    case CurState.csLoaded:
                        StartBtn.IsEnabled = true;
                        StopBtn.IsEnabled = false;
                        StepBtn.IsEnabled = true;
                        StatusLabel.Content = cThread._cycFile.FileName;
                        break;
                    case CurState.csLoadedWithErrors:
                        StartBtn.IsEnabled = false;
                        StopBtn.IsEnabled = false;
                        StepBtn.IsEnabled = false;
                        statusText = "Ошибки в циклограмме!";
                        StatusLabel.Content = statusText;
                        break;
                    case CurState.csNone:
                        StartBtn.IsEnabled = true;
                        StopBtn.IsEnabled = false;
                        StepBtn.IsEnabled = true;
                        StatusLabel.Content = "";
                        break;
                    case CurState.csRunning:
                        StartBtn.IsEnabled = false;
                        StopBtn.IsEnabled = true;
                        StepBtn.IsEnabled = false;
                        break;
                }
            }));
        }

        private void onNewCmd(CyclogramLine cycCommand)
        {
            if (cycCommand == null) return;

            DG.Dispatcher.Invoke(new Action(delegate { 
                DG.SelectedItem = cycCommand;
                DG.ScrollIntoView(cycCommand);
            }));
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".cyc"; // Default file extension 
            dlg.Filter = "Файл циклограмм (.cyc)|*.cyc"; // Filter files by extension 

            Nullable<bool> res = dlg.ShowDialog();

            if (res == true)
            {
                try
                {
                    DG.DataContext = null;
                    cThread.Load(dlg.FileName, cycCommandsAvailable);
                    DG.DataContext = cThread._cycFile.commands;
                }
                catch (CyclogramParsingException exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            cThread.Start();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            cThread.Stop();
        }

        private void DG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cThread.State)
            {
                case CurState.csLoaded:
                    if (cThread.IsCommandOnLine(DG.SelectedIndex))
                    {
                        StartBtn.IsEnabled = true;
                        StopBtn.IsEnabled = false;
                        StepBtn.IsEnabled = true;
                    }
                    else
                    {
                        StartBtn.IsEnabled = false;
                        StopBtn.IsEnabled = false;
                        StepBtn.IsEnabled = false;
                    }
                    break;
                case CurState.csLoadedWithErrors:
                    if ((DG.SelectedItem != null) && (DG.SelectedItem as CyclogramLine).wasError)
                    {
                        StatusLabel.Content = (DG.SelectedItem as CyclogramLine).errorInCommand;
                    }
                    else
                    {
                        StatusLabel.Content = statusText;
                    }
                    break;
            }
        }
    }
}
