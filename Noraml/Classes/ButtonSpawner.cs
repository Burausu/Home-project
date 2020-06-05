using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Noraml
{
    public class ButtonSpawner : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Оповещает CLR о том, что было изменено некоторое свойство.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName]string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        #region Command

        private CommandClick closeWindow;

        /// <summary>
        /// Комманда для закрытия приложения
        /// </summary>
        public CommandClick CloseWindow => closeWindow ?? (closeWindow = new CommandClick(obj =>
        {
            Application.Current.Shutdown();
        }));

        private CommandClick _gotFocus;

        /// <summary>
        /// Меняет свойство <see cref="System.Windows.Controls.TextBox.Text"/> класса <see cref="System.Windows.Controls.TextBox"/> при получении фокуса.
        /// </summary>
        public CommandClick GotFocus => _gotFocus ?? (_gotFocus = new CommandClick(obj =>
        {
            TextBox textBox = obj as TextBox;
            textBox.Text = ChangedTextBoxText(textBox.Text, textBox, false);

        }));

        private CommandClick _lostFocus;

        /// <summary>
        /// Меняет свойство <see cref="System.Windows.Controls.TextBox.Text"/> класса <see cref="System.Windows.Controls.TextBox"/> при потере фокуса.
        /// </summary>
        public CommandClick LostFocus => _lostFocus ?? (_lostFocus = new CommandClick(obj =>
        {
            TextBox textBox = obj as TextBox;
            textBox.Text = ChangedTextBoxText(textBox.Text, textBox, false);

        }));

        /// <summary>
        /// Меняет текст <see cref="System.Windows.Controls.TextBox"/> с тегами.
        /// </summary>
        /// <param name="text">Свойство <see cref="System.Windows.Controls.TextBox.Text"/> класса <see cref="System.Windows.Controls.TextBox"/></param>
        /// <param name="textBox"><see cref="System.Windows.Controls.TextBox"/> с тегами.</param>
        /// <param name="isRefresh">Булево значение. Возвращает true, если страница обновлена</param>
        /// <returns></returns>
        private string ChangedTextBoxText(string text, TextBox textBox, bool isRefresh)
        {
            if (isRefresh != true)
            {
                if (text == "Search...")
                {
                    text = string.Empty;
                    textBox.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (text == string.Empty)
                {
                    isSearchedByTags = false;
                    text = "Search...";
                    textBox.Foreground = new SolidColorBrush(Colors.LightGray);
                }
            }
            else
            {
                isSearchedByTags = false;
                text = "Search...";
                textBox.Foreground = new SolidColorBrush(Colors.LightGray);
            }

            return text;
        }

        private CommandClick _clickButton;

        /// <summary>
        /// Изменяет значение страницы на следующее или предыдущее с отображением соответсвующих изображений в зависимости от нажатой кнопки.
        /// </summary>
        public CommandClick ClickButton => _clickButton ?? (_clickButton = new CommandClick(obj =>
        {
            previousPage = pageNumber;
            string buttonTag = (string)obj;
            if (buttonTag.Contains("Left Button"))
            {
                pageNumber--;
                Page = pageNumber.ToString(); 
            }
            if (buttonTag.Contains("Right Button"))
            {
                pageNumber++;
                Page = pageNumber.ToString();
            }
            CheckPage();
            if (isSearchedByTags == true)
            {
                ImageViewModel.ImageFiller.LoadItems(pageNumber, ImagesBySearchedTags);
            }
            if (isSearchedByTags == false)
                ImageViewModel.ImageFiller.LoadItems(pageNumber);

        }));

        private CommandClick _loadByPageNumber;

        /// <summary>
        /// Отображает изображения согласно введенной странице.
        /// </summary>
        public CommandClick LoadByPageNumber => _loadByPageNumber ?? (_loadByPageNumber = new CommandClick(obj =>
        {
            previousPage = pageNumber;
            pageNumber = int.Parse(obj.ToString());
            if (pageNumber > ItemsSummary)
            {
                pageNumber = previousPage;
                Page = pageNumber.ToString();
            }
            CheckPage();

            if (isSearchedByTags == true)
            {
                ImageViewModel.ImageFiller.LoadItems(pageNumber, ImagesBySearchedTags);
            }
            if (isSearchedByTags == false)
                ImageViewModel.ImageFiller.LoadItems(pageNumber);
        }));

        private CommandClick _search;

        /// <summary>
        /// Запускает поиск по тегам.
        /// </summary>
        public CommandClick Search => _search ?? (_search = new CommandClick(obj =>
        {
            string text = (string)obj;
            //text = text.ToLower();

            if (text != "Search..." && text != string.Empty)
            {
                isSearchedByTags = true;
                string[] searchedTag = text.Split(' ');
                ImagesBySearchedTags = new List<byte[]>();
                using (DataBase db = new DataBase())
                {
                    foreach (string tag2 in searchedTag)
                    {
                        List<byte[]> result = (from catalog in db.Catalogs
                                               join tag in db.Tags on catalog.TagID equals tag.TagID
                                               join image in db.Images on catalog.ImageID equals image.ImageID
                                               where tag.TagName == tag2
                                               select image.ImageSource).ToList();
                        foreach (byte[] ImageSource in result)
                        {
                            ImagesBySearchedTags.Add(ImageSource);
                        }
                        result = null;
                    }
                }
                pageNumber = 1;
                Page = pageNumber.ToString();
                ItemsSummary = Math.Ceiling((double)ImagesBySearchedTags.Count() / ImageViewModel.ImageFiller.NumberOfElements);
                CheckPage();
                ImageViewModel.ImageFiller.LoadItems(pageNumber, ImagesBySearchedTags);
            }
        }));

        private CommandClick backToMain;

        /// <summary>
        /// Возвращает пользователя на главную страницу.
        /// </summary>
        public CommandClick BackToMain => backToMain ?? (backToMain = new CommandClick(obj =>
        {
            pageNumber = 1;
            Page = pageNumber.ToString();
            using (DataBase db = new DataBase())
            {
                ItemsSummary = Math.Ceiling((double)db.Images.Count() / ImageViewModel.ImageFiller.NumberOfElements);
            }
            CheckPage();
            TextBox textBox = obj as TextBox;
            textBox.Text = ChangedTextBoxText(textBox.Text, textBox, true);
            ImageViewModel.ImageFiller.LoadItems(pageNumber);

        }));

        #endregion

        /// <summary>
        /// Базовый конструктор без параметров. Инициализирует новый экземпляр класса <see cref="ButtonSpawner"/>.
        /// </summary>
        public ButtonSpawner()
        {
            using (DataBase db = new DataBase())
            {
                ItemsSummary = Math.Ceiling((double)db.Images.Count() / ImageViewModel.ImageFiller.NumberOfElements);
            }
            VisibilityForLeftButton = Visibility.Hidden;
            VisibilityForRightButton = Visibility.Visible;
            Page = pageNumber.ToString();
        }

        /// <summary>
        /// Булево значение, возвращающее true, если поиск производится по тегам. Значение по умолчанию , false.
        /// </summary>
        bool isSearchedByTags = false;

        /// <summary>
        /// Коллекция, хранящая битовый поток изображении по соответсвующим тегам.
        /// </summary>
        List<byte[]> ImagesBySearchedTags { get; set; }

        /// <summary>
        /// Хранит предыдущую страницу. По умолчанию хранит значение 1.
        /// </summary>
        private int previousPage = 1;

        private double itemsSummary;

        /// <summary>
        /// Количество всех элементов.
        /// </summary>
        public double ItemsSummary
        { 
            get { return itemsSummary; }
            set
            {
                itemsSummary = value;
                OnPropertyChanged("ItemsSummary");
            }
        }

        private Visibility visibilityForLeftButton;

        /// <summary>
        /// Видимость для левой кнопки
        /// </summary>
        public Visibility VisibilityForLeftButton
        {
            get { return visibilityForLeftButton; }
            set
            {
                visibilityForLeftButton = value;
                OnPropertyChanged("VisibilityForLeftButton");
            }
        }

        private Visibility visibilityForRightButton;

        /// <summary>
        /// Видимость для правой кнопки
        /// </summary>
        public Visibility VisibilityForRightButton
        {
            get { return visibilityForRightButton; }
            set
            {
                visibilityForRightButton = value;
                OnPropertyChanged("VisibilityForRightButton");
            }
        }

        /// <summary>
        /// Номер текущей страницы. По умолчанию значение 1.
        /// </summary>
        private int pageNumber = 1;

        private string page;

        /// <summary>
        /// Хранит значение страницы для <see cref="System.Windows.Controls.TextBox"/> на форме.
        /// </summary>
        public string Page
        {
            get { return page; }
            set
            {
                page = value;
                OnPropertyChanged("Page");
            }
        }

        /// <summary>
        /// Устанавливает значение свойству <see cref="System.Windows.Visibility"/> для элемента <see cref="System.Windows.Controls.Button"/>.
        /// </summary>
        private void CheckPage()
        {
            if (pageNumber > 1)
                VisibilityForLeftButton = Visibility.Visible;
            if (pageNumber == 1)
                VisibilityForLeftButton = Visibility.Hidden;
            if (pageNumber == ItemsSummary)
                VisibilityForRightButton = Visibility.Hidden;
            if (pageNumber < ItemsSummary)
                VisibilityForRightButton = Visibility.Visible;
        }
    }
}
