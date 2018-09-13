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

        private ReelManager reelManager = null;

        private int initDelta = 0;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow( int iDelta, IGetImage k, ReelManager r)
        {
            initDelta = iDelta;

            kinectManager = k;
            reelManager = r;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

            reelManager.CreateRects( canvas, initDelta );
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
            reelManager.DrawImages(kinectManager.HowMuch);
        }

    }

}
