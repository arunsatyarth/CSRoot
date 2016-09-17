using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MissionControl
{
    [Serializable]
    class ZForm : MarshalByRefObject, IWinForm
    {
        IntPtr m_WindowHandle;
        Hashtable m_ControlList = new Hashtable();
        Hashtable m_ControlListTypes = new Hashtable();

        const int ENUM_TIMEOUT = 10;
        public ZForm(int windowHandle)
        {
            m_WindowHandle = new IntPtr(windowHandle);
        }


        public IWinControl GetControl(string controlId, Type controltype)
        {
            IWinControl control = null;

            try
            {
                if (m_ControlList.Contains(controltype))
                {
                    control = (IWinControl)m_ControlList[controltype];
                }
                else
                {
                    IChildControlFinder selector = new ControlIdentifierByName(m_WindowHandle, controlId);
                    ChildControlCrawler childlearner = new ChildControlCrawler();
                    childlearner.FindChildWindowAsync(m_WindowHandle, selector.FindChild, ENUM_TIMEOUT);
                    if (!selector.ControlFound)
                        return null;
                    control = (IWinControl)selector.ChildControl;

                    m_ControlList.Add(controlId, control);
                }
                return control;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<IWinControl> GetControls(Type controltype)
        {

            List<IWinControl> controlList = null;
            try
            {
                if (m_ControlListTypes.Contains(controltype))
                {
                    controlList = (List<IWinControl>)m_ControlListTypes[controltype];
                }
                else
                {
                    IChildControlFinder selector = new ControlIdentifierByType(controltype);
                    ChildControlCrawler controlIterator = new ChildControlCrawler();
                    bool ctrlFound = controlIterator.FindChildWindowAsync(m_WindowHandle, selector.FindChild, ENUM_TIMEOUT);
                    if (!selector.ControlFound)
                        return null;
                    controlList = (List<IWinControl>)selector.ChildControl;
                    m_ControlListTypes.Add(controltype, controlList);
                }
                return controlList;
            }
            catch (Exception e)
            {
                return null;
            }

        }
    }
}
