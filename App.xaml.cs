//------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App
    /// </summary>
    public partial class App : Application
    {
        // From https://social.msdn.microsoft.com/Forums/vstudio/en-US/5d181304-8952-4663-8c3c-dc4d986aa8dd/dual-screen-application-and-wpf?forum=wpf
        // and https://stackoverflow.com/questions/2704887/is-there-a-wpf-equaivalent-to-system-windows-forms-screen
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow w1 = new MainWindow();
            MainWindow w2 = new MainWindow();

            w1.Top = SystemParameters.VirtualScreenTop;
            w1.Left = SystemParameters.VirtualScreenLeft;
            w1.Width = SystemParameters.VirtualScreenWidth / 2;
            w1.Height = SystemParameters.VirtualScreenHeight;

            w2.Top = SystemParameters.VirtualScreenTop;
            w2.Left = SystemParameters.VirtualScreenWidth/2;
            w2.Width = SystemParameters.VirtualScreenWidth / 2;
            w2.Height = SystemParameters.VirtualScreenHeight;

            w1.Show();
            w2.Show();

            w2.Owner = w1;
        }
    }
}
