//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Wpf
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using DetectNetVersion.DotNetFramework;
    using Egse.Defaults;
    using Egse.Utilites;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Идентификатор сообщения для общей рассылки.
        /// </summary>
        private const int HwndBroadCast = 0xffff;

        /// <summary>
        /// Идентификатор зарегистрированного в системе сообщения.
        /// </summary>
        private static uint message;

        /// <summary>
        /// объект Mutex, для обнаружения запущеного экземпляра ПО.
        /// </summary>
        private Mutex mutex;

        /// <summary>
        /// Handles the messages.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="paramW">The parameter w.</param>
        /// <param name="paramL">The parameter l.</param>
        /// <param name="handled">if set to <c>true</c> [handled].</param>
        /// <returns>Идентификатор сообщения.</returns>
        public static IntPtr HandleMessages(IntPtr handle, int msg, IntPtr paramW, IntPtr paramL, ref bool handled)
        {
            if (msg == message)
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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string mutexName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Local\\{{{0}}}{{{1}}}", assembly.GetType().GUID, assembly.GetName().Name);

            this.mutex = new Mutex(false, mutexName);
            
            message = SafeNativeMethods.RegisterWindowMessage(mutexName);
            
            if (!this.mutex.WaitOne(TimeSpan.Zero, false))
            {
                SafeNativeMethods.PostMessage(HwndBroadCast, message, IntPtr.Zero, IntPtr.Zero);
                Current.Shutdown();
                return;
            }

            string ownVer, needVer;
            //// выходим из приложения если не хватает версии DotNet Framework (необходима .Net Framework 4.5)
            if (IsCheckDotNetVersion(out needVer, out ownVer))
            {
                MessageBox.Show(string.Format(Resource.Get(@"eNetVersion"), needVer, ownVer));
                Current.Shutdown();
                return; 
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Exit" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            Dispose();
            base.OnExit(e);
        }

        /// <summary>
        /// Determines whether [is check dot net version] [the specified need version].
        /// </summary>
        /// <param name="needVersion">The need version.</param>
        /// <param name="ownerVersion">The owner version.</param>
        /// <returns></returns>
        private bool IsCheckDotNetVersion(out string needVersion, out string ownerVersion)
        {
            NetFrameworkInfo frameworkInfo = new NetFrameworkInfo();
            ownerVersion = frameworkInfo.HighestFrameworkVersion;
            needVersion = string.Format("{0}.{1}.{2}", Environment.Version.Major, Environment.Version.Minor, Environment.Version.Build);
            return 0 > ownerVersion.CompareTo(needVersion);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (disposing && (this.mutex != null))
            {
                this.mutex.Close();
            }
        }

        /// <summary>
        /// Предназначен для методов, которые являются безопасными для всех, кто их вызывает.
        /// </summary>
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
