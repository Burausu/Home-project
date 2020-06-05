using System.Windows;
using System.Windows.Input;
using System.Linq;
using WpfAnimatedGif;

namespace Noraml
{
    /// <summary>
    /// Interaction logic for ImagesWindow.xaml
    /// </summary>
    public partial class ImagesWindow : Window
    {
        public ImagesWindow()
        {
            InitializeComponent();
            DataContext = new ImageViewModel();
        }

        /// <summary>
        /// Перенаправляет на форму добавления тегов.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ImageEditor w = new ImageEditor();
            ImageInfo element = listBox.SelectedItem as ImageInfo;
            w.imageName.Text = element.NameInDataBase;
            using (DataBase db = new DataBase())
            {
                if (db.Images.Where(s => s.ImageName == element.NameInDataBase).Single().ImageExstension == "gif")
                {
                    ImageBehavior.SetAnimatedSource(w.currentImage, element.Source);
                }
                else
                {
                    w.currentImage.Source = element.Source;
                }
            }
            w.ShowDialog();
        }

        /// <summary>
        /// Запрещает писать в <see cref="System.Windows.Controls.TextBox"/> что-то, кроме цифр.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!char.IsDigit((char)KeyInterop.VirtualKeyFromKey(e.Key)) & (e.Key != Key.Back | e.Key == Key.Space) & (e.Key != Key.Enter | e.Key != Key.Return))
            {
                e.Handled = true;
            }
        }
    }
}
