using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private int tolerance = 10;
        private Brush defaultColor = System.Windows.Media.Brushes.White;
        private ResizeDirection resizeDirection = ResizeDirection.None;
        private enum ResizeDirection
        {
            None,
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left
        }


        public MainWindow()
        {
            InitializeComponent();
            ColorPicker.ItemsSource = typeof(Colors).GetProperties();
            // to enable previewKeyDown
            MyCanvas.Focus();
        }

        private void UpdateCanvasSize(double newWidth, double newHeight, double aspectRatio)
        {
            MyCanvas.MaxHeight = newHeight;
            MyCanvas.MaxWidth = newHeight * aspectRatio;

            MyCanvas.Width = Math.Min(MyCanvas.MaxWidth, newWidth);
            MyCanvas.Height = Math.Min(MyCanvas.MaxHeight, newHeight);
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
                selectedRectangle = null;
                rectTop = 0; rectWidth = 0;
                // Load the selected image
                BitmapImage image = new();
                image.BeginInit();
                image.UriSource = new Uri(openFileDialog.FileName);
                image.EndInit();
                double aspectRatio = image.Width / image.Height;
                UpdateCanvasSize(Application.Current.MainWindow.Width - 40, Application.Current.MainWindow.Height - 80, aspectRatio);
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
            rectLeft = 0;
            rectTop = 0;
            rectWidth = 0;
            rectHeight = 0;
            this.Cursor = Cursors.Arrow;
            if (rect == null)
            {
                return;
            }
            selectedRectangle.Stroke = System.Windows.Media.Brushes.LightGray;
            selectedRectangle.StrokeThickness = 1;
            selectedRectangle = null;
        }

        private ResizeDirection GetResizeDirection(Point point, double tolerance)
        {
            resizeDirection = ResizeDirection.None;
            if (rectLeft - point.X > tolerance || point.X - rectLeft - rectWidth > tolerance
             || rectTop  - point.Y > tolerance || point.Y - rectTop - rectHeight > tolerance)
            {
                return resizeDirection;
            }
            if (Math.Abs(point.Y - rectTop) <= tolerance && Math.Abs(point.X - rectLeft) <= tolerance)
            {
                resizeDirection = ResizeDirection.TopLeft;
            }
            else if (Math.Abs(point.Y - rectTop) <= tolerance && Math.Abs(point.X - rectLeft - rectWidth) <= tolerance)
            {
                resizeDirection = ResizeDirection.TopRight;
            }
            else if (Math.Abs(point.Y - rectTop - rectHeight) <= tolerance && Math.Abs(point.X - rectLeft - rectWidth) <= tolerance)
            {
                resizeDirection = ResizeDirection.BottomRight;
            }
            else if (Math.Abs(point.Y - rectTop - rectHeight) <= tolerance && Math.Abs(point.X - rectLeft) <= tolerance)
            {
                resizeDirection = ResizeDirection.BottomLeft;
            }
            else if (Math.Abs(point.X - rectLeft) <= tolerance)
            {
                resizeDirection = ResizeDirection.Left;
            }
            else if (Math.Abs(point.X - rectLeft - rectWidth) <= tolerance)
            {
                resizeDirection = ResizeDirection.Right;
            }
            else if (Math.Abs(point.Y - rectTop) <= tolerance)
            {
                resizeDirection = ResizeDirection.Top;
            }
            else if (Math.Abs(point.Y - rectTop - rectHeight) <= tolerance)
            {
                resizeDirection = ResizeDirection.Bottom;
            }
            return resizeDirection;
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
                GetResizeDirection(clickedPoint, tolerance);
                if (resizeDirection == ResizeDirection.TopLeft)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing TopLeft");
                    isResizing = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeNWSE;
                }
                else if (resizeDirection == ResizeDirection.TopRight)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing TopRight");
                    isResizing = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeNESW;
                }
                else if (resizeDirection == ResizeDirection.BottomRight)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing BottomRight");
                    isResizing = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeNWSE;
                }
                else if (resizeDirection == ResizeDirection.BottomLeft)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing BottomLeft");
                    isResizing = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeNESW;
                }
                else if (resizeDirection == ResizeDirection.Left)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing Left");

                    isResizing = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeWE;
                }
                else if (resizeDirection == ResizeDirection.Right)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing Right");

                    isResizing = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeWE;
                }
                else if (resizeDirection == ResizeDirection.Top)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing Top");

                    isResizing = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeNS;
                }
                else if (resizeDirection == ResizeDirection.Bottom)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing Bottom");

                    isResizing = true;
                    startPoint = clickedPoint;
                    this.Cursor = Cursors.SizeNS;
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
            // return if no rectangle is selected
            if (selectedRectangle == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(MyCanvas);
                    // resize a rectangle
                    // resize on right bottom corner
                    if (isResizing)
                    {
                        if (resizeDirection == ResizeDirection.TopLeft)
                        {
                            double rectRight = rectLeft + rectWidth;
                            double rectBottom = rectTop + rectHeight;
                            double x = Math.Max(0, Math.Min(currentPoint.X, rectRight));
                            double y = Math.Max(0, Math.Min(currentPoint.Y, rectBottom));
                            double width = Math.Max(currentPoint.X, rectRight) - x;
                            double height = Math.Max(currentPoint.Y, rectBottom) - y;
                            selectedRectangle.Width = Math.Min(width, MyCanvas.ActualWidth - x);
                            selectedRectangle.Height = Math.Min(height, MyCanvas.ActualHeight - y);
                            Canvas.SetLeft(selectedRectangle, x);
                            Canvas.SetTop(selectedRectangle, y);
                        }
                        else if (resizeDirection == ResizeDirection.TopRight)
                        {
                            double rectBottom = rectTop + rectHeight;
                            double x = Math.Max(0, Math.Min(currentPoint.X, rectLeft));
                            double y = Math.Max(0, Math.Min(currentPoint.Y, rectBottom));
                            double width = Math.Max(currentPoint.X, rectLeft) - x;
                            double height = Math.Max(currentPoint.Y, rectBottom) - y;
                            selectedRectangle.Width = Math.Min(width, MyCanvas.ActualWidth - x);
                            selectedRectangle.Height = Math.Min(height, MyCanvas.ActualHeight - y);
                            Canvas.SetLeft(selectedRectangle, x);
                            Canvas.SetTop(selectedRectangle, y);
                        }
                        else if (resizeDirection == ResizeDirection.BottomRight)
                        {
                            double x = Math.Max(0, Math.Min(currentPoint.X, rectLeft));
                            double y = Math.Max(0, Math.Min(currentPoint.Y, rectTop));
                            double width = Math.Max(currentPoint.X, rectLeft) - x;
                            double height = Math.Max(currentPoint.Y, rectTop) - y;
                            selectedRectangle.Width = Math.Min(width, MyCanvas.ActualWidth - x);
                            selectedRectangle.Height = Math.Min(height, MyCanvas.ActualHeight - y);
                            Canvas.SetLeft(selectedRectangle, x);
                            Canvas.SetTop(selectedRectangle, y);
                        }
                        else if (resizeDirection == ResizeDirection.BottomLeft)
                        {
                            double rectRight = rectLeft + rectWidth;
                            double x = Math.Max(0, Math.Min(currentPoint.X, rectRight));
                            double y = Math.Max(0, Math.Min(currentPoint.Y, rectTop));
                            double width = Math.Max(currentPoint.X, rectRight) - x;
                            double height = Math.Max(currentPoint.Y, rectTop) - y;
                            selectedRectangle.Width = Math.Min(width, MyCanvas.ActualWidth - x);
                            selectedRectangle.Height = Math.Min(height, MyCanvas.ActualHeight - y);
                            Canvas.SetLeft(selectedRectangle, x);
                            Canvas.SetTop(selectedRectangle, y);
                        }
                        else if (resizeDirection == ResizeDirection.Left)
                        {
                            double rectRight = rectLeft + rectWidth;
                            double x = Math.Max(0, Math.Min(currentPoint.X, rectRight));
                            double width = Math.Max(currentPoint.X, rectRight) - x;
                            selectedRectangle.Width = Math.Min(width, MyCanvas.ActualWidth - x);
                            Canvas.SetLeft(selectedRectangle, x);
                        }
                        else if (resizeDirection == ResizeDirection.Top)
                        {
                            double rectBottom = rectTop + rectHeight;
                            double y = Math.Max(0, Math.Min(currentPoint.Y, rectBottom));
                            double height = Math.Max(currentPoint.Y, rectBottom) - y;
                            selectedRectangle.Height = Math.Min(height, MyCanvas.ActualHeight - y);
                            Canvas.SetTop(selectedRectangle, y);
                        }
                        else if (resizeDirection == ResizeDirection.Right)
                        {
                            double x = Math.Max(0, Math.Min(currentPoint.X, rectLeft));
                            double width = Math.Max(currentPoint.X, rectLeft) - x;
                            selectedRectangle.Width = Math.Min(width, MyCanvas.ActualWidth - x);
                            Canvas.SetLeft(selectedRectangle, x);
                        }
                        else if (resizeDirection == ResizeDirection.Bottom)
                        {
                            double y = Math.Max(0, Math.Min(currentPoint.Y, rectTop));
                            double height = Math.Max(currentPoint.Y, rectTop) - y;
                            selectedRectangle.Height = Math.Min(height, MyCanvas.ActualHeight - y);
                            Canvas.SetTop(selectedRectangle, y);
                        }

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
                        double x = Math.Max(0, Math.Min(currentPoint.X, startPoint.X));
                        double y = Math.Max(0, Math.Min(currentPoint.Y, startPoint.Y));
                        double width = Math.Max(currentPoint.X, startPoint.X) - x;
                        double height = Math.Max(currentPoint.Y, startPoint.Y) - y;
                        selectedRectangle.Width = Math.Min(width, MyCanvas.ActualWidth - x);
                        selectedRectangle.Height = Math.Min(height, MyCanvas.ActualHeight - y);
                        Canvas.SetLeft(selectedRectangle, x);
                        Canvas.SetTop(selectedRectangle, y);
                    }
            }
            else
            {
                Point currentPoint = e.GetPosition(MyCanvas);
                GetResizeDirection(currentPoint, tolerance);
                if (resizeDirection == ResizeDirection.TopLeft)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing TopLeft");
                    this.Cursor = Cursors.SizeNWSE;
                }
                else if (resizeDirection == ResizeDirection.TopRight)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing TopRight");
                    this.Cursor = Cursors.SizeNESW;
                }
                else if (resizeDirection == ResizeDirection.BottomRight)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing BottomRight");
                    this.Cursor = Cursors.SizeNWSE;
                }
                else if (resizeDirection == ResizeDirection.BottomLeft)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing BottomLeft");
                    this.Cursor = Cursors.SizeNESW;
                }
                else if (resizeDirection == ResizeDirection.Left)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing Left");

                    this.Cursor = Cursors.SizeWE;
                }
                else if (resizeDirection == ResizeDirection.Right)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing Right");

                    this.Cursor = Cursors.SizeWE;
                }
                else if (resizeDirection == ResizeDirection.Top)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing Top");

                    this.Cursor = Cursors.SizeNS;
                }
                else if (resizeDirection == ResizeDirection.Bottom)
                {
                    System.Diagnostics.Debug.WriteLine("Resizing Bottom");

                    this.Cursor = Cursors.SizeNS;
                }
                else if (rectLeft <= currentPoint.X && currentPoint.X <= rectLeft + rectWidth && rectTop <= currentPoint.Y && currentPoint.Y <= rectTop + rectHeight)
                {
                    this.Cursor = Cursors.SizeAll;
                }
                else
                {
                    this.Cursor = Cursors.Arrow;
                }

            }
        }

        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Stop drawing the rectangle
            this.Cursor = Cursors.Arrow;
            isMoving = false;
            isResizing = false;
            resizeDirection = ResizeDirection.None;
            isDrawing = false;
            if (selectedRectangle != null)
            {
                Select_Rectangle(selectedRectangle);
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
            Unselect_Rectangle(selectedRectangle);
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
