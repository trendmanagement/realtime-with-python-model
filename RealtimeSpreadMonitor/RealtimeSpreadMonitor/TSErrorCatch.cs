using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using log4net;

namespace RealtimeSpreadMonitor
{
    public class TSErrorCatch
    {
        //public TSErrorCatch()
        //{
        //    XmlConfigurator.Configure();
        //}

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void errorCatchOut(Exception ex)
        {
#if DEBUG
            try
#endif
            {
                Trace.WriteLine(ex);
                //Debug.WriteLine(ex);
            }
#if DEBUG
            catch (Exception ex2)
            {
                Trace.WriteLine("TSErrorCatch " + ex2);
            }
#endif
        }

        public static void errorCatchOut(String errorStringOut, Exception ex)
        {
#if DEBUG
            try
#endif
	        {
                Trace.WriteLine(errorStringOut + " " + ex);
		        //Debug.WriteLine(errorStringOut + " " + ex);
	        }
#if DEBUG
            catch (Exception ex2)
            {
                Trace.WriteLine("TSErrorCatch " + ex2);
            }
#endif
        }

        public static void debugWriteOut(String stringOut)
        {
#if DEBUG
            try
#endif
            {
                Trace.WriteLine(stringOut);
                //Debug.WriteLine(stringOut);
                //log.Info("Info message");
                log.Debug(stringOut);
            }
#if DEBUG
            catch (Exception ex2)
            {
                Trace.WriteLine("TSErrorCatch " + ex2);
            }
#endif
        }

        public static void ErrorCatchSetup()
        {
#if DEBUG
            try
#endif
            {
               

                //XmlConfigurator.Configure();

                //ILoggerFactory loggerFactory = new LoggerFactory()
                //    .AddConsole()
                //    .AddDebug();
                //                ILogger logger = loggerFactory.CreateLogger<Program>();
                //                logger.LogInformation(
                //                  "This is a test of the emergency broadcast system.");

                //LoggingConfiguration loggingConfiguration = BuildProgrammaticConfig();
                //LogWriter defaultWriter = new LogWriter(loggingConfiguration);

                //TextWriterTraceListener tr1 = new TextWriterTraceListener(System.Console.Out);
                //Trace.Listeners.Add(tr1);
                //String dir = Directory.GetCurrentDirectory() + "\\DEBUG_OUTPUT\\";
                //if (!Directory.Exists(dir))
                //{
                //    Directory.CreateDirectory(dir);
                //}
                //String dateTimeFileMarker = dir + "Output_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", DateTimeFormatInfo.InvariantInfo) + ".txt";
                //Trace.WriteLine(dateTimeFileMarker);
                //TextWriterTraceListener tr2 = new TextWriterTraceListener(System.IO.File.CreateText(dateTimeFileMarker));
                //Trace.Listeners.Add(tr2);

                //Trace.AutoFlush = true;
            }
#if DEBUG
            catch (Exception ex2)
            {
                Trace.WriteLine("TSErrorCatch " + ex2);
            }
#endif
        }

        public static void DebugPrintTrace(int lineNumber, String message)
        {
//             StackTrace st = new StackTrace(true);
//             StackFrame sf = st.GetFrame(1);
//             Trace.WriteLine("Trace "
//                 + sf.GetMethod().Name + " "
//                 //+ sf.GetFileName() + ":"
//                 + sf.GetFileLineNumber());
            Trace.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", DateTimeFormatInfo.InvariantInfo)
                + "  " + lineNumber + "  " + message);
        }
    }
}
