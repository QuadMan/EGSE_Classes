using EGSE.Cyclogram;
using EGSE.Threading;
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

namespace EGSE.Cyclogram
{
    /// <summary>
    /// Interaction logic for CyclogramControl.xaml
    /// </summary>
    public partial class CyclogramControl : UserControl
    {
        private CyclogramThread cThread;
        private string statusText;

        public CyclogramCommands cycCommandsAvailable;

        public CyclogramControl()
        {
            InitializeComponent();
            //
            cThread = new CyclogramThread();
            cThread.NextCommandEvent = onNewCmd;
            cThread.ChangeStateEvent = onCycStateChange;
            cThread.FinishedEvent = onCycFinished;
            //
            onCycStateChange(CurState.csNone);

        }

        private void onCycFinished(string str)
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
                        StartBtn.IsEnabled = false;
                        StopBtn.IsEnabled = false;
                        StepBtn.IsEnabled = false;
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

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            cThread.Start();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            cThread.Stop();
        }

        private void onNewCmd(CyclogramLine cycCommand)
        {
            if (cycCommand == null) return;

            DG.Dispatcher.Invoke(new Action(delegate
            {
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
                    if ((DG.SelectedItem != null) && (DG.SelectedItem as CyclogramLine).WasError)
                    {
                        StatusLabel.Content = (DG.SelectedItem as CyclogramLine).ErrorInCommand;
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
