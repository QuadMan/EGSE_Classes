//-----------------------------------------------------------------------
// <copyright file="AppEntryPoint.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Wpf
{
    using System;
    using Egse.Utilites;

    /// <summary>
    /// Реализует "точку входа" для приложения.
    /// </summary>
    public class AppEntryPoint
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            try
            {
                EmbeddedAssembly.Load("Wpf.DetectNetVersion.dll", "DetectNetVersion.dll");

                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                Egse.Wpf.App app = new Egse.Wpf.App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(string.Format(Resource.Get(@"eGlobal"), e.Message, e.TargetSite));
            }
        }

        /// <summary>
        /// Handles the AssemblyResolve event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ResolveEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
