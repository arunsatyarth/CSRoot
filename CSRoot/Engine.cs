using MissionControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;

namespace CSRoot
{
    public class Engine
    {
        volatile bool m_success = false;
        volatile int m_PortToClient = 60000;
        int m_Timeout;
        AutoResetEvent m_ThreadSynchronizer = new AutoResetEvent(false);
        Hashtable m_HookTracker = new Hashtable();
        static ILogger sLogger = Logger.Instance();
        TcpClientChannel m_ServerToClientChannel = null;

        public Engine()
        {
            m_Timeout = 20000;
#if DEBUG
            m_Timeout = 200000;
#endif
            
        }
        private void DllInjector(object param)
        {
            object[] actualParams=(object[])param;
            string windowName = actualParams[0] as string;
            int servertimeout = 100000;
            ClientInfoProvider server = new ClientInfoProvider(servertimeout);
            m_PortToClient = server.Listen();

            EventWaitHandle wh = new EventWaitHandle(false, EventResetMode.AutoReset, "SniperWaiting");

            //Inject DLL on the intended process
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Arguments = windowName;
            processInfo.CreateNoWindow = true;
            processInfo.FileName = "Sniper.exe";

            Process process = Process.Start(processInfo);

            bool signalled=wh.WaitOne(m_Timeout-5000);
            m_success=signalled? true : false;

            m_ThreadSynchronizer.Set();
             

        }
        public IChannelClientToServer ConnectToClient(int portNumber)
        {
            IChannelClientToServer remoteObject = null;
            try
            {
                //connect to the MissionControl client at port number given to client from hookengine
                m_ServerToClientChannel = new TcpClientChannel("WinSniperClient" + portNumber.ToString(), null);
                ChannelServices.RegisterChannel(m_ServerToClientChannel, true);
                string url = "tcp://localhost:" + portNumber.ToString() + "/WinSniperClient.rem";
                //get the marshalled object from MissionControl
                remoteObject = (IChannelClientToServer)Activator.GetObject(
                    typeof(IChannelClientToServer), url);
            }
            catch (Exception e)
            {
                sLogger.LogError("Client server could not be found");
                return null;
                
            }
            return remoteObject;
        }
        private uint GetProcId(string windowName,out int handle)
        {

            uint procId;
            WindowFinder windowFinder = new WindowFinder();
            IntPtr hwnd=windowFinder.FindWindowAsynch(windowName, 10000, out procId);
            handle = hwnd.ToInt32();
            if (procId > 0)//Todo:wtf is this in revrse
                return procId;
            else
                throw new Mayday("Could Not Find this Window and Process");
        }
        public Window GetWindow(string windowName)
        {
            IChannelClientToServer serverToClientStream=null;
            try
            {
                int handle;
                uint procId = GetProcId(windowName,out handle);
                if (m_HookTracker.Contains(procId))
                {
                    serverToClientStream = (IChannelClientToServer)m_HookTracker[procId];
                }
                else
                {
                    Thread hookwindow_thread = new Thread(new ParameterizedThreadStart(DllInjector));

                    object[] param = new object[] { windowName };
                    hookwindow_thread.Start((object)param);
                    m_ThreadSynchronizer.WaitOne(m_Timeout);
                    if (!m_success)
                        return null;
                    serverToClientStream = ConnectToClient(m_PortToClient);
                    if (serverToClientStream == null)
                        return null;
                    m_HookTracker.Add(procId, serverToClientStream);

                }

                return new Window(serverToClientStream,handle);
            }
            catch (Mayday ex)
            {
                sLogger.LogError(ex.Message);
                return null;
            }

        }
    }
}
