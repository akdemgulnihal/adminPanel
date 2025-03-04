using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Data;
using AdminPanel;

namespace AdminPanel
{
    public class IsGreaterThanConverter : IValueConverter
    {
        public static IsGreaterThanConverter Instance { get; } = new IsGreaterThanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                if (parameter != null && double.TryParse(parameter.ToString(), out double threshold))
                {
                    return width > threshold;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Interaction logic for UserLogin.xaml
    /// </summary>
    public partial class UserLoginPage : Window
    {
        public UserLoginPage()
        {
            InitializeComponent();
        }

        //Kullanicinin, kullanici adini giris yaptıgı alan tiklandiginda metin siliniyor
        private void NameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginService.ClearTextOnFocus(sender, e, "örnek_kullanici");
        }

        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LoginService.RestoreTextOnLostFocus(sender, e, "örnek_kullanici");
        }

        //Kullanicinin, sifreyi giris yaptıgı alan tiklandiginda metin siliniyor
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginService.ClearTextOnFocus(sender, e, "ksifre123");
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LoginService.RestoreTextOnLostFocus(sender, e, "ksifre123");
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string enteredUsername = NameTextBox.Text; // TextBox'tan alınan kullanıcı adı
            string enteredPassword = PasswordBox.Password; // PasswordBox'tan alınan şifre

            bool loginSuccess = await LoginService.LoginAsync(enteredUsername, enteredPassword, "StandartUserTable");

            if (loginSuccess)
            {
                UserHomePage homePage = new UserHomePage(enteredUsername);
                homePage.Show(); // Show UserHomePage as a new window
                this.Close(); // Close the login window
            }
            else
            {
                MessageBox.Show("Geçersiz kullanıcı adı veya şifre", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SwitchBtn_Click(object sender, RoutedEventArgs e)
        {
            AdminLoginPage adminLoginPage = new AdminLoginPage();
            adminLoginPage.Show(); // Open the AdminLoginPage as a new window
            this.Close(); // Close the current login window
        }

    }


}
