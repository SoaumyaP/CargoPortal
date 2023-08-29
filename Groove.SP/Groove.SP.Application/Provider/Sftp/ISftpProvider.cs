using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Provider.Sftp
{
    public interface ISftpProvider
    {
        /// <summary>
        /// Upload a file to server via sftp.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <param name="folder"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        Task UploadFileAsync(byte[] file, string fileName, string folder, SftpProfile profile);

        /// <summary>
        /// To move all files in current directory to another directory.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="fromDirectory"></param>
        /// <param name="toDirectory"></param>
        /// <returns></returns>
        Task<List<string>> GetAndMoveFilesAsync(SftpProfile profile, string fromDirectory, string toDirectory);

        Task<byte[]> GetFileAsync(SftpProfile profile, string fileName);
    }
}