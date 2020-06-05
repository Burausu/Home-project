using System;
using System.Linq;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Collections.ObjectModel;
using WImage = System.Windows.Controls.Image;

namespace Noraml
{
    public class ImageFiller : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Оповещает CLR о том, что было изменено некоторое свойство.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName]string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        /// <summary>
        /// Количество элементов на форме.
        /// </summary>
        public int NumberOfElements { get; set; } = 10;

        private ObservableCollection<WImage> _images;

        /// <summary>
        /// <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>, содержащая в себе изображения из базы данных.
        /// </summary>
        public ObservableCollection<WImage> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                OnPropertyChanged("Images");
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImageFiller"/>.
        /// </summary>
        /// <param name="isFirstLoad">Возвращает значение true, если загрузка производится впервые. По умолчание возвращает значение true.</param>
        public ImageFiller(bool isFirstLoad)
        {
            Images = new ObservableCollection<WImage>();
            LoadItems(isFirstLoad);
        }

        /// <summary>
        /// Конвертирует байтовый поток в изображение типа <see cref="System.Windows.Media.Imaging.BitmapImage"/>.
        /// </summary>
        /// <param name="source">Источник изображения.</param>
        /// <returns></returns>
        public BitmapImage ConvertFromByte(byte[] source)
        {
            MemoryStream stream = new MemoryStream(source);
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = stream;
            img.EndInit();
            img.Freeze();
            return img;

            //using (MemoryStream stream = new MemoryStream(source))
            //{
            //    BitmapImage img = new BitmapImage();
            //    img.BeginInit();
            //    img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            //    img.CacheOption = BitmapCacheOption.OnLoad;
            //    img.StreamSource = stream;
            //    img.EndInit();
            //    img.Freeze();
            //    return img;
            //}
        }

        /// <summary>
        /// Очищает и удаляет коллекцию
        /// </summary>
        private void Clear()
        {
            if (Images != null)
            {
                Images.Clear();
                Images = null;
            }
        }

        /// <summary>
        /// Загружает изображения в коллекцию <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> при нажатии на кнопку.
        /// </summary>
        /// <param name="pageNumber">Номер страницы</param>
        public void LoadItems(int pageNumber)
        {
            Clear();
            Images = new ObservableCollection<WImage>();
            using (DataBase db = new DataBase())
            {
                int index = NumberOfElements * pageNumber - NumberOfElements;
                if (index == 0)
                    index = 1;
                if (pageNumber > 1)
                {
                    for (int imageIndex = index + 1; imageIndex <= NumberOfElements * pageNumber; imageIndex++)
                    {
                        if (imageIndex < db.Images.Count())
                        {
                            ImageInfo img = new ImageInfo { Source = ConvertFromByte(db.Images.Where(s => s.ImageID == imageIndex).Single().ImageSource), NameInDataBase = db.Images.Where(s => s.ImageID == imageIndex).Single().ImageName, Stretch = Stretch.UniformToFill };
                            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant);
                            Images.Add(img);
                            img = null;
                        }
                    }
                }
                else
                {
                    for (int imageIndex = index; imageIndex <= NumberOfElements * pageNumber; imageIndex++)
                    {
                        if (imageIndex < db.Images.Count())
                        {
                            ImageInfo img = new ImageInfo { Source = ConvertFromByte(db.Images.Where(s => s.ImageID == imageIndex).Single().ImageSource), NameInDataBase = db.Images.Where(s => s.ImageID == imageIndex).Single().ImageName, Stretch = Stretch.UniformToFill };
                            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant);
                            Images.Add(img);
                            img = null;
                        }
                    }
                }
            }  
        }

        public void LoadItems(int pageNumber, List<byte[]> imageSources)
        {
            Clear();
            Images = new ObservableCollection<WImage>();
            using (DataBase db = new DataBase())
            {
                int index = NumberOfElements * pageNumber - NumberOfElements;
                if (index == 0)
                    index = 1;
                if (pageNumber > 1)
                {
                    for (int imageIndex = index + 1; imageIndex <= NumberOfElements * pageNumber; imageIndex++)
                    {
                        if (imageIndex < imageSources.Count)
                        {
                            try
                            {
                                byte[] t = imageSources[imageIndex];
                                ImageInfo img = new ImageInfo { Source = ConvertFromByte(imageSources[imageIndex - 1]), Stretch = Stretch.UniformToFill, NameInDataBase = db.Images.Where(s => s.ImageSource == t).FirstOrDefault().ImageName };
                                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant);
                                Images.Add(img);
                                img = null;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                        }
                    }
                }
                else
                {
                    for (int imageIndex = index; imageIndex <= NumberOfElements * pageNumber; imageIndex++)
                    {
                        if (imageIndex <= imageSources.Count)
                        {
                            byte[] t = imageSources[imageIndex - 1];
                            ImageInfo img = new ImageInfo { Source = ConvertFromByte(imageSources[imageIndex - 1]), Stretch = Stretch.UniformToFill, NameInDataBase = db.Images.Where(s => s.ImageSource == t).FirstOrDefault().ImageName };
                            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant);
                            Images.Add(img);
                            img = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Загружает изображения в коллекцию <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> при первой загрузке.
        /// </summary>
        /// <param name="isFirstLoad">Возвращает значение true, если загрузка производится впервые.</param>
        public void LoadItems(bool isFirstLoad)
        {
            using (DataBase db = new DataBase())
            {
                if (isFirstLoad == true)
                {
                    for (int imageIndex = 1; imageIndex <= NumberOfElements; imageIndex++)
                    {
                        if (imageIndex < db.Images.Count())
                        {
                            ImageInfo img = new ImageInfo { Source = ConvertFromByte(db.Images.Where(s => s.ImageID == imageIndex).Single().ImageSource), NameInDataBase = db.Images.Where(s => s.ImageID == imageIndex).Single().ImageName, Stretch = Stretch.UniformToFill };
                            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant);
                            Images.Add(img);
                            img = null;
                        }

                    }
                }
            }
        }
    }
}
