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
        static SurrogateControl()
        {
            try
            {
                Assembly runtimeSerializer = Assembly.LoadFile("RuntimeSerializer.dll");
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

        public object Invoke(string functionName, object[] parameters, BindingFlags flags = BindingFlags.Default)
        {
            try
            {
                return m_WinControl.Invoke(functionName, parameters,flags);
            }
            catch (SerializationException ex)
            {
            }
            catch (Exception ex)
            {
                return null;
            }
            Type typeOfControl;
            int i = 0;
            try
            {
                object[] remotableParamsters = new Object[parameters.Length];
                foreach (object item in parameters)
                {
                    typeOfControl = item.GetType();
                    if (typeOfControl.IsValueType)//value types generally do not cause serialization issues
                    {
                        remotableParamsters[i++] = item;
                        continue;
                    }
                    object inst = Serialize(item);
                    remotableParamsters[i++] = inst;
                }
                return m_WinControl.Invoke(functionName, remotableParamsters, flags);
            }
            catch (Exception ex)
            {
                return null;    
            }

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
            object retVal = null;
            bResult = false;
            try
            {
                return retVal = m_WinControl.GetPropertyValue(filedName, out bResult);
            }
            catch (SerializationException ex)
            {
            }
            catch (Exception ex)
            {
                return null;
            }
            try
            {
                retVal = m_WinControl.GetPropertyValueEx(filedName, out bResult);
            }
            catch (Exception ex)
            {
            }


            return retVal;
        }

        public object GetPropertyValueEx(string filedName, out bool bResult)
        {
            //I dont think we need it. I think above thing covers it
            throw new NotImplementedException();
        }

        public bool SetPropertyValue(string filedName, object value)
        {
            try
            {
                return m_WinControl.SetPropertyValue(filedName, value);
            }
            catch (Exception ex)
            {
            }

            try
            {
                object inst = Serialize(value);
                return m_WinControl.SetPropertyValue(filedName, inst);
            }
            catch (Exception ex)
            {
            }
            return false;

        }

        public Type GetControlType()
        {
            return m_WinControl.GetControlType();
        }
    }
}
