using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class FtpServerModel : Entity
    {
        public long Id { get; set; }

        public FileProtocolType FileProtocol { get; set; }

        public string HostName { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Passphrase { get; set; }

        public string PrivateKey { get; set; }

        public string FolderName { get; set; }
    }
}