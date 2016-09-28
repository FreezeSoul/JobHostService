#region MIT License
/*
 *  Copyright (c) 2010 Nathan Palmer
 *  http://www.nathanpalmer.com/
 * 
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 * 
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 * 
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 */
#endregion

using System;
using System.Collections;
using System.IO;
using System.Threading;

namespace QuartzJobService.Core.Common
{
    public class FileWatcher : IDisposable
    {
        private readonly Hashtable fileChangeTable = new Hashtable();
        private readonly ReaderWriterLockSlim fileChangeTableLock = new ReaderWriterLockSlim();
        private readonly FileSystemWatcher watcher = new FileSystemWatcher();

        public event FileSystemEventHandler Changed;
        public void InvokeChanged(FileSystemEventArgs e)
        {
            FileSystemEventHandler handler = Changed;
            if (handler != null)
                handler(this, e);
        }

        public event RenamedEventHandler Renamed;
        public void InvokeRenamed(RenamedEventArgs e)
        {
            RenamedEventHandler handler = Renamed;
            if (handler != null)
                handler(this, e);
        }

        public event FileSystemEventHandler Deleted;
        public void InvokeDeleted(FileSystemEventArgs e)
        {
            FileSystemEventHandler handler = Deleted;
            if (handler != null)
                handler(this, e);
        }


        public FileWatcher(string FolderPath)
        {
            watcher.Path = FolderPath;
            watcher.Filter = "*.xml";
            watcher.Created += new FileSystemEventHandler(OnFileSystemChanged);
            watcher.Changed += new FileSystemEventHandler(OnFileSystemChanged);
            watcher.Renamed += new RenamedEventHandler(OnFileSystemRenamed);
            watcher.Deleted += new FileSystemEventHandler(OnFileSystemDeleted);
            watcher.EnableRaisingEvents = true;
        }

        private void OnFileSystemDeleted(object sender, FileSystemEventArgs e)
        {
            InvokeDeleted(e);
        }

        private void OnFileSystemRenamed(object sender, RenamedEventArgs e)
        {
            InvokeRenamed(e);
        }

        private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            fileChangeTableLock.EnterWriteLock();
            try
            {
                if (!(fileChangeTable.Contains(e.FullPath)))
                {
                    FileInfo fInfo = new FileInfo(e.FullPath);
                    fileChangeTable.Add(e.FullPath, e);
                    new Thread(() => CheckFileStatus(fInfo)).Start();
                }
                else
                {
                    Console.WriteLine(string.Format("Ignoring duplicate change request for '{0}'", e.Name));
                }
            }
            finally
            {
                fileChangeTableLock.ExitWriteLock();
            }
        }

        public void CheckFileStatus(FileInfo fInfo)
        {
            long fileSize = 0;
            while (fileSize != fInfo.Length)
            {
                Console.WriteLine(string.Format("File size of '{0}' does not match '{1}' waiting for 2.5 seconds", fileSize, fInfo.Length));
                fileSize = fInfo.Length;
                Thread.Sleep(2500);
                fInfo.Refresh();
            }

            fileChangeTableLock.EnterWriteLock();
            try
            {
                if (fileChangeTable.Contains(fInfo.FullName))
                {
                    fileChangeTable.Remove(fInfo.FullName);
                    InvokeChanged(new FileSystemEventArgs(WatcherChangeTypes.Changed, fInfo.DirectoryName, fInfo.Name));
                }
            }
            finally
            {
                fileChangeTableLock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Created -= new FileSystemEventHandler(OnFileSystemChanged);
            watcher.Changed -= new FileSystemEventHandler(OnFileSystemChanged);
        }
    }
}