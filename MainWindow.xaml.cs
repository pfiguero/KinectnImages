//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Resources;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Newtonsoft.Json;

    public delegate void MyRefreshScreenHandler(object source, EventArgs e);

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// This is the image we'll show
        /// </summary>
        private WriteableBitmap outBitmap = null;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        //private byte[] depthPixels = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        private static KinectManager kinectManager = null;

        private static ImageManager imageManager = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            if( kinectManager == null || imageManager== null  )
            {
                kinectManager = new KinectManager();
                imageManager = new ImageManager();
            }

            kinectManager.OnRefresh += new MyRefreshScreenHandler(RefreshScreen);

            // allocate space to put the pixels being received and converted
            // this.depthPixels = new byte[kinectManager.Width * kinectManager.Height];

            // This is the image we'll show
            this.outBitmap = null;

            // set the status text
            // this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
            //                                                 : Properties.Resources.NoSensorStatusText;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        
        public ImageSource ImageSource
        {
            get
            {
                if(outBitmap == null)
                {
                    Uri uri = new System.Uri(Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\earth_tone_brown_blue_sky_mountain_nature_glacier-272202.jpg"));
                    ImageSource imageSource = new BitmapImage(uri);
                    outBitmap = new WriteableBitmap(imageSource as BitmapSource);

                    //StreamResourceInfo x = Application.GetRemoteStream(uri);
                    BitmapDecoder dec = BitmapDecoder.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    kinectManager.SetImage( dec.Frames[0] );
                    //outBitmap = new WriteableBitmap(imageSource.Width,imageSource.Height,96,96,PixelFormats.Rgb24,BitmapPalettes.WebPalette)

                    // create the output stream of bytes. Assuming 4 bytes per pixel
                    //pixels = new byte[outBitmap.PixelWidth * outBitmap.PixelHeight * 4];
                    kinectManager.SetPixels(outBitmap.PixelWidth * outBitmap.PixelHeight * 4);
                    //pixels = new byte[image.PixelWidth * image.PixelHeight * 4];
                }
                return outBitmap;
            }
        }
        
        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // do nothing... Can´t dispose yet, since the kinect is shared between the two windows
            //kinectManager.Dispose();
            //kinectManager = null;
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /*
        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.depthBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.depthBitmap));

                string time = System.DateTime.UtcNow.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "KinectScreenshot-Depth-" + time + ".png");

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SavedScreenshotStatusTextFormat, path);
                }
                catch (IOException)
                {
                    this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }
            }
        }
        */

        public void RefreshScreen(object source, EventArgs e)
        {
            this.RenderDepthPixels();
        }

        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// It now has to modify the pixels at the outBitmap based on the information at depthPixels
        /// It takes into account size differences between outBitmap and depthPixels
        /// </summary>
        private void RenderDepthPixels()
        {            
            this.outBitmap.WritePixels(
                new Int32Rect(0, 0, this.outBitmap.PixelWidth, this.outBitmap.PixelHeight),
                kinectManager.Pixels,
                this.outBitmap.PixelWidth*4,
                0);


            //outBitmap.WritePixels(new Int32Rect(0, 0, image.PixelWidth, image.PixelHeight), pixels, image.PixelWidth * 4, 0);

        }
    }

    public class KinectManager : IDisposable
    {

        public event MyRefreshScreenHandler OnRefresh;

        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 8000 / 256;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for depth frames
        /// </summary>
        private DepthFrameReader depthFrameReader = null;

        /// <summary>
        /// Description of the data contained in the depth frame
        /// </summary>
        private FrameDescription depthFrameDescription = null;

        private static int frameCount = 0;

        /// <summary>
        /// this is the image we modify and copy at the end to outBitmap
        /// </summary>
        private BitmapFrame image = null;

        private byte[] pixels = null;

        public int Width
        {
            get
            {
                return this.depthFrameDescription.Width;
            }
        }

        public int Height
        {
            get
            {
                return this.depthFrameDescription.Height;
            }
        }
        
        public byte[] Pixels
        {
            get
            {
                return pixels;
            }
        }

        public uint BytesPerPixel
        {
            get
            {
                return this.depthFrameDescription.BytesPerPixel;
            }
        }

        public KinectManager()
        {
            // this is the image we modify and copy at the end to outBitmap
            image = null;

            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the depth frames
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();

            // wire handler for frame arrival
            this.depthFrameReader.FrameArrived += this.Reader_FrameArrived;

            // get FrameDescription from DepthFrameSource
            this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();


        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            //this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
            //                                                : Properties.Resources.SensorNotAvailableStatusText;
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            image.CopyPixels(pixels, image.PixelWidth * 4, 0);

            // modify the pixels, according to what is read from the Kinect
            for (int i = 0; i < pixels.Length / 4; ++i)
            {
                // coordinates of the current pixel in the output image
                int col = i % image.PixelWidth;
                int row = i / image.PixelHeight;

                // convert to the space of the Kinect image
                int rE = (int)(((double)col * this.Width) / ((double)image.PixelWidth));
                int cE = (int)(((double)row * this.Height) / ((double)image.PixelHeight));
                int iE = rE + cE * this.Width; // * this.depthFrameDescription.BytesPerPixel;

                ushort depth = 0;
                if (iE < (int)(depthFrameDataSize / this.BytesPerPixel))
                {
                    depth = frameData[iE];
                }
                byte depthByte = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
                //Debug.WriteLine("iE: " + iE + "depth: " + depth + "depthByte" + depthByte );

                byte b = pixels[i * 4];
                byte g = pixels[i * 4 + 1];
                byte r = pixels[i * 4 + 2];
                byte a = pixels[i * 4 + 3];

                Color c = Color.FromArgb(a, r, g, b);
                double hue, sat, val;
                ColorToHSV(c, out hue, out sat, out val);
                hue = ((double)depthByte / 255d);

                c = ColorFromHSV(hue, sat, val);

                // grays
                pixels[i * 4] = depthByte;
                pixels[i * 4 + 1] = depthByte;
                pixels[i * 4 + 2] = depthByte;
                pixels[i * 4 + 3] = 255;
            }

            //outBitmap.WritePixels(new Int32Rect(0, 0, image.PixelWidth, image.PixelHeight), pixels, image.PixelWidth * 4, 0);

            /*
            
           
            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
            */
        }


        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            //hue = color.GetHue();
            // From https://stackoverflow.com/questions/23090019/fastest-formula-to-get-hue-from-rgb
            if (max == color.R)
                hue = (color.G - color.B) / ((max - min) * 255d);
            else if (max == color.G)
                hue = 2.0 + (color.B - color.R) / ((max - min) * 255d);
            else
                hue = 4.0 + (color.R - color.G) / ((max - min) * 255d);

            hue *= 60;
            hue = hue < 0 ? hue + 360 : hue;

            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = (int)(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = (byte)(value);
            byte p = (byte)(value * (1 - saturation));
            byte q = (byte)(value * (1 - f * saturation));
            byte t = (byte)(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }


        /// <summary>
        /// Handles the depth frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;

            if (frameCount++ % 10 != 0)
                return;

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel))
                            //    && (this.depthFrameDescription.Width == this.depthBitmap.PixelWidth) && (this.depthFrameDescription.Height == this.depthBitmap.PixelHeight)
                            )
                        {
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                            ushort maxDepth = ushort.MaxValue;

                            // If you wish to filter by reliable depth distance, uncomment the following line:
                            //// maxDepth = depthFrame.DepthMaxReliableDistance

                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if(depthFrameProcessed)
            {
                OnRefresh(this, new EventArgs());
            }
        }

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (this.depthFrameReader != null)
            {
                // DepthFrameReader is IDisposable
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
        //dispose unmanaged resources
        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

        public void SetImage(BitmapFrame bitmapFrame)
        {
            image = bitmapFrame;
        }

        public void SetPixels(int v)
        {
            pixels = new byte[v];
        }
    }

    public class ImageManager
    {
        public ImageManager()
        {
            // Open the json file
            var json = new StreamReader(Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\50anios.json"));
            JsonTextReader reader = new JsonTextReader(json);
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    Debug.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                }
                else
                {
                    Debug.WriteLine("Token: {0}", reader.TokenType);
                }
            }
            json.Close();
        }
    }


}
