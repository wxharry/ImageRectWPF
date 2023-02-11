using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Point = System.Windows.Point;

namespace ImageRectWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private Rectangle selectedRectangle;
        private double rectLeft;
        private double rectTop;
        private double rectWidth;
        private double rectHeight;
        private bool isResizing;
        private bool isMoving;
        private bool isDrawing;
        private int maxZindex = 0;
        private Brush defaultColor = System.Windows.Media.Brushes.White;

        public MainWindow()
        {
            InitializeComponent();
            ColorPicker.ItemsSource = typeof(Colors).GetProperties();
        }

        private void Upload_Image(object sender, RoutedEventArgs e)
        {
            // TBD: check if there exits an image
            // clear all for now;
            MyCanvas.Children.Clear();
            // Open a file dialog to select the image
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (openFileDialog.ShowDialog() == true)
            {
                // Load the selected image
                BitmapImage image = new();
                image.BeginInit();
                image.UriSource = new Uri(openFileDialog.FileName);
                image.EndInit();

                // Create an image control to display the image
                Image imageControl = new Image();
                imageControl.Source = image;

                // specify image maxheight and maxwidth
                // var toolbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight;
                imageControl.MaxHeight = this.ActualHeight - TopBar.ActualHeight - 40; // the height is off a little; guessing causing by the topbar
                imageControl.MaxWidth = this.ActualWidth;
                MyCanvas.Children.Add(imageControl);
            }
        }

        private void Select_Rectangle(Rectangle rect)
        {
            if (rect == null)
            {
                return;
            }
            selectedRectangle = rect;
            Canvas.SetZIndex(selectedRectangle, maxZindex + 1);
            selectedRectangle.Stroke = System.Windows.Media.Brushes.SkyBlue;
            selectedRectangle.StrokeThickness = 2;
        }
        private void Unselect_Rectangle(Rectangle rect)
        {
            if (rect == null)
            {
                return;
            }
            selectedRectangle.Stroke = System.Windows.Media.Brushes.LightGray;
            selectedRectangle.StrokeThickness = 1;
            selectedRectangle = null;
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickedPoint = e.GetPosition(this);
            clickedPoint.Y -= TopBar.ActualHeight;
            System.Diagnostics.Debug.WriteLine("click on " + e.Source);
            Rectangle clickedRectangle = null;
            if (e.Source is Rectangle)
            {
                clickedRectangle = (Rectangle)e.Source;
            }
            if (clickedRectangle != null)
            {
                Unselect_Rectangle(selectedRectangle);
                Select_Rectangle(clickedRectangle);
                // get the size and position of the selectedRectangle
                rectLeft = Canvas.GetLeft(selectedRectangle);
                rectTop = Canvas.GetTop(selectedRectangle);
                rectWidth = selectedRectangle.Width;
                rectHeight = selectedRectangle.Height;

                //Check if the user clicked on a resize handle
                if (Math.Abs(clickedPoint.Y - rectTop - rectHeight) <= 10 && Math.Abs(clickedPoint.X - rectLeft - rectWidth) <= 10)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing");

                    isResizing = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeNWSE;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Moving");
                    isMoving = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeAll;
                }

            }
            else
            {
                Unselect_Rectangle(selectedRectangle);
                // Record the starting point of the rectangle
                startPoint = clickedPoint;
                isDrawing = true;
                // Create the rectangle
                selectedRectangle = new Rectangle();
                Canvas.SetZIndex(selectedRectangle, maxZindex);
                selectedRectangle.Fill = defaultColor;
                selectedRectangle.Stroke = System.Windows.Media.Brushes.LightGray;
                Select_Rectangle(selectedRectangle);

                // Add the rectangle to the canvas
                MyCanvas.Children.Add(selectedRectangle);
            }

        }

        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
                currentPoint.Y -= TopBar.ActualHeight;
                // resize on right bottom corner
                if (selectedRectangle != null)
                {
                    // resize a rectangle
                    if (isResizing)
                    {
                        // Update the size and position of the selected rectangle
                        selectedRectangle.Width = Math.Abs(currentPoint.X - rectLeft);
                        selectedRectangle.Height = Math.Abs(currentPoint.Y - rectTop);
                        Canvas.SetLeft(selectedRectangle, Math.Min(rectLeft, currentPoint.X));
                        Canvas.SetTop(selectedRectangle, Math.Min(rectTop, currentPoint.Y));
                    }
                    // move a rectangle
                    if (isMoving)
                    {      
                        // keep updating the position of the rectangle
                        double left = Canvas.GetLeft(selectedRectangle);
                        double top = Canvas.GetTop(selectedRectangle);
                        Canvas.SetLeft(selectedRectangle, left + (currentPoint.X - startPoint.X));
                        Canvas.SetTop(selectedRectangle, top + (currentPoint.Y - startPoint.Y));
                        startPoint = currentPoint;
                    }
                    // draw a rectangle
                    if (isDrawing)
                    {
                        // Update the size and position of the rectangle
                        selectedRectangle.Width = Math.Abs(currentPoint.X - startPoint.X);
                        selectedRectangle.Height = Math.Abs(currentPoint.Y - startPoint.Y);
                        Canvas.SetLeft(selectedRectangle, Math.Min(startPoint.X, currentPoint.X));
                        Canvas.SetTop(selectedRectangle, Math.Min(startPoint.Y, currentPoint.Y));
                    }

                }
            }
        }

        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Stop drawing the rectangle
            this.Cursor = Cursors.Arrow;
            isMoving = false;
            isResizing = false;
            isDrawing = false;
            if (selectedRectangle != null)
            {
                Canvas.SetZIndex(selectedRectangle, maxZindex+1);
                ++maxZindex;
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // Change the color of the selected rectangle
            if (selectedRectangle != null)
            {
                Color selectedColor = (Color)(ColorPicker.SelectedItem as PropertyInfo).GetValue(null, null);
                selectedRectangle.Fill = new SolidColorBrush(selectedColor);
            }
        }
    }
}
