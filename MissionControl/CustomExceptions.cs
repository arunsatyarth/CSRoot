using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MissionControl
{
    public class Mayday:Exception
    {
        Exception m_InnerException = null;
        public Mayday(string msg):base(msg)
        {
            m_InnerException=new Exception(msg);
            Trace.WriteLine(msg);
        }
        public Mayday(Exception ex)
        {
            m_InnerException=ex;
            Trace.WriteLine(ex.Message);

        }
        public string ToString()
        {
            return m_InnerException.ToString();
        }
        public string Message
        {
            get{ return m_InnerException.Message;}
        }
    }
}
