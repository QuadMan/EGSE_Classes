//-----------------------------------------------------------------------
// <copyright file="EGSEThreadingCyclogram.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

namespace EGSE.Threading 
{
    using System;
    using System.Threading;
    using EGSE.Cyclogram;

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
    /// <param name="cycFileName">Имя файла циклограмм</param>
    public delegate void StartFinishEventHandler(string cycFileName);

    /// <summary>
    /// Делегат, вызываемый при возникновении ошибки выполнения команды циклограммы
    /// </summary>
    /// <param name="cycCommand">Команда, которая произвела ошибку при выполнении</param>
    public delegate void ExecErrorEventHandler(CyclogramLine cycCommand);

    /// <summary>
    /// Состояния циклограммы
    /// </summary>
    public enum CurState 
    {
        /// <summary>
        /// The cyclo none
        /// </summary>
        cycloNone,

        /// <summary>
        /// The cyclo loaded
        /// </summary>
        cycloLoaded,

        /// <summary>
        /// The cyclo loaded with errors
        /// </summary>
        cycloLoadedWithErrors,

        /// <summary>
        /// The cyclo running
        /// </summary>
        cycloRunning 
    }
        
    /// <summary>
    /// Класс потока циклограммы
    /// </summary>
    public class CyclogramThread
    {
        /// <summary>
        /// The _cyclo thread
        /// </summary>
        private Thread _cycloThread;

        /// <summary>
        /// The _cyclo state
        /// </summary>
        private CurState _cycloState;

        /// <summary>
        /// The _cyclo thread terminated
        /// </summary>
        private volatile bool _cycloThreadTerminated;

        /// <summary>
        /// Признак, что циклограмма загружена без ошибок и можно ее выполнять
        /// при завершении потока, нужно ли переходить на следующую команду.
        /// </summary>
        private bool _setNextCmd;

        /// <summary>
        /// текущая позиция в файле циклограмм
        /// </summary>
        private CycPosition _cycloPos;

        /// <summary>
        /// Событие, вызываемое при переходе на следующую команду
        /// </summary>
        private StepEventHandler _nextCommandEvent;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramThread" />.
        /// </summary>
        public CyclogramThread()
        {
            _cycloState = CurState.cycloNone;
            _cycloThreadTerminated = false;

            CycloFile = new CyclogramFile();
            _cycloPos = new CycPosition(CycloFile);

            _setNextCmd = false;
        }

        /// <summary>
        /// Получает или задает экземпляр класса, представляющий файл циклограммы.
        /// </summary>
        public CyclogramFile CycloFile { get; set; }

        /// <summary>
        /// Получает или задает метод, вызываемый при изменении состояния циклограммы.
        /// </summary>
        public StateChangeEventHandler ChangeStateEvent { get; set; }

        /// <summary>
        /// Получает или задает событие, вызываемый при отсчете секунд ожидания выполнения текущей команды.
        /// TODO Может и не нужен?
        /// </summary>
        public StepEventHandler DelaySecondEvent { get; set; }

        /// <summary>
        /// Получает или задает событие, вызываемое при переходе на новую команду.
        /// </summary>
        public StepEventHandler NextCommandEvent
        {
            get
            {
                return _nextCommandEvent;
            }

            set
            {
                _nextCommandEvent = value;
                _cycloPos.SetCmdEvent = value;
            }
        }

        /// <summary>
        /// Получает или задает метод, вызываемый при окончании выполнения циклограммы.
        /// Примечание:
        /// Когда доходим до конца циклограммы.
        /// </summary>
        public StartFinishEventHandler FinishedEvent { get; set; }

        /// <summary>
        /// Получает или задает метод, вызываемый при остановке циклограммы по команде STOP, или по кнопке стоп.
        /// </summary>
        public StartFinishEventHandler StopEvent { get; set; }

        /// <summary>
        /// Получает или задает событие, вызываемое при старте циклограммы.
        /// </summary>
        public StartFinishEventHandler StartEvent { get; set; }

        /// <summary>
        /// Получает или задает метод, вызываемый при ошибке выполнения команды.
        /// </summary>
        public ExecErrorEventHandler CommandExecErrorEvent { get; set; }

        /// <summary>
        /// Получает текущее состояние циклограммы.
        /// </summary>
        public CurState State
        {
            get
            {
                return _cycloState;
            }
        }

        /// <summary>
        /// Загружаем циклограмму.
        /// Примечание:
        /// В случае ошмибки загрузки циклограммы, генерируем исключение.
        /// </summary>
        /// <param name="fileName">Путь к файлу циклограмм.</param>
        /// <param name="availableCommands">Список доступных команд.</param>
        public void Load(string fileName, CyclogramCommands availableCommands)
        {
            try 
            {
                CycloFile.TryLoad(fileName, availableCommands);
                if (CycloFile.WasError)
                {
                    CyclogramParsingException exc = new CyclogramParsingException("Ошибки в циклограмме");
                    throw exc;
                }
                else
                {
                    ChangeState(CurState.cycloLoaded);
                    CycloFile.CalcAbsoluteTime();
                    _cycloPos.SetToLine(0, true);
                }
            }
            catch
            {
                ChangeState(CurState.cycloLoadedWithErrors);
            }
        }

        /// <summary>
        /// Проверяем, есть ли на этой строке команда (или это комментарий)
        /// </summary>
        /// <param name="lineNum">номер строки</param>
        /// <returns>TRUE, если на строке команда</returns>
        public bool IsCommandOnLine(int lineNum)
        {
            return ((uint)lineNum < (uint)CycloFile.Commands.Count) && (_cycloState == CurState.cycloLoaded) && CycloFile.Commands[lineNum].IsCommand;
        }

