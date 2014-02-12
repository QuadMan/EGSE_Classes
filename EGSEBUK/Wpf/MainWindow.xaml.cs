//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Defaults
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
    using EGSE.Constants;
    using EGSE.Cyclogram.Command;
    using EGSE.Devices;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*private HSIWindow winHSI = new HSIWindow();
        private SpaceWireWindow winSW = new SpaceWireWindow();
        private SDWindow winSD = new SDWindow();*/

        /// <summary>
        /// Окно "имитатор БМ-4"
        /// </summary>
        private SimRouterWindow winSimRouter = new SimRouterWindow();

        /// <summary>
        /// Экземпляр для работы с командами циклограммы.
        /// </summary>
        private CyclogramCommandBUK _bukCycCommands = new CyclogramCommandBUK();

        /// <summary>
        /// Интерфейс работы с устройством устройства.
        /// </summary>
        private IBUK _intfEGSE = new IBUK();

        /// <summary>
        /// Инициализация параметров управления
        /// </summary>
        private void InitControlValues()
        {
            _bukCycCommands.BUK = _intfEGSE;
        }

        /// <summary>
        /// Для синхронизации параметров управления.
        /// </summary>
        private void OnTimerWork()
        {
            _intfEGSE.TickAllControlsValues();
            /* выведем значения АЦП
            if (_EGSE.Tm.IsPowerOn)
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
            }*/

            UpdateTM();
        }

        /// <summary>
        /// Обновляет состояние UI телеметрии.
        /// </summary>
        private void UpdateTM()
        {
            // Индикация питания
            if (_intfEGSE.Tele.BUSKPower1)
            {
                BUSKPower1.Content = "ВКЛ";
                BUSKPower1.Background = Brushes.LightGreen;
            }
            else
            {
                BUSKPower1.Content = "ВЫКЛ";
                BUSKPower1.Background = Brushes.Red;
            }

            if (_intfEGSE.Tele.BUSKPower2)
            {
                BUSKPower2.Content = "ВКЛ";
                BUSKPower2.Background = Brushes.LightGreen;
            }
            else
            {
                BUSKPower2.Content = "ВЫКЛ";
                BUSKPower2.Background = Brushes.Red;
            }

            if (_intfEGSE.Tele.BUNDPower1)
            {
                BUNDPower1.Content = "ВКЛ";
                BUNDPower1.Background = Brushes.LightGreen;
            }
            else
            {
                BUNDPower1.Content = "ВЫКЛ";
                BUNDPower1.Background = Brushes.Red;
            }

            if (_intfEGSE.Tele.BUNDPower2)
            {
                BUNDPower2.Content = "ВКЛ";
                BUNDPower2.Background = Brushes.LightGreen;
            }
            else
            {
                BUNDPower2.Content = "ВЫКЛ";
                BUNDPower2.Background = Brushes.Red;
            }
        }

        /// <summary>
        /// Сохраняем специфические настройки приложения
        /// В данном случае - видимость панели телеметрии
        /// </summary>
        private void SaveAppSettings()
        {
           // AppSettings.Save("PowerLabel", Convert.ToString(TMGrid.Visibility));
           // AppSettings.SaveList(hsiWin.UksSendedList, "UksItems");
        }

        /// <summary>
        /// Загружаем специфичные настройки приложения при загрузке
        /// </summary>
        private void LoadAppSettings()
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
        private void InitModules()
        {
            CycloGrid.AddCycCommands(_bukCycCommands.CyclogramCommandsAvailable);

            //// hsiWin.Init(_EGSE);

            DataContext = _intfEGSE;
        }

        /// <summary>
        /// Handles the Click event of the BUSKPower1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BUSKPower1_Click(object sender, RoutedEventArgs e)
        {
            _intfEGSE.ControlValuesList[BUKConst.PowerControl].SetProperty(BUKConst.PropertyBUSKPower1, Convert.ToInt32(!_intfEGSE.Tele.BUSKPower1));
        }

        /// <summary>
        /// Handles the Click event of the BUSKPower2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BUSKPower2_Click(object sender, RoutedEventArgs e)
        {
            _intfEGSE.ControlValuesList[BUKConst.PowerControl].SetProperty(BUKConst.PropertyBUSKPower2, Convert.ToInt32(!_intfEGSE.Tele.BUSKPower2));
        }

        /// <summary>
        /// Handles the Click event of the BUNDPower1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BUNDPower1_Click(object sender, RoutedEventArgs e)
        {
            _intfEGSE.ControlValuesList[BUKConst.PowerControl].SetProperty(BUKConst.PropertyBUNDPower1, Convert.ToInt32(!_intfEGSE.Tele.BUNDPower1));
        }

        /// <summary>
        /// Handles the Click event of the BUNDPower2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BUNDPower2_Click(object sender, RoutedEventArgs e)
        {
            _intfEGSE.ControlValuesList[BUKConst.PowerControl].SetProperty(BUKConst.PropertyBUNDPower2, Convert.ToInt32(!_intfEGSE.Tele.BUNDPower2));
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

        }*/

        /// <summary>
        /// Handles the Click event of the ControlSimRouter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
