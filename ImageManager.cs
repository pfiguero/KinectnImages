namespace Pfiguero.Samples.ImageReel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Newtonsoft.Json;

    public class ImageManager
    {
        struct ImageInfo
        {
            public String filename;
            public String title;
        }
        struct MyFile
        {
            public String directory;
            public ImageInfo[] images;
        }

        public struct InfoReel
        {
            public ImageSource image;
            public int xPos;
            public int yPos;
        }

        public InfoReel[] reel = null;

        public int LastPos { get; }

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

        public ImageManager()
        {
            //// Background
            //Uri uri = new System.Uri(Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\PromociónRedes.jpg"));
            //ImageSource imageSource = new BitmapImage(uri);
            // Open the json file and de serialize it
            MyFile f = JsonConvert.DeserializeObject<MyFile>(File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\50anios.json")));

            reel = new InfoReel[f.images.Length];
            Uri uri;
            for (int i = 0; i < reel.Length; i++)
            {
                String s = @"..\..\..\images\" + f.directory + "\\" + f.images[i].filename;
                uri = new System.Uri(System.IO.Path.Combine(Environment.CurrentDirectory, s));
                reel[i].image = new BitmapImage(uri);
                Debug.WriteLine("Width: " + reel[i].image.Width + " Height: " + reel[i].image.Height);
            }

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
        }
        //'C:\Users\COLIVRI\Documents\KinectnImages\images\50anios\002-22.jpg'.'
    }

}
