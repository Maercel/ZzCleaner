using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZzCleaner
{
    internal static class Program 
    {
        static EventWaitHandle s_event;
        private static bool mainProgramRunning; 

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            s_event = new EventWaitHandle(false, EventResetMode.ManualReset, "Form", out mainProgramRunning);
            if (mainProgramRunning)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}
