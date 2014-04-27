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

    class AppEntryPoint
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

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
