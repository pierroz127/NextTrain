//   Copyright 2015 Pierre Leroy
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Diagnostics;
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
