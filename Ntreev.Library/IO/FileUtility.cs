//Released under the MIT License.
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

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Ntreev.Library;
using System.Security.Cryptography;
using Ntreev.Library.ObjectModel;

namespace Ntreev.Library.IO
{
    public static class FileUtility
    {
        private const string backupPostFix = ".bak";

        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            var isExistPrevImage = false;
            var attr = FileAttributes.Archive;
            if (File.Exists(destFileName) == true)
            {
                attr = File.GetAttributes(destFileName);
                File.SetAttributes(destFileName, FileAttributes.Archive);
                isExistPrevImage = true;
            }

            File.Copy(sourceFileName, destFileName, true);

            if (isExistPrevImage == true)
                File.SetAttributes(destFileName, attr);

        }

        public static void SetAttribute(string path, FileAttributes fileAttributes)
        {
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                SetAttribute(dir, fileAttributes);
            }

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                File.SetAttributes(file, fileAttributes);
            }
        }

        public static void OverwriteString(string filename, string text)
        {
            FileInfo file = new FileInfo(filename);
            file.MoveTo(file.FullName + ".bak");

            using (StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                sw.Write(text);
            }

            file.Delete();
        }

        public static string GetString(string filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                return sr.ReadToEnd();
            }
        }

        public static string ToAbsolutePath(string path)
        {
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri == false)
            {
                return Path.Combine(Directory.GetCurrentDirectory(), path);
            }

            return uri.LocalPath;
        }

        public static string ToLocalPath(string path, string dirPath)
        {
            if (path.IndexOf(ToAbsolutePath(dirPath), StringComparison.CurrentCultureIgnoreCase) < 0)
                throw new ArgumentException("파일 경로에 부모 경로가 포함되어 있지 않습니다.", nameof(path));
            if (path == dirPath)
                return string.Empty;

            var pathUri = new Uri(path);
            if (dirPath.Last() != Path.DirectorySeparatorChar)
                dirPath += Path.DirectorySeparatorChar;
            var dirUri = new Uri(dirPath);
            var relativeUri = dirUri.MakeRelativeUri(pathUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return relativePath;
        }

        public static bool IsFile(string path)
        {
            return DirectoryUtility.IsDirectory(path) == false;
        }

        public static string GetHash(string filename)
        {
            using (var sha = new SHA256CryptoServiceProvider())
            using (var stream = File.OpenRead(filename))
            {
                var bytes = sha.ComputeHash(stream);

                var sBuilder = new StringBuilder();
                for (var i = 0; i < bytes.Length; i++)
                {
                    sBuilder.Append(bytes[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static void Prepare(string filename)
        {
            var fileInfo = new FileInfo(filename);
            var directory = Path.GetDirectoryName(fileInfo.FullName);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);
        }

        /// <summary>
        /// 주어진 인자를 결합하여 파일 경로를 만들고 파일의 디렉토리가 없을때 디렉토리를 생성합니다.
        /// </summary>
        public static string Prepare(params string[] paths)
        {
            var filename = Path.Combine(paths);
            var fileInfo = new FileInfo(filename);
            var directory = Path.GetDirectoryName(fileInfo.FullName);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);
            return fileInfo.FullName;
        }

        public static string GetFilename(string basePath, IItem item)
        {
            var path1 = basePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
            var path2 = item.Path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(path1, path2);
        }

        public static string GetFilename(string basePath, IItem item, string extension)
        {
            var path1 = basePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
            var path2 = item.Path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(path1, path2, extension);
        }

        public static string ChangeExtension(string filename, string extension)
        {
            return Path.ChangeExtension(filename, extension);
        }

        public static string RemoveExtension(string filename)
        {
            string extension = Path.GetExtension(filename);
            return filename.Substring(0, filename.Length - extension.Length);
        }

        public static bool IsAbsolute(string filename)
        {
            // svnadmin 으로 저장소를 복구한 경우 mac os 환경에서 절대 경로를 IsAbsoluteUri=false 를 반환하는 버그
            // return new Uri(filename, UriKind.RelativeOrAbsolute).IsAbsoluteUri;
            try
            {
                return new Uri(filename).IsAbsoluteUri;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static void Delete(string filename)
        {
            if (File.Exists(filename) == true)
                File.Delete(filename);
        }

        public static void Delete(params string[] paths)
        {
            FileUtility.Delete(Path.Combine(paths));
        }

        public static void Clean(string filename)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileName(filename) + backupPostFix);
            FileUtility.Delete(backupPath);
        }

        public static void Backup(string filename)
        {
            if (File.Exists(filename) == false)
                return;
            var backupPath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileName(filename) + backupPostFix);
            if (File.Exists(backupPath) == false)
                File.Delete(backupPath);
            FileUtility.Delete(filename);
        }

        public static void Restore(string filename)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileName(filename) + backupPostFix);
            if (File.Exists(backupPath) == false)
                return;
            FileUtility.Delete(filename);
            File.Move(backupPath, filename);
            FileUtility.Delete(backupPath);
        }

        public static bool Exists(string filename)
        {
            return File.Exists(filename);
        }

        public static bool Exists(params string[] paths)
        {
            return File.Exists(Path.Combine(paths));
        }

        public static string ReadAllText(string filename)
        {
            return File.ReadAllText(filename);
        }

        public static string ReadAllText(params string[] paths)
        {
            return File.ReadAllText(Path.Combine(paths));
        }

        public static string[] ReadAllLines(string filename)
        {
            return File.ReadAllLines(filename);
        }

        public static string[] ReadAllLines(params string[] paths)
        {
            return File.ReadAllLines(Path.Combine(paths));
        }

        /// <summary>
        /// 지정한 경로가 없으면 만들고 내용을 저장합니다.
        /// </summary>
        public static string WriteAllLines(string[] contents, params string[] paths)
        {
            var filename = FileUtility.Prepare(paths);
            File.WriteAllLines(filename, contents);
            return filename;
        }

        public static string WriteAllLines(IEnumerable<string> contents, params string[] paths)
        {
            var filename = FileUtility.Prepare(paths);
            File.WriteAllLines(filename, contents);
            return filename;
        }

        /// <summary>
        /// 지정한 경로를 조합해 새로운 파일 경로를 만들고 파일 경로에 내용을 저장합니다.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="paths"></param>
        /// <returns> 저장에 성공하면 새로 만들어진 파일 경로를 반환합니다.</returns>
        public static string WriteAllText(string contents, params string[] paths)
        {
            var filename = FileUtility.Prepare(paths);
            File.WriteAllText(filename, contents);
            return filename;
        }

        public static string WriteAllText(string contents, Encoding encoding, params string[] paths)
        {
            var filename = FileUtility.Prepare(paths);
            File.WriteAllText(filename, contents, encoding);
            return filename;
        }

        public static Stream OpenRead(params string[] paths)
        {
            return File.OpenRead(Path.Combine(paths));
        }

        public static Stream OpenWrite(params string[] paths)
        {
            var filename = Path.Combine(paths);
            return new OpenWriteStream(filename);
        }

        #region classes

        class OpenWriteStream : Stream
        {
            private readonly string filename;
            private readonly Stream stream;

            public OpenWriteStream(string filename)
            {
                FileUtility.Prepare(filename);
                FileUtility.Backup(filename);
                this.stream = File.OpenWrite(filename);
                this.filename = filename;
            }

            public override bool CanRead => this.stream.CanRead;

            public override bool CanSeek => this.stream.CanSeek;

            public override bool CanWrite => this.stream.CanWrite;

            public override long Length => this.stream.Length;

            public override long Position { get => this.stream.Position; set => this.stream.Position = value; }

            public override void Flush()
            {
                this.stream.Flush();
                FileUtility.Clean(this.filename);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return this.stream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                this.stream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                this.stream.Write(buffer, offset, count);
            }

            public override void Close()
            {
                this.stream.Close();
                base.Close();
            }
        }

        #endregion
    }
}
