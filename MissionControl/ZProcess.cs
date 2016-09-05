using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MissionControl
{
    /// <summary>
    /// ZProcess is also the object that is marshelled to the server fromn the client, wihch means it 
    /// is the counterpart to ClientInfoProvider. 
    /// At the server the communocation will start with this class by requesting a Window(Form)
    /// </summary>
    [Serializable]
    class ZProcess : MarshalByRefObject, IChannelClientToServer
    {
        Hashtable m_FormStore = new Hashtable();
        private static string s_ClientExepath = null;
        public IWinForm GetWindow(int handle)
        {

            if (!m_FormStore.Contains(handle))
            {
                IWinForm newForm = new ZForm(handle);
                m_FormStore.Add(handle, newForm);
            }
            return (IWinForm)m_FormStore[handle];
        }

        public string GetClientProcessPath()
        {
            if(s_ClientExepath==null)
                s_ClientExepath = Assembly.GetExecutingAssembly().Location;
            return s_ClientExepath;
        }
    }
}
