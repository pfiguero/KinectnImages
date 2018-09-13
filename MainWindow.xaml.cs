//------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Pfiguero">
//     GPL
// </copyright>
//------------------------------------------------------------------------------

namespace Pfiguero.Samples.ImageReel
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

        private IGetImage kinectManager = null;

        private static ReelManager reelManager = null;

        private int initDelta = 0;

        private Rectangle[] rects = null;

        private int[] xPosRects = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow( int iDelta, IGetImage k)
        {
            initDelta = iDelta;

            if( kinectManager == null || reelManager== null  )
            {
                kinectManager = k;
                reelManager = new ReelManager();
            }

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

        public ImageSource ImageSource => kinectManager.GetImage();


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
            kinectManager.RefreshImage();
            this.DrawImages();
        }

        private void CreateRects()
        {
            int screenWidth = 1280;
            rects = new Rectangle[reelManager.reel.Length];
            xPosRects = new int[reelManager.reel.Length];
            for (int i = 0; i < reelManager.reel.Length; i++)
            {
                int imgStart = reelManager.reel[i].xPos;
                int imgEnd = (int)(reelManager.reel[i].xPos + reelManager.reel[i].image.Width);
                rects[i] = new Rectangle()
                {
                    Width = reelManager.reel[i].image.Width,
                    Height = reelManager.reel[i].image.Height
                };
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = reelManager.reel[i].image;
                rects[i].Fill = ib;

                canvas.Children.Add(rects[i]);
                Canvas.SetTop(rects[i], reelManager.reel[i].yPos);
                xPosRects[i] = reelManager.reel[i].xPos - initDelta;
                Canvas.SetLeft(rects[i], xPosRects[i]);
            }
        }

        private void DrawImages()
        {
            for (int i = 0; i < reelManager.reel.Length; i++)
            {
                //int x = xPosRects[i] - howMuch;
                //if (x + reelManager.reel[i].image.Width < 0)
                //    x = reelManager.LastPos + 30; // margin!!! Unify!!!
                xPosRects[i] -= kinectManager.HowMuch;
                if (xPosRects[i] + reelManager.reel[i].image.Width < 0)
                {
                    int last = i - 1;
                    if (last < 0)
                        last = reelManager.reel.Length-1;
                    xPosRects[i] = (int)(xPosRects[last] + reelManager.reel[last].image.Width + 30); // margin!!! Unify!!!
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
            for(int i=0; i< reelManager.reel.Length; i++)
            {
                int imgStart = reelManager.reel[i].xPos;
                int imgEnd = (int) (reelManager.reel[i].xPos + reelManager.reel[i].image.Width);
                if( !(imgStart > initDelta + screenWidth || imgEnd < initDelta) )
                {
                    Rectangle rect = new Rectangle()
                    {
                        Width = reelManager.reel[i].image.Width,
                        Height = reelManager.reel[i].image.Height
                    };
                    ImageBrush ib = new ImageBrush();
                    ib.ImageSource = reelManager.reel[i].image;
                    rect.Fill = ib;

                    canvas.Children.Add(rect);
                    Canvas.SetTop(rect, reelManager.reel[i].yPos);
                    Canvas.SetLeft(rect, reelManager.reel[i].xPos - initDelta);
                }
            }
            initDelta += 10;
        }
    }

}
