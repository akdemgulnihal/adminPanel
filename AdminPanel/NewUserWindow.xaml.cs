using AdminPanel;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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


        /**
         * Bu metodun işlevi, bir kullanıcı başarılı şekilde eklendikten sonra UserAdded adında bir olay(event) tetiklemektir
         * UserAdded --- event
         * ? (null condition operator)
         * Invoke metodu UserAdded olayını tetikler ve bu olayın herhangi bir dinleyicisini(event handler) çalıştırır
         * this olayın tetiklenmesinde sender olarak this yani mevcut sınıfın örneği(instance) kullanılır.
         *
         *EventArgs.Empty Olay için herhangi bir ek veri gönderilmiyor, sadece olayın tetiklendiği bildirilmek isteniyor. 
         * Bu nedenle EventArgs.Empty kullanılır. 
         */

        private void OnUserAdded()

        {
            UserAdded?.Invoke(this, EventArgs.Empty);
        }


        /**
         * Save Butonu basıldığında bu fonksiyon çalışıyor
         * newUser Yeni kullanıcı için girlen değerlerden obje oluşturluyor 
         * 
         * Sonra firebase ile ilgili gerekli bağlantılar yapiliyor/
         * response başarılı şekilde gerçekleşirse
         *
         */
        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Yeni kullanıcı için girlen değerlerden obje oluşturluyor 
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
                //FirebaseClient nesnesi oluşturuluyor. Bu nesne, Firebase veritabanına bağlanmanızı sağlar.
                var firebase = new FirebaseClient(FirebaseService.FirebaseUrl);

                // Firebase veritabanındaki "StandartUserTable" adlı veri tablosuna referans alınır.
                // Bu, kullanıcının verilerinin saklandığı yer
                var userRef = firebase.Child("StandartUserTable");

                // veriyi alabilmek için  gerekli URL oluşturuluyor.
                string url = FirebaseService.FirebaseUrl + "StandartUserTable.json";

                //await  GetAsync'nin sonucunu bekler. await işlem tamamlanana kadar diğer işlemler devam etmez
                var response = await FirebaseService.Client.GetAsync(url);

                //Eğer firebaseden gelen yanıt başarılı ise
                if (response.IsSuccessStatusCode)
                {
                    //Firebase'den geln yanıt json  formatında geliyor ve string değişkenine atanıyor
                    string responseData = await response.Content.ReadAsStringAsync();

                    // JsonConvert.DeserializeObject kullanılarak Dictionary<string,
                    // UsersData> formatında bir veri yapısına dönüştürülür. 
                    var UsersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                    // AdminTable'daki tüm kullanıcıları kontrol et
                    foreach (var item in UsersData)
                    {
                        //Username uyuşuyorsa
                        if (item.Value.Username == userid.Text)
                        {   
                            //mesaj bastırılır
                            MessageBox.Show("Sistemde bu username'e sahip birisi bulunmaktadır.");
                            flag = true;
                        }
                    }
                }

                // Bu username'de birisi yoksa
                if (!flag)
                {
                    // yeni kullanıcı sisteme eklenir ve ilgili mesaj bastırılır
                    var result = await userRef.PostAsync(newUser);
                    MessageBox.Show("Yeni kullanıcı başarılı şekilde veritabanına yüklendi.");
                    //isDataSaved = true;
                }
            }
            catch (Exception ex)
            {
                // Hata mesaji
                MessageBox.Show("Hata: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnUserAdded(); 

            // Bir önceki sayfaya dönülüyor
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            // Bir önceki sayfaya dönülüyor
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
