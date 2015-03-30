using System;
using System.Diagnostics;
using Microsoft.FSharp.Core;
using NextTrain.Lib;

namespace NextTrain.WorkerRole
{
    public class Logger : ITweetLogger
    {
        public void logDebug(string msg)
        {
            Trace.TraceInformation("{0} DEBUG {1}", DateTime.UtcNow.ToString("u"), msg);
        }

        public void logInfo(string msg)
        {
            Trace.TraceInformation("{0} INFO {1}", DateTime.UtcNow.ToString("u"), msg);
        }

        public void logWarn(string msg)
        {
            Trace.TraceWarning("{0} WARN {1}", DateTime.UtcNow.ToString("u"), msg);
        }

        public void logError(string msg)
        {
            Trace.TraceError("{0} ERROR {1}", DateTime.UtcNow.ToString("u"), msg);
        }
    }
}
