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
    /// Interaction logic for UserHomePage.xaml
    /// </summary>
    public partial class UserHomePage : Page
    {

        private int currentPage = 1;
        private int pageSize = 15;
        private int totalPages = 1;
        private List<UsersData> UserList;

        string firebaseUrl = FirebaseConfig.FirebaseUrl; // Firebase URL,
        private string _username;
        public UserHomePage(string username)
        {
            InitializeComponent();
            _username = username;
            LoadUserData();

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


                // Toplam sayfa sayısını hesaplıyor
                totalPages = (int)Math.Ceiling((double)UserList.Count / pageSize);

                // Get the users for the current page
                var pagedUsers = UserList.Skip((currentPage- 1) * pageSize).Take(pageSize).ToList();


                // Bind the user list to the DataGrid
                userDataGrid.ItemsSource = userList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri alınırken bir hata oluştu: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    string url = firebaseUrl + "/StandartUserTable.json";

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
                                var userRef = firebase.Child("StandartUserTable").Child(item.Key); // Use the appropriate table (AdminTable or StandartUserTable)

                                //Login status'un Passive olarak guncellenmesi
                                var dataToSend = new Dictionary<string, object>
                        {
                                    { "Username", item.Value.Username },
                                    { "Password", item.Value.Password },
                                    { "Name", item.Value.Name },
                                    { "Surname", item.Value.Surname },
                                    { "Age", item.Value.Age },
                                    { "Email", item.Value.Email },
                                    { "Birthday", item.Value.Birthday },
                                    { "University", item.Value.University },
                                    { "Experience", item.Value.Experience },
                                    { "LoginStatus", "Passive" }
                                };


                                await userRef.PutAsync(dataToSend);

                                break;
                            }
                        }

                        //Admin Login Page'e yonlendirilme yapiliyor
                        var loginPage = new UserLoginPage();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Username alınıyor (from the button's Tag property)
            // var username = (sender as Button).Tag.ToString();

            // Edit ekranı acılıyor ve düzenleme yapılabiliyor
            MessageBox.Show($"Edit user: {_username}");

            try
            {
                // Yeni pencereyi açıyoruz
                var editUserWindow = new UserProfileUpdate(_username);

                // Kullanıcıyı düzenlemek için pencereyi göster
                NavigationService.Navigate(editUserWindow);

                // Update successful, show a success message
                //MessageBox.Show("Updated successfully");

                // UserLoginPage'e dön
                // Assuming UserLoginPage is the previous page in your navigation history
                //NavigationService.GoBack();


                editUserWindow.UserEdit += (s, args) =>
                {
                    LoadUserData();  // Kullanıcı eklendikten sonra veri yeniden yüklenecek
                };


            }
            catch (Exception ex)
            {
                // Handle any errors here if needed
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

           
        }

    }
}
