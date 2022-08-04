using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOFOS2_Inventory_Recompute
{
    public class ThreadHelper
    {
        delegate void SetLabelText(Form f, Control c, string text);
        delegate void SetButtonState(Form f, Control c, bool stat);
        delegate void SetMessage(Form f, MessageBox c, bool stat);

        public static void SetLabel(Form f, Control c, string val)
        {
            if (c.InvokeRequired)
            {
                SetLabelText d = new SetLabelText(SetLabel);
                f.Invoke(d, new object[] { f, c, val });
            }
            else
            {
                c.Text = val;
            }
        }

        public static void SetControlState(Form f, Control c, bool stat)
        {
            if (c.InvokeRequired)
            {
                SetButtonState d = new SetButtonState(SetControlState);
                f.Invoke(d, new object[] { f, c, stat });
            }
            else
            {
                c.Enabled = stat;
            }
        }
    }
}
