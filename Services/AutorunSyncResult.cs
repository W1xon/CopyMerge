namespace CopyMerge.Services
{
    public class AutorunSyncResult
    {
        public bool Success { get; set; }

        public bool SynchronizationNeeded { get; set; }

        public bool CurrentState { get; set; }

        public string Message { get; set; }

        public string ErrorMessage { get; set; }
    }
}