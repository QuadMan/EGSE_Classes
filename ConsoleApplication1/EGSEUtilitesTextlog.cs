/*** EDGEUtilitesTextlog.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль логирования для КИА
**
** Author: Мурзин Святослав
** Project: КИА
** Module: EDGE UTILITES TEXTLOG
** Requires: 
** Comments: TODO: в ~TxtLog() разобраться с некорректным закрытием файла!
 *                       пока после выполнения всех записей в log-файл необходимо вызвать метод FlushAll()
** TODO: указать в конструкторе путь, где создавать структуру директорий в формате ГГММ/ДД/
 * при создании лог-файла необходимо перед расширением добавлять следующий текст "_HHMMSS"
 * TODO: при закрытии лог-файла проверяем, если размер его равен 0, то стираем файл
 * 
** History:
**  0.1.0	(05.12.2013) -	Начальная версия
**
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EGSE.Utilites
{
    /// <summary>
    /// Класс единичного текстлога
    /// </summary>
    public class TxtLogger
    {
        /// <summary>
        /// Имя файла
        /// Объект StreamWriter, работающий с log-файлом
        /// </summary>
        private string _fName;

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

        /// <summary>
        /// Объект DateTime, хранящий текущее время : (HH:MM:SS:MMS) с log-файлом
        /// </summary>
        private DateTime _logtime;
        
        /// <summary>
        /// Конструктор, принимает имя файла
        /// </summary>
        /// <param name="fName"></param>
        public TxtLogger(string fName)
        {
            _fName = fName;
            _enableTextWrite = true;
            _enableTimeWrite = true;
            _logtime = new DateTime();
            _sw = new StreamWriter(_fName, true);
        }

        /*public double LogTime
        {
            get { return seconds / 3600; }
            set { seconds = value * 3600; }

        }*/

        /// <summary>
        /// Метод, реализующий запись текста и времени в log-файл
        /// Запись осуществляется только если _enableTextWrite == true
        /// Запись времени осуществляется только если значение _enableTimeWrite == true
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
        /// Метод, управляющий разрешением/запретом записи текста в log-файл
        /// По умолчанию: разрешена
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
        /// Свойство, указывающее разрешена/запрещена запись времени в файл
        /// По умолчанию: разрешена
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
        /// Метод, очищающий буфер и производящий сброс информации непосредственно в log-файл
        /// </summary>
        public void LogFlush()
        {
            _sw.Flush();
        }
        ~TxtLogger()
        {
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
        List<TxtLogger>txtLoggers;
 
        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public TxtLoggers()
        {
            txtLoggers = new List<TxtLogger>();
        }

        /// <summary>
        /// Метод, создающий новый log-файл
        /// </summary>
        public void AddFile(string filename)
        {
            txtLoggers.Add(new TxtLogger(filename));
        }

        /// <summary>
        /// Метод, разрешающий/запрещающий запись во все log-файлы
        /// </summary>
        public bool GlobalEnable
        {
            set
            {
                foreach (TxtLogger tl in txtLoggers)
                    tl.LogEnable = value;
            }
        }

        /// <summary>
        /// Метод, очищающий все буфера и производящий сброс всей информации 
        /// непосредственно в соответствующие log-файлы
        /// </summary>
        public void FlushAll()
        {
            foreach (TxtLogger tl in txtLoggers)
                tl.LogFlush(); 
        }

        /// <summary>
        /// Индексатор для текстлога
        /// </summary>
        public TxtLogger this[int i]
        {
            get
            {
                return txtLoggers[i];
            }
        }
    }
}
