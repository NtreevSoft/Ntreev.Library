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

namespace Ntreev.Library.IO.Virtualization.Memory
{
    public class MemoryStorage : ItemContext<MemoryFile, MemoryFolder, MemoryFileCollection, MemoryFolderCollection, MemoryStorage>, IStorage
    {
        private string name;
        private Uri uri = null;

        public MemoryStorage()
            : this(string.Empty)
        {

        }

        public MemoryStorage(string name)
        {
            this.name = name ?? string.Empty;
        }

        public override string ToString()
        {
            return this.name;
        }

        public string Name
        {
            get { return this.name; }
        }

        public Uri Uri
        {
            get { return this.uri; }
        }

        internal string GetHashValue(MemoryFile file)
        {
            using (SHA256 hashBuilder = SHA256.Create())
            using (Stream stream = new MemoryStream(file.Data))
            {
                byte[] data = hashBuilder.ComputeHash(stream);

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
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
