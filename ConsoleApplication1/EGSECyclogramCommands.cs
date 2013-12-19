﻿/*** EDGECyclogramCommands.cs
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
    public delegate bool TestFunctionEventhandler(string[] Params, string errString);

    /// <summary>
    /// Делегат для функции выполнения команды циклограммы
    /// </summary>
    /// <param name="Params">Массив параметров функции</param>
    /// <returns>TRUE, если результат выполнения функции положительный</returns>
    public delegate bool ExecFunctionEventHandler(string[] Params);

    /// <summary>
    /// Класс описания команды циклограммы
    /// </summary>
    public class CyclogramLine : INotifyPropertyChanged
    {
        /// <summary>
        /// Идентификатор неизвестной команды
        /// </summary>
        public const int CYC_UNKNOWN_COMMAND_ID = -1;

        /// <summary>
        /// Функция тестирования команды
        /// </summary>
        public TestFunctionEventhandler TestFunction;

        /// <summary>
        /// Функция выполнения команды
        /// </summary>
        public ExecFunctionEventHandler ExecFunction;

        /// <summary>
        /// Является ли эта строка командой
        /// </summary>
        public bool IsCommand;

        /// <summary>
        /// Название команды
        /// </summary>
        public string CmdName;

        /// <summary>
        /// Цвет, которым выводить команду (может быть не нужен)?
        /// </summary>
        public string Color;

        /// <summary>
        /// Идентификатор команды
        /// </summary>
        public int Id;

        /// <summary>
        /// Полный путь к циклограмме
        /// </summary>
        public string CycFullFileName;

        /// <summary>
        /// Индекс циклограммы в массиве циклограмм (для поддержки работы с несколькими циклограммами, сейчас не используется)
        /// </summary>
        public uint CycIdx;

        /// <summary>
        /// Параметры команды
        /// </summary>
        public string[] Parameters;

        /// <summary>
        /// Комментарии для команды. В эту строку собираются комментарии, которые занимают всю строку и располагаются выше данной команды.
        /// Для разделения комментариев по строкам используется символ /t
        /// </summary>
        public string Comments;

        /// <summary>
        /// Список ошибок, которые обнаружены парсером для этой команды
        /// </summary>
        public string ErrorInCommand;

        /// <summary>
        /// Признак была ли ошибка для этой команды при парсинге
        /// </summary>
        public bool WasError
        {
            get
            {
                return (ErrorInCommand != "");
            }
        }

        // Изначальное значение задержки выполнения команды
        // используется для восстановления значения после отсчета времени при выполненении циклограммы
        private int _delayOriginal;
        private int _delayMs;
        private string _delayStr;
        private int _line;
        private string _absoluteTime;       
        private string _command;

        /// <summary>
        /// Строка, на которой находится команда
        /// </summary>
        public int Line 
        { 
            get 
            { 
                return _line; 
            } 
            set 
            { 
                _line = value; 
                FirePropertyChangedEvent("Line"); 
            } 
        }

        /// <summary>
        /// абсолютное время (ЧЧ:ММ:СС.МСМС) выполнения команды
        /// </summary>
        public string AbsoluteTime 
        { 
            get 
            {
                if (IsCommand) return _absoluteTime;
                else return "";
            } 
            set 
            { 
                _absoluteTime = value;
                FirePropertyChangedEvent("AbsoluteTime"); 
            } 
        }

        /// <summary>
        /// задержка в мс перед выполнением команды
        /// при обновлении, также меняет DelayStr
        /// </summary>
        public int DelayMs 
        { 
            get 
            { 
                return _delayMs; 
            }
 
            set 
            { 
                _delayMs = value;
                _delayStr = "";
                if (IsCommand) _delayStr = ((float)_delayMs / 1000).ToString();
                FirePropertyChangedEvent("DelayStr"); 
            } 
        }

        public int DelayOriginal
        {
            //get
            //{
            //    return _delayOriginal;
            //}

            set
            {
                _delayOriginal = value;
            }
        }
        
        /// <summary>
        /// Предствление задержки в виде строки (в секундах)
        /// </summary>
        public string DelayStr 
        { 
            get 
            {
                return _delayStr;
            } 
        }

        /// <summary>
        /// строка команды (которая после времени) вместе с комментариями
        /// </summary>
        public string Str 
        { 
            get 
            { 
                return _command; 
            } 
            
            set 
            { 
                _command = value; 
                FirePropertyChangedEvent("Str"); 
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Восстановление значения задержки в исходное значение, так как в циклограмме идет обратный отсчет по этому полю
        /// </summary>
        public void RestoreDelay()
        {
            DelayMs = _delayOriginal;
        }

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public CyclogramLine()
        {
            Reset();
        }

        /// <summary>
        /// Основной конструктор
        /// </summary>
        /// <param name="name">Название команды</param>
        /// <param name="testFunc">Функция тестирования</param>
        /// <param name="execFunc">Функция выполнения</param>
        /// <param name="color">Цвет команды</param>
        public CyclogramLine(string name, TestFunctionEventhandler testFunc, ExecFunctionEventHandler execFunc, string color)
        {
            Reset();
            CmdName = name;
            TestFunction = testFunc;
            ExecFunction = execFunc;
            Color = color;
        }

        /// <summary>
        /// Инициализация команды по-умолчанию
        /// </summary>
        public void Reset()
        {
            _command = "";
            _line = 0;
            _delayOriginal = 0;
            _delayMs = 0;
            _delayStr = "";
            _absoluteTime = "";

            IsCommand = false;
            CmdName = "";
            TestFunction = null;
            ExecFunction = null;
            Color = "";
            Id = CYC_UNKNOWN_COMMAND_ID;
            CycFullFileName = "";
            CycIdx = 0;
            Parameters = null;
            Comments = "";
            ErrorInCommand = "";
        }

        /// <summary>
        /// Запуск и проверка результатов выполнения тестовой функции для данной команды
        /// В случае неудачного выполнения, функция генерирует исключение
        /// </summary>
        public void runTestFunction()
        {
            if (TestFunction != null)
            {
                if (TestFunction(Parameters, ErrorInCommand))
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
            if (TestFunction != null)
            {
                tFunStr = TestFunction.Method.ToString();
            }
            if (ExecFunction != null)
            {
                eFunStr = ExecFunction.Method.ToString();
            }
            if (ErrorInCommand != "")
            {
                errsStr = " Errors:" + ErrorInCommand;
            }
            //if (comments != "")
            //{
            //    commStr = " Comments:" + comments;
            //}
            return "<" + _line.ToString() + "> " + CmdName + " " + tFunStr + " " + eFunStr + errsStr;// +commStr;
        }
    }

    /// <summary>
    /// Класс поддержки списка команд.
    /// Используется для реализации списка поддерживаемых команд
    /// В случае поддерживаемых команд, ключем словаря является название команды (должно быть уникальным)
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
            cmd.Id = _totalCommandsCount;
            Add(cmdKey, cmd);
            _totalCommandsCount++;

            return true;
        }
    }

}