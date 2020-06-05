using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Noraml
{
    public class ImageEditorViewModel : INotifyPropertyChanged
    {
        #region Commands

        private CommandClick _deleteItem;
        public CommandClick DeleteItem => _deleteItem ?? (_deleteItem = new CommandClick(obj =>
        {
            MessageBoxResult result = MessageBox.Show("Вы уверенны, что хотите удалить изображение?", "Удаление", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                using (DataBase db = new DataBase())
                {
                    Image item = db.Images.Where(s => s.ImageName == _imageName).Single();
                    db.Images.Remove(item);
                    db.SaveChanges();
                    ChangeItemsId(item.ImageID);
                }
            }
        }));

        private CommandClick _saveItem;

        public CommandClick SaveItem => _saveItem ?? (_saveItem = new CommandClick(obj =>
        {
            MessageBoxResult result = MessageBox.Show("Вы уверенны, что хотите сохранить изменения?", "Сохранение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                //ItemCollection items = obj as ItemCollection;
                using (DataBase db = new DataBase())
                {
                    Image image = db.Images.Single(s => s.ImageName == _imageName);

                    string[] tagDB = (from catalog in db.Catalogs
                               join tag in db.Tags on catalog.TagID equals tag.TagID
                               join images in db.Images on catalog.ImageID equals images.ImageID
                               where images.ImageName == _imageName
                               select tag.TagName).ToArray();
                    if (tagDB.Length > ImageTags.Count)
                    {
                        foreach (string tag in tagDB)
                        {
                            if (!ImageTags.Contains(tag))
                            {
                                int[] t = (from catalog in db.Catalogs join tag2 in db.Tags on catalog.TagID equals tag2.TagID join images in db.Images on catalog.ImageID equals images.ImageID
                                                  where images.ImageName == _imageName
                                                  where tag2.TagName == tag
                                              select catalog.CatalogID).ToArray();
                                int ID = t[0];
                                Catalog removeRow = db.Catalogs.Single(s => s.CatalogID == ID);
                                db.Catalogs.Remove(removeRow);
                                db.SaveChanges();
                            }
                        }
                    }
                    foreach (string item in ImageTags)
                    {
                        if (tagDB.Length != 0)
                        {
                            if (!tagDB.Contains(item))
                            {
                                db.Database.ExecuteSqlCommand($"INSERT INTO Catalog(ImageID, TagID) VALUES (\'{image.ImageID}\', \'{db.Tags.Where(s => s.TagName == item).Single().TagID}\')");
                            }
                        }
                        else
                        {
                            db.Database.ExecuteSqlCommand($"INSERT INTO Catalog(ImageID, TagID) VALUES (\'{image.ImageID}\', \'{db.Tags.Where(s => s.TagName == item).Single().TagID}\')");
                        }
                    }
                }
                window.Close();
            }
        }));

        private CommandClick _deleteTag;

        public CommandClick DeleteTag => _deleteTag ?? (_deleteTag = new CommandClick(obj =>
        {
            using(DataBase db = new DataBase())
            {
                ImageTags.Remove((string)obj);
                //string tagName = (string)obj;
                //ImageTags.Remove(tagName);
                //int[] t = (from catalog in db.Catalogs
                // join tag in db.Tags on catalog.TagID equals tag.TagID
                // join images in db.Images on catalog.ImageID equals images.ImageID
                // where images.ImageName == _imageName
                // where tag.TagName == tagName
                //           select catalog.CatalogID).ToArray();
                //int ID = t[0];
                //Catalog removeRow = db.Catalogs.Single(s => s.CatalogID == ID);
                //db.Catalogs.Remove(removeRow);
                //db.SaveChanges();
            }
        }));

        private CommandClick _addTag;

        public CommandClick AddTag => _addTag ?? (_addTag = new CommandClick(obj =>
        {
            if (!ImageTags.Contains((string)obj))
                ImageTags.Add((string)obj);
        }));

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private readonly string _imageName;

        private readonly Window window;

        public ObservableCollection<string> AllTags { get; set; }

        private ObservableCollection<string> _imageTags;

        public ObservableCollection<string> ImageTags
        {
            get { return _imageTags; }
            set
            {
                _imageTags = value;
                OnPropertyChanged("ImageTags");
            }
        }

        private string _selectedTag;

        public string SelectedTag
        {
            get { return _selectedTag; }
            set
            {
                _selectedTag = value;
                OnPropertyChanged("SelectedTag");
            }
        }

        public ImageEditorViewModel(string imageName, Window window)
        {
            this.window = window;
            _imageName = imageName;
            AllTags = new ObservableCollection<string>();
            FillTagsList();
        }


        private void FillTagsList()
        {
            using (DataBase db = new DataBase())
            {
                for(int i = 1; i <= db.Tags.Count(); i++)
                {
                    Tag tag = db.Tags.Single(s => s.TagID == i);
                    AllTags.Add(tag.TagName);
                }

                ImageTags = new ObservableCollection<string>(from catalog in db.Catalogs
                          join tag in db.Tags on catalog.TagID equals tag.TagID
                          join images in db.Images on catalog.ImageID equals images.ImageID
                          where images.ImageName == _imageName
                          select tag.TagName);
            }
        }

        /// <summary>
        /// Забудем об этом недоразумении.
        /// </summary>
        /// <param name="removedId"></param>
        private void ChangeItemsId(int removedId)
        {
            using (DataBase db = new DataBase())
            {
                //db.Database.ExecuteSqlCommand("ALTER TABLE Images DROP PK_Images;");
                //db.Database.ExecuteSqlCommand("ALTER TABLE Images ALTER COLUMN ImageID INT;");
                for (int i = removedId + 1; i <= db.Images.Count() + 1; i++)
                {
                    Image img = db.Images.Where(s => s.ImageID == i).Single();
                    Image image = new Image()
                    {
                        ImageID = img.ImageID - 1,
                        ImageSource = img.ImageSource,
                        ImageName = img.ImageName,
                    };
                    db.Images.Remove(img);
                    db.Images.Add(image);
                    db.SaveChanges();
                    img = null;
                    image = null;
                }
                //db.Database.ExecuteSqlCommand("ALTER TABLE Images ADD CONSTRAINT PK_Images PRIMARY KEY CLUSTERED(ImageID);");
            }
        }
    }
}