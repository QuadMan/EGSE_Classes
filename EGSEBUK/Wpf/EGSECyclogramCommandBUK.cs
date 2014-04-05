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
            CyclogramCommandsAvailable.AddCommand("efc854841c554c529966abe5f1e6c2d7", new CyclogramLine("GATE", GateTest, GateExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("b1f06daf0810463892707e7edf5d93be", new CyclogramLine("KVV_IMIT", KvvImitTest, KvvImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("e333bfb23a204e748699528fb787012c", new CyclogramLine("BUSK_IMIT", BuskImitTest, BuskImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("9e3e94f708eb44819a8ea9d01aa321e9", new CyclogramLine("BUSK_IMIT_CMD", BuskImitCmdTest, BuskImitCmdExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("2e92651815794233bc062365b3e8278b", new CyclogramLine("BUSK_IMIT_CMD_FILE", BuskImitCmdFileTest, BuskImitCmdFileExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("ec4d4c82d1044d3fbcad59d4403c54aa", new CyclogramLine("BUSK_IMIT_LOGIC", BuskImitLogicTest, BuskImitLogicExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("bdeae631daf8489983760003e615852a", new CyclogramLine("SCIDEV_IMIT", ScidevImitTest, ScidevImitExec, string.Empty));
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

        /// <summary>
        /// Аргумент "выбор НП".
        /// </summary>
        private enum Scidev
        {
            /// <summary>
            /// Параметр "НП УФЕС" аргумента.
            /// </summary>
            UFES,

            /// <summary>
            /// Параметр "НП ВУФЕС" аргумента.
            /// </summary>
            VUFES,

            /// <summary>
            /// Параметр "НП СДЩ" аргумента.
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
            /// Параметр "сформировать команду RMAP" аргумента.
            /// </summary>
            RMAP,

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

        /// <summary>
        /// Аргумент "КБВ".
        /// </summary>
        private enum Onboardtime
        {
            /// <summary>
            /// Параметр "включить выдачу КБВ" аргумента.
            /// </summary>
            ONBOARDTIME_ON,

            /// <summary>
            /// Параметр "выключить выдачу КБВ" аргумента.
            /// </summary>
            ONBOARDTIME_OFF
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
        private bool IncludeTest<TEnum>(string cmd, TEnum[] exclude = null)
        {
            return GetAllList<TEnum>(exclude).Exists(x => x == cmd);
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
            errString = string.Empty;
            switch (cmdParams.Length)
            {
                case 3:
                    {
                        if (!IncludeTest<Device>(cmdParams[0], new Device[1] { Device.BUK }))
                        {
                            errString = string.Format(Resource.Get(@"eArg1"), string.Join(" или ", GetAllList<Device>(new Device[1] { Device.BUK }).ToArray()));
                            return false;
                        }

                        if (!IncludeTest<Sets>(cmdParams[1]))
                        {
                            errString = string.Format(Resource.Get(@"eArg2"), string.Join(" или ", GetAllList<Sets>().ToArray()));
                            return false;
                        }  
                    }

                    break;
                case 4:
                    break;
                default:
                    errString = Resource.Get(@"eParamCount");
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
        private bool GateTest(string[] cmdParams, out string errString)
        {
            errString = string.Empty;
            switch (cmdParams.Length)
            {
                case 3:
                    break;
                default:
                    errString = Resource.Get(@"eParamCount");
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
        private bool GateExec(string[] cmdParams)
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
        private bool BuskImitTest(string[] cmdParams, out string errString)
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
                case 4:
                    break;
                case 5:
                    break;
                default:
                    errString = Resource.Get(@"eParamCount");
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
        private bool BuskImitExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busks the imit command test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool BuskImitCmdTest(string[] cmdParams, out string errString)
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
        private bool BuskImitCmdExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busks the imit command file test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool BuskImitCmdFileTest(string[] cmdParams, out string errString)
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
        private bool BuskImitCmdFileExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busks the imit logic test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool BuskImitLogicTest(string[] cmdParams, out string errString)
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
        private bool BuskImitLogicExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Scidevs the imit test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool ScidevImitTest(string[] cmdParams, out string errString)
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
        private bool ScidevImitExec(string[] cmdParams)
        {
            throw new NotImplementedException();
        }
    }
}
