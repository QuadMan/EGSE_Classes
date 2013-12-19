/*** EDGEThreadingCyclogram.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль поддержки потока циклограмм
**
** Author: Семенов Александр
** Project: КИА
** Module: EDGEThreadingCyclogram
** Requires: 
** Comments:
 *
** History:
**  0.1.0	(06.12.2013) -	Начальная версия
**
*/

using System;
using System.Collections.Generic;
using System.Threading;
using EGSE.Cyclogram;

namespace EGSE.Threading {
    /// <summary>
    /// Состояния циклограммы
    /// </summary>
    public enum CurState { csNone, csLoaded, csLoadedWithErrors, csRunning };
        
    /// <summary>
    /// Делегат, вызываемый при изменении состояния циклограмм (выполняет, пауза)
    /// </summary>
    /// <param name="cState">В какое состояние перешли</param>
    public delegate void onChangeCyclogramStateDelegate(CurState cState);

    /// <summary>
    /// Делегат, вызываемый при выполнении очередного шага циклограммы
    /// </summary>
    /// <param name="cycCommand">Какая команда выполнилась</param>
    public delegate void onCyclogramStepDelegate(CyclogramLine cycCommand);

    /// <summary>
    /// Делегат, вызываемый при окончании выполнения циклограммы
    /// </summary>
    public delegate void onCyclogramFinishedDelegate();

    /// <summary>
    /// Делегат, вызываемый при возникновении ошибки выполнения команды циклограммы
    /// </summary>
    /// <param name="cycCommand"></param>
    public delegate void onCyclogramCommandExecErrorDelegate(CyclogramLine cycCommand);

    /// <summary>
    /// Класс потока циклограммы
    /// </summary>
    public class CyclogramThread
    {
        // поток циклограммы
        private Thread _cThread;
        // состояние циклограммы
        private CurState _cState;

//        private CyclogramCommands _cycCommands;
        // файл циклограммы
        public CyclogramFile _cycFile;

        // признак завершения циклограммы
        private volatile bool _terminated;

        // признак, что циклограмма загружена без ошибок
        private bool _cycLoaded;

        /// <summary>
        /// путь до текущей циклограммы (для поддержки подгрузки циклограмм, сейчас не используется)
        /// </summary>
        public string curCycFileName { get;  set; }
        /// <summary>
        /// Текущая строка выполнения команды в файле цкилограмм
        /// </summary>
        public uint curLineNum { get; set; }
        /// <summary>
        /// текущая команда
        /// </summary>
        public CyclogramLine curCmd;
        /// <summary>
        /// текущая задержка
        /// </summary>
        public uint curCmdDelayMs;

        /// <summary>
        /// Метод, вызываемый при изменении состояния циклограммы
        /// </summary>
        public onChangeCyclogramStateDelegate onChangeState;

        /// <summary>
        /// Метод, вызываемый при очередном шаге циклограммы (либо секунда, либо выполнение команды)
        /// </summary>
        public onCyclogramStepDelegate onCmdStep;

        public onCyclogramStepDelegate onGetCmd;

        /// <summary>
        /// Метод, вызываемый при окончании выполнения циклограммы
        /// </summary>
        public onCyclogramFinishedDelegate onFinished;

        /// <summary>
        /// Метод, вызываемый при ошибке выполнения команды
        /// </summary>
        public onCyclogramCommandExecErrorDelegate onCmdExecError;

	    /// <summary>
	    /// Основной конструктор
	    /// </summary>
	    /// <param name="availableCommands">Список доступных команд циклограммы</param>
        public CyclogramThread(CyclogramCommands availableCommands)
	    {
            //_cycCommands = new CyclogramCommands();
            _cycFile = new CyclogramFile(availableCommands);

            _cState = CurState.csNone;
            _terminated = false;
            _cycLoaded = false;
	    }

        /// <summary>
        /// Метод изменения текущего состояния потока циклограмм
        /// </summary>
        /// <param name="newState"></param>
        private void changeState(CurState newState)
        {
            if (_cState != newState)
            {
                _cState = newState;
                if (_cState != CurState.csRunning)      // если поменяли состояние на отличное от "выполнение", поток останавливаем
                {
                    _terminated = true;
                }
                if (onChangeState != null)
                {
                    onChangeState(_cState);
                }
            }
        }

