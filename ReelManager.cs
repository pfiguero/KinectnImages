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

    // Computed info from the input structure
    public abstract class InfoReelAbs
    {
        public abstract ImageSource Image { get; set; }
        public TransformGroup trGroup;
        public TransformGroup trGroupLocal;
        public ScaleTransform scaleT;
        public bool IsBig { get; set; }

        public InfoReelAbs() { }
    }

    public class InfoReel: InfoReelAbs
    {
        private ImageSource image;

        public override ImageSource Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
            }
        }

        public InfoReel(): base() { }
    }

    public class ReelManager<InfoReelT> where InfoReelT : InfoReelAbs, new()
    {
        // Input structure from the file
        // The last image is used as background.
        protected struct MyFile
        {
            public ImageInfo[] images;
        }
        protected struct ImageInfo
        {
            public String filename;
            public String title;
        }

        private InfoReelT[] reel = null;

        protected ImageSource background = null;

        protected double LastPos { get; set;  }

        // for the animation of the reel
        protected TranslateTransform tr = null;
        protected double width;
        protected double height;

        public ReelManager(String jsonName, double _width, double _height )
        {
            width = _width;
            height = _height;
            //// Background
            //Uri uri = new System.Uri(Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\PromociónRedes.jpg"));
            //ImageSource imageSource = new BitmapImage(uri);
            // Open the json file and de serialize it
            String p = @"..\..\..\data\" + jsonName;
            MyFile f = JsonConvert.DeserializeObject<MyFile>(File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, p)));

            // Last image is saved for background
            reel = new InfoReelT[f.images.Length-1];
            Uri uri;
            char[] seps = { '.' };
            String directory = jsonName.Split(seps)[0];
            String s;
            for (int i = 0; i < reel.Length; i++)
            {
                s = @"..\..\..\data\" + directory + "\\" + f.images[i].filename;
                uri = new System.Uri(System.IO.Path.Combine(Environment.CurrentDirectory, s));
                reel[i] = new InfoReelT();
                reel[i].Image = CheckSize(new BitmapImage(uri), width, height);
                // Debug.WriteLine("Width: " + reel[i].image.Width + " Height: " + reel[i].image.Height);
            }

            s = @"..\..\..\data\" + directory + "\\" + f.images[f.images.Length-1].filename;
            uri = new System.Uri(System.IO.Path.Combine(Environment.CurrentDirectory, s));
            background = CheckSize(new BitmapImage(uri), width, height);

            tr = null;
        }

        private ImageSource CheckSize(ImageSource img, double width, double height)
        {
            ImageSource result = img;
            if(img.Width > width || img.Height > height )
            {
                double ratioWidth = width / img.Width ;
                double ratioHeight = height / img.Height;
                if( ratioWidth > ratioHeight )
                {
                    result = CreateResizedImage(img, (int) (img.Width * ratioHeight), (int) (img.Height * ratioHeight), 1);
                }
                else
                {
                    result = CreateResizedImage(img, (int)(img.Width * ratioWidth), (int)(img.Height * ratioWidth), 1);
                }
            }
            return result;
        }

        private static ImageSource CreateResizedImage(ImageSource source, int width, int height, int margin)
        {
            var rect = new System.Windows.Rect(margin, margin, width - margin * 2, height - margin * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
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
            reel[0].scaleT = new ScaleTransform();
            reel[0].scaleT.ScaleX = 1;
            reel[0].scaleT.ScaleY = 1;
            reel[0].IsBig = true;
            reel[0].trGroupLocal.Children.Add(reel[0].scaleT);
            reel[0].trGroupLocal.Children.Add(new TranslateTransform(0.0, (screenHeight - reel[0].Image.Height) / 2));

            LastPos += trMargin.X + reel[0].Image.Width;

            for (int i = 1; i < reel.Length; i++)
            {
                reel[i].trGroup = new TransformGroup();
                reel[i].trGroup.Children.Add(reel[i-1].trGroup);
                reel[i].trGroup.Children.Add(new TranslateTransform(reel[i-1].Image.Width,0.0));
                reel[i].trGroup.Children.Add(trMargin);

                reel[i].trGroupLocal = new TransformGroup();
                reel[i].trGroupLocal.Children.Add(reel[i].trGroup);
                reel[i].scaleT = new ScaleTransform();
                reel[i].scaleT.ScaleX = 1;
                reel[i].scaleT.ScaleY = 1;
                reel[i].IsBig = true;
                reel[i].trGroupLocal.Children.Add(reel[i].scaleT);
                reel[i].trGroupLocal.Children.Add(new TranslateTransform(0.0, (screenHeight - reel[i].Image.Height) / 2));

                LastPos += trMargin.X + reel[i].Image.Width;
            }

            LastPos += trMargin.X + reel[reel.Length-1].Image.Width;

            Canvas canvas = w.canvas;
            // background
            {
                Rectangle r = new Rectangle()
                {
                    Width = width,
                    Height = height
                };
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = background;
                r.Fill = ib;
                canvas.Children.Add(r);
                Canvas.SetTop(r, 0.0);
                Canvas.SetLeft(r, 0.0);
            }

            // Reel
            Rectangle[] rects = new Rectangle[this.reel.Length];         
            for (int i = 0; i < this.reel.Length; i++)
            {
                rects[i] = new Rectangle()
                {
                    Width = this.reel[i].Image.Width,
                    Height = this.reel[i].Image.Height
                };
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = this.reel[i].Image;
                rects[i].Fill = ib;
                rects[i].RenderTransform = reel[i].trGroupLocal;

                canvas.Children.Add(rects[i]);
            }

        }

        public void StartAnimation()
        {
            DoubleAnimation da = null;

            da = new DoubleAnimation(0, -LastPos, new Duration(TimeSpan.FromSeconds(200)));
            da.SpeedRatio = 5;
            da.AccelerationRatio = .1;
            da.RepeatBehavior = RepeatBehavior.Forever;
            tr.BeginAnimation(TranslateTransform.XProperty, da);
        }

        private static bool stopping = true;

        public void ToggleStop()
        {
            DoubleAnimation da = null;
            if (stopping)
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

        public void ToggleSize()
        {
            DoubleAnimation da = null;
            //Debug.WriteLine("ToggleSize");
            for (int i=0;i<reel.Length;i++)
            {
                if (reel[i].IsBig)
                {
                    da = new DoubleAnimation(fromValue: 1, toValue: 0.5, duration: new Duration(TimeSpan.FromSeconds(2)));
                    da.SpeedRatio = 5;
                    da.AccelerationRatio = .1;
                    reel[i].scaleT.BeginAnimation(ScaleTransform.ScaleXProperty, da);
                    reel[i].scaleT.BeginAnimation(ScaleTransform.ScaleYProperty, da);
                    reel[i].IsBig = false;
                }
                else
                {
                    da = new DoubleAnimation(fromValue: 0.5, toValue: 1, duration: new Duration(TimeSpan.FromSeconds(2)));
                    da.SpeedRatio = 5;
                    da.AccelerationRatio = .1;
                    reel[i].scaleT.BeginAnimation(ScaleTransform.ScaleXProperty, da);
                    reel[i].scaleT.BeginAnimation(ScaleTransform.ScaleYProperty, da);
                    reel[i].IsBig = true;
                }
            }
        }



        // If the animation finishes, it means it was interrupted. Create it again from the beginning
        private void CompletedAnimation(object sender, EventArgs e)
        {
            DoubleAnimation da = null;

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


        //public void DrawImages(int howMuch, Rectangle[] rects, int[] xPosRects)
        //{
        //    if (tr != null)
        //        return;
        //    for (int i = 0; i < this.reel.Length; i++)
        //    {
        //        //int x = xPosRects[i] - howMuch;
        //        //if (x + this.reel[i].image.Width < 0)
        //        //    x = this.LastPos + 30; // margin!!! Unify!!!
        //        xPosRects[i] -= howMuch;
        //        if (xPosRects[i] + this.reel[i].image.Width < 0)
        //        {
        //            int last = i - 1;
        //            if (last < 0)
        //                last = this.reel.Length - 1;
        //            xPosRects[i] = (int)(xPosRects[last] + this.reel[last].image.Width + 30); // margin!!! Unify!!!
        //        }
        //        Canvas.SetLeft(rects[i], xPosRects[i]);
        //    }
        //    //howMuch += 10;
        //    //if (howMuch > 2560)
        //    //    howMuch -= 2560;
        //}

        public void SetAnimationTranslation(TranslateTransform _tr)
        {
            tr = _tr;
        }

    }


}
