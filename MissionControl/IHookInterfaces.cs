using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MissionControl
{
    public interface IChannelServerToClient
    {
        int GetServerPort();
        string GetClientDirectory();
        string GetCallingExe();
    }
    public interface IChannelClientToServer
    {
        IWinForm GetWindow(int handle);
        string GetClientProcessPath();
    }
    public interface IWinForm
    {
        IWinControl GetControl(string controlId, Type controltype);
        List<IWinControl> GetControls(Type controltype);

    }
    public interface IWinFormEx:IWinForm
    {
        IWinControl GetControlAsynch(string controlId, Type controltype);

    }
    interface IChildControlFinder
    {
        bool FindChild(IntPtr childHandle);
        bool ControlFound { get; }
        object ChildControl { get; }

    }
    public interface IRemoteRunnable
    {
        object Run(Control ctrl);
    }
    public interface IWinControl
    {
        object Invoke(string functionname, object[] parameters);
        object InvokeAsync(string functionname, object[] parameters);
        object InvokeRemote(IRemoteRunnable invokeObj);
        object GetFieldValue(string filedName, out bool bResult);
        object GetFieldValueEx(string filedName, out bool bResult);
        bool SetFieldValue(string filedName, object value, Type typeofField);
        Type GetControlType();
    }

    public interface ILogger
    {
        void LogError(string error);
        void LogInfo(string message);
    }

}
