// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAzureBlobContext.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Groove.SP.Application.Providers.Azure
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IAzureBlobContext
    {
       
        Task GetBlobAsync(string containerName, string path, Stream outStream);

        Task PutBlobAsync(string containerName, string path, Stream inStream);

        Task CreateContainerAsync(string containerName);

        Task DeleteBlobAsync(string containerName, string path);
    }
}