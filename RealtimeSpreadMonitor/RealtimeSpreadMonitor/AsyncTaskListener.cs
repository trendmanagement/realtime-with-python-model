using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeSpreadMonitor
{
    public enum STATUS_TYPE
    {
        NO_STATUS,
        CQG_CONNECTION_STATUS,
        DATA_SUBSCRIPTION_STATUS,
        DATA_STATUS

    }

    public enum STATUS_FORMAT
    {
        DEFAULT,
        ALARM,
        CAUTION
    }

    /// <summary>
    /// This helper class carries three roles for asynchronous tasks:
    /// 1. The reporter of messages.
    /// 2. The reporter of progress.
    /// </summary>
    /// 
    static class AsyncTaskListener
    {
        public delegate void UpdateLogDelegate(string msg = null);
        public static event UpdateLogDelegate UpdatedLog;

        public delegate void UpdateStatusDelegate(string msg = null,
            STATUS_FORMAT statusFormat = STATUS_FORMAT.DEFAULT,
            STATUS_TYPE connStatus = STATUS_TYPE.NO_STATUS);
        public static event UpdateStatusDelegate UpdatedStatus;


        public static void LogMessageAsync(string msg)
        {
            // Update text box
            UpdatedLog.Invoke(msg);
        }

        public static void StatusUpdateAsync(string msg,
            STATUS_FORMAT statusFormat, STATUS_TYPE connStatus)
        {
            // Update status strip
            UpdatedStatus.Invoke(msg, statusFormat, connStatus);
        }
    }
}
