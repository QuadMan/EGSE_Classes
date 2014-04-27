//-----------------------------------------------------------------------
// <copyright file="EGSECyclogramCommands.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

namespace Egse.Cyclogram
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Делегат для функции тесторования команды циклограммы.
    /// </summary>
    /// <param name="cmdParams">The command parameters.</param>
    /// <param name="errString">Строка ошибок, которые обнаружила функция проверки.</param>
    /// <returns>
    ///   <c>true</c>, если результат проверки функции положительный.
    /// </returns>
    public delegate bool TestFunctionEventhandler(string[] cmdParams, out string errString);

    /// <summary>
    /// Делегат для функции выполнения команды циклограммы.
    /// </summary>
    /// <param name="cmdParams">Параметры команды.</param>
    /// <returns>
    ///   <c>true</c>, если результат выполнения функции положительный.
    /// </returns>
    public delegate bool ExecFunctionEventHandler(string[] cmdParams);

    /// <summary>
    /// Класс описания команды циклограммы.
    /// </summary>
    public class CyclogramLine : INotifyPropertyChanged
    {
        /// <summary>
        /// Идентификатор неизвестной команды.
        /// </summary>
        public const int CYCUnknownCommandID = -1;

        /// <summary>
        /// Изначальное значение задержки выполнения команды используется для восстановления значения после отсчета времени при выполненении циклограммы.
        /// </summary>
        private int delayOriginal;

        /// <summary>
        /// Значение задержки в мс.
        /// </summary>
        private int _delayMs;

        /// <summary>
        /// Значение задержки в секундах, переведенное в строку.
        /// </summary>
        private string _delayStr;

        /// <summary>
        /// Строка в файле циклограмм.
        /// </summary>
        private int _line;

        /// <summary>
        /// Рассчитанное абсолютное время.
        /// </summary>
        private string _absoluteTime;

        /// <summary>
        /// Команда с параметрами для вывода в последнем столбце таблицы.
        /// </summary>
        private string _command;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramLine" />.
        /// </summary>
        /// <param name="name">Название команды.</param>
        /// <param name="testFunc">Функция тестирования.</param>
        /// <param name="execFunc">Функция выполнения.</param>
        /// <param name="color">Цвет команды.</param>
        public CyclogramLine(string name, TestFunctionEventhandler testFunc, ExecFunctionEventHandler execFunc, string color)
        {
            Reset();
            CmdName = name;
            TestFunction = testFunc;
            ExecFunction = execFunc;
            Color = color;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramLine" />.
        /// </summary>
        public CyclogramLine()
        {
            Reset();
        }

        /// <summary>
        /// Событие вызывается для обеспечения INotifyEvent - и вывода команд в таблицу.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Получает или задает функцию тестирования команды.
        /// </summary>
        public TestFunctionEventhandler TestFunction { get; set; }

        /// <summary>
        /// Получает или задает функцию выполнения команды.
        /// </summary>
        public ExecFunctionEventHandler ExecFunction { get; set; }

        /// <summary>
        /// Получает или задает значение, показывающее, является ли эта строка командой.
        /// </summary>
        public bool IsCommand { get; set; }

        /// <summary>
        /// Получает или задает название команды.
        /// </summary>
        public string CmdName { get; set; }

        /// <summary>
        /// Получает или задает цвет, которым выводить команду.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Получает или задает идентификатор команды.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Получает или задает полный путь к циклограмме.
        /// </summary>
        public string CycFullFileName { get; set; }

        /// <summary>
        /// Получает или задает индекс циклограммы в массиве циклограмм. 
        /// (для поддержки работы с несколькими циклограммами, сейчас не используется).
        /// </summary>
        public uint CycIdx { get; set; }

        /// <summary>
        /// Получает или задает параметры команды.
        /// </summary>
        public string[] Parameters { get; set; }

        /// <summary>
        /// Получает или задает комментарии для команды. 
        /// В эту строку собираются комментарии, которые занимают всю строку и располагаются выше данной команды.
        /// Для разделения комментариев по строкам используется символ /t.
        /// </summary>
        public string Comments { get; set; } 

        /// <summary>
        /// Получает или задает список ошибок, которые обнаружены парсером для этой команды.
        /// </summary>
        public string ErrorInCommand { get; set; }

        /// <summary>
        /// Получает значение, показывающее, была ли ошибка для этой команды при парсинге.
        /// </summary>
        public bool WasError
        {
            get
            {
                return ErrorInCommand != string.Empty;
            }
        }

        /// <summary>
        /// Получает или задает строку, на которой находится команда.
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
        /// Получает или задает абсолютное время (ЧЧ:ММ:СС.МСМС) выполнения команды.
        /// </summary>
        public string AbsoluteTime 
        { 
            get 
            {
                if (IsCommand)
                {
                    return this._absoluteTime;
                }
                else
                {
                    return string.Empty;
                }
            } 

            set 
            { 
                this._absoluteTime = value;
                FirePropertyChangedEvent("AbsoluteTime"); 
            } 
        }

        /// <summary>
        /// Получает или задает задержку в мс перед выполнением команды.
        /// При обновлении, также меняет DelayStr.
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
                _delayStr = string.Empty;
                if (IsCommand)
                {
                    _delayStr = ((float)_delayMs / 1000).ToString();
                }

                FirePropertyChangedEvent("DelayStr"); 
            } 
        }

        /// <summary>
        /// Задает оригинальную задержку выпонения команд.
        /// </summary>
        public int DelayOriginal
        {
            set
            {
                this.delayOriginal = value;
            }
        }
        
        /// <summary>
        /// Получает предствление задержки в виде строки (в секундах).
        /// </summary>
        public string DelayStr 
        { 
            get 
            {
                return _delayStr;
            } 
        }

        /// <summary>
        /// Получает или задает строку команды (которая после времени) вместе с комментариями.
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

        /// <summary>
        /// Восстановление значения задержки в исходное значение, так как в циклограмме идет обратный отсчет по этому полю.
        /// </summary>
        public void RestoreDelay()
        {
            DelayMs = this.delayOriginal;
        }

        /// <summary>
        /// Инициализация команды по-умолчанию.
        /// </summary>
        public void Reset()
        {
            _command = string.Empty;
            _line = 0;
            this.delayOriginal = 0;
            _delayMs = 0;
            _delayStr = string.Empty;
            this._absoluteTime = string.Empty;

            IsCommand = false;
            CmdName = string.Empty;
            TestFunction = null;
            ExecFunction = null;
            Color = string.Empty;
            Id = CYCUnknownCommandID;
            CycFullFileName = string.Empty;
            CycIdx = 0;
            Parameters = null;
            Comments = string.Empty;
            ErrorInCommand = string.Empty;
        }

        /// <summary>
        /// Запуск и проверка результатов выполнения тестовой функции для данной команды.
        /// В случае неудачного выполнения, функция генерирует исключение.
        /// </summary>
        /// <returns>Результат выполнения функции тестирования команды.</returns>
        public bool RunTestFunction()
        {
            Debug.Assert(TestFunction != null, string.Format("Не задана функция тестирования команды {0}", CmdName));

            if (TestFunction != null)
            {
                string errInCommand;
                if (!TestFunction(Parameters, out errInCommand))
                {
                    ErrorInCommand += errInCommand + "\t";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Вызываем INotifyChanged.
        /// </summary>
        /// <param name="propertyName">Название свойства.</param>
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Класс поддержки списка команд.
    /// Используется для реализации списка поддерживаемых команд.
    /// В случае поддерживаемых команд, ключем словаря является название команды (должно быть уникальным).
    /// </summary>
    public class CyclogramCommands : Dictionary<string, CyclogramLine>
    {
        /// <summary>
        /// Количество команд в словаре (для присваивания Id новой команде).
        /// </summary>
        private int _totalCommandsCount;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramCommands" />.
        /// </summary>
        public CyclogramCommands() : base()
        {
            _totalCommandsCount = 0;
        }

        /// <summary>
        /// Функция добавляет команду в список.
        /// </summary>
        /// <param name="cmdKey">Уникальный идентификатор команды(повторения не допускаются).</param>
        /// <param name="cmd">Команда для данного идентификатора.</param>
        /// <returns>Если ключ уникален, команда добавляется и возвращается <c>true</c>, в противном случае, команда
        /// не добавляется и функция возвращает <c>false</c>.</returns>
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