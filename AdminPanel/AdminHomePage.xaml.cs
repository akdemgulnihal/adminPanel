using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Windows.Threading;

namespace AdminPanel
{
    public partial class AdminHomePage : Window
    {
        private DispatcherTimer _refreshTimer;
        private string _username;

        public AdminHomePage(string username)
        {
            InitializeComponent();
            _username = username;
            LoadUserData();
            StartRefreshTimer();
        }

        private void StartRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer();
            HomePageHelper.StartRefreshTimer(_refreshTimer, RefreshTimer_Tick, TimeSpan.FromSeconds(5));
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadUserData();
        }

        private void NewUser_Click(object sender, RoutedEventArgs e)
        {
            var newUserWindow = new NewUserWindow(_username);
            newUserWindow.Show();

            newUserWindow.UserAdded += (s, args) =>
            {
                LoadUserData();
            };
        }

        private async void LoadUserData()
        {
            var userList = await HomePageHelper.LoadUserDataAsycn("StandartUserTable");

            if (userList != null)
            {
                userDataGrid.ItemsSource = userList;
            }
            else
            {
                MessageBox.Show("Veri alınırken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var username = (sender as Button).Tag.ToString();
            var editUserWindow = new EditWindow(username);
            editUserWindow.Show();

            editUserWindow.UserEdit += (s, args) =>
            {
                LoadUserData();
            };
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var username = (sender as Button).Tag.ToString();
            var result = MessageBox.Show("Kullanıcıyı silmek üzeresiniz. İşlemden emin misiniz?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string url = FirebaseService.FirebaseUrl + "/StandartUserTable.json";
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseData = await response.Content.ReadAsStringAsync();
                            var usersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                            foreach (var item in usersData)
                            {
                                if (item.Value.Username == username)
                                {
                                    var firebase = new FirebaseClient(FirebaseService.FirebaseUrl);
                                    var userRef = firebase.Child("StandartUserTable").Child(item.Key);
                                    await userRef.DeleteAsync();
                                    MessageBox.Show("Kullanıcı sistemden silindi.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                    break;
                                }
                            }
                        }

                        LoadUserData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kullanıcıyı silmeye çalışırken hata ile karşılaşıldı. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HomePageHelper.HandleWindowDrag(sender, e);
        }

        private bool isMaximized = false;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HomePageHelper.HandleWindowDragAndMaximize(sender, e, ref isMaximized);
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = FirebaseService.FirebaseUrl + "/AdminTable.json";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        var usersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                        foreach (var item in usersData)
                        {
                            if (item.Value.Username == _username && item.Value.LoginStatus == "Active")
                            {
                                var firebase = new FirebaseClient(FirebaseService.FirebaseUrl);
                                var userRef = firebase.Child("AdminTable").Child(item.Key);

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

                        var loginPage = new AdminLoginPage();
                        loginPage.Show();
                        this.Close(); // Close current window
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
