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
using System.Linq;
using System.Threading;
using EGSE.Cyclogram;

namespace EGSE.Threading {

    /// <summary>
    /// Состояния циклограммы
    ///     csNone - ничего
    ///     csLoaded - циклограмма загружена без ошибок
    ///     csLoadedWithErrors - циклограмма загружена с ошибками
    ///     csRunning - циклограмма выполняется
    /// </summary>
    public enum CurState { csNone, csLoaded, csLoadedWithErrors, csRunning };
        
    /// <summary>
    /// Делегат, вызываемый при изменении состояния циклограмм (выполняет, пауза)
    /// </summary>
    /// <param name="cState">В какое состояние перешли</param>
    public delegate void StateChangeEventHandler(CurState cState);

    /// <summary>
    /// Делегат, вызываемый при выполнении очередного шага циклограммы
    /// </summary>
    /// <param name="cycCommand">Какая команда выполнилась</param>
    public delegate void StepEventHandler(CyclogramLine cycCommand);

    /// <summary>
    /// Делегат, вызываемый при окончании выполнения циклограммы
    /// </summary>
    public delegate void StartFinishEventHandler(string cycFileName);

    /// <summary>
    /// Делегат, вызываемый при возникновении ошибки выполнения команды циклограммы
    /// </summary>
    /// <param name="cycCommand"></param>
    public delegate void ExecErrorEventHandler(CyclogramLine cycCommand);

    /// <summary>
    /// Класс потока циклограммы
    /// </summary>
    public class CyclogramThread
    {
        // поток циклограммы
        private Thread _thread;
        // состояние циклограммы
        private CurState _state;
        // файл циклограммы
        public CyclogramFile _cycFile;
        // признак завершения потока циклограммы
        private volatile bool _terminated;
        // признак, что циклограмма загружена без ошибок и можно ее выполнять
        private bool _cycLoaded;
        // при завершении потока, нужно ли переходить на следующую команду
        private bool _setNextCmd;

        private CycPosition _cPos;

        /// <summary>
        /// путь до текущей циклограммы (для поддержки подгрузки циклограмм, сейчас не используется)
        /// </summary>
        //public string curCycFileName { get;  set; }
        /// <summary>
        /// Текущая строка выполнения команды в файле цкилограмм
        /// </summary>
        //public uint curLineNum { get; set; }

        /// <summary>
        /// текущая команда
        /// </summary>
        //!public CyclogramLine curCmd;

        /// <summary>
        /// Метод, вызываемый при изменении состояния циклограммы
        /// </summary>
        public StateChangeEventHandler ChangeStateEvent;

        /// <summary>
        /// Событие, вызываемый при отсчете секунд ожидания выполнения текущей команды
        /// Может и не нужен?
        /// </summary>
        public StepEventHandler DelaySecondEvent;

        private StepEventHandler _nextCommandEvent;
        /// <summary>
        /// Событие, вызываемое при переходе на новую команду
        /// </summary>
        public StepEventHandler NextCommandEvent
        {
            get { return _nextCommandEvent; }
            set { _nextCommandEvent = value;
                _cPos.SetCmdEvent = value;
            } 
        }

        /// <summary>
        /// Метод, вызываемый при окончании выполнения циклограммы
        /// </summary>
        public StartFinishEventHandler FinishedEvent;

        /// <summary>
        /// Событие, вызываемое при старте циклограммы
        /// </summary>
        public StartFinishEventHandler StartEvent;

        /// <summary>
        /// Метод, вызываемый при ошибке выполнения команды
        /// </summary>
        public ExecErrorEventHandler CommandExecErrorEvent;
    
        /// <summary>
        /// Состояние циклограммы
        /// </summary>
        public CurState State
        {
            get
            {
                return _state;
            }
        }

	    /// <summary>
	    /// Основной конструктор
	    /// </summary>
        public CyclogramThread()
	    {
            _state = CurState.csNone;
            _terminated = false;

            _cycFile = new CyclogramFile();
            _cycLoaded = false;
            _cPos = new CycPosition(_cycFile);

	        _setNextCmd = false;
	    }

        /// <summary>
        /// Метод изменения текущего состояния потока циклограмм
        /// </summary>
        /// <param name="newState"></param>
        private void changeState(CurState newState)
        {
            if (_state != newState)
            {
                _state = newState;
                if (_state != CurState.csRunning)      // если поменяли состояние на отличное от "выполнение", поток останавливаем
                {
                    _terminated = true;
                }
                if (ChangeStateEvent != null)
                {
                    ChangeStateEvent(_state);
                }
            }
        }

