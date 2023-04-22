using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Dao.SingleSiteDataLock
{
    class LockObject
    {
        readonly object syncObj = new object();

        internal LockObject(string key)
        {
            Key = key;
            ReaderLocks = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        }

        internal string Key { get; }
        internal WriterLock WriteLock { get; private set; }
        internal ConcurrentDictionary<string, DateTime> ReaderLocks { get; }

        void ClearExpiredWriterLock(ForceReleaseOption option, DateTime now)
        {
            if (WriteLock == null)
                return;

            if (option.WriterTimeoutSeconds > 0 && WriteLock.Time.AddSeconds(option.WriterTimeoutSeconds) < now)
                WriteLock = null;
        }

        void ClearExpiredReaderLocks(ForceReleaseOption option, DateTime now)
        {
            if (ReaderLocks.Count == 0)
                return;

            ReaderLocks.Where(w => option.ReaderTimeoutSeconds > 0 && w.Value.AddSeconds(option.ReaderTimeoutSeconds) < now)
                .Select(s => s.Key)
                .ToList()
                .ParallelForEach(e => ReaderLocks.TryRemove(e, out _));
        }

        void ClearExpiredLocks(ForceReleaseOption option)
        {
            var now = DateTime.UtcNow;
            ClearExpiredWriterLock(option, now);
            ClearExpiredReaderLocks(option, now);
        }

        bool IsWriterLockFree(string user) =>
            WriteLock == null || string.Equals(WriteLock.User, user, StringComparison.OrdinalIgnoreCase);

        bool HasWriterLocked(string user, ForceReleaseOption option) =>
            !option.HasPrivilege && !IsWriterLockFree(user);

        internal bool CanWrite(string user, ForceReleaseOption option)
        {
            if (user == null)
                user = string.Empty;

            lock (this.syncObj)
            {
                ClearExpiredLocks(option);

                return !HasWriterLocked(user, option);
            }
        }

        internal bool TryWriterLock(string user, ForceReleaseOption option, out bool updated)
        {
            if (user == null)
                user = string.Empty;

            updated = false;

            lock (this.syncObj)
            {
                if (!CanWrite(user, option))
                    return false;

                var readerLocked = !option.HasPrivilege && ReaderLocks.Any(w => !w.Key.Equals(user, StringComparison.OrdinalIgnoreCase));

                if (WriteLock != null)
                {
                    if (!IsWriterLockFree(user))
                        return !readerLocked;

                    WriteLock.Time = DateTime.UtcNow;
                    updated = true;
                }
                else
                {
                    if (readerLocked)
                        return false;

                    WriteLock = new WriterLock(user);
                    updated = true;
                }

                return !readerLocked;
            }
        }

        internal bool TryReaderLock(string user, ForceReleaseOption option, out bool updated)
        {
            if (user == null)
                user = string.Empty;

            updated = false;

            lock (this.syncObj)
            {
                if (!CanWrite(user, option))
                    return false;

                ReaderLocks[user] = DateTime.UtcNow;
                updated = true;
                return true;
            }
        }

        internal bool TryReleaseWriterLock(string user, ForceReleaseOption option, ConcurrentDictionary<string, LockObject> locks)
        {
            if (user == null)
                user = string.Empty;

            lock (this.syncObj)
            {
                if (!CanWrite(user, option))
                    return false;

                WriteLock = null;

                if (WriteLock == null && ReaderLocks.Count == 0)
                    locks.TryRemove(Key, out _);

                return true;
            }
        }

        internal bool TryReleaseReaderLock(string user, ForceReleaseOption option, ConcurrentDictionary<string, LockObject> locks)
        {
            if (user == null)
                user = string.Empty;

            lock (this.syncObj)
            {
                ClearExpiredLocks(option);

                ReaderLocks.TryRemove(user, out _);

                if (WriteLock == null && ReaderLocks.Count == 0)
                    locks.TryRemove(Key, out _);

                return true;
            }
        }

        internal bool TryReleaseBothLocks(string user, ForceReleaseOption option, ConcurrentDictionary<string, LockObject> locks)
        {
            if (user == null)
                user = string.Empty;

            lock (this.syncObj)
            {
                ClearExpiredLocks(option);

                ReaderLocks.TryRemove(user, out _);

                if (HasWriterLocked(user, option))
                    return false;
                WriteLock = null;

                if (WriteLock == null && ReaderLocks.Count == 0)
                    locks.TryRemove(Key, out _);

                return true;
            }
        }

        internal LockView CreateView()
        {
            lock (this.syncObj)
            {
                ClearExpiredLocks(new ForceReleaseOption());

                return new LockView
                {
                    WriterUser = WriteLock?.User,
                    WriterTime = WriteLock?.Time,
                    ReaderView = ReaderLocks.Select(s => new ReaderLockDetail
                    {
                        ReaderUser = s.Key,
                        ReaderTime = s.Value,
                    }).ToList()
                };
            }
        }
    }

    class WriterLock
    {
        internal WriterLock(string user)
        {
            User = user ?? string.Empty;
            Time = DateTime.UtcNow;
        }

        internal string User { get; set; }
        internal DateTime Time { get; set; }
    }
}