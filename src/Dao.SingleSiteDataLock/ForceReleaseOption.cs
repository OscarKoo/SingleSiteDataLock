namespace Dao.SingleSiteDataLock
{
    public class ForceReleaseOption
    {
        int timeoutSeconds;
        public int TimeoutSeconds
        {
            get => this.timeoutSeconds;
            set => this.timeoutSeconds = value < 0 ? 0 : value;
        }
        public bool HasPrivilege { get; set; }
    }
}