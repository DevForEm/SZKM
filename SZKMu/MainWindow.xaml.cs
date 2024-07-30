using System.Windows;

namespace SZKMu;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenValidationWindow(object sender, RoutedEventArgs e)
    {
        ValidationWindow validationWindow = new ValidationWindow();
        validationWindow.Show();
    }
}