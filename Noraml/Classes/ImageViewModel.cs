using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;

namespace Noraml
{
    public class ImageViewModel
    {

        public ButtonSpawner ButtonSpawner { get; set; }

        public static ImageFiller ImageFiller { get; set; }

        public ImageViewModel()
        {
            ImageFiller = new ImageFiller(true);
            ButtonSpawner = new ButtonSpawner();  
        }

        private CommandClick _changeText;

        public CommandClick ChangeText => _changeText ?? (_changeText = new CommandClick(obj =>
        {

        }));
    }
}
