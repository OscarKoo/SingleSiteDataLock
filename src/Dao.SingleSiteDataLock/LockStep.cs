using System;

namespace Dao.SingleSiteDataLock
{
    class LockStep
    {
        internal LockStep(string key, string user, bool isWriter)
        {
            Key = key;
            User = user ?? string.Empty;
            IsWriter = isWriter;
        }

        internal string Key { get; }
        internal string User { get; }
        internal bool IsWriter { get; }

        public override int GetHashCode()
        {
            var hash1 = StringComparer.OrdinalIgnoreCase.GetHashCode(Key);
            var hash2 = StringComparer.OrdinalIgnoreCase.GetHashCode(User);
            var hash3 = IsWriter.GetHashCode();
            return hash1.CombineHash(hash2).CombineHash(hash3);
        }

        public override bool Equals(object obj) =>
            obj is LockStep step
            && string.Equals(Key, step.Key, StringComparison.OrdinalIgnoreCase)
            && string.Equals(User, step.User, StringComparison.OrdinalIgnoreCase)
            && IsWriter == step.IsWriter;
    }
}