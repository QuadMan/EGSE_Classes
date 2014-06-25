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
        private string fileName;

        /// <summary>
        /// Разрешена/запрещена запись в log-файл
        /// </summary>
        private bool enableTextWrite;

        /// <summary>
        /// Разрешена/запрещена запись времени в log-файл
        /// </summary>
        private bool enableTimeWrite;

        /// <summary>
        /// Объект StreamWriter, работающий с log-файлом
        /// </summary>
        private StreamWriter streamWriter;

        /// <summary>
        /// Необходимо начать новый текстовый лог-файл.
        /// </summary>
        private bool isNeedNewLog;

        /// <summary>
        /// Объект DateTime, хранящий текущее время : (HH:MM:SS:MMS) с log-файлом
        /// </summary>
        private DateTime logTime;

        /// <summary>
        /// Наименование лог-файла по-умолчанию.
        /// Примечание:
        /// Загружается из ресурсов.
        /// </summary>
        private string defaultFileName;
        
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TxtLogger" />.
        /// </summary>
        /// <param name="fileName">Имя лог-файла</param>
        public TxtLogger(string fileName)
        {
            this.enableTextWrite = true;
            this.enableTimeWrite = true;
            this.defaultFileName = fileName;
            this.isNeedNewLog = true;
            //MakeNewFile(this.defaultFileName);       
        }

        public event FileSystemEventHandler GotLogChange;

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
                return this.fileName;
            }
        }

        public long FileSize
        {
            get
            {
                return !string.IsNullOrEmpty(FileName) ? new FileInfo(FileName).Length : 0;
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
                if (this.enableTextWrite)
                {
                    if (this.isNeedNewLog)
                    {
                        this.isNeedNewLog = false;
                        if (null != this.streamWriter)
                        {
                            this.streamWriter.Close();
                        }
                        MakeNewFile(this.defaultFileName);
                    }

                    if (this.enableTimeWrite)
                    {
                        this.logTime = DateTime.Now;
                        this.streamWriter.Write("{0:d2}:{1:d2}:{2:d2}:{3:d3} ", this.logTime.Hour, this.logTime.Minute, this.logTime.Second, this.logTime.Millisecond);
                    }

                    this.streamWriter.WriteLine(value);

                    // введена для отладки USB, может быть вдальнейшем ввести параметр лога
                    this.streamWriter.Flush();
                }
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, можно ли производить запись текста в log-файл.
        /// По умолчанию: разрешена.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return this.enableTextWrite;
            }

            set
            {
                this.enableTextWrite = value; 
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
                return this.enableTimeWrite;
            }

            set 
            {
                this.enableTimeWrite = value;
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
        /// Инициализация нового текстового лог-файла.
        /// </summary>
        public void NewLog()
        {
            this.isNeedNewLog = true;
            LogText = Resource.Get(@"stNewLog");
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

            this.logTime = DateTime.Now;

            strName = strFileName.Split(new char[] { '.' });
            strRes += strName[0];
            for (int i = 1; i < strName.GetLength(0) - 1; i++)
            {
                strRes += "." + strName[i];
            }

            strRes += string.Format("_{0:d2}_{1:d2}{2:d2}{3:d2}.", this.logTime.Day, this.logTime.Hour, this.logTime.Minute, this.logTime.Second);
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

            this.logTime = DateTime.Now;
            strRes = Directory.GetCurrentDirectory().ToString();
            if (!strRes.EndsWith("\\") && !strRes.EndsWith("/"))
            {
                strRes += "\\";
            }

            strRes += "LOGS\\";
            strRes += this.logTime.Year.ToString().Substring(2);
            strRes += string.Format("{0:d2}", this.logTime.Month);
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
                this.streamWriter.Close();
            }
            //// тут освобождаем управляемые ресурсы
        }

        /// <summary>
        /// Для инизиализации нового лог-файла.
        /// </summary>
        /// <param name="fileName">Наименование файла для лог-файла.</param>
        private void MakeNewFile(string fileName)
        {
            this.logTime = new DateTime();
            string dir = MakeLoggerDir() + "\\";
            string name = GetLoggerFileName(fileName);
            this.fileName = dir + name;
            this.streamWriter = new StreamWriter(this.fileName, true);
        }

        private void OnLogChanged(object sender, FileSystemEventArgs e)
        {
            if (this.GotLogChange != null)
            {
                this.GotLogChange(sender, e);
            }
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
                    tl.Enabled = value;
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
