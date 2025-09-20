namespace CopyMerge.Services
{
    public class AutorunInfo
    {
        public bool IsEnabled { get; set; }

        public string ExecutablePath { get; set; }

        public string RegistryPath { get; set; }

        public string RegistryKey { get; set; }

        public string ValueName { get; set; }

        public string ErrorMessage { get; set; }
    }
}