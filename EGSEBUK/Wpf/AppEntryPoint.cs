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
            EmbeddedAssembly.Load("Wpf.DetectNetVersion.dll", "DetectNetVersion.dll");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            Egse.Wpf.App app = new Egse.Wpf.App();
            app.InitializeComponent();
            app.Run();
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
