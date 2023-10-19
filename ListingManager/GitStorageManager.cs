using LibGit2Sharp;
using System;

namespace EssentialCSharp.ListingManager;

public class GitStorageManager : IStorageManager, IDisposable
{
    private bool _DisposedValue;
    private Repository Repository { get; }

    public GitStorageManager(string repoPath)
    {
        if (!Repository.IsValid(repoPath)) throw new ArgumentException("The specified path is not a valid git repository.", nameof(repoPath));
        Repository = new Repository(repoPath);
    }

    public void Move(string oldPath, string newPath)
    {
        Commands.Move(Repository, oldPath, newPath);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_DisposedValue)
        {
            if (disposing)
            {
                Repository.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _DisposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
