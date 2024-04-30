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
            if (Properties.Settings.Default.Autoran)
            {
                
            }
            SetComboBoxValue();
            isInitializing = false;
            notifyIcon = new NotifyIcon();
            notifyIcon.BalloonTipText = "Программа свернута в трей, вы можете продолжать ей пользоваться :)";
            notifyIcon.BalloonTipTitle = "Go To tray";
            notifyIcon.Text = "CopyMerge";
            string iconPath = "icon.ico";
            notifyIcon.Icon = new System.Drawing.Icon(System.AppDomain.CurrentDomain.BaseDirectory + iconPath);
            notifyIcon.Click += new EventHandler(NotifyIcon_Click);
            KeyLogger.KeyBufferClear();
            KeyLogger.KeyChecker();
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
            switch (Properties.Settings.Default.Autoran)
            {
                case true:
                    comboBoxAutoran.SelectedIndex = 0;
                    break;

                case false:
                    comboBoxAutoran.SelectedIndex = 1;
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

        private void comboBoxAutoran_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitializing)
            {
                switch (((ComboBoxItem)comboBoxAutoran.SelectedItem).Content.ToString())
                {
                    case "Выключено":
                        Properties.Settings.Default.Autoran = false;
                        SetAutoLaunch(false, Assembly.GetEntryAssembly().Location);
                        break;

                    case "Включено":
                        Properties.Settings.Default.Autoran = true;
                        SetAutoLaunch(true, Assembly.GetEntryAssembly().Location);
                        break;
                }
                Properties.Settings.Default.Save();
            }
        }
    }
}
