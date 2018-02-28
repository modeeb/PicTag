using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PicTag.Data
{
    internal class FormState : INotifyPropertyChanged
    {
        private FileSystem fs;
        private Metadata imageInfo;
        private Metadata copiedMetadata;
        private IEnumerable<Metadata> selectedImages;
        private IEnumerable<Metadata> listedImages;

        public FormState(FileSystem fs)
        {
            this.fs = fs;
        }

        public string RootFolder
        {
            get => fs.RootFolder;
            internal set
            {
                if (value != fs.RootFolder)
                {
                    fs.RootFolder = value;
                    SelectedFolder = fs.RootFolder;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedFolder
        {
            get => fs.SelectedFolder;
            internal set
            {
                if (value != fs.SelectedFolder)
                {
                    fs.SelectedFolder = value;
                    listedImages = fs.GetImages(value);
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<Metadata> SelectedImages
        {
            get => selectedImages;
            set
            {
                selectedImages = value;
                OnPropertyChanged();
                ImageInfo = selectedImages?.FirstOrDefault();
            }
        }

        public Metadata ImageInfo
        {
            get => imageInfo;
            internal set
            {
                imageInfo = value;
                OnPropertyChanged();
            }
        }

        internal IEnumerable<Metadata> Images => listedImages;

        internal void SaveSelected()
        {
            Save(selectedImages);
        }

        internal void Save(IEnumerable<Metadata> savinglist)
        {
            if (savinglist != null)
            {
                foreach (var image in savinglist)
                {
                    fs.Save(image);
                }
            }
        }

        internal void SaveAs(string newPath)
        {
            if (selectedImages != null)
            {
                fs.SaveAs(imageInfo, newPath);
            }
        }

        internal void CopyMetadata()
        {
            copiedMetadata = ImageInfo;
        }

        internal void DeleteSelected()
        {
            if (selectedImages != null)
            {
                foreach (var image in selectedImages)
                {
                    fs.Delete(image);
                }
                OnPropertyChanged(nameof(SelectedFolder));
            }
        }

        internal void PasteMetadata()
        {
            if (copiedMetadata != null)
            {
                foreach (var image in selectedImages)
                {
                    copiedMetadata.DateTimeOriginal = copiedMetadata.DateTimeOriginal.AddMilliseconds(1);
                    image.UpdateImageMetadata(copiedMetadata);
                }
                OnPropertyChanged(nameof(ImageInfo));
            }
        }

        internal string GetParentName(object dir)
        {
            return fs.GetDirectory(dir).Name;
        }

        internal T[] GetChildren<T>(object dir, Converter<object, T> converter)
        {
            return fs.GetChildren(dir, converter);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}