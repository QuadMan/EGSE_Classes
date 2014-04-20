namespace Egse.Wpf
{
    using Egse.Utilites;
    using System;

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
