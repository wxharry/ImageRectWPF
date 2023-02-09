using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageRectWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyCanvas.Visibility= Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to select the image
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (openFileDialog.ShowDialog() == true)
            {
                // Load the selected image
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(openFileDialog.FileName);
                image.EndInit();

                // Create an image control to display the image
                Image imageControl = new Image();
                imageControl.Source = image;

                // var toolbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight;
                imageControl.MaxHeight = this.ActualHeight - 35; // the hight is off a little; guessing causing by the topbar
                imageControl.MaxWidth = this.ActualWidth;
                Grid1.Children.Remove(myBtn);
                MyCanvas.Children.Add(imageControl);
                myBtn.Visibility = Visibility.Collapsed;
                MyCanvas.Visibility = Visibility.Visible;
            }
        }
    }
}
