using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MissionControl;
using System.Runtime.Serialization;
namespace CSRoot
{
    class SurrogateControl: IWinControl
    {
        IWinControl m_WinControl = null;
        public SurrogateControl(IWinControl control)
        {
            m_WinControl = control;

        }

        public object Invoke(string functionname, object[] parameters)
        {
            throw new NotImplementedException();
        }

        public object InvokeAsync(string functionname, object[] parameters)
        {
            throw new NotImplementedException();
        }

        public object InvokeRemote(IRemoteRunnable invokeObj)
        {
            throw new NotImplementedException();
        }

        public object GetFieldValue(string filedName, out bool bResult)
        {
            object retVal = null;
            bResult = false;
            try
            {
                retVal = m_WinControl.GetFieldValue(filedName, out bResult);
                return retVal;
            }
            catch (Exception e)
            {
            }

            //Try with marshalled value
            try
            {
                retVal = m_WinControl.GetFieldValueEx(filedName, out bResult);
            }
            catch (Exception ex)
            {
            }


            return retVal;
        }

        public object GetFieldValueEx(string filedName, out bool bResult)
        {
            throw new NotImplementedException();
        }

        public bool SetFieldValue(string filedName, object value, Type typeofField)
        {
            throw new NotImplementedException();
        }

        public Type GetControlType()
        {
            throw new NotImplementedException();
        }
    }
}
