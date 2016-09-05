using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MissionControl;
using System.Runtime.InteropServices;
namespace CSRoot
{
    class WindowFinder:IDisposable
    {
        AutoResetEvent m_WindowFoundEvent = new AutoResetEvent(false);

        volatile bool m_WindowFound = false;
        IntPtr m_WindowHandle = IntPtr.Zero;
        int m_Index = 0;
        bool exactMatch = true;

        private void Clear()
        {
            m_WindowFound = false;
            m_WindowHandle = IntPtr.Zero;
        }
        private int FindWinForm(IntPtr hwnd, IntPtr lparam)
        {
            if (hwnd == null)
                return 1;//true

            if (Win32APIs.IsWindow(hwnd) && Win32APIs.IsWindowVisible(hwnd))
            {
                string windowname = Marshal.PtrToStringUni(lparam);
                StringBuilder stringBuffer = new StringBuilder(256);

                int result = Win32APIs.GetWindowText(hwnd, stringBuffer, 256);
                string temp = stringBuffer.ToString();
                bool found = false;
                if(exactMatch)
                    found = temp.Equals(windowname);
                else
                    found=temp.Contains(windowname);
                if (found)
                {
                    if (m_Index == 0)//if user is interested in forst occurence then return
                    {
                        m_WindowHandle = hwnd;
                        m_WindowFound = true;
                        return 0;
                    }
                    else//maybe user is interested in nth occurence so keep looking
                    {
                        m_Index--;
                        return 1;
                    }
                }

            }
            return 1;

        }
        private void FindWindow_ThreadProc(object param)
        {
            IntPtr ptrToString = Marshal.StringToHGlobalUni((string)param);
            bool bresult = Win32APIs.EnumWindows(FindWinForm, ptrToString);
            m_WindowFoundEvent.Set();

        }
        public IntPtr FindWindowAsynch(string windowName, int timeOut)
        {
            Clear();
            string nameWithoutWildChar = windowName;
            if (windowName.Contains('*'))
            {
                nameWithoutWildChar = windowName.Substring(0, windowName.IndexOf('*'));
                exactMatch = false;
            }
            Thread windowFinder = new Thread(new ParameterizedThreadStart(FindWindow_ThreadProc));
            windowFinder.Start((object)nameWithoutWildChar);
            m_WindowFoundEvent.WaitOne(timeOut);//Todo make sec wait
            if (m_WindowFound)
                return m_WindowHandle;
            else
                return IntPtr.Zero;

        }


        public IntPtr FindWindowAsynch(string windowName, int timeOut, out uint procId)
        {
            procId = 0;
            m_Index = 0;
            FindWindowAsynch(windowName, timeOut);
            if (m_WindowFound)
            {
                Win32APIs.GetWindowThreadProcessId(m_WindowHandle, out procId);
                return m_WindowHandle;
            }
            else
                return IntPtr.Zero;
        }
        public IntPtr FindWindowAsynch(string windowName, int timeOut, int index, out uint procId)
        {
            procId = 0;
            m_Index = index;
            FindWindowAsynch(windowName, timeOut);
            if (m_WindowFound)
            {
                Win32APIs.GetWindowThreadProcessId(m_WindowHandle, out procId);
                return m_WindowHandle;
            }
            else
                return IntPtr.Zero;
        }
        public IntPtr FindWindowAsynch(string windowName, int timeOut, int index)
        {
            m_Index = index;
            FindWindowAsynch(windowName, timeOut);
            if (m_WindowFound)
                return m_WindowHandle;
            else
                return IntPtr.Zero;
        }

        #region IDisposable Members

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
                    m_WindowFoundEvent.Close();
                }
            }
            m_DisposeCalled = true;
        }

        #endregion
    }
}
