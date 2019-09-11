using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace WpfApp30
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            OutputTextBox.Text += string.IsNullOrEmpty(OutputTextBox.Text) ? e.Key.ToString() : "\r\n" + e.Key.ToString();
            OutputTextBox.SelectionStart = OutputTextBox.Text.Length + 1;
        }
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(
            byte bVk,    //虚拟键值
            byte bScan,// 一般为0
            int dwFlags,  //这里是整数类型  0 为按下，2为释放
            int dwExtraInfo  //这里是整数类型 一般情况下设成为 0
        );
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Focus();
            OutputTextBox.Text += string.IsNullOrEmpty(OutputTextBox.Text) ? "组合键：" : "\r\n" + "组合键：";
            keybd_event((byte)Keys.LControlKey, 0, 0, 0);
            keybd_event((byte)Keys.LShiftKey, 0, 0, 0);
            keybd_event((byte)Keys.Divide, 0, 0, 0);
            keybd_event((byte)Keys.LControlKey, 0, 2, 0);
            keybd_event((byte)Keys.LShiftKey, 0, 2, 0);
            keybd_event((byte)Keys.Divide, 0, 2, 0);
        }
    }
}
