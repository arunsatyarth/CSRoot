using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Security.AccessControl;

namespace MissionControl
{
    public class Listener 
    {
        static bool s_AlreadyHooked = false;
        ILogger s_Logger = Logger.Instance();
        private int m_Port = 60000;//default
        public static string m_CallerDirectory;
        public static string m_CallerExe;

        public void Listen()
        {
            try
            {
                if (s_AlreadyHooked == true)
                    return;
                s_AlreadyHooked = true;


                //Connect to server at port 60200 and get the port number at which to open service
                TcpClientChannel clientChannel = new TcpClientChannel("HookedClient", null);
                ChannelServices.RegisterChannel(clientChannel, true);
                Trace.WriteLine("MissionControl::injectHook going to connect to server");
                IChannelServerToClient remoteObject = (IChannelServerToClient)Activator.GetObject(
                    typeof(IChannelServerToClient), "tcp://localhost:60200/CSRoot.rem");
                if (remoteObject == null)
                    s_Logger.LogError("Connection to CSRoot server could not be established");

                SyncDataFromServer(remoteObject);

                s_Logger.LogInfo("The port sent from server is " + m_Port.ToString(CultureInfo.CurrentCulture));
                
                //now start thread to listen to client
                object param = (object)m_Port;
                Thread clientListenerThread = new Thread(new ParameterizedThreadStart(StartClientServer));
                clientListenerThread.Start(param);

                s_Logger.LogInfo("Server has been started at client side");

            }
            catch (Exception e)
            {
                s_Logger.LogInfo("Exception at Listener:Listen");
            }

        }
        MarshalByRefObject GetProcessProxy()
        {
            return new ZProcess();
        }
        private void StartClientServer(object obj)
        {
            try
            {
                int clientServerPort = (int)obj;
                s_Logger.LogInfo("IMissionControl::ClientThe port sent from server is " + clientServerPort.ToString());
                MarshalByRefObject processProxyObject = GetProcessProxy();//factory
                TcpServerChannel clientServer = new TcpServerChannel("WinSniperClient", clientServerPort);
                ChannelServices.RegisterChannel(clientServer, true);

                RemotingServices.Marshal(processProxyObject, "WinSniperClient.rem");
                s_Logger.LogInfo("Sniper ClientServer has been started and listening at " + clientServerPort.ToString());


                System.Threading.EventWaitHandle wh = EventWaitHandle.OpenExisting("SniperWaiting", EventWaitHandleRights.Modify);
                wh.Set();
                while (true)
                {
                    Thread.Sleep(1000);
                }

            }
            catch (ThreadAbortException e)
            {
                s_Logger.LogError("Client Server Thread has been aborted. ");
            }
            catch (Exception e)
            {
                new Mayday(e);
            }
        }
        private void  SyncDataFromServer(IChannelServerToClient remoteObject)
        {
            try
            {
                m_Port = remoteObject.GetServerPort();
                m_CallerDirectory = remoteObject.GetClientDirectory();
                m_CallerExe = remoteObject.GetCallingExe();
                Trace.WriteLine("data was queried successfully from server");
            }
            catch (Exception ex)
            {
                //we can recover from this error
                s_Logger.LogInfo("Could not retriev data from Sniper server" + ex.ToString());
            }
        }
    }
}