        /// <summary>
        /// Поток выполнения циклограммы
        /// </summary>
        private void Execute() {
            int delayMs;

            while (!_terminated)
            {
                // в состоянии выполнения циклограммы
                if (_cState == CurState.csRunning)
                {
                    // сколько мс осталось
                    delayMs = (curCmd.Delay > 1000) ? 1000 : curCmd.Delay;
                    if (onCmdStep != null)
                    {
                        onCmdStep(curCmd);
                    }
                }
                else
                {
                    changeState(CurState.csLoaded);       // останавливаем поток
                    continue;
                }
                System.Threading.Thread.Sleep(delayMs);

                curCmd.Delay -= delayMs;     // уменьшаем время до выполнения команды

                if ((curCmd.Delay == 0) && (!_terminated))       // пришло время выполнить команду и мы не остановлены извне
                {
                    bool cmdResult = curCmd.execFunction(curCmd.parameters);
                    // если команда выполнилась с ошибкой, вызовем соответствующий делегат
                    if ((!cmdResult) && (onCmdExecError != null))
                    {
                        onCmdExecError(curCmd);
                    }
                    curCmd.RestoreDelay();
                    curCmd = _cycFile.GetNextCmd();
                    if (curCmd == null)
                    {
                        changeState(CurState.csLoaded);       // больше команд нет, останавливаем поток выполнения циклограммы
                    }
                    if (onGetCmd != null)
                    {
                        onGetCmd(curCmd);
                    }
                }
            }
            if (onFinished != null)
            {
                onFinished();
            }
        }

        /// <summary>
        /// Загружаем циклограмму
        /// В случае ошмибки загрузки циклограммы, генерируем исключение
        /// </summary>
        /// <param name="fName">Путь к файлу циклограмм</param>
        /// <param name="availableCommands">Список доступных команд</param>
        public void Load(string fName, CyclogramCommands availableCommands)
        {
            _cycLoaded = false;
            try {
                _cycFile.TryLoad(fName);//, availableCommands);
                if (_cycFile.WasError)
                {
                    //throw new ApplicationException();
                    changeState(CurState.csLoadedWithErrors);
                }
                else
                {
                    changeState(CurState.csLoaded);
                    _cycLoaded = true;
                    _cycFile.CalcAbsoluteTime();
                    setToFirstLine();
                }
            }
            catch
            {
                throw; // System.Console.WriteLine("В циклограмме найдены ошибки!");
            }
        }

        public CurState State
        {
            get
            {
                return _cState;
            }
        }

        //public bool IsLoaded()
        //{
        //    return _cState == CurState.csLoaded;
        //}

        private void setToFirstLine()
        {
            curCmd = _cycFile.GetFirstCmd();
            if (onGetCmd != null)
            {
                onGetCmd(curCmd);
            }
        }

        public bool IsCommandOnLine(int lineNum)
        {
            return (((uint)lineNum < (uint)_cycFile.commands.Count) && (_cState == CurState.csLoaded) && (_cycFile.commands[lineNum].isCommand));
        }

        /// <summary>
        /// Запускаем циклограмму, если она успешно загружена
        /// </summary>
        public void Start()
        {
            if (_cycLoaded)
            {
                setToFirstLine();
                if (curCmd != null)
                {
                    changeState(CurState.csRunning);
                    _cycFile.CalcAbsoluteTime(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                    _cThread = new Thread(Execute);
                    _terminated = false;
                    _cThread.Start();
                }
            }
        }

        /// <summary>
        ///  Запускаем циклограмму с определенной строки
        /// </summary>
        /// <param name="LineNum"></param>
        public void Start(uint LineNum)
        {

        }

        /// <summary>
        /// Останавливаем выполнение циклограммы
        /// </summary>
        public void Stop()
        {
            changeState(CurState.csLoaded);
        }

        /// <summary>
        /// Проверяем, есть ли на строке команда
        /// </summary>
        /// <param name="cycLine">Номер строки в файле циклограмм</param>
        /// <returns></returns>
        public bool isCmdExistsOnLine(uint cycLine)
        {
            if (!_cycLoaded)
                return false;
            return _cycFile.isCmdExistsOnLine(cycLine);
        }
    }

}