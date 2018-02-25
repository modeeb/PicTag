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

        public IEnumerable<object> SelectedImages
        {
            get => selectedImages;
            set
            {
                selectedImages = value?.Cast<Metadata>();
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

        internal void Save()
        {
            if (selectedImages != null)
            {
                foreach (var image in selectedImages)
                {
                    //image.UpdateImageMetadata();
                    fs.Save(image);
                }
                OnPropertyChanged(nameof(SelectedFolder));
            }
        }

        internal void CopyMetadata()
        {
            copiedMetadata = ImageInfo;
        }

        internal void PasteMetadata()
        {
            if (copiedMetadata != null)
            {
                ImageInfo.UpdateImageMetadata(copiedMetadata);
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