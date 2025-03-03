using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Newtonsoft.Json;
using Firebase.Database;
using Firebase.Database.Query;
using AdminPanel;


namespace WpfApp1
{
  

    public partial class AdminLoginPage : Page
    {

        public AdminLoginPage()
        {
            InitializeComponent();
        }


        //Kullanicinin, kullanici adini giris yaptıgı alan tiklandiginda metin siliniyor
        private void NameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Eğer TextBox'a tıklanmışsa ve metin 'örnek_kullanici' ise, metni temizle
            if (NameTextBox.Text != "")
            {
                NameTextBox.Text = "";
            }
        }

        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Eğer TextBox boşsa, tekrar varsayılan metni yerleştir
            if (string.IsNullOrEmpty(NameTextBox.Text))
            {
                NameTextBox.Text = "örnek_kullanici";
            }
        }

        //Kullanicinin, sifreyi giris yaptıgı alan tiklandiginda metin siliniyor
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Eğer Password'a tıklanmışsa ve metin 'sifre123' ise, metni temizle
            if (PasswordBox.Password != "")
            {
                PasswordBox.Password = "";
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Eğer PasswordBox boşsa, tekrar varsayılan metni yerleştir
            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                PasswordBox.Password = "sifre123";
            }
        }



        /*private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            //BURADA DEGISIKLIK YAPILACAK(DATABSE ILE BAGLANTI) 

            // Kullanıcı adı ve şifreyi al
            string enteredUsername = NameTextBox.Text; // TextBox'tan alınan kullanıcı adı
            string enteredPassword = PasswordBox.Password; // PasswordBox'tan alınan şifre



            // Giriş bilgilerini kontrol et
            if (enteredUsername == "admin" && enteredPassword == "admin")
            {
                // Giriş başarılıysa AdminHomePage sayfasına yönlendir
                NavigationService.Navigate(new AdminHomePage());
            }
            else
            {
                // Giriş bilgileri yanlışsa uyarı mesajı göster
                MessageBox.Show("Geçersiz kullanıcı adı veya şifre", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }*/


        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            // Kullanıcı adı ve şifreyi al
            string enteredUsername = NameTextBox.Text; // TextBox'tan alınan kullanıcı adı
            string enteredPassword = PasswordBox.Password; // PasswordBox'tan alınan şifre

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Firebase'den AdminTable verisini al
                    string url = FirebaseConfig.FirebaseUrl + "AdminTable.json";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        // JSON verisini deserialize et
                        var UsersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                        // AdminTable'daki tüm kullanıcıları kontrol et
                        bool loginSuccess = false;
                        foreach (var item in UsersData)
                        {
                            if (item.Value.Username == enteredUsername && item.Value.Password == enteredPassword && item.Value.LoginStatus == "Passive")
                            {
                                loginSuccess = true;

                                var firebase = new FirebaseClient(FirebaseConfig.FirebaseUrl);
                                var userRef = firebase.Child("AdminTable").Child(item.Key);


                                //Login status Active olarak guncelleniyor
                                var dataToSend = new Dictionary<string, object>
                                {
                                    {"Username" ,item.Value.Username },
                                     {"Password" ,item.Value.Password },
                                    { "LoginStatus", "Active" }
                                };

                                // 
                                await userRef.PutAsync(dataToSend);

                                break;
                            }
                            //Birden fazla cihazda LoginStatus Active mi olup olmadiginin kontrolu
                            else if (item.Value.Username == enteredUsername && item.Value.Password == enteredPassword && item.Value.LoginStatus == "Active")
                            {
                                MessageBox.Show("Birden fazla cihazda aynı anda oturum açmak mümkün değildir. Lütfen önce mevcut oturumu kapatın ve tekrar deneyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                        }

                        // Eger giris basar'l'ysa AdminHomePage'e yonlendirme yapiliyor
                        if (loginSuccess)
                        {
                            NavigationService.Navigate(new AdminHomePage(enteredUsername));
                        }
                        else
                        {
                            MessageBox.Show("Geçersiz kullanıcı adı veya şifre", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Veri alınırken hata oluştu: " + response.StatusCode, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void SwitchBtn_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the UserLoginPage
            NavigationService.Navigate(new UserLoginPage());
        }


        //private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        //{
        //    // Any initialization logic when the page is loaded (if needed).
        //}
        //
        //private void LoginButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // Validate username and password
        //    string username = UsernameTextBox.Text;
        //    string password = PasswordBox.Password;
        //
        //    if (IsValidUser(username, password))
        //    {
        //        // If login is successful, navigate to another page (e.g., ExpenseItHome)
        //        NavigationService.Navigate(new ExpenseReportPage());
        //    }
        //    else
        //    {
        //        // Show error message if login fails
        //        ErrorMessage.Text = "Invalid username or password. Please try again.";
        //        ErrorMessage.Visibility = Visibility.Visible;
        //    }
        //}
        //
        //private bool IsValidUser(string username, string password)
        //{
        //    // Example hardcoded username and password validation
        //    return username == "admin" && password == "admin";
        //}

    }


}
