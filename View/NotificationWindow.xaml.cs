using System.Windows;
using System.Windows.Input;

namespace CopyMerge.View
{
    public partial class NotificationWindow : Window
    {
        public NotificationWindow(string message = "История буфера обмена очищена")
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}