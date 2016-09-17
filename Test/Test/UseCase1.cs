using CSRoot;
using MissionControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine e = new Engine();
            Window w = e.GetWindow("Proc1Form1");
            if (w == null)
                Console.WriteLine("Window not found");
            else
            {
                List<IWinControl> ctrls = w.GetControls(typeof(Button));
                foreach (IWinControl item in ctrls)
                {
                    bool bsuccess = false;
                    string str = (string)item.GetPropertyValue("Text", out bsuccess);
                    Console.WriteLine(str);
                    item.SetPropertyValue("Text","Changed");
                }
                bool dddd = false;

                IWinControl ctrl = w.GetControl("this", typeof(Form));
                string ssss = (string)ctrl.GetPropertyValue("Text", out dddd);
                Console.WriteLine(ssss);


            }


            while (true)
            {
                Console.WriteLine("Inside while");
                Thread.Sleep(1000);
            }
            //IHookInterfaces.IWinForm win = engine.FindWindow("Form1", 20);
            ///    IWinControl ctrl = win.GetControl("OK", typeof(Button));
            ///    SizeF s = new SizeF(3, 3);
            ///    ctrl.Invoke("Scale", new object[] { s });
        }
    }
}
