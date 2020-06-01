using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
using WindowsInput;
using WindowsInput.Native;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace KeyboardEventTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var keysList = GetEnumDict(typeof(Key));
            KeysList = keysList.Distinct().ToList();
            KeyComboBox1.ItemsSource = KeysList;
            KeyComboBox2.ItemsSource = KeysList;
            KeyComboBox3.ItemsSource = KeysList;
            KeyComboBox1.SelectedValue = Key.LeftCtrl.ToString();
            KeyComboBox2.SelectedValue = Key.None.ToString();
            KeyComboBox3.SelectedValue = Keys.None.ToString();
        }

        #region 发送键盘消息

        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(
            byte bVk,    //虚拟键值
            byte bScan,// 一般为0
            int dwFlags,  //这里是整数类型  0 为按下，2为释放
            int dwExtraInfo  //这里是整数类型 一般情况下设成为 0
        );
        /// <summary>
        /// 通过user32.dll发送键盘消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendKeysByUser32_OnClick(object sender, RoutedEventArgs e)
        {
            SendKeysOperation(() =>
            {
                if (KeyComboBox1.Text is string wpfKeyString &&
                    KeysTransferHelper.TryConvertToWinformKey(wpfKeyString, out var winformKey))
                {
                    keybd_event((byte)winformKey, 0, 0, 0);
                    if (KeyComboBox2.Text is string wpfKeyString2 &&
                        KeysTransferHelper.TryConvertToWinformKey(wpfKeyString2, out var winformKey2))
                    {
                        keybd_event((byte)winformKey2, 0, 0, 0);
                        if (KeyComboBox3.Text is string wpfKeyString3 &&
                            KeysTransferHelper.TryConvertToWinformKey(wpfKeyString3, out var winformKey3))
                        {
                            keybd_event((byte)winformKey3, 0, 0, 0);
                            keybd_event((byte)winformKey3, 0, 2, 0);
                        }
                        keybd_event((byte)winformKey2, 0, 2, 0);
                    }
                    keybd_event((byte)winformKey, 0, 2, 0);
                }
            });
        }

        private async void SendKeysOperation(Action action)
        {
            try
            {
                //默认发送给当前窗口
                OutputTextBox.Focus();

                //延迟发送
                var text = SendKeysTimeTextBox.Text;
                if (!(ShowTimerCheckBox.IsChecked ?? false) || string.IsNullOrEmpty(text))
                {
                    action.Invoke();
                }
                else if (double.TryParse(text, out var delayTime))
                {
                    await Task.Delay(TimeSpan.FromSeconds(delayTime));
                    action.Invoke();
                }
                else
                {
                    MessageBox.Show($"键盘事件发送失败，延时设置异常:{text}");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"{nameof(SendKeysOperation)}:{e.Message}");
            }
        }

        private InputSimulator _inputSimulator = new InputSimulator();
        private void SendKeysByWindowsInput_OnClick(object sender, RoutedEventArgs e)
        {
            SendKeysOperation(() =>
            {
                var ctrlAndAlt = new List<VirtualKeyCode>() { VirtualKeyCode.LCONTROL, VirtualKeyCode.LMENU };
                _inputSimulator.Keyboard.ModifiedKeyStroke(ctrlAndAlt, VirtualKeyCode.MULTIPLY);
            });
        }

        #endregion

        #region 监听键盘消息

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_showKeyDown)
            {
                OutputTextBox.Text += string.IsNullOrEmpty(OutputTextBox.Text) ? e.Key.ToString() : "\r\n" + e.Key.ToString();
                OutputTextBox.SelectionStart = OutputTextBox.Text.Length + 1;
            }
        }

        private void MainWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (_showKeyUp)
            {
                OutputTextBox.Text += string.IsNullOrEmpty(OutputTextBox.Text) ? e.Key.ToString() : "\r\n" + e.Key.ToString();
                OutputTextBox.SelectionStart = OutputTextBox.Text.Length + 1;
            }
        }
        private bool _showKeyDown = true;
        private void ShowKeyDownCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                _showKeyDown = checkBox.IsChecked ?? false;
            }
        }
        private bool _showKeyUp = false;
        private void ShowKeyUpCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                _showKeyUp = checkBox.IsChecked ?? false;
            }
        }
        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Text = string.Empty;
        }
        #endregion

        #region 其它

        private void SendKeysTimeTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.]+");

            e.Handled = re.IsMatch(e.Text);
        }

        public List<string> KeysList { get; set; }
        /// <summary>
        /// 获取枚举字典
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static List<string> GetEnumDict(Type enumType)
        {
            var list = new List<string>();
            FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    list.Add(field.Name);
                }
            }

            return list;
        }

        #endregion
    }
}
