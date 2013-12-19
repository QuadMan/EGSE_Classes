/*** EDGECyclogramCommands.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль поддержки команд циклограммы
**
** Author: Семенов Александр
** Project: КИА
** Module: EDGECyclogramCommands
** Requires: 
** Comments:
 *
** History:
**  0.1.0	(06.12.2013) -	Начальная версия
**
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace EGSE.Cyclogram
{
    /// <summary>
    /// Делегат для функции тесторования команды циклограммы
    /// </summary>
    /// <param name="Params">Массив параметров функции</param>
    /// <param name="errString">Строка ошибок, которые обнаружила фкнция проверки</param>
    /// <returns>TRUE, если результат проверки функции положительный</returns>
    public delegate bool testFunctionDelegate(string[] Params, string errString);

    /// <summary>
    /// Делегат для функции выполнения команды циклограммы
    /// </summary>
    /// <param name="Params">Массив параметров функции</param>
    /// <returns>TRUE, если результат выполнения функции положительный</returns>
    public delegate bool execFunctionDelegate(string[] Params);

    /// <summary>
    /// Класс команды циклограммы
    /// </summary>
    public class CyclogramLine : INotifyPropertyChanged
    {
        /// <summary>
        /// Идентификатор неизвестной команды
        /// </summary>
        public const int CYC_UNKNOWN_COMMAND_ID = -1;

        /// <summary>
        /// Является ли эта строка командой
        /// </summary>
        public bool isCommand;

        /// <summary>
        /// Название команды
        /// </summary>
        public string cmdName;

        /// <summary>
        /// Функция тестирования команды
        /// </summary>
        public testFunctionDelegate testFunction;

        /// <summary>
        /// Функция выполнения команды
        /// </summary>
        public execFunctionDelegate execFunction;

        /// <summary>
        /// Цвет, которым выводить команду (может быть не нужен)?
        /// </summary>
        public string color;

        /// <summary>
        /// Идентификатор команды
        /// </summary>
        public int id;

        /// <summary>
        /// Полный путь к циклограмме
        /// </summary>
        public string cycFullFileName;

        /// <summary>
        /// Индекс циклограммы в массиве циклограмм (для поддержки работы с несколькими циклограммами, сейчас не используется)
        /// </summary>
        public uint cycIdx;

        /// <summary>
        /// Параметры команды
        /// </summary>
        public string[] parameters;

        /// <summary>
        /// Комментарии для команды. В эту строку собираются комментарии, которые занимают всю строку и располагаются выше данной команды.
        /// Для разделения комментариев по строкам используется символ /t
        /// </summary>
        public string comments;

        /// <summary>
        /// Задержка перед выполнением этой команды, в милисекундах
        /// </summary>
        //public int delayBeforeCmdMs;

        /// <summary>
        /// Абсолютное время выполнения этой команды (в данный моммент не используется)
        /// </summary>
        //public string absoluteTime;

        /// <summary>
        /// Была ли ошибка при распозновании этой команды
        /// </summary>
        public string errorInCommand;

        public bool wasError
        {
            get
            {
                return (errorInCommand != "");
            }
        }

        public int DelayOriginal;

            private int _line;
            private string _absoluteTime;
            private int _delay;
            private string _delayStr;
            private string _command;

            public int Line { get { return _line; } set { _line = value; FirePropertyChangedEvent("Line"); } }
            public string AbsoluteTime 
            { 
                get 
                {
                    if (isCommand) return _absoluteTime;
                    else return "";
                } 
                set 
                { 
                    _absoluteTime = value;
                    FirePropertyChangedEvent("AbsoluteTime"); 
                } 
            }
            public int Delay 
            { 
                get 
                { 
                    return _delay; 
                }
 
                set 
                { 
                    _delay = value; 
                    //FirePropertyChangedEvent("Delay"); 
                    FirePropertyChangedEvent("DelayStr"); 
                } 
            }
            public string DelayStr 
            { 
                get 
                { 
                    if (isCommand) return  ((float)_delay / 1000).ToString();
                    else return "";
                } 
            }
            public string Command { get { return _command; } set { _command = value; FirePropertyChangedEvent("Command"); } }

            public event PropertyChangedEventHandler PropertyChanged;
            private void FirePropertyChangedEvent(string propertyName)
            {
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            public void RestoreDelay()
            {
                //Debug.WriteLine("delay restore:{0}",DelayOriginal);
                Delay = DelayOriginal;
            }

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public CyclogramLine()
        {
            clear();
        }

        /// <summary>
        /// Основной конструктор
        /// </summary>
        /// <param name="name">Название команды</param>
        /// <param name="testFunc">Функция тестирования</param>
        /// <param name="execFunc">Функция выполнения</param>
        /// <param name="color">Цвет команды</param>
        public CyclogramLine(string name, testFunctionDelegate testFunc, execFunctionDelegate execFunc, string color)
        {
            clear();
            this.cmdName = name;
            testFunction = testFunc;
            execFunction = execFunc;
            this.color = color;
        }

        /// <summary>
        /// Инициализация команды по-умолчанию
        /// </summary>
        public void clear()
        {
            cmdName = "";
            _command = "";
            testFunction = null;
            execFunction = null;
            color = "";
            id = CYC_UNKNOWN_COMMAND_ID;
            cycFullFileName = "";
            cycIdx = 0;
            _line = 0;
            isCommand = false;
            comments = "";
            DelayOriginal = 0;
            
            parameters = null;

            _delay = 0;
            AbsoluteTime = "";
            errorInCommand = "";
        }

        /// <summary>
        /// Запуск и проверка результатов выполнения тестовой функции для данной команды
        /// В случае неудачного выполнения, функция генерирует исключение
        /// </summary>
        public void runTestFunction()
        {
            if (testFunction != null)
            {
                if (testFunction(parameters, errorInCommand))
                {

                }
                else
                {
                    // TODO throw 
                }
            }
            else
            {
                // TODO throw;
            }
        }

        /// <summary>
        /// Используется для отладки, вывод информации о команде
        /// </summary>
        /// <returns></returns>
        new public string ToString()
        {
            string tFunStr = "";
            string eFunStr = "";
            string errsStr = "";
            //string commStr = "";
            if (testFunction != null)
            {
                tFunStr = testFunction.Method.ToString();
            }
            if (execFunction != null)
            {
                eFunStr = execFunction.Method.ToString();
            }
            if (errorInCommand != "")
            {
                errsStr = " Errors:" + errorInCommand;
            }
            //if (comments != "")
            //{
            //    commStr = " Comments:" + comments;
            //}
            return "<" + _line.ToString() + "> " + cmdName + " " + tFunStr + " " + eFunStr + errsStr;// +commStr;
        }
    }

    /// <summary>
    /// Класс поддержки списка команд.
    /// Используется для реализации списка поддерживаемых команд и списка команд из файла циклограмм
    /// В случае поддерживаемых команд, ключем словаря является название команды (должно быть уникальным)
    /// В случае команд из файла циклограмм, ключем является номер (порядковый) команды циклограммы
    /// </summary>
    public class CyclogramCommands : Dictionary<string, CyclogramLine>
    {
        private int _totalCommandsCount;

        public CyclogramCommands() :base()
        {
            _totalCommandsCount = 0;
        }

        /// <summary>
        /// Функция добавляет команду в список
        /// Ключ должен быть уникален
        /// </summary>
        /// <param name="cmdKey">Ключ</param>
        /// <param name="cmd">Команда</param>
        /// <returns>Если ключ уникален, команда добавляется и возвращается TRUE, в противном случае, команда
        /// не добавляется и функция возвращает FALSE</returns>
        public bool AddCommand(string cmdKey, CyclogramLine cmd)
        {
            if (ContainsKey(cmdKey))
            {
                return false;
            }
            cmd.id = _totalCommandsCount;
            Add(cmdKey, cmd);
            _totalCommandsCount++;

            return true;
        }
    }

}