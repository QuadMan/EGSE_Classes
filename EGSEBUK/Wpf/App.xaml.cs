//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.WPF
{
    using System.Windows;
    using System.Windows.Input;
    using Egse.Defaults;
    using Egse.Utilites;
    using System;
    using System.Threading;
    using System.Reflection;
    using System.Windows.Interop;
    using System.Security;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int HwndBroadCast = 0xffff;
        private Mutex mutex;

        private static uint Message;

        private void Dispose(Boolean disposing)
        {
            if (disposing && (this.mutex != null))
            {
                this.mutex.ReleaseMutex();
                this.mutex.Close();
                this.mutex = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static IntPtr HandleMessages(IntPtr handle, int message, IntPtr paramW, IntPtr paramL, ref bool handled)
        {
            if (message == Message)
            {
                if (WindowState.Minimized == Application.Current.MainWindow.WindowState)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                }
                bool topmost = Application.Current.MainWindow.Topmost;
                Application.Current.MainWindow.Topmost = true;
                Application.Current.MainWindow.Topmost = topmost;
            }

            return IntPtr.Zero;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string mutexName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Local\\{{{0}}}{{{1}}}", assembly.GetType().GUID, assembly.GetName().Name);

            this.mutex = new Mutex(false, mutexName);
            
            Message = SafeNativeMethods.RegisterWindowMessage(mutexName);
            
            if (!this.mutex.WaitOne(TimeSpan.Zero, false))
            {
                this.mutex = null;

                SafeNativeMethods.PostMessage(HwndBroadCast, Message, IntPtr.Zero, IntPtr.Zero);

                Current.Shutdown();

                return;
            }         
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Dispose();
            base.OnExit(e);
        }
 
        /// <summary>
        /// Mouses the logger event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        public void MouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = EventClickToString.ElementClicked(e);
            if (logEvent != null)
            {
                LogsClass.LogOperator.LogText = logEvent;
            }
        }

        [SuppressUnmanagedCodeSecurityAttribute]
        internal static class SafeNativeMethods
        {            
            [DllImport("user32")]
            public static extern bool PostMessage(int hwnd, uint msg, IntPtr wparam, IntPtr lparam);
            [DllImport("user32")]
            public static extern uint RegisterWindowMessage(string message);

        }
    }
}
