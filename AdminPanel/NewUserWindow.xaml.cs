using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{


    public partial class NewUserWindow : Page
    {
        // Define the UserAdded event
        public event EventHandler UserAdded;

       private bool flag = false;
        //private bool isDataSaved = false;

        public NewUserWindow()
        {
            InitializeComponent();
        }

        // Call this method after successfully adding a new user
        private void OnUserAdded()

        {
            // Trigger the event
            UserAdded?.Invoke(this, EventArgs.Empty);
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Create a new User object with input data
            var newUser = new UsersData
            {
                Name = NameTextBox.Text,
                Surname = SurnameTextBox.Text,
                Age = Age.Text,
                Email = EmailTextBox.Text,
                Birthday = Birthday.Text,
                University = University.Text,
                Experience = Experience.Text,
                Username = userid.Text,
                Password = PasswordBox.Password,
                LoginStatus = "Passive"
            };

            try
            {
                var firebase = new FirebaseClient(FirebaseService.FirebaseUrl);
                var userRef = firebase.Child("StandartUserTable");
                // Firebase'den AdminTable verisini al
                string url = FirebaseService.FirebaseUrl + "StandartUserTable.json";
                var response = await FirebaseService.Client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    // JSON verisini deserialize et
                    var UsersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                    // AdminTable'daki tüm kullanıcıları kontrol et
                    foreach (var item in UsersData)
                    {
                        if (item.Value.Username == userid.Text)
                        {
                            MessageBox.Show("Sistemde bu username'e sahip birisi bulunmaktadır.");
                            flag = true;
                        }
                    }
                }

                if (!flag)
                {
                    var result = await userRef.PostAsync(newUser);
                    MessageBox.Show("Yeni kullanıcı başarılı şekilde veritabanına yüklendi.");
                    //isDataSaved = true;
                }
            }
            catch (Exception ex)
            {
                // Hata mesaji
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnUserAdded(); // Trigger the UserAdded event

            // Optionally, navigate back to the previous page after saving the user
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
