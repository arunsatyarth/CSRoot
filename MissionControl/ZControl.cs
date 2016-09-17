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
        static ILogger s_Logger = null;
        [NonSerialized]
        AutoResetEvent m_FunctionInvocationEvent = null;
        private static MethodInfo s_SerializerFunction=null;
        private static object s_SerializerObject=null;
        static ZControl()
        {
            try
            {
                s_Logger = Logger.Instance();
                string runtimeSerialzerPath = Listener.m_CallerDirectory + @"\RuntimeSerializer.dll";
                Assembly runtimeSerializer = Assembly.LoadFile(runtimeSerialzerPath);
                if (runtimeSerializer != null)
                {
                    Type serializer = runtimeSerializer.GetType("RuntimeSerializer.RuntimeSerializer");
                    if (serializer != null)
                    {
                        s_SerializerObject = Activator.CreateInstance(serializer);
                        s_SerializerFunction = serializer.GetMethod("GenerateSerializableObject", BindingFlags.Static | BindingFlags.Public);
                    }

                }
            }
            catch (Exception e)
            {
            }
        }
      
        public ZControl(string name, IntPtr handle, object instance, Type controlType)
        {
            try
            {
                m_FunctionInvocationEvent = new AutoResetEvent(false);
                m_ControlName = name;
                m_ChildControlHandle = handle;
                m_ControlInstance = instance;
                m_ControlType = controlType;
            }
            catch (Exception e)
            {
                s_Logger.LogError(e.Message);
            }
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

        private object Serialize(object obj)
        {
            object newObject = null;
            object []parameter=null;
            try
            {
                if (s_SerializerObject != null && s_SerializerFunction != null)
                {
                    parameter = new object[1] { obj };
                    newObject = s_SerializerFunction.Invoke(s_SerializerObject, parameter);
                }

            }
            catch ( Exception e)
            {
                s_Logger.LogError("Exception in ZControl:Serilaize " + e.Message);
            }
            return newObject;
        }
        private MethodInfo FindMethod(string functionName, object [] parameters,BindingFlags flags)
        {
            try
            {
                if(flags==BindingFlags.Default)
                    flags=BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                MethodInfo method = null;
                Type[] parameterType = new Type[parameters.Length];
                int i = 0;
                foreach (object parameter in parameters)
                {
                    parameterType[i++] = parameter.GetType();
                }
                method = m_ControlType.GetMethod(functionName,flags,Type.DefaultBinder, parameterType, null);
                return method;
            }
            catch (Exception e)
            {
                s_Logger.LogError(e.Message);
            }
            return null;
        }
        public object Invoke(string functionname, object[] parameters,BindingFlags flags)
        {
            MethodInfo function = null;
            object retVal = null;

            try
            {
                function = FindMethod(functionname, parameters,flags);
                if (function == null)
                    return false;
                retVal = function.Invoke(m_ControlInstance, parameters);
            }
            catch (Exception e)
            {
                s_Logger.LogError(e.Message);
            }

            return retVal;
        }

        public object InvokeAsync(string functionname, object[] parameters)
        {
            throw new NotImplementedException();
        }

        public object InvokeRemote(IRemoteRunnable invokeObj)
        {
            throw new NotImplementedException();
        }

        public object GetPropertyValue(string filedName, out bool bResult)
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
            }
            catch (Exception e)
            {
                s_Logger.LogError(e.Message);
            }
            return retVal;

        }

        public object GetPropertyValueEx(string filedName, out bool bResult)
        {
            PropertyInfo property = null;
            bResult = false;
            object retVal = null;
            object remotableObject = null;

            if (m_ControlType == null || m_ControlInstance == null)
                return false;
            try
            {
                property = m_ControlType.GetProperty(filedName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property == null)
                    return null;

                retVal = property.GetValue(m_ControlInstance, null);


                remotableObject = Serialize(retVal);
                bResult = true;
            }
            catch (Exception e)
            {
                s_Logger.LogError(e.Message);
            }
            return remotableObject;
        }

        public bool SetPropertyValue(string filedName, object value)
        {
            PropertyInfo property = null;
            bool success = false;
            if (m_ControlType == null || m_ControlInstance == null)
                return false;
            try
            {
                property = m_ControlType.GetProperty(filedName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property == null)
                    throw new Mayday("Property NOt Found");

                property.SetValue(m_ControlInstance, value, null);
                success= true;
            }
            catch (Exception e)
            {
                s_Logger.LogError(e.Message);
            }
            return success; 
        }

        public Type GetControlType()
        {
            return m_ControlType;
        }
    }
}
