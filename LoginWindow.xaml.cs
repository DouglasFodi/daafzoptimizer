using System.Windows;
using System.Windows.Input;
using RegOptimizer.Services;

namespace RegOptimizer;

public partial class LoginWindow : Window
{
    private bool _busy;

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        PasswordInput.Focus();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        await TryLoginAsync();
    }

    private async void PasswordInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await TryLoginAsync();
        }
    }

    private async Task TryLoginAsync()
    {
        if (_busy) return;

        var password = PasswordInput.Password;
        if (string.IsNullOrWhiteSpace(password))
        {
            StatusText.Text = "Informe a senha.";
            return;
        }

        _busy = true;
        LoginButton.IsEnabled = false;
        PasswordInput.IsEnabled = false;
        StatusText.Foreground = System.Windows.Media.Brushes.LightGray;
        StatusText.Text = "Validando acesso...";

        try
        {
            var result = await AuthService.AuthenticateAsync(password);
            PasswordInput.Clear();

            if (result.Ok)
            {
                DialogResult = true;
                Close();
                return;
            }

            StatusText.Foreground = System.Windows.Media.Brushes.LightCoral;
            StatusText.Text = string.IsNullOrWhiteSpace(result.Message) ? "Acesso recusado." : result.Message;
        }
        finally
        {
            _busy = false;
            LoginButton.IsEnabled = true;
            PasswordInput.IsEnabled = true;
            PasswordInput.Focus();
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
