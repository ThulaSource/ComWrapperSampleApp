using COMWrapperSampleApp.Common;
using System;
using System.Linq;
using System.Windows.Forms;

namespace COMWrapperSampleApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Contains("/install", StringComparer.CurrentCultureIgnoreCase))
            {
                EpjApiComClass.Install(typeof(Program).Module);
                Console.WriteLine("EPJ API test COM Server installed successfully.");
                return;
            }

            EpjApiComClass.Register();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ComWrapper());
        }
    }
}
