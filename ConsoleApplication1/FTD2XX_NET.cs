//-----------------------------------------------------------------------
// <copyright file="FTD2XX_NET.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) 2009-2012 Future Technology Devices International Limited.
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>FTDI, Семенов Александр</author>
//-----------------------------------------------------------------------

namespace FTD2XXNET
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;  
    using System.Text;
    using System.Threading;    
          
    /// <summary>
    /// Class wrapper for FTD2XX.DLL
    /// </summary>
    public class FTDICustom
    {
        #region DEFAULT_VALUES
        /// <summary>
        /// The ft COM port not assigned
        /// </summary>
        private const int FTComPortNotAssigned = -1;

        /// <summary>
        /// The ft default baund rate
        /// </summary>
        private const uint FTDefaultBaundRate = 9600;

        /// <summary>
        /// The ft default deadman timeout
        /// </summary>
        private const uint FTDefaultDeadmanTimeout = 5000;

        /// <summary>
        /// The ft default in transfer size
        /// </summary>
        private const uint FTDefaultInTransferSize = 0x1000;

        /// <summary>
        /// The ft default out transfer size
        /// </summary>
        private const uint FTDefaultOutTransferSize = 0x1000;

        /// <summary>
        /// The ft default latency
        /// </summary>
        private const byte FTDefaultLatency = 16;

        /// <summary>
        /// The ft default device identifier
        /// </summary>
        private const uint FTDefaultDeviceID = 0x04036001;
        #endregion

        // Flags for FT_OpenEx

        /// <summary>
        /// The ft open by serial number
        /// </summary>
        private const uint FTOpenBySerialNumber = 0x00000001;

        /// <summary>
        /// The ft open by description
        /// </summary>
        private const uint FTOpenByDescription = 0x00000002;

        /// <summary>
        /// The ft open by location
        /// </summary>
        private const uint FTOpenByLocation = 0x00000004;

        #region FUNCTION_IMPORTS_FTD2XX.DLL
        // Handle to our DLL - used with GetProcAddress to load all of our functions

        /// <summary>
        /// The handle ft d2 XXDLL
        /// </summary>
        private IntPtr handleFTD2XXDLL = IntPtr.Zero;

        // Declare pointers to each of the functions we are going to use in FT2DXX.DLL
        // These are assigned in our constructor and freed in our destructor.

        /// <summary>
        /// Точка входа для функции FT_Open.
        /// </summary>
        private IntPtr entryFTOpen = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_OpenEx.
        /// </summary>
        private IntPtr entryFTOpenEx = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_Close.
        /// </summary>
        private IntPtr entryFTClose = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_Read.
        /// </summary>
        private IntPtr entryFTRead = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_Write.
        /// </summary>
        private IntPtr entryFTWrite = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_GetQueueStatus.
        /// </summary>
        private IntPtr entryFTGetQueueStatus = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_GetStatus.
        /// </summary>
        private IntPtr entryFTGetStatus = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_ResetDevice.
        /// </summary>
        private IntPtr entryFTResetDevice = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_ResetPort.
        /// </summary>
        private IntPtr entryFTResetPort = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_CyclePort.
        /// </summary>
        private IntPtr entryFTCyclePort = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_Rescan.
        /// </summary>
        private IntPtr entryFTRescan = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_Reload.
        /// </summary>
        private IntPtr entryFTReload = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_Purge.
        /// </summary>
        private IntPtr entryFTPurge = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_SetTimeouts.
        /// </summary>
        private IntPtr entryFTSetTimeouts = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_GetDriverVersion.
        /// </summary>
        private IntPtr entryFTGetDriverVersion = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_GetLibraryVersion.
        /// </summary>
        private IntPtr entryFTGetLibraryVersion = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_SetDeadmanTimeout.
        /// </summary>
        private IntPtr entryFTSetDeadmanTimeout = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_SetBitMode.
        /// </summary>
        private IntPtr entryFTSetBitMode = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_SetLatencyTimer.
        /// </summary>
        private IntPtr entryFTSetLatencyTimer = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_GetLatencyTimer.
        /// </summary>
        private IntPtr entryFTGetLatencyTimer = IntPtr.Zero;

        /// <summary>
        /// Точка входа для функции FT_SetUSBParameters.
        /// </summary>
        private IntPtr entryFTSetUSBParameters = IntPtr.Zero;

        #endregion

        // Create private variables for the device within the class

        /// <summary>
        /// The _fthandle
        /// </summary>
        private IntPtr _fthandle = IntPtr.Zero;

        #region CONSTRUCTOR_DESTRUCTOR

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FTDICustom" />.
        /// Constructor for the FTDI class.
        /// </summary>
        public FTDICustom()
        {
            // If FTD2XX.DLL is NOT loaded already, load it
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                // Load our FTD2XX.DLL library
                handleFTD2XXDLL = LoadLibrary(@"FTD2XX.DLL");
                if (handleFTD2XXDLL == IntPtr.Zero)
                {
                    // Failed to load our FTD2XX.DLL library from System32 or the application directory
                    // Try the same directory that this FTD2XX_NET DLL is in
                    // !MessageBox.Show("Attempting to load FTD2XX.DLL from:\n" + Path.GetDirectoryName(GetType().Assembly.Location));
                    handleFTD2XXDLL = LoadLibrary(@Path.GetDirectoryName(GetType().Assembly.Location) + "\\FTD2XX.DLL");
                }
            }

            // If we have succesfully loaded the library, get the function pointers set up
            if (handleFTD2XXDLL != IntPtr.Zero)
            {
                // Set up our function pointers for use through our exported methods
//                pFT_Open = GetProcAddress(hFTD2XXDLL, "FT_Open");
                entryFTOpenEx = GetProcAddress(handleFTD2XXDLL, "FT_OpenEx");
                if (entryFTOpenEx != IntPtr.Zero)
                {
                    FT_OpenEx = (TFT_OpenEx)Marshal.GetDelegateForFunctionPointer(entryFTOpenEx, typeof(TFT_OpenEx));
                }
                else
                {
                }

                entryFTClose = GetProcAddress(handleFTD2XXDLL, "FT_Close");
                FT_Close = (TFT_Close)Marshal.GetDelegateForFunctionPointer(entryFTClose, typeof(TFT_Close));

                entryFTRead = GetProcAddress(handleFTD2XXDLL, "FT_Read");
                FT_Read = (TFT_Read)Marshal.GetDelegateForFunctionPointer(entryFTRead, typeof(TFT_Read));

                entryFTWrite = GetProcAddress(handleFTD2XXDLL, "FT_Write");
                FT_Write = (TFT_Write)Marshal.GetDelegateForFunctionPointer(entryFTWrite, typeof(TFT_Write));

                entryFTGetQueueStatus = GetProcAddress(handleFTD2XXDLL, "FT_GetQueueStatus");
                FT_GetQueueStatus = (TFT_GetQueueStatus)Marshal.GetDelegateForFunctionPointer(entryFTGetQueueStatus, typeof(TFT_GetQueueStatus));

                entryFTGetStatus = GetProcAddress(handleFTD2XXDLL, "FT_GetStatus");
                FT_GetStatus = (TFT_GetStatus)Marshal.GetDelegateForFunctionPointer(entryFTGetStatus, typeof(TFT_GetStatus));

                entryFTResetDevice = GetProcAddress(handleFTD2XXDLL, "FT_ResetDevice");
                FT_ResetDevice = (TFT_ResetDevice)Marshal.GetDelegateForFunctionPointer(entryFTResetDevice, typeof(TFT_ResetDevice));

                entryFTResetPort = GetProcAddress(handleFTD2XXDLL, "FT_ResetPort");
                FT_ResetPort = (TFT_ResetPort)Marshal.GetDelegateForFunctionPointer(entryFTResetPort, typeof(TFT_ResetPort));

                entryFTCyclePort = GetProcAddress(handleFTD2XXDLL, "FT_CyclePort");
                FT_CyclePort = (TFT_CyclePort)Marshal.GetDelegateForFunctionPointer(entryFTCyclePort, typeof(TFT_CyclePort));

                entryFTRescan = GetProcAddress(handleFTD2XXDLL, "FT_Rescan");
                FT_Rescan = (TFT_Rescan)Marshal.GetDelegateForFunctionPointer(entryFTRescan, typeof(TFT_Rescan));

                entryFTReload = GetProcAddress(handleFTD2XXDLL, "FT_Reload");
                FT_Reload = (TFT_Reload)Marshal.GetDelegateForFunctionPointer(entryFTReload, typeof(TFT_Reload));

                entryFTPurge = GetProcAddress(handleFTD2XXDLL, "FT_Purge");
                FT_Purge = (TFT_Purge)Marshal.GetDelegateForFunctionPointer(entryFTPurge, typeof(TFT_Purge));

                entryFTSetTimeouts = GetProcAddress(handleFTD2XXDLL, "FT_SetTimeouts");
                FT_SetTimeouts = (TFT_SetTimeouts)Marshal.GetDelegateForFunctionPointer(entryFTSetTimeouts, typeof(TFT_SetTimeouts));

                entryFTGetDriverVersion = GetProcAddress(handleFTD2XXDLL, "FT_GetDriverVersion");
                FT_GetDriverVersion = (TFT_GetDriverVersion)Marshal.GetDelegateForFunctionPointer(entryFTGetDriverVersion, typeof(TFT_GetDriverVersion));

                entryFTGetLibraryVersion = GetProcAddress(handleFTD2XXDLL, "FT_GetLibraryVersion");
                FT_GetLibraryVersion = (TFT_GetLibraryVersion)Marshal.GetDelegateForFunctionPointer(entryFTGetLibraryVersion, typeof(TFT_GetLibraryVersion));

                entryFTSetDeadmanTimeout = GetProcAddress(handleFTD2XXDLL, "FT_SetDeadmanTimeout");

                entryFTSetBitMode = GetProcAddress(handleFTD2XXDLL, "FT_SetBitMode");
                FT_SetBitMode = (TFT_SetBitMode)Marshal.GetDelegateForFunctionPointer(entryFTSetBitMode, typeof(TFT_SetBitMode));
                
                entryFTSetLatencyTimer = GetProcAddress(handleFTD2XXDLL, "FT_SetLatencyTimer");
                FT_SetLatencyTimer = (TFT_SetLatencyTimer)Marshal.GetDelegateForFunctionPointer(entryFTSetLatencyTimer, typeof(TFT_SetLatencyTimer));

                entryFTGetLatencyTimer = GetProcAddress(handleFTD2XXDLL, "FT_GetLatencyTimer");
                FT_GetLatencyTimer = (TFT_GetLatencyTimer)Marshal.GetDelegateForFunctionPointer(entryFTGetLatencyTimer, typeof(TFT_GetLatencyTimer));

                entryFTSetUSBParameters = GetProcAddress(handleFTD2XXDLL, "FT_SetUSBParameters");
                FT_SetUSBParameters = (TFT_SetUSBParameters)Marshal.GetDelegateForFunctionPointer(entryFTSetUSBParameters, typeof(TFT_SetUSBParameters));
            }
            else
            {
                // Failed to load our DLL - alert the user
                // !MessageBox.Show("Failed to load FTD2XX.DLL.  Are the FTDI drivers installed?");
            }
        }

        /// <summary>
        /// Уничтожает экземпляр класса <see cref="FTDICustom" />.
        /// </summary>
        ~FTDICustom()
        {
            // FreeLibrary here - we should only do this if we are completely finished
            FreeLibrary(handleFTD2XXDLL);
            handleFTD2XXDLL = IntPtr.Zero;
        }
        #endregion

        #region VARIABLES

        #endregion

        #region DELEGATES
        // Definitions for FTD2XX functions

        /// <summary>
        /// Функция "Открыть" USB устройство.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="ftHandle">The ft handle.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_Open(uint index, ref IntPtr ftHandle);

        /// <summary>
        /// Функция "Открыть" USB устройство (Расширенная).
        /// </summary>
        /// <param name="devstring">The devstring.</param>
        /// <param name="dwFlags">The dw flags.</param>
        /// <param name="ftHandle">The ft handle.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_OpenEx(string devstring, uint dwFlags, ref IntPtr ftHandle);

        /// <summary>
        /// Функция "Закрыть" USB устройства.
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_Close(IntPtr ftHandle);

        /// <summary>
        /// Функция "Прочитать" USB устрйоства.
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="lpBuffer">The lp buffer.</param>
        /// <param name="dwBytesToRead">The dw bytes to read.</param>
        /// <param name="lpdwBytesReturned">The LPDW bytes returned.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_Read(IntPtr ftHandle, byte[] lpBuffer, int dwBytesToRead, ref int lpdwBytesReturned);

        /// <summary>
        /// Функция "Записать" USB устрйоства.
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="lpBuffer">The lp buffer.</param>
        /// <param name="dwBytesToWrite">The dw bytes to write.</param>
        /// <param name="lpdwBytesWritten">The LPDW bytes written.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_Write(IntPtr ftHandle, byte[] lpBuffer, uint dwBytesToWrite, ref uint lpdwBytesWritten);

        /// <summary>
        /// Функция "Получить состояние квоты".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="lpdwAmountInRxQueue">The LPDW amount in rx queue.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_GetQueueStatus(IntPtr ftHandle, ref int lpdwAmountInRxQueue);

        /// <summary>
        /// Функция "Получить текущий статус устройства".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="lpdwAmountInRxQueue">The LPDW amount in rx queue.</param>
        /// <param name="lpdwAmountInTxQueue">The LPDW amount in tx queue.</param>
        /// <param name="lpdwEventStatus">The LPDW event status.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_GetStatus(IntPtr ftHandle, ref uint lpdwAmountInRxQueue, ref uint lpdwAmountInTxQueue, ref uint lpdwEventStatus);

        /// <summary>
        /// Функция "Перезагрузить устройство".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_ResetDevice(IntPtr ftHandle);

        /// <summary>
        /// Функция "Перезагрузить порт".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_ResetPort(IntPtr ftHandle);

        /// <summary>
        /// Функция "Задать порт".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_CyclePort(IntPtr ftHandle);

        /// <summary>
        /// Функция "Обновить". 
        /// </summary>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_Rescan();

        /// <summary>
        /// Функция "Перезагрузить".
        /// </summary>
        /// <param name="wVID">The w vid.</param>
        /// <param name="wPID">The w pid.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_Reload(ushort wVID, ushort wPID);

        /// <summary>
        /// Функция "Очистить".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="dwMask">The dw mask.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_Purge(IntPtr ftHandle, uint dwMask);

        /// <summary>
        /// Функция "Задать Timeout-ы".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="dwReadTimeout">The dw read timeout.</param>
        /// <param name="dwWriteTimeout">The dw write timeout.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_SetTimeouts(IntPtr ftHandle, uint dwReadTimeout, uint dwWriteTimeout);

        /// <summary>
        /// Функция "Получить текущую версию драйвера".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="lpdwDriverVersion">The LPDW driver version.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_GetDriverVersion(IntPtr ftHandle, ref uint lpdwDriverVersion);

        /// <summary>
        /// Функция "Получить версию библиотеки".
        /// </summary>
        /// <param name="lpdwLibraryVersion">The LPDW library version.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_GetLibraryVersion(ref uint lpdwLibraryVersion);

        /// <summary>
        /// Функция "Задать время авто-отключения устройства"
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="dwDeadmanTimeout">The dw deadman timeout.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_SetDeadmanTimeout(IntPtr ftHandle, uint dwDeadmanTimeout);

        /// <summary>
        /// Функция "Установить BitMode".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="ucMask">The uc mask.</param>
        /// <param name="ucMode">The uc mode.</param>
        /// <returns>Функция "Установить время задержки".</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_SetBitMode(IntPtr ftHandle, byte ucMask, byte ucMode);

        /// <summary>
        /// Функция "Установить время задержки".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="ucLatency">The uc latency.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_SetLatencyTimer(IntPtr ftHandle, byte ucLatency);

        /// <summary>
        /// Функция "Получить время задержки".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="ucLatency">The uc latency.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_GetLatencyTimer(IntPtr ftHandle, ref byte ucLatency);

        /// <summary>
        /// Функция "Установить USB параметры".
        /// </summary>
        /// <param name="ftHandle">The ft handle.</param>
        /// <param name="dwInTransferSize">Size of the dw in transfer.</param>
        /// <param name="dwOutTransferSize">Size of the dw out transfer.</param>
        /// <returns>Состояние выполнения.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate FT_STATUS TFT_SetUSBParameters(IntPtr ftHandle, uint dwInTransferSize, uint dwOutTransferSize);
        #endregion

        #region CONSTANT_VALUES
        // Constants for FT_STATUS

        /// <summary>
        /// Status values for FTDI devices.
        /// </summary>
        public enum FT_STATUS
        {
            /// <summary>
            /// Status OK
            /// </summary>
            FT_OK = 0,

            /// <summary>
            /// The device handle is invalid
            /// </summary>
            FT_INVALID_HANDLE,

            /// <summary>
            /// Device not found
            /// </summary>
            FT_DEVICE_NOT_FOUND,

            /// <summary>
            /// Device is not open
            /// </summary>
            FT_DEVICE_NOT_OPENED,

            /// <summary>
            /// IO error
            /// </summary>
            FT_IO_ERROR,

            /// <summary>
            /// Insufficient resources
            /// </summary>
            FT_INSUFFICIENT_RESOURCES,

            /// <summary>
            /// A parameter was invalid
            /// </summary>
            FT_INVALID_PARAMETER,

            /// <summary>
            /// The requested baud rate is invalid
            /// </summary>
            FT_INVALID_BAUD_RATE,

            /// <summary>
            /// Device not opened for erase
            /// </summary>
            FT_DEVICE_NOT_OPENED_FOR_ERASE,

            /// <summary>
            /// Device not poened for write
            /// </summary>
            FT_DEVICE_NOT_OPENED_FOR_WRITE,

            /// <summary>
            /// Failed to write to device
            /// </summary>
            FT_FAILED_TO_WRITE_DEVICE,

            /// <summary>
            /// Failed to read the device EEPROM
            /// </summary>
            FT_EEPROM_READ_FAILED,

            /// <summary>
            /// Failed to write the device EEPROM
            /// </summary>
            FT_EEPROM_WRITE_FAILED,

            /// <summary>
            /// Failed to erase the device EEPROM
            /// </summary>
            FT_EEPROM_ERASE_FAILED,

            /// <summary>
            /// An EEPROM is not fitted to the device
            /// </summary>
            FT_EEPROM_NOT_PRESENT,

            /// <summary>
            /// Device EEPROM is blank
            /// </summary>
            FT_EEPROM_NOT_PROGRAMMED,

            /// <summary>
            /// Invalid arguments
            /// </summary>
            FT_INVALID_ARGS,

            /// <summary>
            /// An other error has occurred
            /// </summary>
            FT_OTHER_ERROR
        }

        // Device type identifiers for FT_GetDeviceInfoDetail and FT_GetDeviceInfo

        /// <summary>
        /// List of FTDI device types
        /// </summary>
        public enum FT_DEVICE
        {
            /// <summary>
            /// FT232B or FT245B device
            /// </summary>
            FT_DEVICE_BM = 0,

            /// <summary>
            /// FT8U232AM or FT8U245AM device
            /// </summary>
            FT_DEVICE_AM,

            /// <summary>
            /// FT8U100AX device
            /// </summary>
            FT_DEVICE_100AX,

            /// <summary>
            /// Unknown device
            /// </summary>
            FT_DEVICE_UNKNOWN,

            /// <summary>
            /// FT2232 device
            /// </summary>
            FT_DEVICE_2232,

            /// <summary>
            /// FT232R or FT245R device
            /// </summary>
            FT_DEVICE_232R,

            /// <summary>
            /// FT2232H device
            /// </summary>
            FT_DEVICE_2232H,

            /// <summary>
            /// FT4232H device
            /// </summary>
            FT_DEVICE_4232H,

            /// <summary>
            /// FT232H device
            /// </summary>
            FT_DEVICE_232H,

            /// <summary>
            /// FT232X device
            /// </summary>
            FT_DEVICE_X_SERIES
        }

        /// <summary>
        /// Error states not supported by FTD2XX DLL.
        /// </summary>
        private enum _error
        {
            /// <summary>
            /// The ft no error
            /// </summary>
            FTNoError = 0,

            /// <summary>
            /// The ft incorrect device
            /// </summary>
            FTIncorrectDevice,

            /// <summary>
            /// The ft invalid bit mode
            /// </summary>
            FTInvalidBitMode,

            /// <summary>
            /// The ft buffer size
            /// </summary>
            FTBufferSize
        }

        #endregion

        /// <summary>
        /// Получает или задает делегата функции FT_OpenEx.
        /// </summary>
        public TFT_OpenEx FT_OpenEx { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_Close.
        /// </summary>
        public TFT_Close FT_Close { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_Read.
        /// </summary>
        public TFT_Read FT_Read { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_Write.
        /// </summary>
        public TFT_Write FT_Write { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_GetQueueStatus.
        /// </summary>
        public TFT_GetQueueStatus FT_GetQueueStatus { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_GetStatus.
        /// </summary>
        public TFT_GetStatus FT_GetStatus { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_ResetDevice.
        /// </summary>
        public TFT_ResetDevice FT_ResetDevice { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_ResetPort.
        /// </summary>
        public TFT_ResetPort FT_ResetPort { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_CyclePort.
        /// </summary>
        public TFT_CyclePort FT_CyclePort { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_Rescan.
        /// </summary>
        public TFT_Rescan FT_Rescan { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_Reload.
        /// </summary>
        public TFT_Reload FT_Reload { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_Purge.
        /// </summary>
        public TFT_Purge FT_Purge { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_SetTimeouts.
        /// </summary>
        public TFT_SetTimeouts FT_SetTimeouts { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_GetDriverVersion.
        /// </summary>
        public TFT_GetDriverVersion FT_GetDriverVersion { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_GetLibraryVersion.
        /// </summary>
        public TFT_GetLibraryVersion FT_GetLibraryVersion { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_SetBitMode.
        /// </summary>
        public TFT_SetBitMode FT_SetBitMode { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_SetLatencyTimer.
        /// </summary>
        public TFT_SetLatencyTimer FT_SetLatencyTimer { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_GetLatencyTimer.
        /// </summary>
        public TFT_GetLatencyTimer FT_GetLatencyTimer { get; set; }

        /// <summary>
        /// Получает или задает делегата функции FT_SetUSBParameters.
        /// </summary>
        public TFT_SetUSBParameters FT_SetUSBParameters { get; set; }

        #region PROPERTY_DEFINITIONS
        // **************************************************************************
        // IsOpen
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Получает значение, показывающее, статус инициализации USB.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (_fthandle == IntPtr.Zero)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        #endregion

        #region METHOD_DEFINITIONS
        // **************************************************************************
        // OpenBySerialNumber
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Opens the FTDI device with the specified serial number.  
        /// </summary>
        /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
        /// <param name="serialnumber">Serial number of the device to open.</param>
        /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
        public FT_STATUS OpenBySerialNumber(string serialnumber)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return status;
            }

            // Call FT_OpenEx
            status = FT_OpenEx(serialnumber, FTOpenBySerialNumber, ref _fthandle);

            // Appears that the handle value can be non-NULL on a fail, so set it explicitly
            if (status != FT_STATUS.FT_OK)
            {
                _fthandle = IntPtr.Zero;
            }

            if (_fthandle != IntPtr.Zero)
            {
                FT_SetBitMode(_fthandle, 0, FT_BIT_MODES.FTBitModeSyncFIFO);         // для поддержки USB 2.0
            }

            return status;
        }

        // **************************************************************************
        // Close
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Closes the handle to an open FTDI device.  
        /// </summary>
        /// <returns>FT_STATUS value from FT_Close in FTD2XX.DLL</returns>
        public FT_STATUS Close()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            // Call FT_Close
            statusFT = FT_Close(_fthandle);

            if (statusFT == FT_STATUS.FT_OK)
            {
                _fthandle = IntPtr.Zero;
            }

            return statusFT;
        }

        // **************************************************************************
        // Read
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Read data from an open FTDI device.
        /// </summary>
        /// <returns>FT_STATUS value from FT_Read in FTD2XX.DLL</returns>
        /// <param name="dataBuffer">An array of bytes which will be populated with the data read from the device.</param>
        /// <param name="numBytesToRead">The number of bytes requested from the device.</param>
        /// <param name="numBytesRead">The number of bytes actually read.</param>
        public FT_STATUS Read(byte[] dataBuffer, int numBytesToRead, ref int numBytesRead)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            // If the buffer is not big enough to receive the amount of data requested, adjust the number of bytes to read
            if (dataBuffer.Length < numBytesToRead)
            {
                numBytesToRead = dataBuffer.Length;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_Read
                statusFT = FT_Read(_fthandle, dataBuffer, numBytesToRead, ref numBytesRead);
            }

            return statusFT;
        }

        // **************************************************************************
        // Write
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Write data to an open FTDI device.
        /// </summary>
        /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
        /// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
        /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
        /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
        public FT_STATUS Write(byte[] dataBuffer, int numBytesToWrite, ref uint numBytesWritten)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_Write
                statusFT = FT_Write(_fthandle, dataBuffer, (uint)numBytesToWrite, ref numBytesWritten);
            }

            return statusFT;
        }

        // **************************************************************************
        // ResetDevice
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Reset an open FTDI device.
        /// </summary>
        /// <returns>FT_STATUS value from FT_ResetDevice in FTD2XX.DLL</returns>
        public FT_STATUS ResetDevice()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_ResetDevice
                statusFT = FT_ResetDevice(_fthandle);
            }

            return statusFT;
        }

        // **************************************************************************
        // Purge
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Purge data from the devices transmit and/or receive buffers.
        /// </summary>
        /// <returns>FT_STATUS value from FT_Purge in FTD2XX.DLL</returns>
        /// <param name="purgemask">Specifies which buffer(s) to be purged.  Valid values are any combination of the following flags: FT_PURGE_RX, FT_PURGE_TX</param>
        public FT_STATUS Purge(uint purgemask)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_Purge
                statusFT = FT_Purge(_fthandle, purgemask);
            }

            return statusFT;
        }

        // **************************************************************************
        // ResetPort
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Resets the device port.
        /// </summary>
        /// <returns>FT_STATUS value from FT_ResetPort in FTD2XX.DLL</returns>
        public FT_STATUS ResetPort()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_ResetPort
                statusFT = FT_ResetPort(_fthandle);
            }

            return statusFT;
        }

        // **************************************************************************
        // CyclePort
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Causes the device to be re-enumerated on the USB bus.  This is equivalent to unplugging and replugging the device.
        /// Also calls FT_Close if FT_CyclePort is successful, so no need to call this separately in the application.
        /// </summary>
        /// <returns>FT_STATUS value from FT_CyclePort in FTD2XX.DLL</returns>
        public FT_STATUS CyclePort()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_CyclePort
                statusFT = FT_CyclePort(_fthandle);
                if (statusFT == FT_STATUS.FT_OK)
                {
                    // If successful, call FT_Close
                    statusFT = FT_Close(_fthandle);
                    if (statusFT == FT_STATUS.FT_OK)
                    {
                        _fthandle = IntPtr.Zero;
                    }
                }
            }

            return statusFT;
        }

        // **************************************************************************
        // Rescan
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Causes the system to check for USB hardware changes.  This is equivalent to clicking on the "Scan for hardware changes" button in the Device Manager.
        /// </summary>
        /// <returns>FT_STATUS value from FT_Rescan in FTD2XX.DLL</returns>
        public FT_STATUS Rescan()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            // Call FT_Rescan
            statusFT = FT_Rescan();
            return statusFT;
        }

        // **************************************************************************
        // Reload
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Forces a reload of the driver for devices with a specific VID and PID combination.
        /// </summary>
        /// <returns>FT_STATUS value from FT_Reload in FTD2XX.DLL</returns>
        /// <remarks>If the VID and PID parameters are 0, the drivers for USB root hubs will be reloaded, causing all USB devices connected to reload their drivers</remarks>
        /// <param name="vendorID">Vendor ID of the devices to have the driver reloaded</param>
        /// <param name="productID">Product ID of the devices to have the driver reloaded</param>
        public FT_STATUS Reload(ushort vendorID, ushort productID)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            // Call FT_Reload
            statusFT = FT_Reload(vendorID, productID);
            return statusFT;
        }

        // **************************************************************************
        // SetBitMode
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Puts the device in a mode other than the default UART or FIFO mode.
        /// </summary>
        /// <returns>FT_STATUS value from FT_SetBitMode in FTD2XX.DLL</returns>
        /// <param name="mask">Sets up which bits are inputs and which are outputs.  A bit value of 0 sets the corresponding pin to an input, a bit value of 1 sets the corresponding pin to an output.
        /// In the case of CBUS Bit Bang, the upper nibble of this value controls which pins are inputs and outputs, while the lower nibble controls which of the outputs are high and low.</param>
        /// <param name="bitMode"> For FT232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_CBUS_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL, FT_BIT_MODE_SYNC_FIFO.
        /// For FT2232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL, FT_BIT_MODE_SYNC_FIFO.
        /// For FT4232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG.
        /// For FT232R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_CBUS_BITBANG.
        /// For FT245R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG.
        /// For FT2232 devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL.
        /// For FT232B and FT245B devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG.</param>
        /// <exception cref="FT_EXCEPTION">Thrown when the current device does not support the requested bit mode.</exception>
        public FT_STATUS SetBitMode(byte mask, byte bitMode)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // !FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

                if (_fthandle != IntPtr.Zero)
                {
                    /*!
                    FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                    // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
                    GetDeviceType(ref DeviceType);
                    if (DeviceType == FT_DEVICE.FT_DEVICE_AM)
                    {
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                    else if (DeviceType == FT_DEVICE.FT_DEVICE_100AX)
                    {
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                    else if ((DeviceType == FT_DEVICE.FT_DEVICE_BM) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                    {
                        if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG)) == 0)
                        {
                            // Throw an exception
                            ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                            ErrorHandler(ftStatus, ftErrorCondition);
                        }
                    }
                    else if ((DeviceType == FT_DEVICE.FT_DEVICE_2232) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                    {
                        if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL)) == 0)
                        {
                            // Throw an exception
                            ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                            ErrorHandler(ftStatus, ftErrorCondition);
                        }
                        if ((BitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & (InterfaceIdentifier != "A"))
                        {
                            // MPSSE mode is only available on channel A
                            // Throw an exception
                            ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                            ErrorHandler(ftStatus, ftErrorCondition);
                        }
                    }
                    else if ((DeviceType == FT_DEVICE.FT_DEVICE_232R) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                    {
                        if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_CBUS_BITBANG)) == 0)
                        {
                            // Throw an exception
                            ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                            ErrorHandler(ftStatus, ftErrorCondition);
                        }
                    }
                    else if ((DeviceType == FT_DEVICE.FT_DEVICE_2232H) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                    {
                        if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL | FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) == 0)
                        {
                            // Throw an exception
                            ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                            ErrorHandler(ftStatus, ftErrorCondition);
                        }
                        if (((BitMode == FT_BIT_MODES.FT_BIT_MODE_MCU_HOST) | (BitMode == FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) & (InterfaceIdentifier != "A"))
                        {
                            // MCU Host Emulation and Single channel synchronous 245 FIFO mode is only available on channel A
                            // Throw an exception
                            ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                            ErrorHandler(ftStatus, ftErrorCondition);
                        }
                    }
                    else if ((DeviceType == FT_DEVICE.FT_DEVICE_4232H) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                    {
                        if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG)) == 0)
                        {
                            // Throw an exception
                            ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                            ErrorHandler(ftStatus, ftErrorCondition);
                        }
                        if ((BitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & ((InterfaceIdentifier != "A") & (InterfaceIdentifier != "B")))
                        {
                            // MPSSE mode is only available on channel A and B
                            // Throw an exception
                            ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                            ErrorHandler(ftStatus, ftErrorCondition);
                        }
                    }
                    else if ((DeviceType == FT_DEVICE.FT_DEVICE_232H) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                    {
                        // FT232H supports all current bit modes!
                        if (BitMode > FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)
                        {
                            // Throw an exception
                            ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                            ErrorHandler(ftStatus, ftErrorCondition);
                        }
                    }
                    */

                    // Requested bit mode is supported
                    // Note FT_BIT_MODES.FT_BIT_MODE_RESET falls through to here - no bits set so cannot check for AND
                    // Call FT_SetBitMode
                    statusFT = FT_SetBitMode(_fthandle, mask, bitMode);
                }

            return statusFT;
        }

        // **************************************************************************
        // GetRxBytesAvailable
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Gets the number of bytes available in the receive buffer.
        /// </summary>
        /// <returns>FT_STATUS value from FT_GetQueueStatus in FTD2XX.DLL</returns>
        /// <param name="queueRX">The number of bytes available to be read.</param>
        public FT_STATUS GetRxBytesAvailable(ref int queueRX)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return status;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_GetQueueStatus
                status = FT_GetQueueStatus(_fthandle, ref queueRX);
            }

            return status;
        }

        // **************************************************************************
        // GetTxBytesWaiting
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Gets the number of bytes waiting in the transmit buffer.
        /// </summary>
        /// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
        /// <param name="queueTX">The number of bytes waiting to be sent.</param>
        public FT_STATUS GetTxBytesWaiting(ref uint queueTX)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            uint queueRX = 0;
            uint eventStatus = 0;

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_GetStatus
                statusFT = FT_GetStatus(_fthandle, ref queueRX, ref queueTX, ref eventStatus);
            }

            return statusFT;
        }

        // **************************************************************************
        // SetTimeouts
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Sets the read and write timeout values.
        /// </summary>
        /// <returns>FT_STATUS value from FT_SetTimeouts in FTD2XX.DLL</returns>
        /// <param name="readTimeout">Read timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
        /// <param name="writeTimeout">Write timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
        public FT_STATUS SetTimeouts(uint readTimeout, uint writeTimeout)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_SetTimeouts
                statusFT = FT_SetTimeouts(_fthandle, readTimeout, writeTimeout);
            }

            return statusFT;
        }

        // **************************************************************************
        // GetDriverVersion
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Gets the current FTDIBUS.SYS driver version number.
        /// </summary>
        /// <returns>FT_STATUS value from FT_GetDriverVersion in FTD2XX.DLL</returns>
        /// <param name="driverVersion">The current driver version number.</param>
        public FT_STATUS GetDriverVersion(ref uint driverVersion)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_GetDriverVersion
                statusFT = FT_GetDriverVersion(_fthandle, ref driverVersion);
            }

            return statusFT;
        }

        // **************************************************************************
        // GetLibraryVersion
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Gets the current FTD2XX.DLL driver version number.
        /// </summary>
        /// <returns>FT_STATUS value from FT_GetLibraryVersion in FTD2XX.DLL</returns>
        /// <param name="libraryVersion">The current library version.</param>
        public FT_STATUS GetLibraryVersion(ref uint libraryVersion)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS statusFT = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return statusFT;
            }

            // Call FT_GetLibraryVersion
            statusFT = FT_GetLibraryVersion(ref libraryVersion);
            return statusFT;
        }

        // **************************************************************************
        // SetDeadmanTimeout
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Sets the USB deadman timeout value.  Default is 5000ms.
        /// </summary>
        /// <returns>FT_STATUS value from FT_SetDeadmanTimeout in FTD2XX.DLL</returns>
        /// <param name="deadmanTimeout">The deadman timeout value in ms.  Default is 5000ms.</param>
        public FT_STATUS SetDeadmanTimeout(uint deadmanTimeout)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            { 
                return status; 
            }

            // Check for our required function pointers being set up
            if (entryFTSetDeadmanTimeout != IntPtr.Zero)
            {
                TFT_SetDeadmanTimeout setDeadmanTimeout = (TFT_SetDeadmanTimeout)Marshal.GetDelegateForFunctionPointer(entryFTSetDeadmanTimeout, typeof(TFT_SetDeadmanTimeout));

                if (_fthandle != IntPtr.Zero)
                {
                    // Call FT_SetDeadmanTimeout
                    status = setDeadmanTimeout(_fthandle, deadmanTimeout);
                }
            }
            else
            {
                if (entryFTSetDeadmanTimeout == IntPtr.Zero)
                {
                    // !MessageBox.Show("Failed to load function FT_SetDeadmanTimeout.");
                }
            }

            return status;
        }

        // **************************************************************************
        // SetLatency
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Sets the value of the latency timer.  Default value is 16ms.
        /// </summary>
        /// <returns>FT_STATUS value from FT_SetLatencyTimer in FTD2XX.DLL</returns>
        /// <param name="latency">The latency timer value in ms.
        /// Valid values are 2ms - 255ms for FT232BM, FT245BM and FT2232 devices.
        /// Valid values are 0ms - 255ms for other devices.</param>
        public FT_STATUS SetLatency(byte latency)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return status;
            }

            if (_fthandle != IntPtr.Zero)
            {
                   /*!
                    FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                    // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
                    GetDeviceType(ref DeviceType);
                    if ((DeviceType == FT_DEVICE.FT_DEVICE_BM) || (DeviceType == FT_DEVICE.FT_DEVICE_2232))
                    {
                        // Do not allow latency of 1ms or 0ms for older devices
                        // since this can cause problems/lock up due to buffering mechanism
                        if (Latency < 2)
                            Latency = 2;
                    }
                    */

                    // Call FT_SetLatencyTimer
                status = FT_SetLatencyTimer(_fthandle, latency);
            }

            return status;
        }

        // **************************************************************************
        // GetLatency
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Gets the value of the latency timer.  Default value is 16ms.
        /// </summary>
        /// <returns>FT_STATUS value from FT_GetLatencyTimer in FTD2XX.DLL</returns>
        /// <param name="latency">The latency timer value in ms.</param>
        public FT_STATUS GetLatency(ref byte latency)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return status;
            }

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_GetLatencyTimer
                status = FT_GetLatencyTimer(_fthandle, ref latency);
            }

            return status;
        }

        // **************************************************************************
        // SetUSBTransferSizes
        // **************************************************************************
        // Intellisense comments

        /// <summary>
        /// Sets the USB IN and OUT transfer sizes.
        /// </summary>
        /// <returns>FT_STATUS value from FT_SetUSBParameters in FTD2XX.DLL</returns>
        /// <param name="inTransferSize">The USB IN transfer size in bytes.</param>
        public FT_STATUS InTransferSize(uint inTransferSize)

        // Only support IN transfer sizes at the moment

        // public UInt32 InTransferSize(UInt32 InTransferSize, UInt32 OutTransferSize)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (handleFTD2XXDLL == IntPtr.Zero)
            {
                return status;
            }

            uint outTransferSize = inTransferSize;

            if (_fthandle != IntPtr.Zero)
            {
                // Call FT_SetUSBParameters
                status = FT_SetUSBParameters(_fthandle, inTransferSize, outTransferSize);
            }

            return status;
        }

        #endregion

        #region LOAD_LIBRARIES
        /// Built-in Windows API functions to allow us to dynamically load our own DLL.
        /// Will allow us to use old versions of the DLL that do not have all of these functions available.
        /// <summary>
        /// Загружает динамическую библиотеку средствами WinAPI.
        /// </summary>
        /// <param name="dllToLoad">Имя библиотеки</param>
        /// <returns>Состояние операции</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        /// <summary>
        /// Возвращает точку входа в заданную процедуру.
        /// </summary>
        /// <param name="handleModule">Handle загруженной dll-ки</param>
        /// <param name="procedureName">Название операции</param>
        /// <returns>Состояние операции</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr handleModule, string procedureName);

        /// <summary>
        /// WinAPI выгрузить dll-ку.
        /// </summary>
        /// <param name="handleModule">Handle загруженной dll-ки</param>
        /// <returns>Состояние операции</returns>
        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr handleModule);

        #endregion

        #region HELPER_METHODS
        // **************************************************************************
        // ErrorHandler
        // **************************************************************************

        /// <summary>
        /// Method to check ftStatus and ftErrorCondition values for error conditions and throw exceptions accordingly.
        /// </summary>
        /// <param name="status">Current status</param>
        /// <param name="errorCondition">Error contaiment</param>
        private void ErrorHandler(FT_STATUS status, _error errorCondition)
        {
            if (status != FT_STATUS.FT_OK)
            {
                // Check FT_STATUS values returned from FTD2XX DLL calls
                switch (status)
                {
                    case FT_STATUS.FT_DEVICE_NOT_FOUND:
                        {
                            throw new FT_EXCEPTION("FTDI device not found.");
                        }

                    case FT_STATUS.FT_DEVICE_NOT_OPENED:
                        {
                            throw new FT_EXCEPTION("FTDI device not opened.");
                        }

                    case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_ERASE:
                        {
                            throw new FT_EXCEPTION("FTDI device not opened for erase.");
                        }

                    case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_WRITE:
                        {
                            throw new FT_EXCEPTION("FTDI device not opened for write.");
                        }

                    case FT_STATUS.FT_EEPROM_ERASE_FAILED:
                        {
                            throw new FT_EXCEPTION("Failed to erase FTDI device EEPROM.");
                        }

                    case FT_STATUS.FT_EEPROM_NOT_PRESENT:
                        {
                            throw new FT_EXCEPTION("No EEPROM fitted to FTDI device.");
                        }

                    case FT_STATUS.FT_EEPROM_NOT_PROGRAMMED:
                        {
                            throw new FT_EXCEPTION("FTDI device EEPROM not programmed.");
                        }

                    case FT_STATUS.FT_EEPROM_READ_FAILED:
                        {
                            throw new FT_EXCEPTION("Failed to read FTDI device EEPROM.");
                        }

                    case FT_STATUS.FT_EEPROM_WRITE_FAILED:
                        {
                            throw new FT_EXCEPTION("Failed to write FTDI device EEPROM.");
                        }

                    case FT_STATUS.FT_FAILED_TO_WRITE_DEVICE:
                        {
                            throw new FT_EXCEPTION("Failed to write to FTDI device.");
                        }

                    case FT_STATUS.FT_INSUFFICIENT_RESOURCES:
                        {
                            throw new FT_EXCEPTION("Insufficient resources.");
                        }

                    case FT_STATUS.FT_INVALID_ARGS:
                        {
                            throw new FT_EXCEPTION("Invalid arguments for FTD2XX function call.");
                        }

                    case FT_STATUS.FT_INVALID_BAUD_RATE:
                        {
                            throw new FT_EXCEPTION("Invalid Baud rate for FTDI device.");
                        }

                    case FT_STATUS.FT_INVALID_HANDLE:
                        {
                            throw new FT_EXCEPTION("Invalid handle for FTDI device.");
                        }

                    case FT_STATUS.FT_INVALID_PARAMETER:
                        {
                            throw new FT_EXCEPTION("Invalid parameter for FTD2XX function call.");
                        }

                    case FT_STATUS.FT_IO_ERROR:
                        {
                            throw new FT_EXCEPTION("FTDI device IO error.");
                        }

                    case FT_STATUS.FT_OTHER_ERROR:
                        {
                            throw new FT_EXCEPTION("An unexpected error has occurred when trying to communicate with the FTDI device.");
                        }

                    default:
                        break;
                }
            }

            if (errorCondition != _error.FTNoError)
            {
                // Check for other error conditions not handled by FTD2XX DLL
                switch (errorCondition)
                {
                    case _error.FTIncorrectDevice:
                        {
                            throw new FT_EXCEPTION("The current device type does not match the EEPROM structure.");
                        }

                    case _error.FTInvalidBitMode:
                        {
                            throw new FT_EXCEPTION("The requested bit mode is not valid for the current device.");
                        }

                    case _error.FTBufferSize:
                        {
                            throw new FT_EXCEPTION("The supplied buffer is not big enough.");
                        }

                    default:
                        break;
                }
            }

            return;
        }
        #endregion
        #region EXCEPTION_HANDLING
        /// <summary>
        /// Exceptions thrown by errors within the FTDI class.
        /// </summary>
        [global::System.Serializable]
        public class FT_EXCEPTION : Exception
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="FT_EXCEPTION" />.
            /// </summary>
            public FT_EXCEPTION()
            {
            }

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="FT_EXCEPTION" />.
            /// </summary>
            /// <param name="message">Сообщение об ошибке</param>
            public FT_EXCEPTION(string message)
                : base(message)
            {
            }

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="FT_EXCEPTION" />.
            /// </summary>
            /// <param name="message">Сообщение об ошибке</param>
            /// <param name="inner">Исходный контекст</param>
            public FT_EXCEPTION(string message, Exception inner)
                : base(message, inner)
            {
            }

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="FT_EXCEPTION" />.
            /// </summary>
            /// <param name="info">Сообщение об ошибке</param>
            /// <param name="context">Текущее состояние USB</param>
            protected FT_EXCEPTION(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            {
            }
        }
        #endregion

        #region TYPEDEFS
        /// <summary>
        /// Type that holds device information for GetDeviceInformation method.
        /// Used with FT_GetDeviceInfo and FT_GetDeviceInfoDetail in FTD2XX.DLL
        /// </summary>
        public class FT_DEVICE_INFO_NODE
        {
            /// <summary>
            /// Получает или задает статус устройства.
            /// Indicates device state.  Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HISPEED
            /// </summary>
            public uint Flags { get; set; }

            /// <summary>
            /// Получает или задает тип устройства.
            /// Indicates the device type.  Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM, FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN
            /// </summary>
            public FT_DEVICE Type { get; set; }

            /// <summary>
            /// Получает или задает Vendor ID устройства.
            /// The Vendor ID and Product ID of the device
            /// </summary>
            public uint ID { get; set; }

            /// <summary>
            /// Получает или задает физический идентификатор устройства.
            /// The physical location identifier of the device
            /// </summary>
            public uint LocId { get; set; }

            /// <summary>
            /// Получает или задает уникальный номер устройства.
            /// The device serial number
            /// </summary>
            public string SerialNumber { get; set; }

            /// <summary>
            /// Получает или задает описание устройства.
            /// The device description
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Получает или задает уникальный идентификатор устройства. 
            /// This value is not used externally and is provided for information only.
            /// If the device is not open, this value is 0.
            /// </summary>
            public IntPtr FTHandle { get; set; }
        }
        #endregion

        // Flow Control

        /// <summary>
        /// Permitted flow control values for FTDI devices
        /// </summary>
        public class FT_FLOW_CONTROL
        {
            /// <summary>
            /// No flow control
            /// </summary>
            public const ushort FTFlowNone = 0x0000;

            /// <summary>
            /// RTS/CTS flow control
            /// </summary>
            public const ushort FTFlowRTSCTS = 0x0100;

            /// <summary>
            /// DTR/DSR flow control
            /// </summary>
            public const ushort FTFlowDTRDSR = 0x0200;

            /// <summary>
            /// Xon/Xoff flow control
            /// </summary>
            public const ushort FTFlowXOnXOff = 0x0400;
        }

        // Purge Rx and Tx buffers

        /// <summary>
        /// Purge buffer constant definitions
        /// </summary>
        public class FT_PURGE
        {
            /// <summary>
            /// Purge Rx buffer
            /// </summary>
            public const byte FTPurgeRX = 0x01;

            /// <summary>
            /// Purge Tx buffer
            /// </summary>
            public const byte FTPurgeTX = 0x02;
        }

        // Bit modes

        /// <summary>
        /// Permitted bit mode values for FTDI devices.  For use with SetBitMode
        /// </summary>
        public class FT_BIT_MODES
        {
            /// <summary>
            /// Reset bit mode
            /// </summary>
            public const byte FTBitModeReset = 0x00;

            /// <summary>
            /// Asynchronous bit-bang mode
            /// </summary>
            public const byte FTBitModeAsyncBitbang = 0x01;

            /// <summary>
            /// MPSSE bit mode - only available on FT2232, FT2232H, FT4232H and FT232H
            /// </summary>
            public const byte FTBitModeMPSSE = 0x02;

            /// <summary>
            /// Synchronous bit-bang mode
            /// </summary>
            public const byte FTBitModeSyncBitbang = 0x04;

            /// <summary>
            /// MCU host bus emulation mode - only available on FT2232, FT2232H, FT4232H and FT232H
            /// </summary>
            public const byte FTBitModeMCUHost = 0x08;

            /// <summary>
            /// Fast opto-isolated serial mode - only available on FT2232, FT2232H, FT4232H and FT232H
            /// </summary>
            public const byte FTBitModeFastSerial = 0x10;

            /// <summary>
            /// CBUS bit-bang mode - only available on FT232R and FT232H
            /// </summary>
            public const byte FTBitModeCBUSBitbang = 0x20;

            /// <summary>
            /// Single channel synchronous 245 FIFO mode - only available on FT2232H channel A and FT232H
            /// </summary>
            public const byte FTBitModeSyncFIFO = 0x40;
        }

        // Flag values for FT_GetDeviceInfoDetail and FT_GetDeviceInfo

        /// <summary>
        /// Flags that provide information on the FTDI device state
        /// </summary>
        public class FT_FLAGS
        {
            /// <summary>
            /// Indicates that the device is open
            /// </summary>
            public const uint FTFlagsOpened = 0x00000001;

            /// <summary>
            /// Indicates that the device is enumerated as a hi-speed USB device
            /// </summary>
            public const uint FTFlagsHiSpeed = 0x00000002;
        }

        // Valid drive current values for FT2232H, FT4232H and FT232H devices

        /// <summary>
        /// Valid values for drive current options on FT2232H, FT4232H and FT232H devices.
        /// </summary>
        public class FT_DRIVE_CURRENT
        {
            /// <summary>
            /// 4mA drive current
            /// </summary>
            public const byte FTDriveCurrent4MA = 4;

            /// <summary>
            /// 8mA drive current
            /// </summary>
            public const byte FTDriveCurrent8MA = 8;

            /// <summary>
            /// 12mA drive current
            /// </summary>
            public const byte FTDriveCurrent12MA = 12;

            /// <summary>
            /// 16mA drive current
            /// </summary>
            public const byte FTDriveCurrent16MA = 16;
        }
    }
}
