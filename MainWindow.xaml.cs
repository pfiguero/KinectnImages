//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Pfiguero.Samples.ImageReel;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
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

        private void DrawImages()
        {
            for (int i = 0; i < imageManager.reel.Length; i++)
            {
                //int x = xPosRects[i] - howMuch;
                //if (x + imageManager.reel[i].image.Width < 0)
                //    x = imageManager.LastPos + 30; // margin!!! Unify!!!
                xPosRects[i] -= kinectManager.HowMuch;
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

}
