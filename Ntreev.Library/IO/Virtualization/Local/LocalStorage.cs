﻿//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Ntreev.Library.IO.Virtualization.Local
{
    public class LocalStorage : ItemContext<LocalFile, LocalFolder, LocalFileCollection, LocalFolderCollection, LocalStorage>, IStorage
    {
        private readonly Uri uri;

        public LocalStorage(Uri uri)
        {
            this.uri = uri;
            this.LoadCategories(this.Root);
        }

        public override string ToString()
        {
            return this.LocalPath;
        }

        public string LocalPath
        {
            get { return this.uri.LocalPath; }
        }

        public string Name
        {
            get { return this.LocalPath; }
        }

        public Uri Uri
        {
            get { return this.uri; }
        }

        internal string GetHashValue(LocalFile file)
        {
            using (var hashBuilder = SHA256.Create())
            using (var stream = File.OpenRead(file.LocalPath))
            {
                var data = hashBuilder.ComputeHash(stream);

                var sBuilder = new StringBuilder();

                for (var i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        private void LoadCategories(LocalFolder parentCategory)
        {
            var path = parentCategory.LocalPath;
            var dirs = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            foreach (var item in files)
            {
                var fileInfo = new FileInfo(item);
                var table = new LocalFile()
                {
                    Name = fileInfo.Name,
                    ModifiedDateTime = fileInfo.LastWriteTime,
                    Size = fileInfo.Length,
                };

                table.Category = parentCategory;
            }

            foreach (var item in dirs)
            {
                var dirInfo = new DirectoryInfo(item);
                var category = new LocalFolder()
                {
                    Name = dirInfo.Name,
                    ModifiedDateTime = dirInfo.LastWriteTime,
                    Parent = parentCategory,
                };

                this.LoadCategories(category);
            }
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {

        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {

        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {

        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        #region IStorage

        IFolder IStorage.Root
        {
            get { return this.Root; }
        }

        IFolderCollection IStorage.Folders
        {
            get { return this.Categories; }
        }

        IFileCollection IStorage.Files
        {
            get { return this.Items; }
        }

        #endregion
    }
}
