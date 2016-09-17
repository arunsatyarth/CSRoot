using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MissionControl
{
    class ControlIdentifierByName : IChildControlFinder
    {
        string m_NameOfControl;
        Type m_ControlType;
        IntPtr m_Childhandle = IntPtr.Zero;
        IntPtr m_MainWindowhandle = IntPtr.Zero;
        Control m_Control = null;
        IWinControl m_ZControl = null;
        bool m_ControlFound = false;
        bool exactMatch = true;
        public ControlIdentifierByName(IntPtr mainWindowhandle,string name)
        {
            m_MainWindowhandle = mainWindowhandle;
            m_NameOfControl = name;
            if (name.Contains('*'))
            {
                m_NameOfControl = name.Substring(0, name.IndexOf('*'));
                exactMatch = false;
            }
        }
        public ControlIdentifierByName(IntPtr mainWindowhandle, string name, Type type)
            : this(mainWindowhandle, name)
        {
            m_ControlType = type;
        }
        public bool ControlFound
        {
            get { return m_ControlFound; }
        }
        public IntPtr Childhandle
        {
            get { return m_Childhandle; }
        }
        public object ChildControl
        {
            get { return m_ZControl; }
        }
        private void CreateControl(IntPtr handle, Control control)
        {
            m_ControlFound = true;
            m_Childhandle = handle;
            m_Control = control;
            m_ControlType = m_Control.GetType();
            string controlId;
            if (control.AccessibleName != null && control.AccessibleName != "")
                controlId = control.AccessibleName;
            else
                controlId = control.Text;
            m_ZControl= new ZControl(controlId, handle, control, m_ControlType);
        }
        public bool FindChild(IntPtr childHandle)
        {
            Control control = null;
            string key;
            bool found = false;
            try
            {
                if (m_NameOfControl == null || m_NameOfControl == "")
                {
                    return false;
                }
                if (m_NameOfControl == "this")
                {
                    control = Control.FromHandle(m_MainWindowhandle);
                    CreateControl(m_MainWindowhandle, control);
                    return true;
                }
                if (childHandle == null)
                    return false;
                control = Control.FromHandle(childHandle);
                if (control == null)
                {
                    return false;
                }

                key = control.AccessibleName;
                if (key != null && key != "")
                {
                    if(exactMatch)
                        found = key.Equals(m_NameOfControl);
                    else
                        found = key.Contains(m_NameOfControl);
                }
                key = control.Text;
                if (found == false && key != null && key != "")//if accname is not there and text contains something
                {
                    if (exactMatch)
                        found = key.Equals(m_NameOfControl);
                    else
                        found = key.Contains(m_NameOfControl);
                }
                if (found)
                {
                    CreateControl(childHandle, control);
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new Mayday(e.ToString());
            }
            return false;
        }
    }
    class ControlIdentifierByType : IChildControlFinder
    {
        List<IWinControl> m_Controls = new List<IWinControl>();
        Type m_TypeOfControl;
        bool m_ControlFound = false;
        public ControlIdentifierByType(Type typeOfControl)
        {
            m_TypeOfControl = typeOfControl;
        }
        public bool ControlFound
        {
            get { return m_ControlFound; }
        }
        public object ChildControl
        {
            get { return m_Controls; }
        }
        public bool FindChild(IntPtr controlHandle)
        {
            //This function always returns false as we want to iterate all controls. if we return true, iteration stops
            Type typeOfCtrl = null;
            try
            {
                Control control = Control.FromHandle(controlHandle);
                if (control == null)
                    return false;
                typeOfCtrl = control.GetType();
                if (typeOfCtrl == m_TypeOfControl || m_TypeOfControl == null)//if type is null, then it returns all controls
                {
                    m_ControlFound = true;
                    string controlId;
                    if (control.AccessibleName != null && control.AccessibleName != "")
                        controlId = control.AccessibleName;
                    else
                        controlId = control.Text;
                    IWinControl zControl = new ZControl(controlId, controlHandle, control, typeOfCtrl);
                    m_Controls.Add(zControl);
                    return false;
                }
                return false;
            }
            catch (Exception e)
            {
                throw new Mayday(e.ToString());
            }
        }
    }
}
