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
    public class CyclogramCommand 
    {
        /// <summary>
        /// Идентификатор неизвестной команды
        /// </summary>
        public const int CYC_UNKNOWN_COMMAND_ID = -1;

        /// <summary>
        /// Название команды
        /// </summary>
        public string name;

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
        /// На какой строке файла циклограмм находится команда
        /// </summary>
        public int onLine;

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
        public int delayBeforeCmdMs;

        /// <summary>
        /// Абсолютное время выполнения этой команды (в данный моммент не используется)
        /// </summary>
        public string absoluteTime;

        /// <summary>
        /// Была ли ошибка при распозновании этой команды
        /// </summary>
        public string errorInCommand;

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public CyclogramCommand()
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
        public CyclogramCommand(string name, testFunctionDelegate testFunc, execFunctionDelegate execFunc, string color)
        {
            clear();
            this.name = name;
            testFunction = testFunc;
            execFunction = execFunc;
            this.color = color;
        }

        /// <summary>
        /// Инициализация команды по-умолчанию
        /// </summary>
        public void clear()
        {
            name = "";
            testFunction = null;
            execFunction = null;
            color = "";
            id = CYC_UNKNOWN_COMMAND_ID;
            cycFullFileName = "";
            cycIdx = 0;
            onLine = 0;

            parameters = null;
            comments = "";

            delayBeforeCmdMs = 0;
            absoluteTime = "";
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
            string commStr = "";
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
            if (comments != "")
            {
                commStr = " Comments:" + comments;
            }
            return "<"+onLine.ToString()+"> "+name + " " + tFunStr + " " + eFunStr + errsStr + commStr;
        }
    }

    /// <summary>
    /// Класс поддержки списка команд.
    /// Используется для реализации списка поддерживаемых команд и списка команд из файла циклограмм
    /// В случае поддерживаемых команд, ключем словаря является название команды (должно быть уникальным)
    /// В случае команд из файла циклограмм, ключем является номер (порядковый) команды циклограммы
    /// </summary>
    public class CyclogramCommands : Dictionary<string, CyclogramCommand>
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
        public bool AddCommand(string cmdKey, CyclogramCommand cmd)
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