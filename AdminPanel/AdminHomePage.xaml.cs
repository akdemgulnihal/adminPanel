using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System;
using WpfApp1;
using System.Linq;

namespace AdminPanel
{
    /// <summary>
    /// Interaction logic for AdminHomePage.xaml
    /// </summary>
    public partial class AdminHomePage : Page
    {

        string firebaseUrl = FirebaseConfig.FirebaseUrl; // Firebase URL,
        private string _username;
        public AdminHomePage(string username)
        {
            InitializeComponent();
            _username = username;
            LoadUserData();

        }

        private void NewUser_Click(object sender, RoutedEventArgs e)
        {
            // Yeni pencereyi açıyoruz
            //NewUserWindow newUserWindow = new NewUserWindow();
            //newUserWindow.Show();  // Pencereyi göster


            var newUserWindow = new NewUserWindow();
            NavigationService.Navigate(newUserWindow);


            newUserWindow.UserAdded += (s, args) =>
            {
                LoadUserData();  // Kullanıcı eklendikten sonra veri yeniden yüklenecek
            }; 
        }

        private async void LoadUserData()
        {
            try
            {
                var firebase = new FirebaseClient(firebaseUrl);
                var userRef = firebase.Child("StandartUserTable");

                //  StandartUserTable'daki userların hepsini alıyot
                var users = await userRef.OnceAsync<UsersData>();

                // DataGrid'de gösterilmek icin Liste olusturuldu
                List<UsersData> userList = users.Select(u => new UsersData
                {
                    Username = u.Object.Username,
                    Name = u.Object.Name,
                    Surname = u.Object.Surname,
                    //Age = u.Object.Age,
                    LoginStatus = u.Object.LoginStatus,
                    Password = u.Object.Password
                }).ToList();

                // Bind the user list to the DataGrid
                userDataGrid.ItemsSource = userList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri alınırken bir hata oluştu: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Edit Button Click
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Username alınıyor (from the button's Tag property)
            var username = (sender as Button).Tag.ToString();

            // Edit ekranı aciliyor ve düzenleme yapılabiliyor
            MessageBox.Show($"Edit user: {username}");


            // Yeni pencereyi açıyoruz
            //EditWindow editUserWindow = new EditWindow(username);
            //editUserWindow.Show();  // Pencereyi göster


            var editUserWindow = new EditWindow(username);
            NavigationService.Navigate(editUserWindow);


            editUserWindow.UserEdit += (s, args) =>
            {
                LoadUserData();  // Kullanıcı eklendikten sonra veri yeniden yüklenecek
            };
        }


        // Remove Button Click
        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Username alınıyor (from the button's Tag property)
            var username = (sender as Button).Tag.ToString();

            // Confirm deletion
            var result = MessageBox.Show("Kullanıcıyı silmek üzeresiniz. İşlemden emin misiniz?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {

                        string url = firebaseUrl + "/StandartUserTable.json";
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseData = await response.Content.ReadAsStringAsync();

                            // Deserialize the JSON data
                            var usersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                            //Kullaniciyi username'ine göre buluyoruz
                            foreach (var item in usersData)
                            {
                                if (item.Value.Username == username)
                                {
                                    // Bulunan kullaniciyi ve detayları ile Firebase'den siliyoruz
                                    var firebase = new FirebaseClient(firebaseUrl);
                                    var userRef = firebase.Child("StandartUserTable").Child(item.Key); // Reference to the specific user
                                    await userRef.DeleteAsync();  // Silme işlemini yapan fonksiyon

                                    // Silindigine dair mesaj ekrana bastiriliyor
                                    MessageBox.Show("Kullanıcı sistemden silindi.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                    break;  //Kullanıcı bulunuca loop'tan cikiyor ve siliniyor
                                }
                            }
                        }




                        // Silme isleminden sonra sayfa tekrar load ediliyor
                        LoadUserData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kullanıcıyı silmeye çalışırken hata ile karşılaşıldı. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        /****
         *  Bu fonksiyonun amacı sayfanın taşınmasını sağlamak
         * 
         *  MouseButtonEventArgs e   fare ile ilgili bilgileri içeren bir nesnedir.
         *  object sender            hangi öğede gerçekleştiğini belirtir. (bu durumda Border öğesi)
         */
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Farenin sol tuşuna basıldığında - When the left mouse button was pressed
            if (e.ChangedButton == MouseButton.Left)
            {
                // Move the page content (drag the page when the left mouse button is pressed and dragged)
                Window parentWindow = Window.GetWindow(this);  // Get the parent window
                if (parentWindow != null)
                {
                    parentWindow.DragMove();
                }
            }
        }

        private bool isMaximized = false;

        /**
         * 
         * MouseButtonEventArgs e   fare ile ilgili bilgileri içeren bir nesnedir.
         * object sender            hangi öğede gerçekleştiğini belirtir. (bu durumda Border öğesi)
         */
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Window parentWindow = Window.GetWindow(this);  // Get the parent window
                if (parentWindow != null)
                {
                    if (isMaximized)
                    {
                        parentWindow.WindowState = WindowState.Normal;
                        parentWindow.Width = 1080;
                        parentWindow.Height = 720;

                        isMaximized = false;
                    }
                    else
                    {
                        parentWindow.WindowState = WindowState.Maximized;
                        isMaximized = true;
                    }
                }
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            //string loggedInUsername = _username;  // Replace with actual logged-in username, you may already have this stored.

            try
            {
                using (HttpClient client = new HttpClient())
                {

                    string url = firebaseUrl + "/AdminTable.json";

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        // Deserialize the JSON data
                        var usersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                        // Find the user by Username
                        foreach (var item in usersData)
                        {
                            if (item.Value.Username == _username && item.Value.LoginStatus == "Active")
                            {
                                // Set LoginStatus to Passive
                                var firebase = new FirebaseClient(firebaseUrl);
                                var userRef = firebase.Child("AdminTable").Child(item.Key); // Use the appropriate table (AdminTable or StandartUserTable)

                                //Login status'un Passive olarak guncellenmesi
                                var dataToSend = new Dictionary<string, object>
                        {
                                    { "Username", item.Value.Username },
                                    { "Password", item.Value.Password },
                                    { "LoginStatus", "Passive" }
                                };


                                await userRef.PutAsync(dataToSend);

                                break;
                            }
                        }

                        //Admin Login Page'e yonlendirilme yapiliyor
                        var loginPage = new AdminLoginPage();
                        NavigationService.Navigate(loginPage);
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
    }
}
