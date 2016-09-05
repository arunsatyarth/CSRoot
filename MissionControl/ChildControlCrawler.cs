using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MissionControl
{
    class ChildControlCrawler : IDisposable
    {
        AutoResetEvent m_ChildControlFoundEvnt = new AutoResetEvent(false);
        public delegate bool Del_Compare(IntPtr hwnd);
        IntPtr m_ChildWindowHandle = IntPtr.Zero;
        bool m_ChildWindowFound = false;
        volatile Del_Compare m_ComparisonFunction = null;
        static ILogger s_Logger = Logger.Instance();
        public void Clear()
        {
            m_ChildWindowHandle = IntPtr.Zero;
            m_ChildWindowFound = false;
        }
        private int EnumChildWindowCallback(IntPtr hwnd, IntPtr lparam)
        {
            if (hwnd == null)
                return 1;
            if (m_ComparisonFunction == null)
            {
                s_Logger.LogError("The callback is not valid");
                return 0;
            }
            try
            {
                if (m_ComparisonFunction(hwnd))//calling the comparion function specified by caleer
                {
                    m_ChildWindowFound = true;
                    return 0;
                }
            }
            catch (Mayday e)
            {
                s_Logger.LogError(e.Message);
            }
            return 1;

        }
        private void ChildControlFinderThreadProc(object param)
        {

            IntPtr mainWindowhandle = (IntPtr)param;
            try
            {
                Win32APIs.EnumChildWindows(mainWindowhandle, EnumChildWindowCallback, IntPtr.Zero);
                m_ChildControlFoundEvnt.Set();
            }
            catch (Exception e)
            {

            }


        }
        public bool FindChildWindowAsync(IntPtr mainWindowhandle, Del_Compare comparisonCallBack, int timeOutSec)
        {
            if (comparisonCallBack == null)
                return false;
            m_ComparisonFunction = comparisonCallBack;
            Clear();

            Thread windFinder = new Thread(new ParameterizedThreadStart(ChildControlFinderThreadProc));
            windFinder.Start((object)mainWindowhandle);
            int waittime=4000;
#if DEBUG
           waittime=100000;
#endif
            m_ChildControlFoundEvnt.WaitOne(waittime);
            return m_ChildWindowFound;
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
                    m_ChildControlFoundEvnt.Close();
                }

            }
            m_DisposeCalled = true;
        }
    }
}
