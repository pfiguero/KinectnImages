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
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Newtonsoft.Json;

    /// <summary>
    /// This event is triggered when new info from the kinect is available.
    /// </summary>
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

        private static KinectManager kinectManager = null;

        private static ImageManager imageManager = null;

        private int initDelta = 0;

        private Rectangle[] rects = null;

        private int[] xPosRects = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow( int iDelta )
        {
            initDelta = iDelta;

            if( kinectManager == null || imageManager== null  )
            {
                kinectManager = new KinectManager();
                imageManager = new ImageManager();
            }

            // register the event handler
            kinectManager.OnRefresh += new MyRefreshScreenHandler(RefreshScreen);

            // This is the image we'll show
            this.outBitmap = null;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

            CreateRects();
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
                    // Background
                    Uri uri = new System.Uri(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\PromociónRedes.jpg"));
                    ImageSource imageSource = new BitmapImage(uri);
                    outBitmap = new WriteableBitmap(imageSource as BitmapSource);

                    // The kinect writes the resulting image here
                    BitmapDecoder dec = BitmapDecoder.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    kinectManager.SetImage( dec.Frames[0] );

                    // create the output stream of bytes. Assuming 4 bytes per pixel
                    kinectManager.SetPixels(outBitmap.PixelWidth * outBitmap.PixelHeight * 4);
                }
                return outBitmap;
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
        }

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
                this.outBitmap.PixelWidth * 4,
                0);
            this.DrawImages();
        }

        private void CreateRects()
        {
            int screenWidth = 1280;
            rects = new Rectangle[imageManager.reel.Length];
            xPosRects = new int[imageManager.reel.Length];
            for (int i = 0; i < imageManager.reel.Length; i++)
            {
                int imgStart = imageManager.reel[i].xPos;
                int imgEnd = (int)(imageManager.reel[i].xPos + imageManager.reel[i].image.Width);
                rects[i] = new Rectangle()
                {
                    Width = imageManager.reel[i].image.Width,
                    Height = imageManager.reel[i].image.Height
                };
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = imageManager.reel[i].image;
                rects[i].Fill = ib;

                canvas.Children.Add(rects[i]);
                Canvas.SetTop(rects[i], imageManager.reel[i].yPos);
                xPosRects[i] = imageManager.reel[i].xPos - initDelta;
                Canvas.SetLeft(rects[i], xPosRects[i]);
            }
        }

        private static int howMuch = 10;

        private void DrawImages()
        {
            for (int i = 0; i < imageManager.reel.Length; i++)
            {
                //int x = xPosRects[i] - howMuch;
                //if (x + imageManager.reel[i].image.Width < 0)
                //    x = imageManager.LastPos + 30; // margin!!! Unify!!!
                xPosRects[i] -= howMuch;
                if (xPosRects[i] + imageManager.reel[i].image.Width < 0)
                {
                    int last = i - 1;
                    if (last < 0)
                        last = imageManager.reel.Length-1;
                    xPosRects[i] = (int)(xPosRects[last] + imageManager.reel[last].image.Width + 30); // margin!!! Unify!!!
                }
                Canvas.SetLeft(rects[i], xPosRects[i]);
            }
            //howMuch += 10;
            //if (howMuch > 2560)
            //    howMuch -= 2560;
        }

        private void DrawImagesOLD()
        {
            int screenWidth = 1280;
            for(int i=0; i< imageManager.reel.Length; i++)
            {
                int imgStart = imageManager.reel[i].xPos;
                int imgEnd = (int) (imageManager.reel[i].xPos + imageManager.reel[i].image.Width);
                if( !(imgStart > initDelta + screenWidth || imgEnd < initDelta) )
                {
                    Rectangle rect = new Rectangle()
                    {
                        Width = imageManager.reel[i].image.Width,
                        Height = imageManager.reel[i].image.Height
                    };
                    ImageBrush ib = new ImageBrush();
                    ib.ImageSource = imageManager.reel[i].image;
                    rect.Fill = ib;

                    canvas.Children.Add(rect);
                    Canvas.SetTop(rect, imageManager.reel[i].yPos);
                    Canvas.SetLeft(rect, imageManager.reel[i].xPos - initDelta);
                }
            }
            initDelta += 10;
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
            // this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();


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
                hue = ((double)depthByte / 255d) + hue;

                c = ColorFromHSV(hue, sat, val);

                // colors
                //pixels[i * 4] = c.B;
                //pixels[i * 4 + 1] = c.G;
                //pixels[i * 4 + 2] = c.R;
                //pixels[i * 4 + 3] = c.A;

                // grays
                //pixels[i * 4] = depthByte;
                //pixels[i * 4 + 1] = depthByte;
                //pixels[i * 4 + 2] = depthByte;
                //pixels[i * 4 + 3] = 255;
            }
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
        struct ImageInfo
        {
            public String filename;
            public String title;
        }
        struct MyFile
        {
            public String directory;
            public ImageInfo[] images;
        }

        public struct InfoReel
        {
            public ImageSource image;
            public int xPos;
            public int yPos;
        }

        public InfoReel[] reel = null;

        public int LastPos { get; }

        private void WriteTestData()
        {
            // MyFile f = { "dir1", { { "f1", "t1" }, { "f2", "t2" } } };
            MyFile f = new MyFile();
            f.directory = "dir1";
            f.images = new ImageInfo[2];
            f.images[0] = new ImageInfo();
            f.images[0].filename = "f1";
            f.images[0].title = "i1";
            f.images[1] = new ImageInfo();
            f.images[1].filename = "f1";
            f.images[1].title = "i1";

            String s = JsonConvert.SerializeObject(f);
            Debug.WriteLine("@@@ " + s);
        }

        public ImageManager()
        {
            //// Background
            //Uri uri = new System.Uri(Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\PromociónRedes.jpg"));
            //ImageSource imageSource = new BitmapImage(uri);
            // Open the json file and de serialize it
            MyFile f = JsonConvert.DeserializeObject<MyFile>(File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\50anios.json")));

            reel = new InfoReel[f.images.Length];
            Uri uri;
            for (int i=0; i<reel.Length; i++)
            {
                String s = @"..\..\..\images\" + f.directory + "\\" + f.images[i].filename;
                uri = new System.Uri(System.IO.Path.Combine(Environment.CurrentDirectory, s));
                reel[i].image = new BitmapImage(uri);
                Debug.WriteLine("Width: " + reel[i].image.Width + " Height: " + reel[i].image.Height);
            }

            // Define positions in the reel...
            int marginX = 30;
            int screenHeight = 720;
            reel[0].xPos = marginX;
            reel[0].yPos = (int) (screenHeight - reel[0].image.Height) / 2;
            for(int i=1; i< reel.Length; i++)
            {
                reel[i].xPos = (int) (reel[i-1].xPos + reel[i-1].image.Width + marginX);
                reel[i].yPos = (int) (screenHeight - reel[i].image.Height) / 2;
            }
            LastPos = (int)(reel[reel.Length - 1].xPos + reel[reel.Length - 1].image.Width + marginX);
        }
        //'C:\Users\COLIVRI\Documents\KinectnImages\images\50anios\002-22.jpg'.'
    }


}
