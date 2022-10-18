using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit.Sdk;

namespace ListingManager.Tests
{
    public abstract class TempFileTestBase : IDisposable
    {
        private readonly Lazy<DirectoryInfo> _WorkingDirectory = new(() =>
       {
           DirectoryInfo working = new(Path.Combine(Path.GetTempPath(),
               typeof(TempFileTestBase).Assembly.GetName().Name!,
               Path.GetRandomFileName()));

           if (working.Exists)
           {
               working.Delete(recursive: true);
           }

           working.Create();
           return working;
       }, LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly List<FileInfo> _TempFiles = new();
        private readonly List<DirectoryInfo> _TempDirectories = new();
        protected DirectoryInfo TempDirectory => _WorkingDirectory.Value;
        private bool _Disposed;


        private FileInfo CreateTempFileWithContent(DirectoryInfo? parentDirectory, string? name = null, byte[]? fileContents = null, string? extension = null)
        {
            var tempFile = new FileInfo(GetPath(parentDirectory, name, extension, false));

            using (FileStream stream = tempFile.OpenWrite())
            {
                if (fileContents != null)
                {
                    stream.Write(fileContents, 0, fileContents.Length);
                }
            }

            _TempFiles.Add(tempFile);
            return tempFile;
        }

        protected FileInfo CreateTempFile(DirectoryInfo? parentDirectory = null, string? name = null, string? contents = null,
            string? extension = null)
        {
            return CreateTempFileWithContent(parentDirectory, name, contents is null ? null : Encoding.ASCII.GetBytes(contents), extension);
        }

        public DirectoryInfo CreateTempDirectory(DirectoryInfo? parentDirectory = null, string? name = null)
        {
            var tempDir = new DirectoryInfo(GetPath(parentDirectory, name, null, false));
            tempDir.Create();
            _TempDirectories.Add(tempDir);
            return tempDir;
        }

        protected void AddTempDirectory(DirectoryInfo directory)
        {
            _ = directory ?? throw new ArgumentNullException(nameof(directory));
            _TempDirectories.Add(directory);
        }

        protected void AddTempFile(FileInfo file)
        {
            _ = file ?? throw new ArgumentNullException(nameof(file));
            _TempFiles.Add(file);
        }


        private string GetPath(DirectoryInfo? parentDirectory, string? name, string? extension, bool randomPrefix)
        {
            DirectoryInfo directory = parentDirectory ?? _WorkingDirectory.Value;

            string prefix = randomPrefix && !string.IsNullOrEmpty(name)
                ? Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                : "";

            string fileName = prefix + (name ?? Path.GetRandomFileName());

            if (!string.IsNullOrEmpty(extension))
            {
                fileName = Path.ChangeExtension(fileName, extension);
            }

            return Path.Combine(directory.FullName, fileName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed || !disposing)
            {
                return;
            }

            _Disposed = true;

            ExceptionAggregator aggregator = new();

            IEnumerable<FileSystemInfo> items = _TempFiles
                .Cast<FileSystemInfo>()
                .Concat(_TempDirectories)
                .Concat(_WorkingDirectory.IsValueCreated ? new[] { _WorkingDirectory.Value } : Enumerable.Empty<DirectoryInfo>());

            foreach (FileSystemInfo fsi in items)
            {
                fsi.Refresh();
                if (!fsi.Exists) continue;

                Action? action = fsi switch
                {
                    FileInfo file => () => file.Delete(),
                    DirectoryInfo dir => () => dir.Delete(recursive: true),
                    _ => null,
                };

                if (action is null) continue;

                aggregator.Run(() =>
                {
                    Policy
                        .Handle<Exception>()
                        .WaitAndRetry(retryCount: 100, _ => TimeSpan.FromMilliseconds(10))
                        .Execute(action);
                });
            }

            if (aggregator.HasExceptions)
            {
                throw aggregator.ToException();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}