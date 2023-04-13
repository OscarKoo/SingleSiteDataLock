using System;

namespace Dao.SingleSiteDataLock
{
    public class LockDetail
    {
        public string Category { get; set; }
        public string Key { get; set; }
        public string User { get; set; }
        public DateTime Time { get; set; }
    }
}