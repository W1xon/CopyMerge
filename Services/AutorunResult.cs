namespace CopyMerge.Services
{
    public class AutorunResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string ErrorMessage { get; set; }

        public bool RequiresElevation { get; set; }
    }
}