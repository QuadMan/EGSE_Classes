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
    public class CyclogramCommandBUK
    {
        /// <summary>
        /// Параметры для выбора канала команд.
        /// </summary>
        private List<string> _channelCMD = new List<string>()
        {
            "CMD_CH_NONE",
            "CMD_CH_MAIN",
            "CMD_CH_RES",
            "CMD_CH_MAIN_RES"
        };

        /// <summary>
        /// Параметры для выбора канала данных.
        /// </summary>
        private List<string> _channelDAT = new List<string>()
        {
            "DAT_CH_NONE",
            "DAT_CH_MAIN",
            "DAT_CH_RES",
            "DAT_CH_MAIN_RES"
        };

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramCommandBUK" />.
        /// </summary>
        public CyclogramCommandBUK()
        {
            CyclogramCommandsAvailable = new CyclogramCommands();
            CyclogramCommandsAvailable.AddCommand("POWER", new CyclogramLine("POWER", PowerTest, PowerExec, string.Empty));
        }

        /// <summary>
        /// Получает набор доступных цикломанд для устройства.
        /// </summary>
        public CyclogramCommands CyclogramCommandsAvailable { get; private set; }

        /// <summary>
        /// Получает или задает ссылку на экземпляр устройства.
        /// </summary>
        public IBUK BUK { private get; set; }

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
        /// <param name="errString">Сообщение об ошибке (в случае таковой)</param>
        /// <returns>False - если произошла ошибка</returns>
        public bool XsanControlTest(string[] cmdParams, out string errString)
        {
            errString = string.Empty;
            switch (cmdParams.Length)
            {
                case 5:
                    if ((cmdParams[0] != "ON") && (cmdParams[0] != "OFF"))
                    {
                        errString = "Ошибка параметра: должно быть ON или OFF";
                        return false;                        
                    }

                    int idx = _channelCMD.IndexOf(cmdParams[1]);
                    if (idx == -1)
                    {
                        errString = "Ошибка параметра: должно быть " + string.Join(" ", _channelCMD.ToArray());
                        return false;
                    }

                    idx = _channelDAT.IndexOf(cmdParams[2]);
                    if (idx == -1)
                    {
                        errString = "Ошибка параметра: должно быть " + string.Join(" ", _channelDAT.ToArray());
                        return false;
                    }

                    if ((cmdParams[3] != "BUSY_ON") && (cmdParams[3] != "BUSY_OFF"))
                    {
                        errString = "Ошибка параметра: должно быть BUSY_ON или BUSY_OFF";
                        return false;                        
                    }

                    if ((cmdParams[4] != "ME_ON") && (cmdParams[4] != "ME_OFF"))
                    {
                        errString = "Ошибка параметра: должно быть ME_ON или ME_OFF";
                        return false;
                    }

                    break;
                default:
                    errString = "Ошибочное количество параметров команды!";
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Выполнение управления
        /// </summary>
        /// <param name="cmdParams">Передаваемые параметры</param>
        /// <returns>False - если ошибка</returns>
        public bool BUKControlExec(string[] cmdParams)
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
        /// <param name="errString">Сообщение об ошибке</param>
        /// <returns>False - если ошибка</returns>
        public bool UksTest(string[] cmdParams, out string errString)
        {
            errString = string.Empty;
            if ((cmdParams == null) || (cmdParams.Length > 62) || (cmdParams.Length < 1))
            {
                errString = "Должно быть задано от 1 до 62 байт данных УКС";
                return false;
            }

            try
            {
                EGSE.Utilites.Converter.HexStrToByteArray(cmdParams);
            }
            catch
            {
                errString = "Ошибка преобразования значений УКС";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Выполнить УКС.
        /// </summary>
        /// <param name="cmdParams">Команда УКС</param>
        /// <returns>True - всегда!!!</returns>
        public bool UksExec(string[] cmdParams)
        {
            byte[] uksData = EGSE.Utilites.Converter.HexStrToByteArray(cmdParams);
            BUK.Device.CmdSendUKS(uksData);

            return true;
        }
        
        /// <summary>
        /// Команда POWER - выдача питания устройства
        /// </summary>
        /// <param name="cmdParams">Возможные ON|OFF</param>
        /// <param name="errString">Сообщение об ошибке</param>
        /// <returns>False - если ошибка</returns>
        public bool PowerTest(string[] cmdParams, out string errString)
        {
            errString = string.Empty;
            if (!((cmdParams.Length == 1) && ((cmdParams[0] == "ON") || (cmdParams[0] == "OFF"))))
            {
                errString = "Ошибка формата команды: должно быть указано ON или OFF";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Выполнить команду POWER
        /// </summary>
        /// <param name="cmdParams">Параметры команды</param>
        /// <returns>False - если не удалось выполнить команду</returns>
        public bool PowerExec(string[] cmdParams)
        {
            int val = Convert.ToInt32(cmdParams[0] == "ON");
            return BUK.ControlValuesList[BUKConst.PowerCTRL].SetProperty(BUKConst.PowerCTRL, val);
        }
    }
}
