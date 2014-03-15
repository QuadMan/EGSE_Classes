//-----------------------------------------------------------------------
// <copyright file="EGSECyclogramFile.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

namespace EGSE.Cyclogram
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;    
    using EGSE.Cyclogram;

    /// <summary>
    /// Специальное исключение при парсинге файла циклограмм.
    /// </summary>
    [Serializable]
    public class CyclogramParsingException : ApplicationException
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramParsingException" />.
        /// </summary>
        public CyclogramParsingException() 
        { 
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramParsingException" />.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public CyclogramParsingException(string message) 
            : base(message) 
        { 
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramParsingException" />.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="ex">The Exception.</param>
        public CyclogramParsingException(string message, Exception ex) 
            : base(message) 
        { 
        }
        
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramParsingException" />.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="contex">The contex.</param>
        protected CyclogramParsingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext contex)
            : base(info, contex) 
        { 
        }
    }

    /// <summary>
    /// Класс, предназначенный для работы с файлом циклограмм.
    /// </summary>
    public class CyclogramFile
    {
        /// <summary>
        /// Символ комментария.
        /// </summary>
        public const char CycloCommentChar = '#';

        /// <summary>
        /// Максимальнео значение секунд в формате времени циклограммы.
        /// </summary>
        public const int MaxSecondsValue = 65535;

        /// <summary>
        /// Текущая команда.
        /// </summary>
        private CyclogramLine _curCommand;

        /// <summary>
        /// Список доступных команд циклограммы.
        /// </summary>
        private CyclogramCommands _availableCommands;

        /// <summary>
        /// Была ли ошибка при парсинге файла.
        /// </summary>
        private bool _wasError;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramFile" />.
        /// </summary>
        public CyclogramFile()
        {
            // !_curLine = 0;
            _wasError = false;
            _availableCommands = null;
            _curCommand = new CyclogramLine();
            Commands = new ObservableCollection<CyclogramLine>();
        }

        /// <summary>
        /// Получает или задает имя файла циклограмм.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Получает или задает список команд, создаваемый из файла циклограмм.
        /// </summary>
        public ObservableCollection<CyclogramLine> Commands { get; set; }

        /// <summary>
        /// Получает значение, показывающее, была ли ошибка при парсинге файла.
        /// </summary>
        public bool WasError
        {
            get
            {
                return _wasError;
            }
        }

        /// <summary>
        /// Функция парсинга строки.
        /// Работаем на исключениях, в вызывающей функции, необходимо проверять исключения от этой функции
        /// </summary>
        /// <param name="cycStr">Строка циклограммы.</param>
        public void TryParseString(string cycStr)
        {
            _curCommand.Reset();

            // убираем лишнее пробелы из строки
            cycStr.Trim();

            // сохраняем на случай, если эта строка - только комментарий
            _curCommand.Str = cycStr;

            // убираем комментарии, сохраняя их в специальной переменной
            _curCommand.Comments += TakeComments(ref cycStr) + "\t";

            // вся строка - комментарий, выходим, нам здесь больше делать нечего
            if (cycStr == string.Empty)
            {
                return;
            }

            // кроме комментариев есть еще что-то, пытаемся понять, что, но в любом случае. считаем что должна быть команда
            _curCommand.IsCommand = true;

            // разделяем строку по словам (разделитель - пробел)
            string[] strTokens = cycStr.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

            // если нашли только одно слово, то генерим исключение об ошибке
            if (strTokens.Length <= 1)
            {
                CyclogramParsingException exc = new CyclogramParsingException("Ошибка в строке " + cycStr + ". Формат задания команд: ВРЕМЯ КОМАНДА ПАРАМЕТРЫ #КОММЕНТАРИИ");
                throw exc;
            }

            // пытаемся разобрать время
            TryParseTimeToken(strTokens[0]);

            // пытаемся разобрать команду
            TryParseCmdToken(strTokens[1]);

            // составляем строку команды для отображения в циклограмме (выкидываем из исходной строки время, так как оно у нас в отдельном столбце)
            _curCommand.Str = strTokens[1];

            // копируем параметры, если они есть в массив параметров команды
            if (strTokens.Length - 2 > 0)
            {
                _curCommand.Parameters = new string[strTokens.Length - 2];
                System.Array.Copy(strTokens, 2, _curCommand.Parameters, 0, _curCommand.Parameters.Length);
                for (ushort i = 2; i < strTokens.Length; i++)
                {
                    _curCommand.Str += " " + strTokens[i];
                }
            }

            // добавляем комментарии к строке, если они есть
            _curCommand.Str += " " + _curCommand.Comments;

            // выполняем функцию тестирования параметров команды
            if (!_curCommand.RunTestFunction())
            {
                CyclogramParsingException exc = new CyclogramParsingException(); // "Ошибка при проверке команды " + _curCommand.CmdName);
                throw exc;
            }
        }

        /// <summary>
        /// Загружаем файл циклограмм.
        /// Даже в случае ошибки, команда добавляется в список команд, но выставляется признак наличия ошибки.
        /// </summary>
        /// <param name="cycFName">Путь к файлу циклограмм.</param>
        /// <param name="availableCommands">Список доступных команд.</param>
        public void TryLoad(string cycFName, CyclogramCommands availableCommands)
        {
            Debug.Assert(availableCommands != null, "Список доступных команд циклограммы пуст!");
            string cycLine;
            int curLineNum = 0;

            _availableCommands = availableCommands;
            _wasError = false;
            if (!File.Exists(cycFName))
            {
                _wasError = true;
                CyclogramParsingException exc = new CyclogramParsingException("Файл " + cycFName + " не существует!");
                throw exc;
            }

            FileName = cycFName;
            Commands.Clear();
            using (StreamReader sr = new StreamReader(cycFName))
            {
                // читам файл по строкам
                while (sr.Peek() >= 0)              
                {
                    cycLine = sr.ReadLine();
                    try
                    {
                        TryParseString(cycLine);
                    }
                    catch (CyclogramParsingException e)
                    {
                        _wasError = true;
                        _curCommand.ErrorInCommand += e.Message + "\t";
                    }
                    catch
                    {
                        _wasError = true;
                        _curCommand.ErrorInCommand += " Общая ошибка проверки команды.";
                    }
                    finally
                    {
                        _curCommand.Line = curLineNum;
                        Commands.Add(_curCommand);

                        // начинаем "собирать" новую команду
                        _curCommand = new CyclogramLine();
                    }

                    curLineNum++;
                }
            }
        }

        /// <summary>
        /// Расчитывает абсолютное время для всех команд циклограммы в формате ЧЧ:ММ:СС.МСС.
        /// Может рассчитать относительно заданного времени.
        /// </summary>
        /// <param name="hr">Час(для относительного рассчета).</param>
        /// <param name="min">Минута(для относительного рассчета).</param>
        /// <param name="sec">Секунда(для относительного расчета).</param>
        /// <param name="fromLineNum">С какой строки рассчитывать время, если запускаем циклограмму не с первой строки.</param>
        public void CalcAbsoluteTime(int hr = 0, int min = 0, int sec = 0, int fromLineNum = 0)
        {
            DateTime dt = new DateTime(1, 1, 1, hr, min, sec, 0);
            foreach (CyclogramLine cl in Commands.Where(l => l.IsCommand && (l.Line >= fromLineNum)))
            {
                cl.AbsoluteTime = dt.ToString("HH:mm:ss.fff");
                dt = dt.AddMilliseconds(cl.DelayMs);
            }
        }

        /// <summary>
        /// Позиционируемся на первую команду циклограммы
        /// перемещаем указатель _curLine
        /// </summary>
        /// <returns>Если команда существует, возвращаем команду, если нет - null</returns>
        /*
        public CyclogramLine GetFirstCmd()
        {
            _curLine = 0;
            foreach (CyclogramLine cl in commands)
            {
                if (cl.IsCommand)
                {
                    return commands[_curLine];
                }
                _curLine++;
            }
            return null;
        }
        */

        /*
        public void SetCmdLine(CyclogramLine cl)
        {
            if (cl == null) return;

            _curLine = cl.Line-1;       // TODO: убрать -1 (привести отсчет строк к одному виду)
        }

        /// <summary>
        /// Получаем текущую строку (может быть не командой, нужно проверять IsCommand)
        /// </summary>
        /// <returns>Возвращает null, если дошли до конца списка команд</returns>
        public CyclogramLine GetCurCmd()
        {
            if (_curLine == commands.Count - 1)
                return null;
            else
            {
                if (commands[_curLine].IsCommand)
                {
                    return commands[_curLine];
                }
                else
                {
                    return GetNextCmd();
                }
            }
        }

        /// <summary>
        /// Ищем следующую команду
        /// </summary>
        /// <returns>Возвращает null, если больше команд нет</returns>
        public CyclogramLine GetNextCmd()
        {
            for (_curLine = _curLine + 1; _curLine < commands.Count; _curLine++)
            {
                if (commands[_curLine].IsCommand)
                {
                    return commands[_curLine];
                }
            }
            return null;
        }
        */

        /// <summary>
        /// Проверяем, есть ли на этой строке файла циклограмм команда.
        /// </summary>
        /// <param name="cycLine">Номер строки.</param>
        /// <returns>Возвращает <c>true</c>, если на этой строке есть команда.</returns>
        public bool IsCmdExistsOnLine(uint cycLine)
        {
            foreach (var cmd in Commands)
            {
                if (cmd.Line == cycLine)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Функция из строки вырезает комментарии и возвращает строку без комментариев по ссылке и сам комментарий в возвращаемом значении.
        /// </summary>
        /// <param name="cycStr">Исходная строка, из которой исключаются комментарии.</param>
        /// <returns>Cтрока комментариев (если их нет, возвращет String.Empty).</returns>
        private string TakeComments(ref string cycStr)
        {
            int commentStartPos = cycStr.IndexOf(CycloCommentChar);
            if (commentStartPos != -1)
            {
                string commentStr = cycStr.Substring(commentStartPos).Trim();

                cycStr = cycStr.Remove(commentStartPos).Trim();                 // убираем из исходной строки комментарии

                return commentStr;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Проверяем время на корректность данных
        /// Время может быть задано в секундах и в долях секунд
        /// 0; 0.230; 33.675.
        /// Миллисекунды должны быть от 0 до 999.
        /// Значение секунд не должно быть больше 65535
        /// TODO избавиться от дубликата проверки секунд
        /// </summary>
        /// <param name="timeStr">распознанное время из команды</param>
        private void TryParseTimeToken(string timeStr)
        {
            string[] timeTokens = timeStr.Split('.');   // timeTokens[0] - значение секунд, timeTokens[1] - значение мс, если задано

            // дробное значение не задано, считаем, что заданы секунды
            if (timeTokens.Length == 1)
            {
                try
                {
                    int tempSec = int.Parse(timeTokens[0]);
                    if ((tempSec < 0) || (tempSec > MaxSecondsValue))
                    {
                        CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразования времени выполнения команды: " + timeStr + ". Секунды должны быть заданы от 0 до " + MaxSecondsValue.ToString());
                        throw exc;
                    }

                    _curCommand.DelayMs = tempSec * 1000;
                    _curCommand.DelayOriginal = _curCommand.DelayMs;
                }
                catch (FormatException)
                {
                    CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразования времени выполнения команды: " + timeStr);
                    throw exc;
                }
            }
            else if (timeTokens.Length == 2)
            { // задано и дробное значение
                try
                {
                    int tempSec = int.Parse(timeTokens[0]);
                    if ((tempSec < 0) || (tempSec > MaxSecondsValue))
                    {
                        CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразования времени выполнения команды: " + timeStr + ". Секунды должны быть заданы от 0 до " + MaxSecondsValue.ToString());
                        throw exc;
                    }

                    int ms = int.Parse(timeTokens[1]);
                    if ((ms < 0) && (ms > 999))
                    {
                        CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразованя времени выполнения команды: " + timeStr + ". Милисекунды должны быть от 0 до 999.");
                        throw exc;
                    }

                    _curCommand.DelayMs = (tempSec * 1000) + ms;
                    _curCommand.DelayOriginal = _curCommand.DelayMs;
                }
                catch (FormatException)
                {
                    CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразованя времени выполнения команды: " + timeStr);
                    throw exc;
                }
            }
            else
            { // ошибка задания времени (время разделено более одной точкой)
                CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразованя времени выполнения команды: " + timeStr);
                throw exc;
            }
        }

        /// <summary>
        /// Проверяем, что команда существует в списке доступных команд.
        /// Если команда не находится в списке, вызывается исключение CyclogramParsingException
        /// Если команда в списке находится, переменной _curCommand присваиваются значения id, execFunction и testFunction.
        /// Выставляется флаг cmdExists.
        /// </summary>
        /// <param name="cmdStr">Название команды.</param>
        private void TryParseCmdToken(string cmdStr)
        {
            Debug.Assert(_availableCommands != null, "Список доступных команд циклограммы пуст!");

            bool cmdExists = false;

            foreach (var cmd in _availableCommands)
            {
                if (cmd.Key == cmdStr)
                {
                    cmdExists = true;
                    _curCommand.Id = cmd.Value.Id;
                    _curCommand.ExecFunction = cmd.Value.ExecFunction;
                    _curCommand.TestFunction = cmd.Value.TestFunction;

                    break;
                }
            }

            if (!cmdExists)
            {
                CyclogramParsingException exc = new CyclogramParsingException("Команды " + cmdStr + " нет в списке поддерживаемых команд!");
                throw exc;
            }
        }
    }

    /// <summary>
    /// Отвечает за представление позиции команды в файле.
    /// </summary>
    public class CycPosition
    {
        /// <summary>
        /// Экземпляр класса, отвечающий за представления файла циклограммы.
        /// </summary>
        private CyclogramFile _cycloFile;

        /// <summary>
        /// Текущая строка по-счету в файле циклограммы.
        /// </summary>
        private int _curLine;

        /// <summary>
        /// Экземпляр класса, отвечающий за представление текущей команды циклограммы.
        /// </summary>
        private CyclogramLine _curCmd;

        /// <summary>
        /// Является ли текущая команда последней в файле циклограмм.
        /// </summary>
        private bool _lastCommand;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CycPosition" />.
        /// </summary>
        /// <param name="cycloFile">Экземпляр класса, представляющий файл циклограммы.</param>
        public CycPosition(CyclogramFile cycloFile)
        {
            _cycloFile = cycloFile;
            _curLine = 0;
            CurCmd = null;
            SetCmdEvent = null;
            _lastCommand = false;
        }

        /// <summary>
        /// Получает или задает обработчик команды для очередного шага циклограммы.
        /// </summary>
        /// <value>
        /// Обработчик команды для очередного шага циклограммы.
        /// </value>
        public EGSE.Threading.StepEventHandler SetCmdEvent { get; set; }

        /// <summary>
        /// Получает значение, показывающее, является ли [текущая команда последняя].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [текущая команда последняя]; иначе, <c>false</c>.
        /// </value>
        public bool IsLastCommand
        {
            get
            {
                return _lastCommand;
            }
        }

        /// <summary>
        /// Получает текущую команду циклограммы.
        /// </summary>
        /// <value>
        /// The current command.
        /// </value>
        public CyclogramLine CurCmd
        {
            get
            {
                return _curCmd;
            }

            private set
            {
                _curCmd = value;
                if (_curCmd != null)
                {
                    _curLine = _curCmd.Line;
                    if (SetCmdEvent != null)
                    {
                        SetCmdEvent(_curCmd);
                    }
                }
            }
        }

        /// <summary>
        /// Sets to line.
        /// </summary>
        /// <param name="lineNum">The line number.</param>
        /// <param name="findFirst">if set to <c>true</c> [find first].</param>
        /// <returns>Экземпляр класса, отвечающий за представление строки циклограммы.</returns>
        public CyclogramLine SetToLine(int lineNum, bool findFirst = false)
        {
            _curCmd = null;
            foreach (CyclogramLine cl in _cycloFile.Commands.Where(l => l.IsCommand))
            {
                if (findFirst)
                {
                    if (cl.Line >= lineNum)
                    {
                        CurCmd = cl;
                        return CurCmd;
                    }
                }
                else if (cl.Line == lineNum)
                {
                    CurCmd = cl;
                    return CurCmd;
                }
            }

            return null;
        }

        /// <summary>
        /// Возвращает следующую команду.
        /// </summary>
        /// <returns>Если существует следующая команда.</returns>
        public CyclogramLine GetNextCmd()
        {
            _lastCommand = false;

            for (int i = _curLine + 1; i < _cycloFile.Commands.Count; i++)
            {
                if (_cycloFile.Commands[i].IsCommand)
                {
                    CurCmd = _cycloFile.Commands[i];
                    return CurCmd;
                }
            }
            
            _lastCommand = true;
            return null;
        }
    }
}