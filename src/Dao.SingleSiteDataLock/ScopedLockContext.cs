﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Dao.SingleSiteDataLock
{
    public class ScopedLockContext : IDisposable
    {
        readonly HashSet<LockStep> steps = new HashSet<LockStep>();

        public static readonly ForceReleaseOption DefaultForceReleaseOption = new ForceReleaseOption
        {
            WriterTimeoutSeconds = 600,
            ReaderTimeoutSeconds = 300
        };

        public void Dispose() => this.steps.Clear();

        public bool TryWriterLock(string key, string user, ForceReleaseOption option = null)
        {
            if (!SingleSiteDataLock.TryWriterLock(key, user, option ?? DefaultForceReleaseOption, out var updated))
            {
                Revert();
                return false;
            }

            if (updated)
                this.steps.Add(new LockStep(key, user, true));
            return true;
        }

        public bool TryWriterLock(IEnumerable<string> keys, string user, ForceReleaseOption option = null)
        {
            return keys.OrderBy(o => o, StringComparer.OrdinalIgnoreCase).All(key => TryWriterLock(key, user, option));
        }

        public bool TryReaderLock(string key, string user, ForceReleaseOption option = null)
        {
            if (!SingleSiteDataLock.TryReaderLock(key, user, option ?? DefaultForceReleaseOption, out var updated))
            {
                Revert();
                return false;
            }

            if (updated)
                this.steps.Add(new LockStep(key, user, false));
            return true;
        }

        public bool TryReaderLock(IEnumerable<string> keys, string user, ForceReleaseOption option = null)
        {
            return keys.OrderBy(o => o, StringComparer.OrdinalIgnoreCase).All(key => TryReaderLock(key, user, option));
        }

        public void Revert()
        {
            this.steps.ParallelForEach(e =>
            {
                if (e.IsWriter)
                    SingleSiteDataLock.ReleaseWriterLock(e.Key, e.User, new ForceReleaseOption());
                else
                    SingleSiteDataLock.ReleaseReaderLock(e.Key, e.User, new ForceReleaseOption());
            });
            this.steps.Clear();
        }

        public bool IsWriterLocked(string key, string user, ForceReleaseOption option = null) =>
            SingleSiteDataLock.IsWriterLocked(key, user, option ?? DefaultForceReleaseOption);

        public bool IsWriterLocked(IEnumerable<string> keys, string user, ForceReleaseOption option = null)
        {
            return keys.OrderBy(o => o, StringComparer.OrdinalIgnoreCase).Any(key => IsWriterLocked(key, user, option));
        }

        public bool ReleaseWriterLock(string key, string user, ForceReleaseOption option = null)
        {
            var released = SingleSiteDataLock.ReleaseWriterLock(key, user, option ?? DefaultForceReleaseOption);
            this.steps.Remove(new LockStep(key, user, true));
            return released;
        }

        public bool ReleaseWriterLock(IEnumerable<string> keys, string user, ForceReleaseOption option = null)
        {
            var released = true;
            foreach (var key in keys.OrderByDescending(o => o, StringComparer.OrdinalIgnoreCase))
            {
                if (!ReleaseWriterLock(key, user, option))
                    released = false;
            }

            return released;
        }

        public bool ReleaseReaderLock(string key, string user, ForceReleaseOption option = null)
        {
            var released = SingleSiteDataLock.ReleaseReaderLock(key, user, option ?? DefaultForceReleaseOption);
            this.steps.Remove(new LockStep(key, user, false));
            return released;
        }

        public bool ReleaseReaderLock(IEnumerable<string> keys, string user, ForceReleaseOption option = null)
        {
            var released = true;
            foreach (var key in keys.OrderByDescending(o => o, StringComparer.OrdinalIgnoreCase))
            {
                if (!ReleaseReaderLock(key, user, option))
                    released = false;
            }

            return released;
        }

        public void ReleaseAll(string user, ForceReleaseOption option = null)
        {
            SingleSiteDataLock.ReleaseAll(user, option ?? DefaultForceReleaseOption);
            this.steps.Clear();
        }

        public List<LockDetail> GetView() =>
            SingleSiteDataLock.Locks.Select(s => new LockDetail
            {
                Key = s.Key,
                LockView = s.Value.CreateView()
            }).ToList();
    }
}