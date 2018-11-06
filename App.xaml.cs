﻿//------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Pfiguero">
//     GPL
// </copyright>
//------------------------------------------------------------------------------

namespace Pfiguero.Samples.ImageReel
{
    using System;
    using System.Diagnostics;
    using System.Timers;
    using System.Windows;
    using System.Windows.Media;
    using System.Collections.Generic;

    /// <summary>
    /// Interaction logic for App
    /// Some References:
    /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/5d181304-8952-4663-8c3c-dc4d986aa8dd/dual-screen-application-and-wpf?forum=wpf
    /// https://stackoverflow.com/questions/2704887/is-there-a-wpf-equaivalent-to-system-windows-forms-screen
    /// </summary>
    public partial class App : Application
    {
        private ReelManager<InfoReel> reelManager = null;
        private KinectManager kinectManager = null;
        private bool allowEvents = false;
        private Dictionary<string, string> cmdLine = null;


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            allowEvents = false;

            cmdLine = new Dictionary<string, string>();
            for (int index = 0; index < e.Args.Length; index += 2)
            {
                cmdLine.Add(e.Args[index], e.Args[index + 1]);
            }

            _OnStartup2(e);
        }

        private void _OnStartup2(StartupEventArgs e)
        {
            kinectManager = new KinectManager();
            //kinectManager.GetImage();
            //kinectManager.RefreshImage();

            Window1 w1 = new Window1(this);
            w1.Top = SystemParameters.VirtualScreenTop;
            w1.Left = SystemParameters.VirtualScreenLeft;
            w1.Width = SystemParameters.VirtualScreenWidth; // /2; // when two screens
            w1.Height = SystemParameters.VirtualScreenHeight;
            w1.Show();

            string jsonFile;
            if(cmdLine.ContainsKey("-json"))
            {
                jsonFile = cmdLine["-json"];
            }
            else
            {
                jsonFile = "test.json";
            }
            reelManager = new ReelManager<InfoReel>(jsonFile, w1.Width, w1.Height );


            var rt = new TranslateTransform();
            reelManager.SetAnimationTranslation(rt);

            reelManager.CreateRects(w1, 0);

            reelManager.StartAnimation();

            allowEvents = true;

            kinectManager.OnSomebody += w1.OnResize;
            kinectManager.OnNobody += w1.OnResize;
            kinectManager.OnNumberLog += this.OnNumberLog;
        }

        public void OnDoubleClick()
        {
            reelManager.ToggleStop();
        }

        public void OnResize()
        {
            if(allowEvents)
            {
                allowEvents = false;
                Timer aTimer = new Timer(5000);
                aTimer.Elapsed += OnTimer;
                aTimer.AutoReset = false;
                aTimer.Enabled = true;
                reelManager.ToggleSize();
            }
        }

        public void OnNumberLog(Object source, LogEventArgs e)
        {
            Debug.WriteLine("Log. Sum: {0}", e.Number);
        }

        private void OnTimer(Object source, ElapsedEventArgs e)
        {
            allowEvents = true;
        }

    }
}
