namespace Groove.SP.Application.Provider.Sftp
{
    public class SftpProfile
    {
        public string HostName { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        /// <summary>
        /// The blob id of the ssh key file which was stored on the Azure storage.
        /// </summary>
        public string BlobKeyId { get; set; }
    }
}