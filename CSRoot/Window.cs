using MissionControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSRoot
{
    public class Window : IWinFormEx
    {
        IChannelClientToServer m_ClientChannel = null;
        int m_WindowHandle;
        AutoResetEvent m_ControlFound = new AutoResetEvent(false);
        Hashtable m_FetchedControls;
        volatile bool m_Success = false;
        volatile IWinControl m_SurrogateControl = null;

        public Window(IChannelClientToServer stream, int windowHandle)
        {
            m_ClientChannel = stream;
            m_WindowHandle = windowHandle;
        }
        private void GetControlThreadProc(object param)
        {
            object[] actualParams = (object[])param;


            string controlId = (string)actualParams[0];
            Type controlType = (Type)actualParams[1];

            if (m_ClientChannel == null)
                return;
            try
            {
                IWinForm form = m_ClientChannel.GetWindow(m_WindowHandle);
                IWinControl controlproxy = form.GetControl(controlId, controlType);
                if (controlproxy == null)
                    return;
                m_SurrogateControl = new SurrogateControl(controlproxy);
                m_Success = true;

            }
            catch (Exception e)
            {
                m_Success = false;
            }
            finally
            {
                m_ControlFound.Set();
            }
        }
        public IWinControl GetControlAsynch(string controlId, Type typeOfControl)
        {
            object[] param = new object[] { controlId,typeOfControl };

            Thread t = new Thread(new ParameterizedThreadStart(GetControlThreadProc));
            t.Start((object)param);
            m_ControlFound.WaitOne(3000);
            return m_SurrogateControl;

        }


        public IWinControl GetControl(string controlId, Type controltype)
        {
            try
            {
                IWinForm form = m_ClientChannel.GetWindow(m_WindowHandle);
                IWinControl controlProxy = form.GetControl(controlId, controltype);
                if (controlProxy == null)
                    return null;
                IWinControl surrogateControl = new SurrogateControl(controlProxy);

                return surrogateControl;
            }
            catch (Exception ex )
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public List<IWinControl> GetControls(Type controltype)
        {
            try
            {
                IWinForm form = m_ClientChannel.GetWindow(m_WindowHandle);
                List<IWinControl> controlProxys = form.GetControls(controltype);
                if (controlProxys == null || controlProxys.Count == 0)
                    return null;
                List<IWinControl> surrogateControls = new List<IWinControl>();
                foreach (IWinControl item in controlProxys)
                {
                    IWinControl surrogateControl = new SurrogateControl(item);
                    surrogateControls.Add(surrogateControl);
                }

                return surrogateControls;
            }
            catch (Exception ex )
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
    }
}
