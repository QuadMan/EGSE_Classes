//-----------------------------------------------------------------------
// <copyright file="EGSECyclogramCommandBUK.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Cyclogram.Command
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Egse.Cyclogram;
    using Egse.Devices;
    using Egse.Utilites;
    using Egse.CustomAttributes;
    using System.Linq.Expressions;
    using System.Windows;

    /// <summary>
    /// Инициализирует набор доступных цикломанд для устройства.
    /// </summary>
    public class CyclogramCommandBuk
    {
         /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramCommandBuk" />.
        /// </summary>
        public CyclogramCommandBuk()
        {
            CyclogramCommandsAvailable = new CyclogramCommands();
            CyclogramCommandsAvailable.AddCommand("62ac720d56ce449d86ca2cde439ba75a", new CyclogramLine("POWER", PowerTest, PowerExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("efc854841c554c529966abe5f1e6c2d7", new CyclogramLine("SHUTTER", ShutterTest, ShutterExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("b1f06daf0810463892707e7edf5d93be", new CyclogramLine("KVV_IMIT", KvvImitTest, KvvImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("954bcc4e3ea240ddacd8766fcc72c9bf", new CyclogramLine("KVV_IMIT_LOG", KvvImitLogTest, KvvImitLogExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("e333bfb23a204e748699528fb787012c", new CyclogramLine("BM4_IMIT", Bm4ImitTest, Bm4ImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("9e3e94f708eb44819a8ea9d01aa321e9", new CyclogramLine("BM4_IMIT_CMD", Bm4ImitCmdTest, Bm4ImitCmdExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("1198f53b3a9a4a6b9c69f3ac6ebe466f", new CyclogramLine("BM4_IMIT_LOG", Bm4ImitLogTest, Bm4ImitLogExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("bdeae631daf8489983760003e615852a", new CyclogramLine("DETECTOR_IMIT", DetectorImitTest, DetectorImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("fac9ef960e6b4a55b48ecd13f51d4acf", new CyclogramLine("DETECTOR_IMIT_LOG", DetectorImitLogTest, DetectorImitLogExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("2e92651815794233bc062365b3e8278b", new CyclogramLine("BUK_KVV_IMIT", BukKvvImitTest, BukKvvImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("ec4d4c82d1044d3fbcad59d4403c54aa", new CyclogramLine("BUK_KVV_IMIT_CMD", BukKvvImitCmdTest, BukKvvImitCmdExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("c8567a505600474fb4f8ab24a5a11246", new CyclogramLine("BUK_BM4_IMIT", BukBm4ImitTest, BukBm4ImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("e1ee52714fa547f38701f984328913d9", new CyclogramLine("BUK_DETECTOR_IMIT", BukDetectorImitTest, BukDetectorImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("e1ee52714fa547f38701f984328913d9", new CyclogramLine("BUK_DETECTOR_IMIT_CMD", BukDetectorImitCmdTest, BukDetectorImitCmdExec, string.Empty));
        }

        private bool BukDetectorImitCmdExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        private bool BukDetectorImitCmdTest(string[] cmdParams, out string errString)
        {
            throw new NotImplementedException();
        }

        private bool BukDetectorImitExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        private bool BukDetectorImitTest(string[] cmdParams, out string errString)
        {
            throw new NotImplementedException();
        }

        private bool BukBm4ImitExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        private bool BukBm4ImitTest(string[] cmdParams, out string errString)
        {
            throw new NotImplementedException();
        }

        private bool DetectorImitLogTest(string[] cmdParams, out string errString)
        {
            throw new NotImplementedException();
        }

        private bool DetectorImitLogExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        private bool Bm4ImitLogTest(string[] cmdParams, out string errString)
        {
            throw new NotImplementedException();
        }

        private bool Bm4ImitLogExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        private bool KvvImitLogTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            errString = Resource.Get(@"eParamCount");

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }
            
            // задаемся минимальным количеством параметров для команды (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                if (!IncludeTest<BinLog, TxtLog>(cmdParams[0], out errParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }                

                return !isParamCountErr;
            }

            if (!IncludeTest<BinLog>(cmdParams[0], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (!IncludeTest<TxtLog>(cmdParams[1], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }
            
            // задаемся максимальным количеством параметров для команды (2).
            isParamCountErr = true;

            if (2 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            return true;
        }

        private bool KvvImitLogExec(string[] cmdParams)
        {
            IncludeExec<BinLog, TxtLog>(cmdParams.ToList());

            return true;
        }
      
        private enum State
        {
            READY,
            BUSY,
            ME
        }

        /// <summary>
        /// Аргумент "переключатель состояния".
        /// </summary>
        private enum Switcher
        {
            /// <summary>
            /// Параметр "включения/активации" аргумента.
            /// </summary>
            ON,

            /// <summary>
            /// Параметр "выключения/деактивации" аргумента.
            /// </summary>
            OFF
        }

        /// <summary>
        /// Аргумент "выбор устройства".
        /// </summary>
        private enum Device
        {
            /// <summary>
            /// Параметр "устройство БУСК" аргумента.
            /// </summary>
            BUSK,

            /// <summary>
            /// Параметр "устройство БУК" аргумента.
            /// </summary>
            BUK,

            /// <summary>
            /// Параметр "устройство БУНД" аргумента.
            /// </summary>
            BUND
        }

        private enum TxtLog
        {
            [ActionAttribute(typeof(Execut), "TxtOn")]
            TXT_ON,
            [ActionAttribute(typeof(Execut), "TxtOff")]
            TXT_OFF
        }

        public static class Execut
        {
            public static readonly Action<EgseBukNotify> BinOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed BinOn"); });
            public static readonly Action<EgseBukNotify> BinOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed BinOff"); });
            public static readonly Action<EgseBukNotify> TxtOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TxtOn"); });
            public static readonly Action<EgseBukNotify> TxtOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TxtOff"); });
        }

        private enum BinLog
        {
            [ActionAttribute(typeof(Execut), "BinOn")]
            BIN_ON,

            [ActionAttribute(typeof(Execut), "BinOff")]
            BIN_OFF
        }

        /// <summary>
        /// Аргумент "каналы БС".
        /// </summary>
        private enum Scidev
        {
            /// <summary>
            /// Параметр "канал БС УФЭС" аргумента.
            /// </summary>
            UFES,

            /// <summary>
            /// Параметр "канал БС ВУФЭС" аргумента.
            /// </summary>
            VUFES,

            /// <summary>
            /// Параметр "канал БС СДЩ" аргумента.
            /// </summary>
            SDSH
        }

        /// <summary>
        /// Аргумент "выбор линии передачи".
        /// </summary>
        private enum Halfset
        {
            /// <summary>
            /// Параметр "основная линия" аргумента.
            /// </summary>
            MAIN,

            /// <summary>
            /// Параметр "резервная линия" аргумента.
            /// </summary>
            RESV,

            /// <summary>
            /// Параметр "основная+резервная линия" аргумента.
            /// </summary>
            BOTH,

            /// <summary>
            /// Параметр "линия передачи отсутствует" аргумента.
            /// </summary>
            NONE
        }

        /// <summary>
        /// Аргумент "выбор линии выдачи релейной команды".
        /// </summary>
        private enum Line
        {
            /// <summary>
            /// Параметр "выдача релейной команды по линии A" аргумента.
            /// </summary>
            A,

            /// <summary>
            /// Параметр "выдача релейной команды по линии B" аргумента.
            /// </summary>
            B,

            /// <summary>
            /// Параметр "выдача релейной команды по линиям A и B" аргумента.
            /// </summary>
            AB
        }

        /// <summary>
        /// Аргумент "включить автоматический режим управления датчиками затворов".
        /// </summary>
        private enum Control
        {
            /// <summary>
            /// Параметр "автоматический режим управления датчиками затворов" аргумента.
            /// </summary>
            AUTO
        }

        /// <summary>
        /// Аргумент "выбор канала spacewire".
        /// </summary>
        private enum Channel
        {
            /// <summary>
            /// Параметр "канал spacewire БУК ПК1 - БУСК ПК1" аргумента.
            /// </summary>
            CH1_1,

            /// <summary>
            /// Параметр "канал spacewire БУК ПК1 - БУСК ПК2" аргумента.
            /// </summary>
            CH1_2,

            /// <summary>
            /// Параметр "канал spacewire БУК ПК2 - БУСК ПК1" аргумента.
            /// </summary>
            CH2_1,

            /// <summary>
            /// Параметр "канал spacewire БУК ПК2 - БУСК ПК2" аргумента.
            /// </summary>
            CH2_2
        }

        /// <summary>
        /// Аргумент "выбор формируемую команду".
        /// </summary>
        private enum Command
        {
            /// <summary>
            /// Параметр "сформировать команду TELE" аргумента.
            /// </summary>
            TELE
        }

        /// <summary>
        /// Аргумент "обмен с прибором".
        /// </summary>
        private enum Transmit
        {
            /// <summary>
            /// Параметр "включить обмен с прибором" аргумента.
            /// </summary>
            TRANSMIT_ON,

            /// <summary>
            /// Параметр "выключить обмен с прибором" аргумента.
            /// </summary>
            TRANSMIT_OFF
        }

        /// <summary>
        /// Аргумент "секундная метка".
        /// </summary>
        private enum Ticktime
        {
            /// <summary>
            /// Параметр "включить выдачу секундной метки" аргумента.
            /// </summary>
            TICKTIME_ON,

            /// <summary>
            /// Параметр "выключить выдачу секундной метки" аргумента.
            /// </summary>
            TICKTIME_OFF
        }

        private enum SensorOpen
        {
            SENS_OPEN_ON,
            SENS_OPEN_OFF
        }

        private enum SensorClose
        {
            SENS_CLOSE_ON,
            SENS_CLOSE_OFF
        }

        /// <summary>
        /// Аргумент "КБВ".
        /// </summary>
        private enum OnBoardTime
        {
            /// <summary>
            /// Параметр "включить выдачу КБВ" аргумента.
            /// </summary>
            OBT_ON,

            /// <summary>
            /// Параметр "выключить выдачу КБВ" аргумента.
            /// </summary>
            OBT_OFF
        }

        /// <summary>
        /// Аргумент "выбор полукомплекта".
        /// </summary>
        private enum Sets
        {
            /// <summary>
            /// Параметр "выбрать первый полукомплект" аргумента.
            /// </summary>
            SET1,

            /// <summary>
            /// Параметр "выбрать второй полукомплект" аргумента.
            /// </summary>
            SET2
        }

        /// <summary>
        /// Получает набор доступных цикломанд для устройства.
        /// </summary>
        public CyclogramCommands CyclogramCommandsAvailable { get; private set; }

        /// <summary>
        /// Получает или задает ссылку на экземпляр устройства.
        /// </summary>
        public EgseBukNotify BukNotify { get; set; }

        /// <summary>
        /// Проверка существования параметров у команды.
        /// </summary>
        /// <typeparam name="TEnum">Перечисление параметров.</typeparam>
        /// <param name="cmd">Наименование команды.</param>
        /// <param name="exclude">Список исключенных параметров.</param>
        /// <returns><c>true</c> если параметры существуют, иначе <c>false</c></returns>
        private bool IncludeTest<TEnum>(string cmd, out string errStr, TEnum[] exclude = null)
        {
            if (!GetAllList<TEnum>(exclude).Exists(x => x == cmd))
            {
                errStr = string.Join(@" или ", GetAllList<TEnum>(exclude).ToArray());
                return false;
            }
            else
            {
                errStr = string.Empty;
                return true;
            }
        }

        private bool IncludeExec<TEnum>(string cmd, TEnum[] exclude = null)
        {
            if (GetAllList<TEnum>(exclude).Exists(x => x == cmd))
            {
                System.Reflection.MemberInfo memberInfo = typeof(TEnum).GetMember(cmd).FirstOrDefault();

                if (null != memberInfo)
                {
                    ActionAttribute attribute = (ActionAttribute)memberInfo.GetCustomAttributes(typeof(ActionAttribute), false).FirstOrDefault();
                    if (null != attribute)
                    {
                        attribute.Act(this.BukNotify);
                        return true;
                    }                    
                }  
                
            }

            return false;
        }

        private void IncludeExec<TEnum1, TEnum2>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null)
        {
            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum1>(str, exclude1))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum2>(str, exclude2))
                {
                    cmds.Remove(str);
                    break;
                }
            }
        }

        private bool IncludeTest<TEnum1, TEnum2>(string cmd, out string errStr, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null)
        {
            if (IncludeTest<TEnum2>(cmd, out errStr, exclude2))
            {
                return true;
            }
            
            if (IncludeTest<TEnum1>(cmd, out errStr, exclude1))
            {
                return true;
            }

            return false;
        }

        private bool IncludeTest<TEnum1, TEnum2, TEnum3>(string cmd, out string errStr, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null)
        {
            if (IncludeTest<TEnum3>(cmd, out errStr, exclude3))
            {
                return true;
            }

            if (IncludeTest<TEnum2>(cmd, out errStr, exclude2))
            {
                return true;
            }

            if (IncludeTest<TEnum1>(cmd, out errStr, exclude1))
            {
                return true;
            }

            return false;
        }

        private bool IncludeTest<TEnum1, TEnum2, TEnum3, TEnum4>(string cmd, out string errStr, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null, TEnum4[] exclude4 = null)
        {
            if (IncludeTest<TEnum4>(cmd, out errStr, exclude4))
            {
                return true;
            }

            if (IncludeTest<TEnum3>(cmd, out errStr, exclude3))
            {
                return true;
            }

            if (IncludeTest<TEnum2>(cmd, out errStr, exclude2))
            {
                return true;
            }

            if (IncludeTest<TEnum1>(cmd, out errStr, exclude1))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Формируем список всех параметров для данной команды.
        /// </summary>
        /// <typeparam name="TEnum">Перечисление параметров.</typeparam>
        /// <param name="exclude">Список исключенных параметров.</param>
        /// <returns>Список наименований параметров, для данной команды.</returns>
        private List<string> GetAllList<TEnum>(TEnum[] exclude = null)
        {
            if (null != exclude)
            {
                return Enum.GetNames(typeof(TEnum)).ToList().Except(exclude.Where(x => x != null).Select(x => x.ToString()).ToList()).ToList();
            }
            else
            {
                return Enum.GetNames(typeof(TEnum)).ToList();
            }
        }

        private ActionAttribute Attribute(Enum effectType)
        {
            System.Reflection.MemberInfo memberInfo = effectType.GetType().GetMember(effectType.ToString()).FirstOrDefault();

            if (memberInfo != null)
            {
                ActionAttribute attribute = (ActionAttribute)
                             memberInfo.GetCustomAttributes(typeof(ActionAttribute), false)
                                       .FirstOrDefault();
                return attribute;
            }

            return null;
        }

        /// <summary>
        /// TODO описание
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns>
        /// <c>true</c> если проверка успешно пройдена, иначе <c>false</c>.
        /// </returns>
        private bool PowerTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            errString = Resource.Get(@"eParamCount");

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Device>(cmdParams[0], out errParam, new Device[1] { Device.BUK }))
            {                
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (2 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Line, Sets>(cmdParams[1], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Sets, Switcher>(cmdParams[2], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            // задаемся минимальным количеством параметров для команды (3).
            isParamCountErr = false;

            if (4 > cmdParams.Length)
            {
                // если количество параметров минимально - второй аргумент обязан быть параметром "Sets".
                if (!IncludeTest<Sets>(cmdParams[1], out errParam))
                {
                    errString = string.Format(Resource.Get(@"eArg2"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            if (!IncludeTest<Switcher>(cmdParams[3], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg4"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (4).
            isParamCountErr = true;

            if (4 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Powers the execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool PowerExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gates the test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool ShutterTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            errString = Resource.Get(@"eParamCount");

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // устанавливаем минимальное количество параметров в команде.
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                // если минимальное количество параметров (1) проверить что это параметр "Control".
                if (!IncludeTest<Control>(cmdParams[0], out errParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            // если же команд более 1 - первый аргумент обязан быть параметром "Scidev".
            if (!IncludeTest<Scidev>(cmdParams[0], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            // следующий параметр может варьироваться.
            if (!IncludeTest<SensorOpen, SensorClose>(cmdParams[1], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SensorClose>(cmdParams[2], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (3).
            isParamCountErr = true;

            if (3 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gates the execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool ShutterExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// KVVs the imit test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool KvvImitTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            errString = Resource.Get(@"eParamCount");

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Sets>(cmdParams[0], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (2 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Switcher, Halfset>(cmdParams[1], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            // задаемся минимальным количеством параметров для команды (2).
            isParamCountErr = false;

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Halfset, State>(cmdParams[2], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }
            
            if (4 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<State>(cmdParams[3], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg4"), errParam);
                return false;
            }

            if (5 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<State>(cmdParams[4], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg5"), errParam);
                return false;
            }

            if (6 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<State>(cmdParams[5], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg6"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (6).
            isParamCountErr = true;

            if (6 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            return true;
        }

        /// <summary>
        /// KVVs the imit execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool KvvImitExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busks the imit test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool Bm4ImitTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            errString = Resource.Get(@"eParamCount");

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // устанавливаем минимальное количество параметров в команде.
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                // если минимальное количество параметров (1) проверить что это параметр "Control".
                if (!IncludeTest<Control>(cmdParams[0], out errParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            // если же команд более 1 - первый аргумент обязан быть параметром "Scidev".
            if (!IncludeTest<Scidev>(cmdParams[0], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            // следующий параметр может варьироваться.
            if (!IncludeTest<SensorOpen, SensorClose>(cmdParams[1], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SensorClose>(cmdParams[2], out errParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (3).
            isParamCountErr = true;

            if (3 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Busks the imit execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool Bm4ImitExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busks the imit command test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool Bm4ImitCmdTest(string[] cmdParams, out string errString)
        {
            errString = string.Empty;
            switch (cmdParams.Length)
            {
                case 1:
                    break;
                case 2:
                    break;
                default:
                    errString = Resource.Get(@"eParamCount");
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Busks the imit command execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool Bm4ImitCmdExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busks the imit command file test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool BukKvvImitTest(string[] cmdParams, out string errString)
        {
            errString = string.Empty;
            switch (cmdParams.Length)
            {
                case 1:
                    break;
                default:
                    errString = Resource.Get(@"eParamCount");
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Busks the imit command file execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool BukKvvImitExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busks the imit logic test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool BukKvvImitCmdTest(string[] cmdParams, out string errString)
        {
            errString = string.Empty;
            switch (cmdParams.Length)
            {
                case 2:
                    break;
                default:
                    errString = Resource.Get(@"eParamCount");
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Busks the imit logic execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool BukKvvImitCmdExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Scidevs the imit test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool DetectorImitTest(string[] cmdParams, out string errString)
        {
            errString = string.Empty;
            switch (cmdParams.Length)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    errString = Resource.Get(@"eParamCount");
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Scidevs the imit execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool DetectorImitExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }
    }
}