        /// <summary>
        /// Запускаем циклограмму, если она успешно загружена
        /// TODO: свести к Start(lineNum)
        /// </summary>
        public void Start()
        {
            Start(0);
        }

        /// <summary>
        /// Запускаем циклограмму с определенной строки.
        /// </summary>
        /// <param name="lineNum">Номер строки, с которой нужно запускать выполнение циклограммы</param>
        public void Start(int lineNum)
        {
            if ((_cycloState != CurState.cycloLoaded) || (_cycloPos.SetToLine(lineNum, true) == null))
            {
                return;
            }

            ChangeState(CurState.cycloRunning);
            CycloFile.CalcAbsoluteTime(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, lineNum);     // рассчитаем абсолютные времена выполнения для команд циклограммы

            _setNextCmd = false;
            _cycloThread = new Thread(Execute);
            _cycloThread.IsBackground = true;
            _cycloThreadTerminated = false;
            _cycloThread.Start();
        }

        /// <summary>
        /// Выполняем шаг циклограммы
        /// </summary>
        /// <param name="lineNum">Номер строки, которую нужно выполнить</param>
        public void Step(int lineNum)
        {
            if ((_cycloState != CurState.cycloLoaded) || (_cycloPos.SetToLine(lineNum, true) == null))
            {
                return;
            }

            ExecCurCmdFunction();
            if (_cycloPos.GetNextCmd() == null)
            {
                ChangeState(CurState.cycloLoaded);       // больше команд нет, останавливаем поток выполнения циклограммы                
            }

            if (NextCommandEvent != null)
            {
                NextCommandEvent(_cycloPos.CurCmd);
            }
        }

        /// <summary>
        /// Останавливаем выполнение циклограммы
        /// </summary>
        public void Stop()
        {
            ChangeState(CurState.cycloLoaded);     // по изменению состояния, поток автоматически завершится
        }

        /// <summary>
        /// Останавливаем выполнение циклограммы и переходим на следующую команду, если она есть
        /// </summary>
        public void StopAndSetNextCmd()
        {
            _setNextCmd = true;
            ChangeState(CurState.cycloLoaded);
        }

        /// <summary>
        /// Метод изменения текущего состояния потока циклограмм.
        /// </summary>
        /// <param name="newState">The new state.</param>
        private void ChangeState(CurState newState)
        {
            if (_cycloState != newState)
            {
                _cycloState = newState;

                // если поменяли состояние на отличное от "выполнение", поток останавливаем
                if (_cycloState != CurState.cycloRunning)
                {
                    _cycloThreadTerminated = true;
                }

                if (ChangeStateEvent != null)
                {
                    ChangeStateEvent(_cycloState);
                }
            }
        }

        /// <summary>
        /// Выполняем текущую команду
        /// </summary>
        private void ExecCurCmdFunction()
        {
            if (_cycloPos.CurCmd == null)
            {
                return;
            }

            bool cmdResult = _cycloPos.CurCmd.ExecFunction(_cycloPos.CurCmd.Parameters);

            // если команда выполнилась с ошибкой, вызовем соответствующий делегат
            if ((!cmdResult) && (CommandExecErrorEvent != null))
            {
                CommandExecErrorEvent(_cycloPos.CurCmd);
            }
        }

        /// <summary>
        /// Поток выполнения циклограммы
        /// </summary>
        private void Execute()
        {
            int delayMs;

            while (!_cycloThreadTerminated)
            {
                // в состоянии выполнения циклограммы
                if (_cycloState == CurState.cycloRunning)
                {
                    delayMs = (_cycloPos.CurCmd.DelayMs > 1000) ? 1000 : _cycloPos.CurCmd.DelayMs;
                    if (DelaySecondEvent != null)
                    {
                        DelaySecondEvent(_cycloPos.CurCmd);
                    }
                }
                else
                {
                    // останавливаем поток
                    ChangeState(CurState.cycloLoaded);
                    if (_setNextCmd)
                    {
                        _cycloPos.GetNextCmd();
                        if (NextCommandEvent != null)
                        {
                            NextCommandEvent(_cycloPos.CurCmd);
                        }
                    }

                    continue;
                }

                System.Threading.Thread.Sleep(delayMs);

                // уменьшаем время до выполнения команды
                _cycloPos.CurCmd.DelayMs -= delayMs;

                // пришло время выполнить команду и мы не остановлены извне
                if ((_cycloPos.CurCmd.DelayMs <= 0) && (!_cycloThreadTerminated))
                {
                    ExecCurCmdFunction();
                    _cycloPos.CurCmd.RestoreDelay();

                    // больше команд нет, останавливаем поток выполнения циклограммы
                    if (_cycloPos.GetNextCmd() == null)
                    {
                        ChangeState(CurState.cycloLoaded);
                    }

                    if (NextCommandEvent != null)
                    {
                        NextCommandEvent(_cycloPos.CurCmd);
                    }
                }
            }

            // восстанавливаем предыдущее значение задержки, если принудительно остановили циклограмму (по кнопку Стоп)
            if (_cycloPos.CurCmd != null)
            {
                _cycloPos.CurCmd.RestoreDelay();
            }

            // циклограмма остановлена
            if (StopEvent != null)
            {
                StopEvent(CycloFile.FileName);
            }

            // проверим, кончилась ли циклограмма
            if (_cycloPos.IsLastCommand && (FinishedEvent != null))
            {
                FinishedEvent(CycloFile.FileName);
            }
        }
    }
}