        /// <summary>
        /// Вполняем текущую команду
        /// </summary>
        private void execCurCmdFunction()
        {
            if (_cPos.CurCmd == null) return;

            bool cmdResult = _cPos.CurCmd.ExecFunction(_cPos.CurCmd.Parameters);            
            // если команда выполнилась с ошибкой, вызовем соответствующий делегат
            if ((!cmdResult) && (CommandExecErrorEvent != null))
            {
                CommandExecErrorEvent(_cPos.CurCmd);
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
                if (_state == CurState.csRunning)
                {
                    delayMs = (_cPos.CurCmd.DelayMs > 1000) ? 1000 : _cPos.CurCmd.DelayMs;     
                    if (DelaySecondEvent != null)
                    {
                        DelaySecondEvent(_cPos.CurCmd);
                    }
                }
                else // останавливаем поток
                {
                    changeState(CurState.csLoaded);
                    if (_setNextCmd)
                    {
                        _cPos.GetNextCmd();
                        if (NextCommandEvent != null)
                        {
                            NextCommandEvent(_cPos.CurCmd);
                        }
                    }
                    continue;
                }

                System.Threading.Thread.Sleep(delayMs);
                
                // уменьшаем время до выполнения команды
                _cPos.CurCmd.DelayMs -= delayMs;     

                if ((_cPos.CurCmd.DelayMs <= 0) && (!_terminated))       // пришло время выполнить команду и мы не остановлены извне
                {
                    execCurCmdFunction();
                    _cPos.CurCmd.RestoreDelay();
                    if (_cPos.GetNextCmd() == null)
                    {
                        changeState(CurState.csLoaded);       // больше команд нет, останавливаем поток выполнения циклограммы
                    }
                    if (NextCommandEvent != null)
                    {
                        NextCommandEvent(_cPos.CurCmd);
                    }
                }
            }
            //восстанавливаем предыдущее значение задержки, если принудительно остановили циклограмму (по кнопку Стоп)
            if (_cPos.CurCmd != null)
            {
                _cPos.CurCmd.RestoreDelay();
            }

            if (FinishedEvent != null)
            {
                FinishedEvent(_cycFile.FileName);
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
                _cycFile.TryLoad(fName, availableCommands);
                if (_cycFile.WasError)
                {
                    changeState(CurState.csLoadedWithErrors);
                }
                else
                {
                    changeState(CurState.csLoaded);
                    _cycLoaded = true;
                    _cycFile.CalcAbsoluteTime();
                    //setToFirstLine();
                    _cPos.SetToLine(0, true);
                }
            }
            catch
            {
                changeState(CurState.csLoadedWithErrors);
                throw; 
            }
        }

        /// <summary>
        /// Позиционируем циклограмму на первую строку
        /// </summary>
        /*
    private void setToFirstLine()
    {
        curCmd = _cycFile.GetFirstCmd();
        if (NextCommandEvent != null)
        {
            NextCommandEvent(curCmd);
        }
    }
         */

        /// <summary>
        /// Проверяем, есть ли на этой строке команда (или это комментарий)
        /// </summary>
        /// <param name="lineNum">номер строки</param>
        /// <returns>TRUE, если на строке команда</returns>
        public bool IsCommandOnLine(int lineNum)
        {
            return (((uint)lineNum < (uint)_cycFile.commands.Count) && (_state == CurState.csLoaded) && (_cycFile.commands[lineNum].IsCommand));
        }

        /// <summary>
        /// Запускаем циклограмму, если она успешно загружена
        /// TODO: свести к Start(lineNum)
        /// </summary>
        public void Start()
        {
            Start(0);
            /*
            if (_cycLoaded)
            {
//                setToFirstLine();
                if (_cPos.SetToLine(0, true) != null)
                {
                    changeState(CurState.csRunning);
                    _cycFile.CalcAbsoluteTime(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);     // рассчитаем абсолютные времена выполнения для команд циклограммы

                    _setNextCmd = false;
                    _thread = new Thread(Execute);
                    _terminated = false;
                    _thread.Start();
                }
            }
             */
        }

        /// <summary>
        ///  Запускаем циклограмму с определенной строки
        /// </summary>
        /// <param name="LineNum"></param>
        public void Start(int lineNum)
        {
            if (!_cycLoaded) return;

            if (_cPos.SetToLine(lineNum, true) == null) return;

            changeState(CurState.csRunning);
            _cycFile.CalcAbsoluteTime(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, lineNum);     // рассчитаем абсолютные времена выполнения для команд циклограммы

            _setNextCmd = false;
            _thread = new Thread(Execute);
            _thread.IsBackground = true;
            _terminated = false;
            _thread.Start();
        }
/*
        private void setToLineNum(int lineNum)
        {
            curCmd = null;
            foreach (CyclogramLine cl in _cycFile.commands.Where(l => (l.IsCommand) && (l.Line == lineNum)))
            {
                curCmd = cl;
                _cycFile.SetCmdLine(curCmd);
                if (NextCommandEvent != null)
                {
                    NextCommandEvent(curCmd);
                }
                return;
            }
        }
        */
        public void Step(int lineNum)
        {
            if (!_cycLoaded) return;

            if (_cPos.SetToLine(lineNum, true) == null) return;

            execCurCmdFunction();
            if (_cPos.GetNextCmd() == null)
            {
                changeState(CurState.csLoaded);       // больше команд нет, останавливаем поток выполнения циклограммы                
            }
            if (NextCommandEvent != null)
            {
                NextCommandEvent(_cPos.CurCmd);
            }
            /*
            setToLineNum(lineNum);
            if (curCmd == null) return;

            execCurCmdFunction();
            curCmd = _cycFile.GetNextCmd();
            if (curCmd == null)
            {
                changeState(CurState.csLoaded);       // больше команд нет, останавливаем поток выполнения циклограммы
            }
            if (NextCommandEvent != null)
            {
                NextCommandEvent(curCmd);
            }
           */
        }

        /// <summary>
        /// Останавливаем выполнение циклограммы
        /// </summary>
        public void Stop()
        {
            changeState(CurState.csLoaded);     // по изменению состояния, поток автоматически завершится
        }

        /// <summary>
        /// Останавливаем выполнение циклограммы и переходим на следующую команду, если она есть
        /// </summary>
        public void StopAndSetNextCmd()
        {
            _setNextCmd = true;
            changeState(CurState.csLoaded);
        }
    }

}