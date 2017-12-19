using RealtimeSpreadMonitor.Forms;
using RealtimeSpreadMonitor.Model;
using System;
using System.Windows.Forms;
using log4net.Config;

namespace RealtimeSpreadMonitor
{
    static class RealtimeStartup
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            XmlConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //InitializationParms initializationParms = new InitializationParms();
            //initializationParms.tmlSystemRunType = startupType;

            //array<bool> createPersistence = new array<bool>(3);
            InitializationForm initializationForm = new InitializationForm();

            //System.Windows.Forms.Application.Run(initializationForm);
            initializationForm.ShowDialog();

            if (DataCollectionLibrary.initializationParms.runLiveSystem)
            {
                Application.Run(new OptionRealtimeStartup());
            }
        }
    }
}
