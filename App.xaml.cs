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

    /// <summary>
    /// Interaction logic for App
    /// </summary>
    public partial class App : Application
    {
        private ReelManager reelManager = null;

        // Excerpts from https://social.msdn.microsoft.com/Forums/vstudio/en-US/5d181304-8952-4663-8c3c-dc4d986aa8dd/dual-screen-application-and-wpf?forum=wpf
        // and https://stackoverflow.com/questions/2704887/is-there-a-wpf-equaivalent-to-system-windows-forms-screen
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _OnStartup2(e);
        }

        private void _OnStartup2(StartupEventArgs e)
        {
            reelManager = new ReelManager("test.json");

            var rt = new TranslateTransform();
            reelManager.SetAnimationTranslation(rt);

            Window1 w1 = new Window1(this);
            w1.Top = SystemParameters.VirtualScreenTop;
            w1.Left = SystemParameters.VirtualScreenLeft;
            w1.Width = SystemParameters.VirtualScreenWidth / 2;
            w1.Height = SystemParameters.VirtualScreenHeight;

            w1.Show();

            reelManager.CreateRects(w1, 0);

            reelManager.StartAnimation();
        }

        public void DoubleClick()
        {
            //da.SpeedRatio = da.SpeedRatio == 5 ? 1 : 5;
            reelManager.StopAnimation();
        }

        private void _OnStartup1(StartupEventArgs e)
        {
            KinectManager kinectManager = new KinectManager();
            ReelManager reelManager = new ReelManager("test.json");

            MainWindow w1 = new MainWindow(0, kinectManager, reelManager);
            //MainWindow w2 = new MainWindow(1280, kinectManager, reelManager);

            // register the event handler
            kinectManager.OnRefresh += new MyRefreshScreenHandler(w1.RefreshScreen);
            //kinectManager.OnRefresh += new MyRefreshScreenHandler(w2.RefreshScreen);

            w1.Top = SystemParameters.VirtualScreenTop;
            w1.Left = SystemParameters.VirtualScreenLeft;
            w1.Width = SystemParameters.VirtualScreenWidth / 2;
            w1.Height = SystemParameters.VirtualScreenHeight;

            //w2.Top = SystemParameters.VirtualScreenTop;
            //w2.Left = SystemParameters.VirtualScreenWidth/2;
            //w2.Width = SystemParameters.VirtualScreenWidth / 2;
            //w2.Height = SystemParameters.VirtualScreenHeight;

            w1.Show();
            //w2.Show();

            //w2.Owner = w1;
        }

    }
}
