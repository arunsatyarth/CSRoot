using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MissionControl
{
    public class Logger:ILogger
    {
        static object m_Singletonsynch=new object();
        object m_LogSynch=new object();
        private static  ILogger s_LoggerObject = null;
        public void LogError(string error)
        {
            lock (m_LogSynch)
            {
                Trace.WriteLine(error);
            }
        }
        public void LogInfo(string error)
        {
            lock (m_LogSynch)
            {
                Trace.WriteLine(error);
            }
        }
        private Logger()
        {
        }
        //Singleton object
        public static ILogger Instance()
        {
            if(s_LoggerObject!=null)
                return s_LoggerObject;
            lock (m_Singletonsynch)
            {
                if(s_LoggerObject==null)
                {
                    s_LoggerObject = new Logger();
                }
            }
            return s_LoggerObject;
        }
    }
}
