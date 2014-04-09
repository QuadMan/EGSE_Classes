//-----------------------------------------------------------------------
// <copyright file="EGSEUtilitesTextlog.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Мурзин Святослав</author>
//-----------------------------------------------------------------------

// TODO в ~TxtLog() разобраться с некорректным закрытием файла! пока после выполнения всех записей в log-файл необходимо вызвать метод FlushAll()
// TODO указать в конструкторе путь, где создавать структуру директорий в формате ГГММ/ДД/
// TODO при создании лог-файла необходимо перед расширением добавлять следующий текст "_HHMMSS"
// TODO при закрытии лог-файла проверяем, если размер его равен 0, то стираем файл
namespace Egse.Utilites
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Класс единичного текстлога
    /// </summary>
    public class TxtLogger : IDisposable
    {
        /// <summary>
        /// Имя файла
        /// Объект StreamWriter, работающий с log-файлом
        /// </summary>
        private string _fileName;

        /// <summary>
        /// Разрешена/запрещена запись в log-файл
        /// </summary>
        private bool _enableTextWrite;

        /// <summary>
        /// Разрешена/запрещена запись времени в log-файл
        /// </summary>
        private bool _enableTimeWrite;
        
        /// <summary>
        /// Объект StreamWriter, работающий с log-файлом
        /// </summary>
        private StreamWriter _sw;

        private bool isNeedNewLog;

        /// <summary>
        /// Объект DateTime, хранящий текущее время : (HH:MM:SS:MMS) с log-файлом
        /// </summary>
        private DateTime _logtime;
        
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TxtLogger" />.
        /// </summary>
        /// <param name="fileName">Имя лог-файла</param>
        public TxtLogger(string fileName)
        {
            _enableTextWrite = true;
            _enableTimeWrite = true;
            _logtime = new DateTime();
            _fileName = MakeLoggerDir() + "\\" + GetLoggerFileName(fileName);
            _sw = new StreamWriter(_fileName, true);
        }

        public void NewLog()
        { 
          
        }

        /// <summary>
        /// Получает наименование лог-файла.
        /// </summary>
        /// <value>
        /// Наименование лог-файла.
        /// </value>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        /// <summary>
        /// Получает или задает метод, реализующий запись текста и времени в log-файл.
        /// Примечание: 
        /// Запись осуществляется только если _enableTextWrite == true.
        /// Запись времени осуществляется только если значение _enableTimeWrite == true.
        /// </summary>
        public string LogText
        {
            get
            {
                return null;
            }

            set
            {
                if (_enableTextWrite)
                {
                    if (_enableTimeWrite)
                    {
                        _logtime = DateTime.Now;
                        _sw.Write("{0:d2}:{1:d2}:{2:d2}:{3:d3} ", _logtime.Hour, _logtime.Minute, _logtime.Second, _logtime.Millisecond);
                    }

                    _sw.WriteLine(value);

                    // введена для отладки USB, может быть вдальнейшем ввести параметр лога
                    _sw.Flush();
                }
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, можно ли производить запись текста в log-файл.
        /// По умолчанию: разрешена.
        /// </summary>
        public bool LogEnable
        {
            get
            {
                return _enableTextWrite;
            }

            set
            {
                _enableTextWrite = value; 
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, разрешена ли запись времени в log-файл.
        /// По умолчанию: разрешена.
        /// </summary>
        public bool LogTimeEnable
        {
            get 
            {
                return _enableTimeWrite;
            }

            set 
            {
                _enableTimeWrite = value;
            }
        }        

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Метод по имени файла возвращает полное имя файла с текущей датой в имени
        /// </summary>
        /// <param name="strFileName">Имя файла/или путь</param>
        /// <returns>Полное имя файла</returns>
        public string GetLoggerFileName(string strFileName)
        {
            string[] strName;
            string strRes = null;

            _logtime = DateTime.Now;

            strName = strFileName.Split(new char[] { '.' });
            strRes += strName[0];
            for (int i = 1; i < strName.GetLength(0) - 1; i++)
            {
                strRes += "." + strName[i];
            }

            strRes += string.Format("_{0:d2}_{1:d2}{2:d2}{3:d2}.", _logtime.Day, _logtime.Hour, _logtime.Minute, _logtime.Second);
            strRes += strName[strName.GetLength(0) - 1];

            return strRes;
        }

        /// <summary>
        /// Метод создает директорию в папке с исполнительным кодом программы
        /// </summary>
        /// <returns>Строка с новой директорией</returns>
        public string MakeLoggerDir()
        {
            string strRes = null;

            strRes = GetLoggerDir();
            Directory.CreateDirectory(strRes);
            return strRes;
        }

        /// <summary>
        /// Метод определяет директорию, в которой будут хранится текстлоги
        /// </summary>
        /// <returns>Строка с директорией</returns>
        public string GetLoggerDir()
        {
            string strRes = null;

            _logtime = DateTime.Now;
            strRes = Directory.GetCurrentDirectory().ToString();
            if (!strRes.EndsWith("\\") && !strRes.EndsWith("/"))
            {
                strRes += "\\";
            }

            strRes += "LOGS\\";
            strRes += _logtime.Year.ToString().Substring(2);
            strRes += string.Format("{0:d2}", _logtime.Month);
            return strRes;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // освобождаем неуправляемые ресурсы
                _sw.Close();
            }
            //// тут освобождаем управляемые ресурсы
        }
    }

    /// <summary>
    /// Класс множества текстлогов
    /// </summary>
    public class TxtLoggers
    {
        /// <summary>
        /// txtLoggers   -  Коллекция единичных логгеров
        /// </summary>
        private List<TxtLogger> txtLoggers;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TxtLoggers" />.
        /// </summary>
        public TxtLoggers()
        {
            txtLoggers = new List<TxtLogger>();
        }

        /// <summary>
        /// Задает значение, показывающее, разрешена ли запись во все log-файлы.
        /// </summary>
        public bool GlobalEnable
        {
            set
            {
                foreach (TxtLogger tl in txtLoggers)
                {
                    tl.LogEnable = value;
                }
            }
        }

        /// <summary>
        /// Индексатор для текстлога.
        /// </summary>
        /// <param name="i">Индекс текстлога</param>
        /// <returns>Экземпляр текстлога</returns>
        public TxtLogger this[int i]
        {
            get
            {
                return txtLoggers[i];
            }
        }

        /// <summary>
        /// Метод, создающий новый log-файл.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void AddFile(string filename)
        {
            txtLoggers.Add(new TxtLogger(filename));
        }

        /// <summary>
        /// Метод, очищающий все буфера и производящий сброс всей информации 
        /// непосредственно в соответствующие log-файлы
        /// </summary>
        public void FlushAll()
        {
            foreach (TxtLogger tl in txtLoggers)
            {
                tl.Dispose();
            }
        }
    }
}
