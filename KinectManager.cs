//------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Pfiguero">
//     GPL
// </copyright>
//------------------------------------------------------------------------------

namespace Pfiguero.Samples.ImageReel
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;

    /// <summary>
    /// This event is triggered when there is somebody in front of the Kinect.
    /// </summary>
    public delegate void MyRefreshScreenHandler(object source, EventArgs e);

    /// <summary>
    /// This event is triggered when nobody is in front.
    /// </summary>
    public delegate void MyRefreshScreenHandler2(object source, EventArgs e);

    public interface IGetImage
    {
        WriteableBitmap GetImage();
        void RefreshImage();
        int HowMuch { get; }
    }


    public class KinectManager : IDisposable, IGetImage
    {

        public event MyRefreshScreenHandler OnSomebody;

        public event MyRefreshScreenHandler OnNobody;

        /// <summary>
        /// This is the image we'll show
        /// </summary>
        private WriteableBitmap outBitmap = null;

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

        public WriteableBitmap GetImage()
        {
            if (outBitmap == null)
            {
                // Background
                Uri uri = new System.Uri(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\PromociónRedes.jpg"));
                ImageSource imageSource = new BitmapImage(uri);
                outBitmap = new WriteableBitmap(imageSource as BitmapSource);

                // The kinect writes the resulting image here
                BitmapDecoder dec = BitmapDecoder.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);
                this.SetImage(dec.Frames[0]);

                // create the output stream of bytes. Assuming 4 bytes per pixel
                this.SetPixels(outBitmap.PixelWidth * outBitmap.PixelHeight * 4);
            }
            return outBitmap;
        }

        public void RefreshImage()
        {
            this.outBitmap.WritePixels(
                new Int32Rect(0, 0, this.outBitmap.PixelWidth, this.outBitmap.PixelHeight),
                this.Pixels,
                this.outBitmap.PixelWidth * 4, 0);
        }


        public byte[] Pixels
        {
            get
            {
                return pixels;
            }
        }

        public uint GetBytesPerPixel()
        {
            return this.depthFrameDescription.BytesPerPixel;
        }

        private int _howMuch = 100;

        public int HowMuch { get { return _howMuch; } }

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

            // This is the image we'll show
            this.outBitmap = null;

            _howMuch = 100;
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
        private unsafe void ProcessDepthFrameDataOLD(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
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
                if (iE < (int)(depthFrameDataSize / this.GetBytesPerPixel()))
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

        public double val = 0;
        public double stdDev = 20000; // from a small sample...
        public int nVal = 0;


        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            image.CopyPixels(pixels, image.PixelWidth * 4, 0);

            // The sum of a row is an indication of what is in front...
            int r = this.Width / 2;
            uint sum = 0;
            for (int c = 0; c < this.Height; c++)
            {
                int i = r + c * this.Width; // * this.depthFrameDescription.BytesPerPixel;
                ushort depth = 0;
                if (i < (int)(depthFrameDataSize / this.GetBytesPerPixel()))
                {
                    depth = frameData[i];
                }
                sum += depth;
            }
            // Debug.WriteLine("SUM: " + sum);
            // sample of the 1st 10 values... Assuming nobody is in front...
            if (nVal < 10)
            {
                nVal++;
                val += sum;
            }
            else if (nVal == 10)
            {
                //stdDev = 0.1; // Math.Sqrt((nVal)/nVal);
                val /= nVal;
                Debug.WriteLine("VAL PROM: " + val);
                nVal++; // just to put it outside this loop
            }
            else
            {
                if (Math.Abs(val - sum) > stdDev)
                {
                    _howMuch = 10;
                    OnSomebody(this, new EventArgs());
                }
                // hack... using _howMuch to generate a new event
                else if(_howMuch == 10 )
                {
                    _howMuch = 100;
                    OnNobody(this, new EventArgs());
                }
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

            if (depthFrameProcessed)
            {
                // OnRefresh(this, new EventArgs());
                _howMuch = _howMuch >= 100 ? _howMuch : _howMuch + 1;
                // Debug.WriteLine("How Much: " + _howMuch);

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

}
