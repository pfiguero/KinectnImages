//------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Pfiguero">
//     GPL
// </copyright>
//------------------------------------------------------------------------------

namespace Pfiguero.Samples.ImageReel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Shapes;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Animation;
    using Newtonsoft.Json;

    public class ReelManager
    {
        // Input structure from the file
        struct MyFile
        {
            public String directory;
            public ImageInfo[] images;
        }
        struct ImageInfo
        {
            public String filename;
            public String title;
        }

        // Computed info from the input structure
        public struct InfoReel
        {
            public ImageSource image;
            public TransformGroup trGroup;
            public TransformGroup trGroupLocal;
            //public ScaleTransform scale;
        }

        public InfoReel[] reel = null;

        public double LastPos { get; set;  }

        // for the animation of the reel
        private TranslateTransform tr = null;

        private DoubleAnimation da = null;

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

        public ReelManager(String jsonName )
        {
            //// Background
            //Uri uri = new System.Uri(Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\PromociónRedes.jpg"));
            //ImageSource imageSource = new BitmapImage(uri);
            // Open the json file and de serialize it
            String p = @"..\..\..\data\" + jsonName;
            MyFile f = JsonConvert.DeserializeObject<MyFile>(File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, p)));

            reel = new InfoReel[f.images.Length];
            Uri uri;
            char[] seps = { '.' };
            String directory = jsonName.Split(seps)[0];
            for (int i = 0; i < reel.Length; i++)
            {
                String s = @"..\..\..\data\" + directory + "\\" + f.images[i].filename;
                uri = new System.Uri(System.IO.Path.Combine(Environment.CurrentDirectory, s));
                reel[i].image = new BitmapImage(uri);
                Debug.WriteLine("Width: " + reel[i].image.Width + " Height: " + reel[i].image.Height);
            }

            tr = null;
        }

        public void CreateRects(Window1 w, int initDelta )
        {
            LastPos = 0.0;

            TranslateTransform trDelta = new TranslateTransform();
            trDelta.X = initDelta;

            // common margin
            TranslateTransform trMargin = new TranslateTransform(30.0, 0.0);

            // it should be read from the window... 
            double screenHeight = w.Height;

            reel[0].trGroup = new TransformGroup();
            reel[0].trGroup.Children.Add(tr);
            reel[0].trGroup.Children.Add(trMargin);

            reel[0].trGroupLocal = new TransformGroup();
            reel[0].trGroupLocal.Children.Add(reel[0].trGroup);
            reel[0].trGroupLocal.Children.Add(new TranslateTransform(0.0, (screenHeight - reel[0].image.Height) / 2));

            LastPos += trMargin.X + reel[0].image.Width;

            for (int i = 1; i < reel.Length; i++)
            {
                reel[i].trGroup = new TransformGroup();
                reel[i].trGroup.Children.Add(reel[i-1].trGroup);
                reel[i].trGroup.Children.Add(new TranslateTransform(reel[i-1].image.Width,0.0));
                reel[i].trGroup.Children.Add(trMargin);

                reel[i].trGroupLocal = new TransformGroup();
                reel[i].trGroupLocal.Children.Add(reel[i].trGroup);
                reel[i].trGroupLocal.Children.Add(new TranslateTransform(0.0, (screenHeight - reel[i].image.Height) / 2));

                LastPos += trMargin.X + reel[i].image.Width;
            }

            LastPos += trMargin.X + reel[reel.Length-1].image.Width;

            Canvas canvas = w.canvas;
            Rectangle[] rects = new Rectangle[this.reel.Length];         
            for (int i = 0; i < this.reel.Length; i++)
            {
                rects[i] = new Rectangle()
                {
                    Width = this.reel[i].image.Width,
                    Height = this.reel[i].image.Height
                };
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = this.reel[i].image;
                rects[i].Fill = ib;
                rects[i].RenderTransform = reel[i].trGroupLocal;

                canvas.Children.Add(rects[i]);
            }
        }

        public void StartAnimation()
        {
            da = new DoubleAnimation(0, -LastPos, new Duration(TimeSpan.FromSeconds(200)));
            da.SpeedRatio = 5;
            da.AccelerationRatio = .1;
            da.RepeatBehavior = RepeatBehavior.Forever;
            tr.BeginAnimation(TranslateTransform.XProperty, da);
        }

        public static bool stopping = true;

        public void StopAnimation()
        {
            if(stopping)
            {
                double curValue = tr.X;
                da = new DoubleAnimation(toValue: tr.X - 100, duration: new Duration(TimeSpan.FromSeconds(2)));
                da.SpeedRatio = 5;
                da.AccelerationRatio = .1;
                tr.BeginAnimation(TranslateTransform.XProperty, da);
                stopping = false;
            }
            else
            {
                da = new DoubleAnimation(toValue: -LastPos, duration: new Duration(TimeSpan.FromSeconds(200)));
                da.SpeedRatio = 5;
                da.AccelerationRatio = .1;
                da.Completed += new EventHandler(CompletedAnimation);
                tr.BeginAnimation(TranslateTransform.XProperty, da);
                stopping = true;
            }
        }

        // If the animation finishes, it means it was interrupted. Create it again from the beginning
        private void CompletedAnimation(object sender, EventArgs e)
        {
            da = new DoubleAnimation(0, -LastPos, new Duration(TimeSpan.FromSeconds(200)));
            da.SpeedRatio = 5;
            da.AccelerationRatio = .1;
            da.RepeatBehavior = RepeatBehavior.Forever;
            tr.BeginAnimation(TranslateTransform.XProperty, da);
        }

        //private void DrawImagesOLD()
        //{
        //    int screenWidth = 1280;
        //    for (int i = 0; i < this.reel.Length; i++)
        //    {
        //        int imgStart = this.reel[i].xPos;
        //        int imgEnd = (int)(this.reel[i].xPos + this.reel[i].image.Width);
        //        if (!(imgStart > initDelta + screenWidth || imgEnd < initDelta))
        //        {
        //            Rectangle rect = new Rectangle()
        //            {
        //                Width = this.reel[i].image.Width,
        //                Height = this.reel[i].image.Height
        //            };
        //            ImageBrush ib = new ImageBrush();
        //            ib.ImageSource = this.reel[i].image;
        //            rect.Fill = ib;

        //            canvas.Children.Add(rect);
        //            Canvas.SetTop(rect, this.reel[i].yPos);
        //            Canvas.SetLeft(rect, this.reel[i].xPos - initDelta);
        //        }
        //    }
        //    initDelta += 10;
        //}


        public void DrawImages(int howMuch, Rectangle[] rects, int[] xPosRects)
        {
            if (tr != null)
                return;
            for (int i = 0; i < this.reel.Length; i++)
            {
                //int x = xPosRects[i] - howMuch;
                //if (x + this.reel[i].image.Width < 0)
                //    x = this.LastPos + 30; // margin!!! Unify!!!
                xPosRects[i] -= howMuch;
                if (xPosRects[i] + this.reel[i].image.Width < 0)
                {
                    int last = i - 1;
                    if (last < 0)
                        last = this.reel.Length - 1;
                    xPosRects[i] = (int)(xPosRects[last] + this.reel[last].image.Width + 30); // margin!!! Unify!!!
                }
                Canvas.SetLeft(rects[i], xPosRects[i]);
            }
            //howMuch += 10;
            //if (howMuch > 2560)
            //    howMuch -= 2560;
        }

        public void SetAnimationTranslation(TranslateTransform _tr)
        {
            tr = _tr;
        }

    }


}
