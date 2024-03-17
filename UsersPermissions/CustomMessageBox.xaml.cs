using System.Windows;
using System.Windows.Input;

namespace UserPermissions
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox()
        {
            InitializeComponent();
        }

        private void CloseButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CustomMessageBoxWindows.Close();
        }

        public static void Show(string message, string caption)
        {
            CustomMessageBox messageBox = new CustomMessageBox();

            messageBox.Message.Text = message;
            messageBox.Caption.Content = caption;
            messageBox.ShowDialog();

        }

    }
}
