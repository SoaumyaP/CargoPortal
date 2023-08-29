namespace Groove.SP.Application.MobileApplication.ViewModels
{
    public class UpdateCheckerMobileModel
    {
        public UpdateCheckerResult Result { get; set; }
        public string NewVersion { get; set; }
        public string Message { get; set; }
        public string PackageName { get; set; }
        public string PackageUrl { get; set; }
    }

    public enum UpdateCheckerResult
    {
        UpToDate = 1,
        NewUpdate = 2,
        ForceUpdate = 3
    }
}
