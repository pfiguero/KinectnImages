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
    using System.Windows.Shapes;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
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
            public int xPos;
            public int yPos;
        }

        public InfoReel[] reel = null;

        public int LastPos { get; set;  }

        TranslateTransform tr = null;

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

        public void CreateRects(Canvas canvas, int initDelta, out Rectangle[] rects, out int[] xPosRects)
        {
            int screenWidth = 1280;

            // Define positions in the reel...
            int marginX = 30;
            int screenHeight = 720;
            reel[0].xPos = marginX;
            reel[0].yPos = (int)(screenHeight - reel[0].image.Height) / 2;
            for (int i = 1; i < reel.Length; i++)
            {
                reel[i].xPos = (int)(reel[i - 1].xPos + reel[i - 1].image.Width + marginX);
                reel[i].yPos = (int)(screenHeight - reel[i].image.Height) / 2;
            }
            LastPos = (int)(reel[reel.Length - 1].xPos + reel[reel.Length - 1].image.Width + marginX);

            rects = new Rectangle[this.reel.Length];
            xPosRects = new int[this.reel.Length];
         
            for (int i = 0; i < this.reel.Length; i++)
            {
                int imgStart = this.reel[i].xPos;
                int imgEnd = (int)(this.reel[i].xPos + this.reel[i].image.Width);
                rects[i] = new Rectangle()
                {
                    Width = this.reel[i].image.Width,
                    Height = this.reel[i].image.Height
                };
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = this.reel[i].image;
                rects[i].Fill = ib;

                canvas.Children.Add(rects[i]);
                Canvas.SetTop(rects[i], this.reel[i].yPos);
                xPosRects[i] = this.reel[i].xPos - initDelta;
                Canvas.SetLeft(rects[i], xPosRects[i]);

                if(tr != null)
                {
                    rects[i].RenderTransform = tr;
                }
            }
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

        public void SetTranslation(TranslateTransform _tr)
        {
            tr = _tr;
        }

    }


}
