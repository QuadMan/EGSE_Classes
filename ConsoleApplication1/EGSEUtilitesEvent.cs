/*** EGSEUtilitesEvent.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль информации нажатого элемента в приложении Windows
**
** Author: Мурзин Святослав
** Project: КИА
** Module: EDGE UTILITES 
** Requires: 
** Comments: Обязательно вставлять проверку на null в обработчике сообщений: 
 *           if (EventClickToString.ElementClicked(dpcEv) == null)
                return;
 *           В XAML коде используется событие PreviewMouseLeftButtonDown
 *
 * ПРИМЕР:
 * 
 * С# ------------------------------------------------------
 * 
 * public partial class MainWindow : Window
    {
 
 *      //НОМЕР СООБЩЕНИЯ 
        private int iEventCount;
 
        public MainWindow()
        {
            InitializeComponent();
        }

 *      //ОБРАБОТКА СООБЩЕНИЙ 
        private void _f_SomeEvent(object objSender, MouseEventArgs dpcEv)
        {
        
 *          //ЕСЛИ ВОЗВРАЩЕНА НУЛЕВАЯ СТРОКА, ЗНАЧИТ ЭЛЕМЕНТ НЕ СООТВЕТСТВУЕТ
 *          //ЗАДАННОМУ НАБОРУ
            if (EventClickToString.ElementClicked(dpcEv) == null)
                return;
 
 *          //НОМЕР СООБЩЕНИЯ УВЕЛИЧИВАЕМ НА 1
            iEventCount++;
          
 *          //СОЗДАЕМ СТРОКУ С ИНФОРМАЦИЕЙ О НАЖАТОМ ЭЛЕМЕНТЕ
            string szTextOfEvent = "#" + iEventCount.ToString() + ":\r\n" +
                "Событие: " + EventClickToString.ElementClicked(dpcEv);
 
 *          //ВЫДАЕМ ИНФОРМАЦИЮ, К ПРИМЕРУ В ПРИВЯЗАННЫЙ LISTBOX 
            elemListBox.Items.Add(szTextOfEvent);
        }
 * 
 * XAML ----------------------------------------------------
 * 
   <Window x:Class="WPF_UTILITIES.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="MainWindow" Height="800" Width="900" Background="LightGray">
        <Grid Margin="3" ShowGridLines="false" PreviewMouseLeftButtonDown ="_f_SomeEvent" >
            . . .
        </Grid>
    </Window>
**
 *
 * 
** History:
**  0.1.0	(13.12.2013) -	Начальная версия 	
**
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace EGSE.Utilites
{
//*****************************************************************************
//*****************************************************************************

    /// <summary>
    /// Класс генерации информации нажатия клавиши мыши на элементе
    /// </summary>
    class EventClickToString
        {

            /// <summary>
            /// Метод генерации информации
            /// </summary>
            /// <param name="reaEv">MouseEventArgs от оброботчика сообщений</param>
            /// <returns>Строку с информацией</returns>
            static public string ElementClicked(MouseEventArgs reaEv)
            {

                string strRes = null;

                if (reaEv.Source.GetType().Equals(typeof(Button)))
                {
                    Button elemSource = reaEv.Source as Button;

                    if (elemSource != null)
                    {
                        strRes += GetString(GetParentElements(elemSource.Parent));
                        strRes += "Нажата кнопка \"" + elemSource.Content + "\"";
                    }

                }
                else if (reaEv.Source.GetType().Equals(typeof(CheckBox)))
                {
                    CheckBox elemSource = reaEv.Source as CheckBox;
                    if (elemSource != null)
                    {
                        strRes += GetString(GetParentElements(elemSource.Parent));
                        if ((bool)elemSource.IsChecked)
                            strRes += "Снят флажок ";
                        else
                            strRes += "Активирован флажок ";
                        strRes += "\"" + elemSource.Content + "\"";


                    }
                }
                else if (reaEv.Source.GetType().Equals(typeof(ComboBoxItem)))
                {
                    ComboBoxItem elemSource = reaEv.Source as ComboBoxItem;

                    if (elemSource != null)
                    {
                        strRes += GetString(GetParentElements(elemSource.Parent));
                        strRes += "Выбран элемент раскрывающегося списка \"" + elemSource.Content + "\"";
                    }
                }
                return strRes;
            }

            /// <summary>
            /// Внутренний метод формирует одну строку из стэка элементов
            /// </summary>
            /// <param name="stkstrElements">стэк элементов</param>
            /// <returns>Строка с именами родительских элементов</returns>
            static private string GetString(Stack<string> stkstrElements)
            {
                string strRes = null;

                if (stkstrElements == null)
                    return null;
                if (stkstrElements.Count != 0)
                    strRes = "В блоке " + "\"" + stkstrElements.Pop() + "\"";
                while (stkstrElements.Count != 0)
                    strRes += ", блока " + "\"" + stkstrElements.Pop() + "\"";
                strRes += ": ";
                return strRes;
            }

            /// <summary>
            /// Внутренний метод создает стэк имен родительских элементов
            /// </summary>
            /// <param name="objParent">Родительский элемент</param>
            /// <returns>Стэк имен родительских элементов</returns>
            static private Stack<string> GetParentElements(object objParent)
            {
                Stack<string> stkstrRes = new Stack<string>();
                object objParentElemSource = objParent;

                while (objParentElemSource != null)
                {
                    if (objParentElemSource.GetType().Equals(typeof(Expander)))
                    {
                        Expander elemSource = objParentElemSource as Expander;
                        if (elemSource != null)
                        {
                            objParentElemSource = LogicalTreeHelper.GetParent(elemSource);
                            stkstrRes.Push((string)elemSource.Header);
                        }
                    }
                    else if (objParentElemSource.GetType().Equals(typeof(GroupBox)))
                    {
                        GroupBox elemSource = objParentElemSource as GroupBox;
                        if (elemSource != null)
                        {
                            objParentElemSource = LogicalTreeHelper.GetParent(elemSource);
                            stkstrRes.Push((string)elemSource.Header);
                        }
                    }
                    else
                        objParentElemSource = IgnoreParentElement(objParentElemSource);
                }
                return stkstrRes;
            }

            /// <summary>
            /// Метод возвращает родительский объект, если текущий объект 
            /// удовлетворяет набору элементов, которые необходимо игнорировать
            /// </summary>
            /// <param name="objElem">Объект, который нужно проверить на игнорирование</param>
            /// <returns>Родительский объект или null,если удовлетворяет набору элементов, 
            /// которые необходимо игнорировать</returns>
            static private object IgnoreParentElement(object objElem)
            {
                if (objElem.GetType().Equals(typeof(StackPanel)))
                {
                    StackPanel elemIgnSource = objElem as StackPanel;
                    if (elemIgnSource != null)
                        return LogicalTreeHelper.GetParent(elemIgnSource);
                }
                else if (objElem.GetType().Equals(typeof(DockPanel)))
                {
                    DockPanel elemIgnSource = objElem as DockPanel;
                    if (elemIgnSource != null)
                        return LogicalTreeHelper.GetParent(elemIgnSource);
                }
                else if (objElem.GetType().Equals(typeof(WrapPanel)))
                {
                    WrapPanel elemIgnSource = objElem as WrapPanel;
                    if (elemIgnSource != null)
                        return LogicalTreeHelper.GetParent(elemIgnSource);
                }
                else if (objElem.GetType().Equals(typeof(Grid)))
                {
                    Grid elemIgnSource = objElem as Grid;
                    if (elemIgnSource != null)
                        return LogicalTreeHelper.GetParent(elemIgnSource);
                }
                else if (objElem.GetType().Equals(typeof(ComboBox)))
                {
                    ComboBox elemIgnSource = objElem as ComboBox;
                    if (elemIgnSource != null)
                        return LogicalTreeHelper.GetParent(elemIgnSource);
                }
                return null;
            }
        }
}
