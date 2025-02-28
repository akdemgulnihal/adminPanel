using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AdminPanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /****
         *  Bu fonksiyonun amacı pencerenin taşınmasını sağlamak
         * 
         *  MouseButtonEventArgs e   fare ile ilgili bilgileri içeren bir nesnedir.
         *  object sender            hangi öğede gerçekleştiğini belirtir. (bu durumda Border öğesi)
         */
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Farenin sol tuşuna basıldığında - When the left mouse button was pressed
            if(e.ChangedButton == MouseButton.Left)
            {
                // Move the window when the left mouse button is pressed and dragged
                this.DragMove();
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
                if (isMaximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;

                    isMaximized = false;

                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    isMaximized = true;
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
