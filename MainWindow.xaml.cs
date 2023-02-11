using System;
using System.Collections.Generic;
using System.IO;
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
            // to enable previewKeyDown
            MyCanvas.Focus();
        }

        private void Upload_Image(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to select the image
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (openFileDialog.ShowDialog() == true)
            {
                // TBD: check if there exits an image
                // clear all for now;
                MyCanvas.Children.Clear();
                // Load the selected image
                BitmapImage image = new();
                image.BeginInit();
                image.UriSource = new Uri(openFileDialog.FileName);
                image.EndInit();

                ImageBrush brush = new(image);
                MyCanvas.Background = brush;
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
            // get the size and position of the selectedRectangle
            rectLeft = Canvas.GetLeft(selectedRectangle);
            rectTop = Canvas.GetTop(selectedRectangle);
            rectWidth = selectedRectangle.Width;
            rectHeight = selectedRectangle.Height;
            MyCanvas.CaptureMouse();
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
            Point clickedPoint = e.GetPosition(MyCanvas);
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
                Point currentPoint = e.GetPosition(MyCanvas);
                if (selectedRectangle != null)
                {
                    // resize a rectangle
                    // resize on right bottom corner
                    if (isResizing)
                    {
                        // Update the size and position of the selected rectangle
                        double x = Math.Min(currentPoint.X, rectLeft);
                        double y = Math.Min(currentPoint.Y, rectTop);
                        double width = Math.Max(currentPoint.X, rectLeft) - x;
                        double height = Math.Max(currentPoint.Y, rectTop) - y;
                        selectedRectangle.Width = Math.Min(width, MyCanvas.ActualWidth - x);
                        selectedRectangle.Height = Math.Min(height, MyCanvas.ActualHeight - y);
                        Canvas.SetLeft(selectedRectangle, x);
                        Canvas.SetTop(selectedRectangle, y);
                    }
                    // move a rectangle
                    if (isMoving)
                    {
                        System.Diagnostics.Debug.WriteLine("ismoving");
                        // keep updating the position of the rectangle
                        double left = Canvas.GetLeft(selectedRectangle);
                        double top = Canvas.GetTop(selectedRectangle);
                        double x = Math.Max(0, Math.Min(currentPoint.X - startPoint.X + left, MyCanvas.ActualWidth - selectedRectangle.Width));
                        double y = Math.Max(0, Math.Min(currentPoint.Y - startPoint.Y + top, MyCanvas.ActualHeight - selectedRectangle.Height));
                        Canvas.SetLeft(selectedRectangle, x);
                        Canvas.SetTop(selectedRectangle, y);
                        startPoint = currentPoint;
                    }
                    // draw a rectangle
                    if (isDrawing)
                    {
                        double x = Math.Min(currentPoint.X, startPoint.X);
                        double y = Math.Min(currentPoint.Y, startPoint.Y);
                        double width = Math.Max(currentPoint.X, startPoint.X) - x;
                        double height = Math.Max(currentPoint.Y, startPoint.Y) - y;
                        selectedRectangle.Width = Math.Min(width, MyCanvas.ActualWidth - x);
                        selectedRectangle.Height = Math.Min(height, MyCanvas.ActualHeight - y);
                        Canvas.SetLeft(selectedRectangle, x);
                        Canvas.SetTop(selectedRectangle, y);
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
                MyCanvas.ReleaseMouseCapture();
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
        private void Delete_SelectedRectangle()
        {
            MyCanvas.Children.Remove(selectedRectangle);
            selectedRectangle = null;
        }
        private void MyCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Press key", e.Key.ToString());
            if (selectedRectangle != null && (e.Key == Key.Delete || e.Key == Key.Back))
            {
                Delete_SelectedRectangle();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            Delete_SelectedRectangle();
        }
        private void SaveImage()
        {
            // Open a file dialog to select the image
            Microsoft.Win32.SaveFileDialog saveFileDialog = new();
            saveFileDialog.Title = "Save picture as ";
            saveFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                RenderTargetBitmap renderTargetBitmap = new((int)MyCanvas.ActualWidth, (int)MyCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(MyCanvas);

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }

        }
        private void Save_Image(object sender, RoutedEventArgs e)
        {
            // check if canvas has no child
            if (MyCanvas.Children.Count == 0) return;
            Unselect_Rectangle(selectedRectangle);
            SaveImage();
        }
    }
}
