using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
namespace CopyMerge
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private WindowState m_storedWindowState = WindowState.Normal;
        private bool isInitializing = true;
        public MainWindow()
        {
            InitializeComponent();
            SetAutoLaunch(true, Assembly.GetEntryAssembly().Location);
           
                SetComboBoxValue();
                isInitializing = false;
                notifyIcon = new NotifyIcon();
                notifyIcon.BalloonTipText = "Программа свернута в трей, вы можете продолжать ей пользоваться :)";
                notifyIcon.BalloonTipTitle = "Go To tray";
                notifyIcon.Text = "CopyMerge";
            try
            {
                System.Windows.MessageBox.Show("e", "1");
                notifyIcon.Icon = new System.Drawing.Icon("icon.ico");
                System.Windows.MessageBox.Show("e", "2");
                notifyIcon.Click += new EventHandler(NotifyIcon_Click);
                System.Windows.MessageBox.Show("e", "3");
                KeyLogger.KeyBufferClear();
                System.Windows.MessageBox.Show("e", "4");
                KeyLogger.KeyChecker();
                System.Windows.MessageBox.Show("e", "5");
                File.AppendAllText("log.txt", "MainWindow: Components Initialized\n");
                System.Windows.MessageBox.Show("e", "6");

            }
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", ex.Message);
            }
            System.Windows.MessageBox.Show("e", "Final");
        }
        private bool SetAutoLaunch(bool launch, string path)
        {
            string name = "CopyMerge";
            string ExePath = path;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                if (launch)
                {
                    reg.SetValue(name, ExePath);
                }
                else
                {
                    reg.DeleteValue(name);
                }
                reg.Flush();
                reg.Close();
            }
            catch (Exception) { return false; }
            return true;
        }
        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                if (notifyIcon != null)
                    notifyIcon.ShowBalloonTip(2000);
            }
            else
                m_storedWindowState = WindowState;
        }
        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        void NotifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = m_storedWindowState;
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (notifyIcon != null)
                notifyIcon.Visible = show;
        }
        void OnClose(object sender, CancelEventArgs args)
        {
            notifyIcon.Dispose();
            notifyIcon = null;
        }

        private void SetComboBoxValue()
        {
            switch (Properties.Settings.Default.Separator)
            {
                case "Enter":
                    comboBox.SelectedIndex = 0;
                    break;

                case "Space":
                    comboBox.SelectedIndex = 1;
                    break;

                case ", ":
                    comboBox.SelectedIndex = 2;
                    break;

                case ". ":
                    comboBox.SelectedIndex = 3;
                    break;
            }
        }
        private void OpenTelegram(object sender, RoutedEventArgs e)
        {
            Process.Start("https://t.me/CoderWorker");
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitializing)
            {
                Properties.Settings.Default.Separator = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
                Properties.Settings.Default.Save();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
