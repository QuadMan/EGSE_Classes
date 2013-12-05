/*** EDGECyclogramFile.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль поддержки файла циклограмм
**
** Author: Семенов Александр
** Project: КИА
** Module: EDGECyclogramFile
** Requires: 
** Comments:
 *
** History:
**  0.1.0	(06.12.2013) -	Начальная версия
**
*/

using System;
using EGSE.Cyclogram;
using System.IO;

namespace EGSE.Cyclogram
{
    /// <summary>
    /// Специальное исключение при парсинге файла циклограмм
    /// </summary>
    [Serializable]
    public class CyclogramParsingException : ApplicationException
    {
        public CyclogramParsingException() { }
        public CyclogramParsingException(string message) : base(message) { }
        public CyclogramParsingException(string message, Exception ex) : base(message) { }
        // Конструктор для обработки сериализации типа
        protected CyclogramParsingException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext contex)
            : base(info, contex) { }
    }


    /// <summary>
    /// Класс, предназначенный для работы с файлом циклограмм
    /// </summary>
    public class CyclogramFile 
    {
        /// <summary>
        /// Символ комментария
        /// </summary>
        const char CYC_COMMENT_CHAR = '#';

        /// <summary>
        /// Максимальнео значение секунд в формате времени циклограммы
        /// </summary>
        const int MAX_SECONDS_VALUE = 65535;

        /// <summary>
        /// 
        /// </summary>
        public int curCommandIdx;

        /// <summary>
        /// текущая команда
        /// </summary>
        private CyclogramCommand _curCommand;

        /// <summary>
        /// Список доступных команд циклограммы
        /// </summary>
        private CyclogramCommands _availableCommands;

        /// <summary>
        /// Текущая строка файла циклограмм
        /// </summary>
        private int _curLine;

        /// <summary>
        /// Была ли ошибка при парсинге файла
        /// </summary>
        private bool _wasError;

        /// <summary>
        /// Признак присутствия команды на текущей строке при парсинге файла
        /// </summary>
        private bool _commandExistsOnLine;

        /// <summary>
        /// Список команд, создаваемый из файла циклограмм
        /// </summary>
        public CyclogramCommands commands;

        /// <summary>
        /// Была ли ошибка при парсинге файла
        /// </summary>
        public bool WasError
        {
            get
            {
                return _wasError;
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="availableCommands">Список доступных команд циклограммы</param>
        public CyclogramFile(CyclogramCommands availableCommands)
        {
            curCommandIdx = 0;
            _commandExistsOnLine = false;
            _curLine = 0;
            _wasError = false;
            _availableCommands = availableCommands;
            commands = new CyclogramCommands();
            _curCommand = new CyclogramCommand();
        }

        /// <summary>
        /// Функция из строки вырезает комментарии и возвращает строку без комментариев по ссылке и сам комментарий в возвращаемом значении
        /// </summary>
        /// <param name="cycStr">Исходная строка, из которой исключаются комментарии</param>
        /// <returns>Возвращает строку комментариев, если они ести и "", если их нет</returns>
        private string TakeComments(ref string cycStr)
        {
            int commentStartPos = cycStr.IndexOf(CYC_COMMENT_CHAR);             // ищем начало комментариев
            if (commentStartPos != -1)
            {
                string commentStr = cycStr.Substring(commentStartPos).Trim();   // получаем комментарии

                cycStr = cycStr.Remove(commentStartPos).Trim();                 // убираем из исходной строки комментарии

                return commentStr;                                              
            }
            else return "";
        }

        /// <summary>
        /// Проверяем время на корректность данных
        /// Время может быть задано в секундах и в долях секунд
        /// 0; 0.230; 33.675.
        /// Миллисекунды должны быть от 0 до 999.
        /// Значение секунд не должно быть больше 65535
        /// 
        /// TODO: избавиться от дубликата проверки секунд
        /// </summary>
        /// <param name="timeStr">распознанное время из команды</param>
        private void tryParseTimeToken(string timeStr)
        {
            string[] timeTokens = timeStr.Split('.');
            if (timeTokens.Length == 1) // дробное значение не задано, считаем, что заданы секунды
            {
                try
                {
                    int tempSec = Int32.Parse(timeTokens[0]);
                    if ((tempSec < 0) || (tempSec > MAX_SECONDS_VALUE))
                    {
                        CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразования времени выполнения команды: " + timeStr+". Секунды должны быть заданы от 0 до " + MAX_SECONDS_VALUE.ToString());
                        throw exc;                        
                    }
                    _curCommand.delayBeforeCmdMs = tempSec * 1000;
                }
                catch (FormatException)
                {
                    CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразования времени выполнения команды: " + timeStr);
                    throw exc;
                }
            }
            else
            {
                try
                {
                    int tempSec = Int32.Parse(timeTokens[0]);
                    if ((tempSec < 0) || (tempSec > MAX_SECONDS_VALUE))
                    {
                        CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразования времени выполнения команды: " + timeStr + ". Секунды должны быть заданы от 0 до " + MAX_SECONDS_VALUE.ToString());
                        throw exc;
                    }

                    int ms = Int32.Parse(timeTokens[1]);
                    if ((ms < 0) && (ms > 999))
                    {
                        CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразованя времени выполнения команды - " + timeStr + ". Милисекунды должны быть от 0 до 999.");
                        throw exc;
                    }

                    _curCommand.delayBeforeCmdMs = tempSec * 1000 + ms;
                }
                catch (FormatException)
                {
                    CyclogramParsingException exc = new CyclogramParsingException("Ошибка преобразованя времени выполнения команды: " + timeStr);
                    throw exc;
                }
            }
        }

        /// <summary>
        /// Проверяем, что команда существует в списке доступных команд
        /// Если команда не находится в списке, вызывается исключение
        /// Если команда в списке находится, переменной _curCommand присваиваются значения id, execFunction и testFunction
        /// Выставляется флаг cmdExists
        /// </summary>
        /// <param name="cmdStr">Название команды</param>
        private void tryParseCmdToken(string cmdStr)
        {
            bool cmdExists = false;

            foreach (var cmd in _availableCommands)
            {
                if (cmd.Key == cmdStr)
                {
                    cmdExists = true;
                    _curCommand.id = cmd.Value.id;
                    _curCommand.execFunction = cmd.Value.execFunction;
                    _curCommand.testFunction = cmd.Value.testFunction;

                    break;
                }
            }
            if (!cmdExists)
            {
                CyclogramParsingException exc = new CyclogramParsingException("Команды " + cmdStr + " нет в списке поддерживаемых команд!");
                throw exc;
            }
        }

        /// <summary>
        /// Функция парсинга строки
        /// Работаем на исключениях, в вызывающей функции, необходимо проверять исключения от этой функции
        /// </summary>
        /// <param name="cycStr">Строка циклограммы</param>
        public void TryParseString(string cycStr)
        {
            // убираем лишнее пробелы из строки
            cycStr.Trim();
            // убираем комментарии, сохраняя их в специальной переменной
            _curCommand.comments += TakeComments(ref cycStr) + "\t";
            // вся строка - комментарий, выходим, нам здесь больше делать нечего
            if (cycStr == "") return;
            // кроме комментариев есть еще что-то, пытаемся понять, что, но в любом случае. считаем что должна быть команда
            _commandExistsOnLine = true;
            // разделяем строку по словам (разделитель - пробел)
            string[] strTokens = cycStr.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            // если нашли только одно слово, то генерим исключение об ошибке
            if (strTokens.Length == 1)
            {
                CyclogramParsingException exc = new CyclogramParsingException("Ошибка в строке " + cycStr + ". Формат задания команд: TIME CMD PARAMS #COMMENTS.");
                throw exc;
            }
            // пытаемся разобрать время
            tryParseTimeToken(strTokens[0]);
            // пытаемся разобрать команду
            tryParseCmdToken(strTokens[1]);
            // копируем параметры, если они есть в массив параметров команды
            if (strTokens.Length - 2 > 0)
            {
                _curCommand.parameters = new string[strTokens.Length - 2];
                System.Array.Copy(strTokens, 2, _curCommand.parameters, 0, _curCommand.parameters.Length);
            }
            // выполняем функцию тестирования параметров команды
            _curCommand.runTestFunction();
        }

        /// <summary>
        /// загружаем файл циклограмм
        /// Даже в случае ошибки, команда добавляется в список команд, но выставляется признак наличия ошибки
        /// </summary>
        /// <param name="cycFName">Путь к файлу циклограмм</param>
        /// <param name="availableCommands">Список доступных команд</param>
        public void TryLoad(string cycFName, CyclogramCommands availableCommands)
        {
            _wasError = false;
            if (!File.Exists(cycFName))
            {
                _wasError = true;
                CyclogramParsingException exc = new CyclogramParsingException("Файл "+cycFName+" не существует!");
                throw exc;
            }
            if (availableCommands == null)
            {
                _wasError = true;
                CyclogramParsingException exc = new CyclogramParsingException("Список доступных команд пуст, циклограмма не может быть загрежена!");
                throw exc;
            }
            string cycLine;
            _curLine = 1;
            using (StreamReader sr = new StreamReader(cycFName))
            {
                while (sr.Peek() >= 0)              // читам файл по строкам
                {
                    cycLine = sr.ReadLine();
                    try
                    {
                        TryParseString(cycLine);
                    }
                    catch (CyclogramParsingException e)
                    {
                        _wasError = true;
                        _curCommand.errorInCommand += e.Message + "\t";
                    }
                    finally
                    {
                        if (_commandExistsOnLine)           // если команда есть на строке, добавляем ее в список
                        {
                            _commandExistsOnLine = false;

                            _curCommand.onLine = _curLine;
                            commands.Add(commands.Count.ToString(), _curCommand);   // ключ - номер текущей распознанной команды в файле циклограмм
                            //начинаем собирать новую команду
                            _curCommand = new CyclogramCommand();
                        }
                    }
                    _curLine++;
                }
            }
        }

        /// <summary>
        /// Получаем текущую команду
        /// </summary>
        /// <returns>Возвращает null, если дошли до конца списка команд</returns>
        public CyclogramCommand GetCurCmd()
        {
            if (curCommandIdx == commands.Count - 1)
                return null;
            else
            {
                return commands[curCommandIdx.ToString()];
            }
        }

        /// <summary>
        /// Получаем следующую команду
        /// </summary>
        /// <returns>Возвращает null, если больше команд нет</returns>
        public CyclogramCommand GetNextCmd()
        {
            if (curCommandIdx == commands.Count - 1)
                return null;
            else
            {
                curCommandIdx++;
                return commands[curCommandIdx.ToString()];
            }
        }

        /// <summary>
        /// Проверяем, есть ли на этой строке файла циклограмм команда
        /// </summary>
        /// <param name="cycLine">Номер строки</param>
        /// <returns>Возвращает TRUE, если на этой строке есть команда</returns>
        public bool isCmdExistsOnLine(uint cycLine)
        {
            foreach (var cmd in commands)
            {
                if (cmd.Value.onLine == cycLine)
                {
                    return true;
                }
            }

            return false;
        }
    }
}