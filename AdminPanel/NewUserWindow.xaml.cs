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

        string firebaseUrl = FirebaseConfig.FirebaseUrl;
        HttpClient client = new HttpClient(); // Declare and initialize the HttpClient

        private bool flag = false;
        private bool isDataSaved = false;

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
            string name = NameTextBox.Text;
            string surname = SurnameTextBox.Text;
            string age = Age.Text;
            string email = EmailTextBox.Text;
            string birthday = Birthday.Text;
            string university = University.Text;
            string experience = Experience.Text;
            string username = userid.Text;
            string password = PasswordBox.Password;

            // Create a new User object with input data
            var newUser = new UsersData
            {
                Name = name,
                Surname = surname,
                Age = age,
                Email = email,
                Birthday = birthday,
                University = university,
                Experience = experience,
                Username = username,
                Password = password,
                LoginStatus = "Passive"
            };

            try
            {
                var firebase = new FirebaseClient(firebaseUrl);
                var userRef = firebase.Child("StandartUserTable");
                // Firebase'den AdminTable verisini al
                string url = firebaseUrl + "StandartUserTable.json";
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    // JSON verisini deserialize et
                    var UsersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                    // AdminTable'daki tüm kullanıcıları kontrol et
                    foreach (var item in UsersData)
                    {
                        if (item.Value.Username == username)
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
                    isDataSaved = true;
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
