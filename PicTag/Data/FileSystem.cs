using PicTag.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicTag.Data
{
    class FileSystem
    {
        private DirectoryInfo rootFolder;
        private DirectoryInfo selectedFolder;

        public FileSystem()
        {
            rootFolder = GetDefaultFolder();
        }

        public string RootFolder
        {
            get => rootFolder.FullName;
            internal set
            {
                rootFolder = new DirectoryInfo(value);
            }
        }

        public string SelectedFolder
        {
            get => selectedFolder?.FullName;
            internal set
            {
                if (value != selectedFolder?.FullName)
                {
                    selectedFolder = new DirectoryInfo(value);
                }
            }
        }

        internal DirectoryInfo GetDefaultFolder()
        {
            string defaultPath;

            defaultPath = Settings.Default.DefaultPath;
            if (string.IsNullOrEmpty(defaultPath))
            {
                defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
            
            return new DirectoryInfo(defaultPath);
        }

        internal DirectoryInfo GetDirectory(object dir)
        {
            return (DirectoryInfo)dir ?? rootFolder;
        }

        internal TOutput[] GetChildren<TOutput>(object dir, Converter<DirectoryInfo, TOutput> converter)
        {
            return Array.ConvertAll(GetDirectories(GetDirectory(dir)).ToArray(), converter);
        }

        internal IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo dir)
        {
            return dir.EnumerateDirectories().Where(d => CanRead(d));
        }

        internal bool CanRead(DirectoryInfo dir)
        {
            try
            {
                dir.EnumerateDirectories();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        internal void Save(Metadata meta)
        {
            SaveAs(meta, meta.FullName);
        }

        internal void SaveAs(Metadata meta, string newPath)
        {
            string tempPath = newPath;
            using (var stream = new FileStream(meta.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                meta.Source = Image.FromStream(stream);
                meta.UpdateImageMetadata();

                while (File.Exists(tempPath))
                {
                    tempPath = Path.GetDirectoryName(tempPath) + "\\_" + Path.GetFileName(tempPath);
                }
                meta.Source.Save(tempPath);
            }

            if (tempPath != newPath)
            {
                File.Delete(newPath);

                using (var stream = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    meta.Source = Image.FromStream(stream);
                    meta.Source.Save(newPath);
                    File.SetLastWriteTime(newPath, meta.DateTimeOriginal);
                    File.SetCreationTime(newPath, meta.DateTimeOriginal);
                }
                File.Delete(tempPath);
            }
            GC.Collect();
        }

        internal void Delete(Metadata image)
        {
            File.Delete(image.FullName);
        }

        internal IEnumerable<Metadata> GetImages(string dir)
        {
            var imageFiles = GetImageFiles(new DirectoryInfo(dir));
            foreach (FileInfo file in imageFiles)
            {
                Metadata meta;
                try
                {
                    meta = GetMetadata(file);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message + " " + file.FullName);
                    continue;
                }
                yield return meta;
            }
        }

        private Metadata GetMetadata(FileInfo file)
        {
            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Metadata meta = new Metadata(stream);
                meta.AppendFileInfo(file.Name, file.FullName, file.LastWriteTime);
                return meta;
            }
        }

        internal IEnumerable<FileInfo> GetImageFiles(DirectoryInfo dir)
        {
            return dir.EnumerateFiles().Where(IsImage);
        }

        private bool IsImage(FileInfo file)
        {
            string fileName = file.Name.ToLower();
            return fileName.EndsWith(".jpg") ||
                fileName.EndsWith(".jpeg") ||
                fileName.EndsWith(".png") ||
                fileName.EndsWith(".bmp");
        }
    }
 }
