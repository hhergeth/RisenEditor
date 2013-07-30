using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using GameLibrary;
using System.Reflection;
using System.Resources;

namespace RisenEditor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                D3DApplication App = new App();
                App.CreateDevice();
                App.Run();
            }
            catch (Exception e)
            {
                string s = e.Message + " Stack trace : " + e.StackTrace;
                SystemLog.Append(LogImportance.Direct, s);
                SystemLog.Flush();
                MessageBox.Show("A critical error has occured, an error message can be found in the log.");
            }
        }
    }
}
