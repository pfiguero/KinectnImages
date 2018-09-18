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
        private App app=null;

        public Window1(App _app)
        {
            app = _app;
            // use the window object as the view model in this simple example
            this.DataContext = this;

            InitializeComponent();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void DoubleClick(object sender, RoutedEventArgs e)
        {
            app.DoubleClick();
        }
    }
}
