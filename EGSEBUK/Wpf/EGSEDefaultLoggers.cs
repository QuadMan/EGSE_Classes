//-----------------------------------------------------------------------
// <copyright file="EGSEDefaultLoggers.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Defaults
{
    using Egse.Utilites;
   
    /// <summary>
    /// Класс логгеров (синглетон)
    /// </summary>
    public sealed class LogsClass
    {
        /// <summary>
        /// Список имен лог-файлов.
        /// </summary>
        private static string[] logsNames = new string[9] 
        { 
            Resource.Get("stMainLog"),
            Resource.Get("stOperatorLog"),
            Resource.Get("stHsiLog"),
            Resource.Get("stErrorsLog"),
            Resource.Get("stProtoEncoderLog"),
            Resource.Get("stSpacewire2"),
            Resource.Get("stSpacewire3"),
            Resource.Get("stUSBLog"),
            Resource.Get("stCycloLog")
        };

        /// <summary>
        /// Единый экземпляр класса
        /// </summary>
        private static volatile LogsClass instance;

        /// <summary>
        /// Экземпляр объекта для критической блокировки.
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// Множество экземпляров лог-класса.
        /// </summary>
        private TxtLoggers loggers;

        /// <summary>
        /// Предотвращает вызов конструктора по умолчанию для класса <see cref="LogsClass" />.
        /// </summary>
        private LogsClass()
        {
            this.loggers = new TxtLoggers();
            foreach (string fileName in logsNames)
            {
                this.loggers.AddFile(fileName);
            }
        }

        /// <summary>
        /// Получает основной лог.
        /// </summary>
        public static TxtLogger LogMain
        {
            get
            {
                return Instance.loggers[0];
            }            
        }

        /// <summary>
        /// Получает лог для интерфейса spacewire2.
        /// </summary>
        public static TxtLogger LogSpacewire2
        {
            get
            {
                return Instance.loggers[5];
            }
        }

        /// <summary>
        /// Получает лог для интерфейса spacewire3.
        /// </summary>
        public static TxtLogger LogSpacewire3
        {
            get
            {
                return Instance.loggers[6];
            }
        }

        /// <summary>
        /// Получает лог оператора.
        /// </summary>
        public static TxtLogger LogOperator
        {
            get
            {
                return Instance.loggers[1];
            }
        }

        /// <summary>
        /// Получает лог ВСИ.
        /// </summary>
        public static TxtLogger LogHsi
        {
            get
            {
                return Instance.loggers[2];
            }
        }

        /// <summary>
        /// Получает лог ошибок.
        /// </summary>
        public static TxtLogger LogErrors
        {
            get
            {
                return Instance.loggers[3];
            }
        }

        /// <summary>
        /// Получает лог кодировщика протокола.
        /// </summary>
        public static TxtLogger LogEncoder
        {
            get
            {
                return Instance.loggers[4];
            }
        }

        /// <summary>
        /// Получает USB-лог (исходящий).
        /// </summary>
        public static TxtLogger LogUSB
        {
            get
            {
                return Instance.loggers[7];
            }
        }

        public static TxtLogger LogCyclo
        {
            get
            {
                return Instance.loggers[8];
            }
        }


        /// <summary>
        /// Получает экземпляр класса.
        /// </summary>
        public static LogsClass Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new LogsClass();
                        }
                    }
                }

                return instance;
            }
        }
    }
}
