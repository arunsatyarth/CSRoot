using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// <summary>
    /// Todo: Add BindingFlags parameter as a default value in all these if necessary
    /// </summary>
    public interface IWinControl
    {
        object Invoke(string functionname, object[] parameters, BindingFlags flags = BindingFlags.Default);
        object InvokeAsync(string functionname, object[] parameters);
        object InvokeRemote(IRemoteRunnable invokeObj);
        object GetPropertyValue(string filedName, out bool bResult);
        object GetPropertyValueEx(string filedName, out bool bResult);
        bool SetPropertyValue(string filedName, object value);
        Type GetControlType();
    }

    public interface ILogger
    {
        void LogError(string error);
        void LogInfo(string message);
    }

}
