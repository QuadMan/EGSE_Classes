//-----------------------------------------------------------------------
// <copyright file="EGSEThreadingCyclogram.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

namespace EGSE.Threading 
{
    using EGSE.Cyclogram;
    using System;
    using System.Threading;

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
        /// csNone - ничего не загружено, начальное состояние
        csNone, 
        
        /// csLoaded - циклограмма загружена без ошибок
        csLoaded, 
        
        /// csLoadedWithErrors - циклограмма загружена с ошибками
        csLoadedWithErrors,
        
        /// csRunning - циклограмма выполняется
        csRunning 
    }
        
    /// <summary>
    /// Класс потока циклограммы
    /// </summary>
    public class CyclogramThread
    {
        /// файл циклограммы
        public CyclogramFile CycFile;

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
        /// Событие, вызываемое при переходе на следующую команду
        /// </summary>
        private StepEventHandler _nextCommandEvent;

        /// <summary>
        /// Событие, вызываемое при переходе на новую команду
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
                _cPos.SetCmdEvent = value;
            } 
        }

        /// <summary>
        /// Метод, вызываемый при окончании выполнения циклограммы (когда доходим до конца циклограммы)
        /// </summary>
        public StartFinishEventHandler FinishedEvent;

        /// <summary>
        /// Метод, вызываемый при остановке циклограммы по команде STOP, или по кнопке стоп
        /// </summary>
        public StartFinishEventHandler StopEvent;

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

        /// поток циклограммы
        private Thread _thread;

        /// состояние циклограммы
        private CurState _state;

        /// признак завершения потока циклограммы
        private volatile bool _terminated;

        /// признак, что циклограмма загружена без ошибок и можно ее выполнять
        /// при завершении потока, нужно ли переходить на следующую команду
        private bool _setNextCmd;

        /// текущая позиция в файле циклограмм
        private CycPosition _cPos;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramThread" />.
        /// </summary>
        public CyclogramThread()
        {
            _state = CurState.csNone;
            _terminated = false;

            CycFile = new CyclogramFile();
            _cPos = new CycPosition(CycFile);

            _setNextCmd = false;
        }

        /// <summary>
        /// Метод изменения текущего состояния потока циклограмм
        /// </summary>
        /// <param name="newState"></param>
        private void ChangeState(CurState newState)
        {
            if (_state != newState)
            {
                _state = newState;

                // если поменяли состояние на отличное от "выполнение", поток останавливаем
                if (_state != CurState.csRunning)      
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
        /// Выполняем текущую команду
        /// </summary>
        private void ExecCurCmdFunction()
        {
            if (_cPos.CurCmd == null)
            {
                return;
            }

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
        private void Execute() 
        {
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
                else 
                {
                    // останавливаем поток
                    ChangeState(CurState.csLoaded);
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

                // пришло время выполнить команду и мы не остановлены извне
                if ((_cPos.CurCmd.DelayMs <= 0) && (!_terminated))       
                {
                    ExecCurCmdFunction();
                    _cPos.CurCmd.RestoreDelay();

                    // больше команд нет, останавливаем поток выполнения циклограммы
                    if (_cPos.GetNextCmd() == null)
                    {
                        ChangeState(CurState.csLoaded);       
                    }

                    if (NextCommandEvent != null)
                    {
                        NextCommandEvent(_cPos.CurCmd);
                    }
                }
            }

            // восстанавливаем предыдущее значение задержки, если принудительно остановили циклограмму (по кнопку Стоп)
            if (_cPos.CurCmd != null)
            {
                _cPos.CurCmd.RestoreDelay();
            }

            // циклограмма остановлена
            if (StopEvent != null)
            {
                StopEvent(CycFile.FileName);
            }

            // проверим, кончилась ли циклограмма
            if ((_cPos.IsLastCommand) && (FinishedEvent != null))
            {
                FinishedEvent(CycFile.FileName);
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
            try 
            {
                CycFile.TryLoad(fName, availableCommands);
                if (CycFile.WasError)
                {
                    CyclogramParsingException exc = new CyclogramParsingException("Ошибки в циклограмме");
                    throw exc;
                }
                else
                {
                    ChangeState(CurState.csLoaded);
                    CycFile.CalcAbsoluteTime();
                    _cPos.SetToLine(0, true);
                }
            }
            catch
            {
                ChangeState(CurState.csLoadedWithErrors);
                // throw; 
            }
        }

        /// <summary>
        /// Проверяем, есть ли на этой строке команда (или это комментарий)
        /// </summary>
        /// <param name="lineNum">номер строки</param>
        /// <returns>TRUE, если на строке команда</returns>
        public bool IsCommandOnLine(int lineNum)
        {
            return ((uint)lineNum < (uint)CycFile.commands.Count) && (_state == CurState.csLoaded) && CycFile.commands[lineNum].IsCommand;
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
        ///  Запускаем циклограмму с определенной строки
        /// </summary>
        /// <param name="LineNum">Номер строки, с которой нужно запускать выполнение циклограммы</param>
        public void Start(int lineNum)
        {
            if ((_state != CurState.csLoaded) || (_cPos.SetToLine(lineNum, true) == null))
            {
                return;
            }

            ChangeState(CurState.csRunning);
            CycFile.CalcAbsoluteTime(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, lineNum);     // рассчитаем абсолютные времена выполнения для команд циклограммы

            _setNextCmd = false;
            _thread = new Thread(Execute);
            _thread.IsBackground = true;
            _terminated = false;
            _thread.Start();
        }

        /// <summary>
        /// Выполняем шаг циклограммы
        /// </summary>
        /// <param name="lineNum">Номер строки, которую нужно выполнить</param>
        public void Step(int lineNum)
        {
            if ((_state != CurState.csLoaded) || (_cPos.SetToLine(lineNum, true) == null))
            {
                return;
            }

            ExecCurCmdFunction();
            if (_cPos.GetNextCmd() == null)
            {
                ChangeState(CurState.csLoaded);       // больше команд нет, останавливаем поток выполнения циклограммы                
            }

            if (NextCommandEvent != null)
            {
                NextCommandEvent(_cPos.CurCmd);
            }
        }

        /// <summary>
        /// Останавливаем выполнение циклограммы
        /// </summary>
        public void Stop()
        {
            ChangeState(CurState.csLoaded);     // по изменению состояния, поток автоматически завершится
        }

        /// <summary>
        /// Останавливаем выполнение циклограммы и переходим на следующую команду, если она есть
        /// </summary>
        public void StopAndSetNextCmd()
        {
            _setNextCmd = true;
            ChangeState(CurState.csLoaded);
        }
    }
}