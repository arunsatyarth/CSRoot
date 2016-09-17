using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using MissionControl;
namespace CSRoot
{

    class ClientInfoProvider
    {
        private static int s_PortCount = 60000;
        volatile private int m_CurrentPortNumber;
        AutoResetEvent m_ServerStarted = new AutoResetEvent(false);
        Thread m_ServerThread = null;
        volatile ServerSetupInfo data = null;
        static ILogger s_Logger;
        int m_Timeout;
        static TcpServerChannel channel = null;
        static ClientInfoProvider()
        {
            try
            {

                channel = new TcpServerChannel("ClientInfoService", 60300);
                ChannelServices.RegisterChannel(channel, true);
                s_Logger = Logger.Instance();
            }
            catch (Exception e)
            {

            }

        }
        public ClientInfoProvider(int timeout)
        {
            m_CurrentPortNumber = s_PortCount++;
            m_Timeout = timeout;
        }
        public int Listen()
        {
            //Start a thread and wait for hooked clients to connect so that we can tell them which port to use
            m_ServerThread = new Thread(Threadproc);
            m_ServerThread.Start();
            m_ServerStarted.WaitOne(m_Timeout);
            return CurrentPortNumber;
        }
        public int CurrentPortNumber
        {
            get
            {
                return m_CurrentPortNumber;
            }
        }

        public void Threadproc()
        {
            try
            {
                data = new ServerSetupInfo();
                data.PortNumber = m_CurrentPortNumber;

                RemotingServices.Marshal(data, "CSRoot.rem");
                m_ServerStarted.Set();
                while (true)
                {
                    Thread.Sleep(1000);
                    s_Logger.LogInfo("ClientInfoProvider is listening  ");
                }
            }
            catch (ThreadAbortException e)
            {
                s_Logger.LogInfo("ClientInfoProvider:Aborting the thread ");
            }
            catch (Exception e)
            {
                s_Logger.LogError("ClientInfoProvider:ThreadProc error  " + e.Message);

            }
            finally
            {
                m_ServerStarted.Set();
                RemotingServices.Disconnect(data);
            }
        }
        public void StopListening()
        {
            try
            {
                m_ServerThread.Abort();
            }
            catch (Exception e)
            {
                s_Logger.LogError("ClientInfoProvider:StopListening error while aborting thread");

            }
        }

        #region IDisposable Members

        private bool m_DisposeCalled = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool disposing)
        {
            if (!m_DisposeCalled)
            {
                if (disposing)
                {
                    m_ServerStarted.Close();
                }

            }
            m_DisposeCalled = true;
        }


        ~ClientInfoProvider()
        {
            Dispose(false);
        }

        #endregion
        
    }
    class ServerSetupInfo : MarshalByRefObject, IChannelServerToClient
    {
        private string m_ClientDirectory = "";
        private string m_CallingExe = "";
        public int PortNumber{ get; set;}

        public int GetServerPort()
        {
            return PortNumber;            
        }

        public string GetClientDirectory()
        {
            if (m_ClientDirectory == "")
                m_ClientDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return m_ClientDirectory;
        }

        public string GetCallingExe()
        {
            if (m_CallingExe == "")
                m_CallingExe = Assembly.GetExecutingAssembly().Location;
            return m_CallingExe;
        }
    }
}
