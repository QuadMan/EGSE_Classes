//-----------------------------------------------------------------------
// <copyright file="EGSECyclogramCommandBUK.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Cyclogram.Command
{    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;   
    using EGSE.Constants;
    using EGSE.Cyclogram;
    using EGSE.Devices;
    using EGSE.Utilites;

    /// <summary>
    /// Инициализирует набор доступных цикломанд для устройства.
    /// </summary>
    public class CyclogramCommandBuk
    {
        /// <summary>
        /// Аргумент "переключатель состояния".
        /// </summary>
        private enum switcher
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
        private enum device
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
        private enum scidev
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
        private enum halfset 
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
        private enum line
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
        private enum channel
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
        private enum command
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
        private enum transmit
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
        private enum ticktime
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
        private enum onboardtime
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
        private enum set
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

        private bool IncludeTest<TEnum>(string cmd, TEnum[] exclude = null)
        {
            return GetAllList<TEnum>(exclude).Exists(x => x == cmd);
        }

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

        private bool PowerTest(string[] cmdParams, out string errString)
        {
            errString = string.Empty;
            switch (cmdParams.Length)
            {
                case 3:
                    {
                        if (!IncludeTest<device>(cmdParams[0], new device[1] { device.BUK }))
                        {
                            errString = string.Format(Resource.Get(@"eArg1"), string.Join(" или ", GetAllList<device>(new device[1] { device.BUK }).ToArray()));
                            return false;
                        }

                        if (!IncludeTest<set>(cmdParams[1]))
                        {
                            errString = string.Format(Resource.Get(@"eArg2"), string.Join(" или ", GetAllList<set>().ToArray()));
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

        private bool PowerExec(string[] Params)
        {
            throw new NotImplementedException();
        }

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

        private bool GateExec(string[] Params)
        {
            throw new NotImplementedException();
        }

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

        private bool KvvImitExec(string[] Params)
        {
            throw new NotImplementedException();
        }

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

        private bool BuskImitExec(string[] Params)
        {
            throw new NotImplementedException();
        }

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

        private bool BuskImitCmdExec(string[] Params)
        {
            throw new NotImplementedException();
        }

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

        private bool BuskImitCmdFileExec(string[] Params)
        {
            throw new NotImplementedException();
        }

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

        private bool BuskImitLogicExec(string[] Params)
        {
            throw new NotImplementedException();
        }

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

        private bool ScidevImitExec(string[] Params)
        {
            throw new NotImplementedException();
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
        /// Команда XSAN 
        /// ON|OFF 
        /// CMD_CH_NONE|CMD_CH_MAIN|CMD_CH_RES|CMD_CH_MAIN_RES 
        /// DAT_CH_NONE|DAT_CH_MAIN|DAT_CH_RES|DAT_CH_MAIN_RES 
        /// BUSY_ON|BUSY_OFF
        /// ME_ON|ME_OFF
        /// В случае использования параметра XSAN ON необходимо прописать все значения
        /// В случае XSAN OFF ничего писать не нужно
        /// </summary>
        /// <param name="cmdParams">Передаваемые параметры</param>
        ///// <param name="errString">Сообщение об ошибке (в случае таковой)</param>
        ///// <returns>False - если произошла ошибка</returns>
        //public bool BUKControlTest(string[] cmdParams, out string errString)
        //{
        //    errString = string.Empty;
        //    switch (cmdParams.Length)
        //    {
        //        case 5:
        //            if ((cmdParams[0] != "ON") && (cmdParams[0] != "OFF"))
        //            {
        //                errString = "Ошибка параметра: должно быть ON или OFF";
        //                return false;                        
        //            }

        //            int idx = _channelCMD.IndexOf(cmdParams[1]);
        //            if (idx == -1)
        //            {
        //                errString = "Ошибка параметра: должно быть " + string.Join(" ", _channelCMD.ToArray());
        //                return false;
        //            }

        //            idx = _channelDAT.IndexOf(cmdParams[2]);
        //            if (idx == -1)
        //            {
        //                errString = "Ошибка параметра: должно быть " + string.Join(" ", _channelDAT.ToArray());
        //                return false;
        //            }

        //            if ((cmdParams[3] != "BUSY_ON") && (cmdParams[3] != "BUSY_OFF"))
        //            {
        //                errString = "Ошибка параметра: должно быть BUSY_ON или BUSY_OFF";
        //                return false;                        
        //            }

        //            if ((cmdParams[4] != "ME_ON") && (cmdParams[4] != "ME_OFF"))
        //            {
        //                errString = "Ошибка параметра: должно быть ME_ON или ME_OFF";
        //                return false;
        //            }

        //            break;
        //        default:
        //            errString = "Ошибочное количество параметров команды!";
        //            return false;
        //    }

        //    return true;
        //}

        /// <summary>
        /// Выполнение управления
        /// </summary>
        /// <param name="cmdParams">Передаваемые параметры</param>
        /// <returns>False - если ошибка</returns>
       /* public bool BUKControlExec(string[] cmdParams)
        {
            switch (cmdParams.Length)
            {
                case 5:
                    int onOffParam = cmdParams[0] == "ON" ? 1 : 0;
                    BUK.ControlValuesList[BUKConst.BUKCTRL].RefreshGetValue();  // вызываем для обновления галочек на экране
                    break;
                default:
                    return false;
            }

            return true;
        }

        /*
        const string BUNI_CMD_FORMAT_STR = "BUNI ON|OFF CMD_CH_MAIN|CMD_CH_RES DAT_CH_NONE|DAT_CH_MAIN|DAT_CH_RES|DAT_CH_MAIN_RES TIME_ON|TIME_OFF OBT_ON|OBT_OFF";

        /// <summary>
        /// Команда BUNI
        /// ON|OFF 
        /// CMD_CH_MAIN|CMD_CH_RES
        /// DAT_CH_NONE|DAT_CH_MAIN|DAT_CH_RES|DAT_CH_MAIN_RES 
        /// TIME_ON|TIME_OFF
        /// OBT_ON|OBT_OFF
        /// В случае использования параметра BUNI ON необходимо прописать все значения
        /// В случае BUNI OFF ничего писать не нужно
        /// </summary>
        /// <param name="Params"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        public bool BuniControlTest(string[] Params, out string errString)
        {
            errString = String.Empty;
            switch (Params.Length)
            {
                case 5:
                    if ((Params[0] != "ON") && (Params[0] != "OFF"))
                    {
                        errString = "Первый параметр должен быть ON или OFF";
                        return false;
                    }

                    int idx = BUKCMD.IndexOf(Params[1]);
                    if (idx == -1)
                    {
                        errString = "Второй параметр должен быть " + string.Join(" ", BUKCMD.ToArray());
                        return false;
                    }

                    idx = BUKDAT.IndexOf(Params[2]);
                    if (idx == -1)
                    {
                        errString = "Третий параметр должен быть " + string.Join(" ", BUKDAT.ToArray());
                        return false;
                    }

                    if ((Params[3] != "TIME_ON") && (Params[3] != "TIME_OFF"))
                    {
                        errString = "Четвертый параметр должен быть TIME_ON | TIME_OFF";
                        return false;
                    }

                    if ((Params[4] != "OBT_ON") && (Params[4] != "OBT_OFF"))
                    {
                        errString = "Пятый параметр должен быть OBT_ON|OBT_OFF";
                        return false;
                    }

                    break;
                default:
                    errString = "Ошибочное количество параметров команды: " + BUNI_CMD_FORMAT_STR;
                    return false;
            }

            return true;
        }
        
        public bool BuniControlExec(string[] Params)
        {
            switch (Params.Length)
            {
                case 5:
                    int onOffParam = Params[0] == "ON" ? 1 : 0;
                    /*BUK.ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_ON_IDX, onOffParam, false);
                    BUK.ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_CMD_CH_IDX, BUNI_CMD_LIST.IndexOf(Params[1]), false);
                    BUK.ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_DAT_CH_IDX, XSAN_DAT_LIST.IndexOf(Params[2]), false);
                    BUK.ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_HZ_IDX, Convert.ToInt16(Params[3] == "TIME_ON"), false);
                    BUK.ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_KBV_IDX, Convert.ToInt16(Params[4] == "OBT_ON"));
                    BUK.ControlValuesList[BUKConst.BUNI_CTRL_IDX].RefreshGetValue();
                    break;
                default:
                    return false;
            }
            return true;
        }*/

        /// <summary>
        /// Проверка на корректность УКС.
        /// Команда UKS - Выдача УКС.
        /// </summary>
        /// <param name="cmdParams">Передаваемые параметры УКС: BYTE1..BYTE62</param>
        ///// <param name="errString">Сообщение об ошибке.</param>
        ///// <returns>False - если ошибка.</returns>
        //public bool UksTest(string[] cmdParams, out string errString)
        //{
        //    errString = string.Empty;
        //    if ((cmdParams == null) || (cmdParams.Length > 62) || (cmdParams.Length < 1))
        //    {
        //        errString = "Должно быть задано от 1 до 62 байт данных УКС";
        //        return false;
        //    }

        //    try
        //    {
        //        EGSE.Utilites.Converter.HexStrToByteArray(cmdParams);
        //    }
        //    catch
        //    {
        //        errString = "Ошибка преобразования значений УКС";
        //        return false;
        //    }

        //    return true;
        //}

        /////// <summary>
        /////// Выполнить УКС.
        /////// </summary>
        /////// <param name="cmdParams">Команда УКС</param>
        /////// <returns>True - всегда!!!</returns>
        ////public bool UksExec(string[] cmdParams)
        ////{
        ////    byte[] uksData = EGSE.Utilites.Converter.HexStrToByteArray(cmdParams);
        ////    BUK.Device.CmdSendUKS(uksData);

        ////    return true;
        ////}
        
        /// <summary>
        /// Команда POWER - выдача питания устройства.
        /// </summary>
        /// <param name="cmdParams">Возможные ON|OFF.</param>
        /// <param name="errString">Сообщение об ошибке.</param>
        /// <returns>False - если ошибка.</returns>
        //public bool PowerTest(string[] cmdParams, out string errString)
        //{
        //    errString = string.Empty;
        //    if (!((cmdParams.Length == 1) && ((cmdParams[0] == "ON") || (cmdParams[0] == "OFF"))))
        //    {
        //        errString = "Ошибка формата команды: должно быть указано ON или OFF";
        //        return false;
        //    }

        //    return true;
        //}
    }
}
