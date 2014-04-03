//-----------------------------------------------------------------------
// <copyright file="EGSEDefaultLoggers.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Defaults
{
    using System;
    using EGSE.Utilites;
   
    /// <summary>
    /// Класс логгеров (синглетон)
    /// </summary>
    public sealed class LogsClass
    {
        /// <summary>
        /// Список имен лог-файлов.
        /// </summary>
        private static string[] logsNames = new string[6] 
        { 
            Resource.Get("stMainLog"),
            Resource.Get("stOperatorLog"),
            Resource.Get("stHSILog"),
            Resource.Get("stErrorsLog"),
            Resource.Get("stUSBLog"),
            Resource.Get("stSpacewire2")
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
        private TxtLoggers _loggers;

        /// <summary>
        /// Предотвращает вызов конструктора по умолчанию для класса <see cref="LogsClass" />.
        /// </summary>
        private LogsClass()
        {
            this._loggers = new TxtLoggers();
            foreach (string fileName in logsNames)
            {
                this._loggers.AddFile(fileName);
            }
        }

        /// <summary>
        /// Получает основной лог.
        /// </summary>
        public static TxtLogger LogMain
        {
            get
            {
                return Instance._loggers[0];
            }            
        }

        /// <summary>
        /// Получает лог для интерфейса spacewire2.
        /// </summary>
        public static TxtLogger LogSpacewire2
        {
            get
            {
                return Instance._loggers[5];
            }
        }

        /// <summary>
        /// Получает лог оператора.
        /// </summary>
        public static TxtLogger LogOperator
        {
            get
            {
                return Instance._loggers[1];
            }
        }

        /// <summary>
        /// Получает лог ВСИ.
        /// </summary>
        public static TxtLogger LogHSI
        {
            get
            {
                return Instance._loggers[2];
            }
        }

        /// <summary>
        /// Получает лог ошибок.
        /// </summary>
        public static TxtLogger LogErrors
        {
            get
            {
                return Instance._loggers[3];
            }
        }

        /// <summary>
        /// Получает лог USB.
        /// </summary>
        public static TxtLogger LogUSB
        {
            get
            {
                return Instance._loggers[4];
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
