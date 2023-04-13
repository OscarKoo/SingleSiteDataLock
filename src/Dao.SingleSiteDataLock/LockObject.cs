using System;

namespace Dao.SingleSiteDataLock
{
    class LockObject
    {
        readonly object syncObj = new object();

        internal LockObject(LockIdentifier identifier, string user)
        {
            Identifier = identifier;
            User = user ?? string.Empty;
            Time = DateTime.UtcNow;
        }

        internal LockIdentifier Identifier { get; }
        internal string User { get; private set; }
        internal DateTime Time { get; private set; }

        internal bool IsMyLock(string user, ForceReleaseOption option)
        {
            if (user == null)
                user = string.Empty;

            lock (this.syncObj)
            {
                if (!option.HasPrivilege
                    && !string.Equals(User, user, StringComparison.OrdinalIgnoreCase)
                    && (option.TimeoutSeconds == 0 || Time.AddSeconds(option.TimeoutSeconds) >= DateTime.UtcNow))
                    return false;

                User = user;
                Time = DateTime.UtcNow;
                return true;
            }
        }
    }
}