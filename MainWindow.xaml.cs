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
using Point = System.Windows.Point;

namespace ImageRectWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private Rectangle rectangle;
        private Rectangle selectedRectangle;
        private double rectLeft;
        private double rectTop;
        private double rectWidth;
        private double rectHeight;
        private bool isResizing;
        private bool isMoving;
        private int maxZindex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
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
                imageControl.MaxHeight = this.ActualHeight - 35; // the hight is off a little; guessing causing by the topbar
                imageControl.MaxWidth = this.ActualWidth;
                Grid1.Children.Remove(myBtn);                    // remove button for useless
                MyCanvas.Children.Add(imageControl);
            }
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("click on " + e.Source);
            selectedRectangle = null;
            if (e.Source is Rectangle)
            {
                selectedRectangle = (Rectangle)e.Source;
            }
            if (selectedRectangle != null)
            {
                Canvas.SetZIndex(selectedRectangle, maxZindex+1);
                Point clickedPoint = e.GetPosition(this);
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
                    // Start resizing the selected rectangle
                    startPoint = clickedPoint;

                    // Change the cursor to indicate resizing
                    this.Cursor = Cursors.SizeNWSE;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Moving");
                    // moving
                    startPoint = e.GetPosition(this);
                    this.Cursor = Cursors.SizeAll;
                    isMoving = true;
                }

            }
            else
            {
                // Record the starting point of the rectangle
                startPoint = e.GetPosition(this);

                // Create the rectangle
                rectangle = new Rectangle();
                rectangle.Stroke = System.Windows.Media.Brushes.Black;
                rectangle.Fill = System.Windows.Media.Brushes.SkyBlue;
                rectangle.StrokeThickness = 1;

                // Add the rectangle to the canvas
                MyCanvas.Children.Add(rectangle);
            }

        }

        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
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
                }
                // draw a rectangle
                else if (rectangle != null)
                {
                    // Update the size and position of the rectangle
                    rectangle.Width = Math.Abs(currentPoint.X - startPoint.X);
                    rectangle.Height = Math.Abs(currentPoint.Y - startPoint.Y);
                    Canvas.SetLeft(rectangle, Math.Min(startPoint.X, currentPoint.X));
                    Canvas.SetTop(rectangle, Math.Min(startPoint.Y, currentPoint.Y));
                }
            }
        }

        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Stop drawing the rectangle
            rectangle = null;

            this.Cursor = Cursors.Arrow;
            if (selectedRectangle != null)
            {
                isMoving = false;
                isResizing = false;
                Canvas.SetZIndex(selectedRectangle, maxZindex + 1);
                maxZindex++;
            }

        }
    }
}
