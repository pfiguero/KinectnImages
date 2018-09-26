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

    public class LogEventArgs: EventArgs
    {
        public int FrameId { get; set; }
        public int Number { get; set; }
        public ImageSource Image { get; set; }
    }

    public delegate void NumberLogEventHandler(object sender, LogEventArgs e);

    public class KinectManager : IDisposable
    {

        // Events
        public event EventHandler OnSomebody;
        public event EventHandler OnNobody;
        // This one only updates the number, not the imagesource
        public event NumberLogEventHandler OnNumberLog;
        // This one updates both the number and the imagesource
        public event NumberLogEventHandler OnImageLog;

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

        // To Do: Change to a normal attribute, non static...
        private static int frameCount = 0;

        private double val = 0;
        private double stdDev = 20000; // from a small sample...
        private int procFrame = 0; // Processed frame
        private bool isCurStateNobody = true;

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

        public uint GetBytesPerPixel()
        {
            return this.depthFrameDescription.BytesPerPixel;
        }

        public KinectManager()
        {
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


        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

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

            // sample of the 1st 10 frames... Assuming nobody is in front...
            if (procFrame < 10)
            {
                val += sum;
                procFrame++;
            }
            else if (procFrame == 10)
            {
                val /= procFrame;
                //Debug.WriteLine("VAL PROM: " + val);
                procFrame++; 
            }
            else
            {
                if (Math.Abs(val - sum) > stdDev && isCurStateNobody == true)
                {
                    isCurStateNobody = false;
                    OnSomebody(this, new EventArgs());
                }
                else if(Math.Abs(val - sum) <= stdDev && isCurStateNobody == false)
                {
                    isCurStateNobody = true;
                    OnNobody(this, new EventArgs());
                }
                procFrame++;
            }

            LogEventArgs e = null;
            if ( OnNumberLog != null || OnImageLog != null)
            {
                e = new LogEventArgs();
                e.FrameId = procFrame;
                e.Number = (int)sum;
            }
            if (OnNumberLog != null)
            {
                OnNumberLog(this, e);
            }
            if ( OnImageLog != null)
            {
                // TO DO: do something to save the image...
                OnImageLog(this, e);
            }
        }



        /// <summary>
        /// Handles the depth frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
           //bool depthFrameProcessed = false;

            // process every tenth frame, for performance purposes
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
                            //depthFrameProcessed = true;
                        }
                    }
                }
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
    }

}
