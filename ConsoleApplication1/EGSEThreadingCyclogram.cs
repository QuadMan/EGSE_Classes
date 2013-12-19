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
        public CyclogramLine curCmd;

        /// <summary>
        /// Метод, вызываемый при изменении состояния циклограммы
        /// </summary>
        public StateChangeEventHandler ChangeStateEvent;

        /// <summary>
        /// Событие, вызываемый при отсчете секунд ожидания выполнения текущей команды
        /// Может и не нужен?
        /// </summary>
        public StepEventHandler DelaySecondEvent;

        /// <summary>
        /// Событие, вызываемое при переходе на новую команду
        /// </summary>
        public StepEventHandler NextCommandEvent;

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
        /// Поток выполнения циклограммы
        /// </summary>
        private void Execute() {
            int delayMs;

            while (!_terminated)
            {
                // в состоянии выполнения циклограммы
                if (_state == CurState.csRunning)
                {
                    delayMs = (curCmd.DelayMs > 1000) ? 1000 : curCmd.DelayMs;     
                    if (DelaySecondEvent != null)
                    {
                        DelaySecondEvent(curCmd);
                    }
                }
                else // останавливаем поток
                {
                    changeState(CurState.csLoaded);       
                    continue;
                }

                System.Threading.Thread.Sleep(delayMs);
                
                // уменьшаем время до выполнения команды
                curCmd.DelayMs -= delayMs;     

                if ((curCmd.DelayMs <= 0) && (!_terminated))       // пришло время выполнить команду и мы не остановлены извне
                {
                    bool cmdResult = curCmd.ExecFunction(curCmd.Parameters);
                    // если команда выполнилась с ошибкой, вызовем соответствующий делегат
                    if ((!cmdResult) && (CommandExecErrorEvent != null))
                    {
                        CommandExecErrorEvent(curCmd);
                    }
                    curCmd.RestoreDelay();
                    curCmd = _cycFile.GetNextCmd();
                    if (curCmd == null)
                    {
                        changeState(CurState.csLoaded);       // больше команд нет, останавливаем поток выполнения циклограммы
                    }
                    if (NextCommandEvent != null)
                    {
                        NextCommandEvent(curCmd);
                    }
                }
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
                    setToFirstLine();
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
        private void setToFirstLine()
        {
            curCmd = _cycFile.GetFirstCmd();
            if (NextCommandEvent != null)
            {
                NextCommandEvent(curCmd);
            }
        }

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
        /// </summary>
        public void Start()
        {
            if (_cycLoaded)
            {
                setToFirstLine();
                if (curCmd != null)
                {
                    changeState(CurState.csRunning);
                    _cycFile.CalcAbsoluteTime(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);     // рассчитаем абсолютные времена выполнения для команд циклограммы

                    _thread = new Thread(Execute);
                    _terminated = false;
                    _thread.Start();
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
            changeState(CurState.csLoaded);     // по изменению состояния, поток автоматически завершится
        }
    }

}