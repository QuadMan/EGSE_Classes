﻿//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Default
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
    using EGSE.Cyclogram.Command;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*private HSIWindow winHSI = new HSIWindow();
        private SpaceWireWindow winSW = new SpaceWireWindow();
        private SDWindow winSD = new SDWindow();
        private SimRouterWindow winSimRouter = new SimRouterWindow();*/
        private CyclogramCommandBUK _bukCycCommands = new CyclogramCommandBUK();

        private IBUK _EGSE = new IBUK();

        private void initControlValues()
        {
            _bukCycCommands.BUK = _EGSE;
            //_bukCycCommands.HsiWin = hsiWin;
        }

        private void OnTimerWork()
        {
            _EGSE.TickAllControlsValues();
            // выведем значения АЦП
            /*if (_EGSE.Tm.IsPowerOn)
            {
                try
                {
                    float tmpUValue = _EGSE.Tm.Adc.GetValue(XsanTm.ADC_CH_U);
                    if (tmpUValue > 15)
                    {
                        U27VLabel.Content = Math.Round(tmpUValue).ToString();
                    }
                    else
                    {
                        U27VLabel.Content = "---";
                    }
                }
                catch (ADCException)
                {
                    U27VLabel.Content = "---";
                }
                try
                {
                    IXSANLabel.Content = Math.Round(EGSE.Tm.Adc.GetValue(XsanTm.ADC_CH_I)).ToString();
                }
                catch (ADCException)
                {
                    IXSANLabel.Content = "---";
                }
            }
            else
            {
                U27VLabel.Content = "---";
                IXSANLabel.Content = "---";
            }

            updateTM();*/
        }
        /// <summary>
        /// Сохраняем специфические настройки приложения
        /// В данном случае - видимость панели телеметрии
        /// </summary>
        private void saveAppSettings()
        {
           // AppSettings.Save("PowerLabel", Convert.ToString(TMGrid.Visibility));
            //AppSettings.SaveList(hsiWin.UksSendedList, "UksItems");
        }
        /// <summary>
        /// Загружаем специфичные настройки приложения при загрузке
        /// </summary>
        private void loadAppSettings()
        {
            // если окно открыто, соответствующий чекбокс должен быть выбран
           /* if (hsiWin.Visibility == System.Windows.Visibility.Visible)
            {
                HSIControlCb.IsChecked = true;
            }
            // управляем отображением телеметрической информацией
            string powerLabelVisible = AppSettings.Load("PowerLabel");
            if (powerLabelVisible != null)
            {
                switch (powerLabelVisible)
                {
                    case "Visible":
                        TMGrid.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "Hidden":
                        TMGrid.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    default:
                        TMGrid.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
            }

            AppSettings.LoadList(hsiWin.UksSendedList, "UksItems");
            hsiWin.UksStrText.ItemsSource = hsiWin.UksSendedList.ToArray<string>();*/
        }

        /// <summary>
        /// Метод инициализируеющий дополнительные модули (если это необходимо)
        /// </summary>
        private void initModules()
        {
            CycloGrid.AddCycCommands(_bukCycCommands.CycCommandsAvailable);
            //hsiWin.Init(_EGSE);

            DataContext = this;
        }

        /*
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

        }*/
    }
}
