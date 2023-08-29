using Groove.SP.Application.ImportData.ViewModels;
using Groove.SP.Application.Translations.Helpers;
using Groove.SP.Core.Entities;
using System;
using System.Threading.Tasks;

namespace Groove.SP.Application.ImportData.Services.Interfaces
{
    public interface IImportDataProgressService
    {
        Task<ImportDataProgressModel> GetByIdAsync(long id);

        Task<ImportDataProgressModel> CreateAsync(string name, string userName);

        /// <summary>
        /// To import Organizations from Excel file
        /// </summary>
        /// <param name="importDataProgressId">Import data progress id</param>
        /// <param name="userName">Username who is importing data</param>
        /// <param name="fileName">Excel file name</param>
        /// <param name="fileContent">Excel file content</param>
        /// <returns></returns>
        Task ImportOrganizationsFromExcelSilentAsync(long importDataProgressId, string userName, string fileName, byte[] fileContent);

        /// <summary>
        /// Export error list as excel file.
        /// </summary>
        /// <param name="importDataProgressId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<byte[]> ExportErrorExcelAsync(long id, string userName, string lang = TranslationUtility.ENGLISH_KEY);

        /// <summary>
        /// Export PO error list as excel file.
        /// </summary>
        /// <param name="importDataProgressId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<byte[]> ExportPOErrorExcelAsync(long id, string userName, string lang = TranslationUtility.ENGLISH_KEY);
    }
}