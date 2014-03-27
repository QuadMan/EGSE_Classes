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
            _cycCommandsAvailable.AddCommand("aa6e98173bf04ae1aa63e95751bbb856", new CyclogramLine("NOP", NopTest, NopExec, string.Empty));
            _cycCommandsAvailable.AddCommand("b05d3add0acd436da6f8ac89e105d1f3", new CyclogramLine("STOP", StopTest, StopExec, string.Empty));
        }

        /// <summary>
        /// Получает или задает значение, показывающее, включен ли режим отладки.
        /// </summary>
        public bool IsTracingMode { get; set; }

        /// <summary>
        /// Функция добавляет список команд циклограммы к исходным командам.
        /// </summary>
        /// <param name="cycCommands">Список команд.</param>
        public void AddCycCommands(CyclogramCommands cycCommands)
        {
            foreach (KeyValuePair<string, CyclogramLine> cycLine in cycCommands) 
            {
                _cycCommandsAvailable.AddCommand(cycLine.Key, cycLine.Value);
            }
        }

        /// <summary>
        /// Проверка команды [STOP].
        /// </summary>
        /// <param name="setParams">Передаваемые параметры.</param>
        /// <param name="errstring">Сообщение об ошибке.</param>
        /// <returns><c>true</c> всегда.</returns>
        public bool StopTest(string[] setParams, out string errstring)
        {
            errstring = string.Empty;
            return true;
        }

        /// <summary>
        /// Выполнение [STOP].
        /// </summary>
        /// <param name="setParams">Параметры команды.</param>
        /// <returns><c>true</c> всегда.</returns>
        public bool StopExec(string[] setParams)
        {
            _cycloThread.StopAndSetNextCmd();
            return true;
        }
        
        /// <summary>
        /// Проверка команды [NOP].
        /// </summary>
        /// <param name="setParams">Параметры команды.</param>
        /// <param name="errstring">Сообщение об ошибке.</param>
        /// <returns><c>true</c> всегда.</returns>
        public bool NopTest(string[] setParams, out string errstring)
        {
            errstring = string.Empty;
            return true;
        }

        /// <summary>
        /// Выполнение [NOP].
        /// </summary>
        /// <param name="setParams">Параметры команды.</param>
        /// <returns><c>true</c> всегда.</returns>
        public bool NopExec(string[] setParams)
        {
            return true;
        }

        /// <summary>
        /// Вызывается когда [выполнение циклограммы окончено].
        /// </summary>
        /// <param name="str">Дополнительная информация для вывода в сообщение.</param>
        private void OnCycFinished(string str)
        {
            MessageBox.Show("Циклограмма завершена!" + str);
        }

        /// <summary>
        /// Sets the state of the buttons by.
        /// </summary>
        /// <param name="cycloState">State of the cyclo.</param>
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

        /// <summary>
        /// Вызывается когда [циклограмма изменяет свой статус].
        /// </summary>
        /// <param name="cycloState">State of the cyclo.</param>
        private void OnCycStateChange(CurState cycloState)
        {
            DG.Dispatcher.Invoke(new Action(delegate
            {
                SetButtonsByState(cycloState);
            }));
        }

        /// <summary>
        /// Handles the Click event of the StartBtn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            CyclogramLine curCycLine = (CyclogramLine)DG.SelectedItem;

            if (!curCycLine.IsCommand)
            {
                return;
            }

            _cycloThread.Start(curCycLine.Line);
        }

        /// <summary>
        /// Handles the Click event of the StopBtn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _cycloThread.Stop();
        }

        /// <summary>
        /// Handles the Click event of the StepBtn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void StepBtn_Click(object sender, RoutedEventArgs e)
        {
            CyclogramLine curCycLine = (CyclogramLine)DG.SelectedItem;

            if (!curCycLine.IsCommand)
            {
                return;
            }

            _cycloThread.Step(curCycLine.Line);
        }

        /// <summary>
        /// Вызывается когда [циклограмма переходит к следующей команде].
        /// </summary>
        /// <param name="cycCommand">The cyc command.</param>
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

        /// <summary>
        /// Handles the Click event of the LoadBtn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".cyc";
            dlg.Filter = "Файл циклограмм (.cyc)|*.cyc"; 

            if (dlg.ShowDialog() == true)
            {
                DG.DataContext = null;
                _cycloThread.Load(dlg.FileName, _cycCommandsAvailable);
                DG.DataContext = _cycloThread.CycloFile.Commands;
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the DG control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
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
    }
}
