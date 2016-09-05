using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MissionControl
{
    [Serializable]
    class ZControl : MarshalByRefObject, IWinControl,IDisposable
    {
        Type m_ControlType = null;
        IntPtr m_ChildControlHandle;
        object m_ControlInstance = null;
        string m_ControlName = "";
        [NonSerialized]
        AutoResetEvent m_FunctionInvocationEvent = new AutoResetEvent(false);
        public ZControl(string name, IntPtr handle, object instance, Type controlType)
        {
            m_ControlName = name;
            m_ChildControlHandle = handle;
            m_ControlInstance = instance;
            m_ControlType = controlType;
        }
        private bool m_DisposeCalled = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!m_DisposeCalled)
            {
                if (disposing)
                {
                    m_FunctionInvocationEvent.Close();
                }

            }
            m_DisposeCalled = true;
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
            PropertyInfo propertyName = null;
            object retVal = null;
            bResult = false;

            try
            {
                if (m_ControlType == null || m_ControlInstance == null)
                    return null;
                propertyName = m_ControlType.GetProperty(filedName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (propertyName == null)
                    return null;

                retVal = propertyName.GetValue(m_ControlInstance, null);
                bResult = true;
                return retVal;
            }
            catch (Exception e)
            {
                return null;
            }
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
