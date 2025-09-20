using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Input;
using CopyMerge.Services;
using CopyMerge.ViewModel;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace CopyMerge.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private WindowState _storedWindowState = WindowState.Normal;
        private bool _isInitializing = true;
        
        public MainWindow()
        {
            DataContext = MainViewModel.Instance;
            InitializeComponent();
            InitializeNotifyIcon();
            
            InitializeAutorun();
            
            SetComboBoxValue();
            KeyLogger.KeyBufferClear();
            KeyLogger.KeyChecker();
        }
        
        #region Initialization
        
        private void InitializeNotifyIcon()
        {
            _isInitializing = false;
            _notifyIcon = new NotifyIcon();
            
            _notifyIcon.Text = "CopyMerge";
            
            string iconPath = $"{AppDomain.CurrentDomain.BaseDirectory}icon.ico";
            _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
            
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Открыть CopyMerge", null, (s, e) => RestoreWindow());
            contextMenu.Items.Add("-"); 
            contextMenu.Items.Add("Выход", null, (s, e) => Application.Current.Shutdown());
            _notifyIcon.ContextMenuStrip = contextMenu;
            
            _notifyIcon.DoubleClick += (s, e) => RestoreWindow();
        }

        private void InitializeAutorun()
        {
            try
            {
                var syncResult = AutorunManager.SynchronizeWithRegistry(Properties.Settings.Default.Autoran);
                
                if (syncResult.Success)
                {
                    
                    if (syncResult.SynchronizationNeeded)
                    {
                        Properties.Settings.Default.Autoran = syncResult.CurrentState;
                        Properties.Settings.Default.Save();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        
        #endregion

        #region Settings Management
        
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
            
            bool actualAutorunState = AutorunManager.IsAutorunEnabled();
            comboBoxAutoran.SelectedIndex = actualAutorunState ? 0 : 1;
            
            if (Properties.Settings.Default.Autoran != actualAutorunState)
            {
                Properties.Settings.Default.Autoran = actualAutorunState;
                Properties.Settings.Default.Save();
            }
        }
        
        #endregion

        #region Event Handlers

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitializing)
            {
                Properties.Settings.Default.Separator = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
                Properties.Settings.Default.Save();
            }
        }

        private void ComboBoxAutoran_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitializing && comboBoxAutoran.SelectedItem != null)
            {
                string selectedContent = ((ComboBoxItem)comboBoxAutoran.SelectedItem).Content.ToString();
                bool newAutorunState = selectedContent.Contains("Включено");
                
                if (Properties.Settings.Default.Autoran != newAutorunState)
                {
                    var result = AutorunManager.SetAutorun(newAutorunState);
                    
                    if (result.Success)
                    {
                        Properties.Settings.Default.Autoran = newAutorunState;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        comboBoxAutoran.SelectedIndex = Properties.Settings.Default.Autoran ? 0 : 1;
                        
                        string errorTitle = result.RequiresElevation ? "Недостаточно прав" : "Ошибка";
                        MessageBox.Show(result.ErrorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        
        #endregion

        #region Tray Icon Management

        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            else
            {
                _storedWindowState = WindowState;
            }
        }
        
        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        private void RestoreWindow()
        {
            Show();
            WindowState = _storedWindowState;
            Activate(); 
            Focus();   
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (_notifyIcon != null)
                _notifyIcon.Visible = show;
        }
        
        void OnClose(object sender, CancelEventArgs args)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false; 
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }
        
        #endregion

        #region Button Handlers
        
        private void OpenTelegram(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://t.me/CoderW0rker");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при открытии ссылки: {ex.Message}");
                MessageBox.Show("Не удалось открыть ссылку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearClipboardHistory_Click(object sender, RoutedEventArgs r)
        {
            MainViewModel.Instance.ClearClipboardStore();
            var notificationWindow = new NotificationWindow("История буфера обмена очищена")
            {
                Owner = this
            };
            notificationWindow.ShowDialog();
        }
        
        private void CopySelectedNote_Click(object sender, RoutedEventArgs r)
        {
            if (ClipboardHistoryCombo.SelectedIndex >= 0 && 
                ClipboardHistoryCombo.SelectedIndex < MainViewModel.Instance.ClipboardStore.Count)
            {
                Clipboard.SetText(MainViewModel.Instance.ClipboardStore[ClipboardHistoryCombo.SelectedIndex]);
                MainViewModel.Instance.AddToClipboardStore(Clipboard.GetText());
            }
        }
        
        private void ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow() { Owner = this};
            helpWindow.ShowDialog();
        }

        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow() { Owner = this };
            aboutWindow.ShowDialog();
        }
        
        #endregion

        #region Window Management
        
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeRestoreWindow();
            }
            else
            {
                DragMove();
            }
        }
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeRestoreWindow();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaximizeRestoreWindow()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                if (MaximizeButton != null)
                    MaximizeButton.Content = "\uE922"; 
            }
            else
            {
                WindowState = WindowState.Maximized;
                if (MaximizeButton != null)
                    MaximizeButton.Content = "\uE923"; 
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
         
            if (MaximizeButton != null)
            {
                if (WindowState == WindowState.Maximized)
                {
                    MaximizeButton.Content = "\uE923"; 
                }
                else
                {
                    MaximizeButton.Content = "\uE922"; 
                }
            }
        }
        
        #endregion
    }
}