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
    /// <summary>
    /// Interaction logic for UserLogin.xaml
    /// </summary>
    public partial class UserLoginPage : Page
    {

        public UserLoginPage()
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
            // Eger TextBox bossa(empty), tekrar varsayilan metni yerlestir
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
                PasswordBox.Password = "ksifre123";
            }
        }

        //private void LoginBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    //BURADA DEGISIKLIK YAPILACAK(DATABSE ILE BAGLANTI) 
        //
        //    // Kullanıcı adı ve şifreyi al
        //    string enteredUsername = NameTextBox.Text; // TextBox'tan alınan kullanıcı adı
        //    string enteredPassword = PasswordBox.Password; // PasswordBox'tan alınan şifre
        //
        //    // Giriş bilgilerini kontrol et
        //    if (enteredUsername == "kullanici" && enteredPassword == "kullanici")
        //    {
        //        // Giriş başarılıysa UserHomePage sayfasına yönlendir
        //        NavigationService.Navigate(new UserHomePage());
        //    }
        //    else
        //    {
        //        // Giriş bilgileri yanlışsa uyarı mesajı göster
        //        MessageBox.Show("Geçersiz kullanıcı adı veya şifre", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            // Kullanici adi ve sifreyi al
            string enteredUsername = NameTextBox.Text; // TextBox'tan alinan kullanici adi
            string enteredPassword = PasswordBox.Password; // PasswordBox'tan alinan sifre

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Firebase'den AdminTable verisini al
                    string url = FirebaseConfig.FirebaseUrl + "StandartUserTable.json";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        // JSON verisini deserialize et
                        var UsersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                        // AdminTable'daki tum kullanicilari kontrol et
                        bool loginSuccess = false;
                        foreach (var item in UsersData)
                        {
                            if (item.Value.Username == enteredUsername && item.Value.Password == enteredPassword && item.Value.LoginStatus == "Passive")
                            {
                                loginSuccess = true;

                                var firebase = new FirebaseClient(FirebaseConfig.FirebaseUrl);
                                var userRef = firebase.Child("StandartUserTable").Child(item.Key);

                                var dataToSend = new Dictionary<string, object>
                                {
                                    {"Username" ,item.Value.Username },
                                    {"Password" ,item.Value.Password },
                                    { "LoginStatus", "Active" },
                                    { "Name", item.Value.Name },
                                    { "Surname", item.Value.Surname },
                                    { "Age", item.Value.Age },
                                    { "Email", item.Value.Email },
                                    { "Birthday", item.Value.Birthday },
                                    { "University", item.Value.University },
                                    { "Experience", item.Value.Experience }
                                };


                                await userRef.PutAsync(dataToSend);



                                break;
                            }
                            else if (item.Value.Username == enteredUsername && item.Value.Password == enteredPassword && item.Value.LoginStatus == "Active")
                            {
                                MessageBox.Show("Birden fazla cihazda aynı anda oturum açmak mümkün değildir. Lütfen önce mevcut oturumu kapatın ve tekrar deneyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                        }

                        // Eger giris basariliysa AdminHomePage'e yonlendir
                        if (loginSuccess)
                        {

                            NavigationService.Navigate(new UserHomePage(enteredUsername));
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
            // Navigate to the AdminLoginPage
            NavigationService.Navigate(new AdminLoginPage());
        }

   
    }
}
