using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using AdminPanel;
using Newtonsoft.Json;
using Firebase.Database;

namespace AdminPanel
{
    public partial class AdminLoginPage : Window
    {
        public AdminLoginPage()
        {
            InitializeComponent();
        }

        //Kullanicinin, kullanici adini giris yaptıgı alan tiklandiginda metin siliniyor
        private void NameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginService.ClearTextOnFocus(sender, e, "örnek_kullanici"); // Admin default username
        }

        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LoginService.RestoreTextOnLostFocus(sender, e, "örnek_kullanici");
        }

        //Kullanicinin, sifreyi giris yaptıgı alan tiklandiginda metin siliniyor
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginService.ClearTextOnFocus(sender, e, "sifre123"); // Admin default password
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LoginService.RestoreTextOnLostFocus(sender, e, "sifre123"); // Admin default password
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string enteredUsername = NameTextBox.Text; // TextBox'tan alınan kullanıcı adı
            string enteredPassword = PasswordBox.Password; // PasswordBox'tan alınan şifre

            bool loginSuccess = await LoginService.LoginAsync(enteredUsername, enteredPassword, "AdminTable");

            if (loginSuccess)
            {
                // Open AdminHomePage as a new Window
                AdminHomePage homePage = new AdminHomePage(enteredUsername);
                homePage.Show();
                this.Close(); // Close the login window
            }
            else
            {
                MessageBox.Show("Geçersiz kullanıcı adı veya şifre", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SwitchBtn_Click(object sender, RoutedEventArgs e)
        {
            // Switch to User Login Window
            UserLoginPage userLoginPage = new UserLoginPage();
            userLoginPage.Show();
            this.Close(); // Close the current login window
        }
    }
}
