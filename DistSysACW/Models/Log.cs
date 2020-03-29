using System;

namespace DistSysACW.Models
{
    public class Log : BaseLog
    {
        public Log()
        {
            
        }

        public Log(string logString)
        {
            LogDateTime = DateTime.Now;
            LogString = logString;
        }
    }
}