/*
 * 
 * 
 * 
 * 
 *  24.01.2014 - добавил событие StopEvent - которое вырабатывается только по команде Stop или нажатию кнопки Stop
 *             - событие FinishedEvent вызывается только когда дошли до конца циклограммы
 * 
 */

using EGSE.Cyclogram;
using EGSE.Threading;
using EGSE.Utilites;
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

        private CyclogramCommands _cycCommandsAvailable = new CyclogramCommands();

        public CyclogramControl()
        {
            InitializeComponent();
            //
            cThread = new CyclogramThread();
            cThread.NextCommandEvent = onNewCmd;
            cThread.ChangeStateEvent = onCycStateChange;
            cThread.FinishedEvent = onCycFinished;
            //
            setButtonsByState(CurState.csNone);
            //
            _cycCommandsAvailable.AddCommand("NOP", new CyclogramLine("NOP", NopTest, NopExec, String.Empty));
            _cycCommandsAvailable.AddCommand("STOP", new CyclogramLine("STOP", StopTest, StopExec, String.Empty));
            //cycCommandsAvailable.AddCommand("LOOP", new CyclogramLine("LOOP", LoopTest, LoopExec, String.Empty));
        }

        /// <summary>
        /// Функция добавляет список команд циклограммы к исходным командам
        /// </summary>
        /// <param name="cycCommands"></param>
        public void AddCycCommands(CyclogramCommands cycCommands)
        {
            foreach (KeyValuePair<string, CyclogramLine> cycLine in cycCommands) {
                _cycCommandsAvailable.AddCommand(cycLine.Key, cycLine.Value);
            }
        }

        public bool IsTracingMode { get; set; }

        public bool StopTest(string[] Params, out string errString)
        {
            errString = String.Empty;
            return true;
        }

        public bool StopExec(string[] Params)
        {
            cThread.StopAndSetNextCmd();
            return true;
        }
        
        public bool NopTest(string[] Params, out string errString)
        {
            errString = String.Empty;
            return true;
        }

        public bool NopExec(string[] Params)
        {
            return true;
        }
        
        public bool LoopTest(string[] Params, out string errString)
        {
            errString = String.Empty;
            return true;
        }

        public bool LoopExec(string[] Params)
        {
            return true;
        }


        private void onCycFinished(string str)
        {
            MessageBox.Show("Циклограмма завершена!");
        }


        private void setButtonsByState(CurState cState)
        {
            switch (cState)
            {
                case CurState.csLoaded:
                    StartBtn.IsEnabled = true;
                    StopBtn.IsEnabled = false;
                    StepBtn.IsEnabled = true;
                    StatusLabel.Content = cThread.CycFile.FileName;
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
                    StatusLabel.Content = String.Empty;
                    break;
                case CurState.csRunning:
                    StartBtn.IsEnabled = false;
                    StopBtn.IsEnabled = true;
                    StepBtn.IsEnabled = false;
                    break;
            }
        }

        private void onCycStateChange(CurState cState)
        {
            DG.Dispatcher.Invoke(new Action(delegate
            {
                setButtonsByState(cState);
            }));
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            CyclogramLine curCycLine = (CyclogramLine)DG.SelectedItem;

            if (!curCycLine.IsCommand) return;

            cThread.Start(curCycLine.Line);
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            cThread.Stop();
        }

        private void StepBtn_Click(object sender, RoutedEventArgs e)
        {
            CyclogramLine curCycLine = (CyclogramLine)DG.SelectedItem;

            if (!curCycLine.IsCommand) return;

            cThread.Step(curCycLine.Line);
        }

        private void onNewCmd(CyclogramLine cycCommand)
        {
            if (cycCommand == null) return;

            DG.Dispatcher.Invoke(new Action(delegate
            {
                DG.SelectedItem = cycCommand;
                if (IsTracingMode)
                {
                    DG.ScrollIntoView(cycCommand);
                }
            }));
        }


        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".cyc"; // Default file extension 
            dlg.Filter = "Файл циклограмм (.cyc)|*.cyc"; // Filter files by extension 

            if (dlg.ShowDialog() == true)
            {
                //try
                //{
                    DG.DataContext = null;
                    cThread.Load(dlg.FileName, _cycCommandsAvailable);
                    DG.DataContext = cThread.CycFile.commands;
                //}
                //catch (CyclogramParsingException exc)
                //{
                    //MessageBox.Show(exc.Message);
                //}
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

        private void TrackingModeCB_Checked(object sender, RoutedEventArgs e)
        {
            IsTracingMode = (bool)TrackingModeCB.IsChecked;
        }

        /// <summary>
        /// Логгируем все нажания кнопок, чекбоксов и т.д.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = EventClickToString.ElementClicked(e);
            if (logEvent != null)
            {
                kia_xan.LogsClass.Instance.Files[kia_xan.LogsClass.OperatorIdx].LogText = logEvent;
            }
        }
    }
}
