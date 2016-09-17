using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MissionControl;
using System.Runtime.Serialization;
using System.Reflection;
namespace CSRoot
{
    class SurrogateControl: IWinControl
    {
        IWinControl m_WinControl = null;
        public SurrogateControl(IWinControl control)
        {
            m_WinControl = control;

        }
        private static MethodInfo s_SerializerFunction;
        private static object s_SerializerObject;
        public static SurrogateControl()
        {
            Assembly runtimeSerializer=Assembly.LoadFile("RuntimeSerializer.dll");
            if(runtimeSerializer!=null)
            {
                Type serializer=runtimeSerializer.GetType("RuntimeSerializer.RuntimeSerializer");
                if (serializer != null)
                {
                    s_SerializerObject = Activator.CreateInstance(serializer);
                    s_SerializerFunction = serializer.GetMethod("GenerateSerializableObject", BindingFlags.Static | BindingFlags.Public);
                }

            }
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
            catch ( Exception)
            {
            }
            return newObject;
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
                return retVal = m_WinControl.GetFieldValue(filedName, out bResult);
            }
            catch (Exception e)
            {
            }

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
            //I dont think we need it. I think above thing covers it
            throw new NotImplementedException();
        }

        public bool SetFieldValue(string filedName, object value, Type typeOfField)
        {
            try
            {
                return m_WinControl.SetFieldValue(filedName, value, typeOfField);
            }
            catch (Exception ex)
            {
            }

            try
            {
                object inst = Serialize(value);
                return m_WinControl.SetFieldValue(filedName, inst, typeOfField);
            }
            catch (Exception ex)
            {
            }
            return false;

        }

        public Type GetControlType()
        {
            throw new NotImplementedException();
        }
    }
}
