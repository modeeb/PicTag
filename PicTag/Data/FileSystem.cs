using PicTag.Properties;
using System;
using System.Collections.Generic;
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
            meta.Source = Image.FromFile(meta.FullName);
            meta.UpdateImageMetadata();

            string path = meta.FullName;
            while (File.Exists(path))
            {
                path = Path.GetDirectoryName(path) + "\\_" + Path.GetFileName(path);
            }
            meta.Source.Save(path);
            meta.Source.Dispose();
            File.Delete(meta.FullName);

            // release memory now
            //Pic.Dispose();
            //Pic = null;
            //GC.Collect();

            //// delete the temporary picture
            //File.Delete(FilenameTemp);

            //string new_path = Path.GetDirectoryName(meta.FullName) + "\\_" + Path.GetFileName(meta.FullName);
            //meta.Source.Save(new_path);
        }

        internal IEnumerable<Metadata> GetImages(string dir)
        {
            var imageFiles = GetImageFiles(new DirectoryInfo(dir));
            foreach (FileInfo file in imageFiles)
            {
                using (
                var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)
                    //;
                    )
                {
                    Metadata meta;
                    try
                    {
                        meta = new Metadata(stream);
                        //Image image = Image.FromFile(file.FullName);
                        //Metadata meta = image.GetMetadata();
                        meta.AppendFileInfo(file.Name, file.FullName, file.LastWriteTime);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    yield return meta;
                }
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
