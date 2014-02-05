//-----------------------------------------------------------------------
// <copyright file="EGSECyclogramControl.xaml.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

namespace EGSE.Cyclogram
{
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
    using EGSE.Cyclogram;
    using EGSE.Threading;
    using EGSE.Utilites;

    /// <summary>
    /// Interaction logic for CyclogramControl.xaml
    /// </summary>
    public partial class CyclogramControl : UserControl
    {
        /// <summary>
        /// Нить работы циклограммы.
        /// </summary>
        private CyclogramThread _cycloThread;

        /// <summary>
        /// Текущий статус выполнения циклограммы.
        /// </summary>
        private string statusText;

        /// <summary>
        /// Список доступных команд циклограммы.
        /// </summary>
        private CyclogramCommands _cycCommandsAvailable = new CyclogramCommands();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramControl" />.
        /// </summary>
        public CyclogramControl()
        {
            InitializeComponent();
            _cycloThread = new CyclogramThread();
            _cycloThread.NextCommandEvent = OnNewCmd;
            _cycloThread.ChangeStateEvent = OnCycStateChange;
            _cycloThread.FinishedEvent = OnCycFinished;
            SetButtonsByState(CurState.cycloNone);
            _cycCommandsAvailable.AddCommand("NOP", new CyclogramLine("NOP", NopTest, NopExec, string.Empty));
            _cycCommandsAvailable.AddCommand("STOP", new CyclogramLine("STOP", StopTest, StopExec, string.Empty));

            // cycCommandsAvailable.AddCommand("LOOP", new CyclogramLine("LOOP", LoopTest, LoopExec, string.Empty));
        }

        /// <summary>
        /// Функция добавляет список команд циклограммы к исходным командам
        /// </summary>
        /// <param name="cycCommands">Список команд</param>
        public void AddCycCommands(CyclogramCommands cycCommands)
        {
            foreach (KeyValuePair<string, CyclogramLine> cycLine in cycCommands) 
            {
                _cycCommandsAvailable.AddCommand(cycLine.Key, cycLine.Value);
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, включен ли режим отладки.
        /// </summary>
        public bool IsTracingMode { get; set; }

        /// <summary>
        /// TODO описание
        /// </summary>
        /// <param name="setParams">Передаваемые параметры</param>
        /// <param name="errstring">Сообщение об ошибке</param>
        /// <returns>Всегда True</returns>
        public bool StopTest(string[] setParams, out string errstring)
        {
            errstring = string.Empty;
            return true;
        }

        /// <summary>
        /// TODO описание
        /// </summary>
        /// <param name="setParams">Передаваемые параметры</param>
        /// <returns>Всегда True</returns>
        public bool StopExec(string[] setParams)
        {
            _cycloThread.StopAndSetNextCmd();
            return true;
        }
        
        /// <summary>
        /// TODO описание
        /// </summary>
        /// <param name="setParams">Передаваемые параметры</param>
        /// <param name="errstring">Сообщение об ошибке</param>
        /// <returns>Всегда True</returns>
        public bool NopTest(string[] setParams, out string errstring)
        {
            errstring = string.Empty;
            return true;
        }

        /// <summary>
        /// TODO описание
        /// </summary>
        /// <param name="setParams">Передаваемые параметры</param>
        /// <returns>Всегда True</returns>
        public bool NopExec(string[] setParams)
        {
            return true;
        }
        
        public bool LoopTest(string[] setParams, out string errstring)
        {
            errstring = string.Empty;
            return true;
        }

        public bool LoopExec(string[] setParams)
        {
            return true;
        }

        /// <summary>
        /// TODO описание
        /// </summary>
        /// <param name="str">TODO параметр</param>
        private void OnCycFinished(string str)
        {
            MessageBox.Show("Циклограмма завершена!");
        }

        private void SetButtonsByState(CurState cycloState)
        {
            switch (cycloState)
            {
                case CurState.cycloLoaded:
                    StartBtn.IsEnabled = true;
                    StopBtn.IsEnabled = false;
                    StepBtn.IsEnabled = true;
                    StatusLabel.Content = _cycloThread.CycloFile.FileName;
                    break;
                case CurState.cycloLoadedWithErrors:
                    StartBtn.IsEnabled = false;
                    StopBtn.IsEnabled = false;
                    StepBtn.IsEnabled = false;
                    statusText = "Ошибки в циклограмме!";
                    StatusLabel.Content = statusText;
                    break;
                case CurState.cycloNone:
                    StartBtn.IsEnabled = false;
                    StopBtn.IsEnabled = false;
                    StepBtn.IsEnabled = false;
                    StatusLabel.Content = string.Empty;
                    break;
                case CurState.cycloRunning:
                    StartBtn.IsEnabled = false;
                    StopBtn.IsEnabled = true;
                    StepBtn.IsEnabled = false;
                    break;
            }
        }

        private void OnCycStateChange(CurState cycloState)
        {
            DG.Dispatcher.Invoke(new Action(delegate
            {
                SetButtonsByState(cycloState);
            }));
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            CyclogramLine curCycLine = (CyclogramLine)DG.SelectedItem;

            if (!curCycLine.IsCommand)
            {
                return;
            }

            _cycloThread.Start(curCycLine.Line);
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _cycloThread.Stop();
        }

        private void StepBtn_Click(object sender, RoutedEventArgs e)
        {
            CyclogramLine curCycLine = (CyclogramLine)DG.SelectedItem;

            if (!curCycLine.IsCommand)
            {
                return;
            }

            _cycloThread.Step(curCycLine.Line);
        }

        private void OnNewCmd(CyclogramLine cycCommand)
        {
            if (cycCommand == null)
            {
                return;
            }

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
                ////try
                ////{
                    DG.DataContext = null;
                    _cycloThread.Load(dlg.FileName, _cycCommandsAvailable);
                    DG.DataContext = _cycloThread.CycloFile.Commands;

                ////}
                ////catch (CyclogramParsingException exc)
                ////{
                ////MessageBox.Show(exc.Message);
                ////}
            }
        }

        private void DG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (_cycloThread.State)
            {
                case CurState.cycloLoaded:
                    if (_cycloThread.IsCommandOnLine(DG.SelectedIndex))
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
                case CurState.cycloLoadedWithErrors:
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

        /// <summary>
        /// Handles the Checked event of the TrackingModeCB control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TrackingModeCB_Checked(object sender, RoutedEventArgs e)
        {
            IsTracingMode = (bool)TrackingModeCB.IsChecked;
        }

        /// <summary>
        /// Логгируем все нажания кнопок, чекбоксов и т.д.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void MouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = EventClickToString.ElementClicked(e);
            if (logEvent != null)
            {
              // LogsClass.LogOperator.LogText = logEvent;
            }
        }
    }
}
