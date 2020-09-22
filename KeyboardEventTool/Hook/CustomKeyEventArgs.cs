using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace KeyboardEventTool
{
    public class CustomKeyEventArgs : EventArgs
    {
        public Keys Key { get; private set; }

        public bool Handled { get; private set; }

        public CustomKeyEventArgs(Keys key)
        {
            Key = key;
        }
    }
}
