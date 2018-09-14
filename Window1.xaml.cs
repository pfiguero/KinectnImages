using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pfiguero.Samples.ImageReel
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private ReelManager reelManager = null;

        private Rectangle[] rects = null;

        private int[] xPosRects = null;


        public Window1(ReelManager r, TranslateTransform tr)
        {
            reelManager = r;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            reelManager.SetTranslation(tr);

            InitializeComponent();

            reelManager.CreateRects(canvas, 0, out rects, out xPosRects);

        }
    }
}
