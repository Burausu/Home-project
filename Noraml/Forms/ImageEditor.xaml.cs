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

namespace Noraml
{
    /// <summary>
    /// Interaction logic for ImageEditor.xaml
    /// </summary>
    public partial class ImageEditor : Window
    {
        public ImageEditor()
        {
            InitializeComponent();
        }

        private void EditorWin_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new ImageEditorViewModel(imageName.Text, editorWin);
        }
    }
}
