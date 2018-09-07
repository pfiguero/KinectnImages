namespace Pfiguero.Samples.ImageReel
{
    using System;
    using Newtonsoft.Json;

    public class ImageManager
    {
        public ImageManager()
        {
            // Open the json file
            var json = new StreamReader(Path.Combine(Environment.CurrentDirectory, @"..\..\..\data\50anios.json"));
            JsonTextReader reader = new JsonTextReader(json);
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    Debug.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                }
                else
                {
                    Debug.WriteLine("Token: {0}", reader.TokenType);
                }
            }

        }
    }
}
