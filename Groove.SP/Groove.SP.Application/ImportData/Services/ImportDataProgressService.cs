using Groove.SP.Application.ImportData.Services.Interfaces;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.Excel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Groove.SP.Infrastructure.CSFE.Models;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.Extensions.Logging;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.ImportData.ViewModels;
using OfficeOpenXml.Style;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Application.Translations.Helpers;

namespace Groove.SP.Application.ImportData.Services
{
    public class ImportDataProgressService : IImportDataProgressService
    {
        private readonly IImportDataProgressRepository _importDataProgressRepository;
        private readonly IUnitOfWork UnitOfWork;
        private readonly IDataQuery _dataQuery;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IExportManager _exportManager;
        private readonly ILogger<ImportDataProgressService> _logger;
        private readonly ITranslationProvider _translation;

        public ImportDataProgressService(IUnitOfWorkProvider unitOfWorkProvider, IExportManager exportManager, IDataQuery dataQuery,
            ICSFEApiClient csfeApiClient, ITranslationProvider translation, ILogger<ImportDataProgressService> logger)
        {
            UnitOfWork = unitOfWorkProvider.CreateUnitOfWork();
            _importDataProgressRepository = (IImportDataProgressRepository)this.UnitOfWork.GetRepository<ImportDataProgressModel>();
            _dataQuery = dataQuery;
            _csfeApiClient = csfeApiClient;
            _exportManager = exportManager;
            _logger = logger;
            _translation = translation;
        }

        public async Task<ImportDataProgressModel> CreateAsync(string name, string userName)
        {
            var importDataProgress = new ImportDataProgressModel
            {
                Name = name
            };

            importDataProgress.Audit(userName);

            await _importDataProgressRepository.AddAsync(importDataProgress);
            await this.UnitOfWork.SaveChangesAsync();

            return importDataProgress;
        }

        public async Task<ImportDataProgressModel> GetByIdAsync(long id)
        {
            return await _importDataProgressRepository.FindAsync(id);
        }

        [DisplayName("Import Organizations from Excel {2} by {1} on import progress {0}")]
        public async Task ImportOrganizationsFromExcelSilentAsync(long importDataProgressId, string userName, string fileName, byte[] fileContent)
        {
            // Begin, set 1/100 progress values
            await _importDataProgressRepository.UpdateProgressAsync(importDataProgressId, 1, 100);

            // Set current status description
            await _importDataProgressRepository.UpdateStatusAsync(importDataProgressId, ImportDataProgressStatus.Started, "importResult.validatingData");

            var serializeObjectSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // Refer to link: https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_(Unicode_block)
            // CJK Unified Ideographs is a Unicode block containing the most common CJK ideographs used in modern Chinese, Japanese, Korean and Vietnamese characters

            // Not allowed Chinese wording
            Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");

            var rowLimit = 1000;
            var rowChuck = 500;
            var validationResult = new List<ValidatorErrorInfo>();
            var toImportOrganizations = new List<BulkInsertOrganization>();

            #region Validate data

            try
            {

                using (Stream fileStream = new MemoryStream(fileContent))
                {
                    using (var excelPackage = new ExcelPackage(fileStream))
                    {
                        var importSheet = excelPackage.Workbook.Worksheets[0];
                        var fromRow = 2;
                        var toRow = importSheet.Dimension.Rows;

                        if (toRow - 1 > rowLimit)
                        {
                            await _importDataProgressRepository.UpdateStatusAsync(
                                   importDataProgressId,
                                   ImportDataProgressStatus.Failed,
                                   "importResult.limit1000Organization");

                            return;
                        }

                        string cellValue;
                        var isValid = true;
                        var validOrganizationTypes = new[]
                        {
                            "Agent", "Principal", "General"
                        };
                        var orgTypesNeedOrgCode = new[]
                        {
                            "Agent", "Principal"
                        };
                        var emailCheck = new EmailAddressAttribute();
                        IEnumerable<string> existingOrganizationCodes = new List<string>();
                        IEnumerable<string> existingOrganizationNames = new List<string>();
                        IEnumerable<string> existingEdisonCompanyCodes = new List<string>();
                        Dictionary<string, long> availableLocations = new Dictionary<string, long>();


                        for (int rowIndex = fromRow; rowIndex <= toRow; rowIndex++)
                        {
                            // Update progess values every ten percentage (10,20,30...) -> save effort to database
                            if (rowIndex * 10 % (toRow - 1) == 0)
                            {
                                await _importDataProgressRepository.UpdateProgressAsync(importDataProgressId, rowIndex * 50 / (toRow - 1), 100);
                            }

                            var newOrganization = new BulkInsertOrganization();
                            if (rowIndex == fromRow)
                            {
                                // Add 2 new columns (R and S) about Organization Code to detect duplicated
                                importSheet.Cells[2, 18, toRow, 18].FormulaR1C1 = "IF(OR(R[0]C[-17]=\"Agent\",R[0]C[-17]=\"Principal\"),R[0]C[-15],\"\")";
                                importSheet.Cells[2, 19, toRow, 19].FormulaR1C1 = $"COUNTIF($R$2:$R${toRow},R[0]C[-1])";

                                // Add 2 new columns (T and U) about Organization Name to detect duplicated
                                importSheet.Cells[2, 20, toRow, 20].FormulaR1C1 = "R[0]C[-17]";
                                importSheet.Cells[2, 21, toRow, 21].FormulaR1C1 = $"COUNTIF($T$2:$T${toRow},R[0]C[-1])";

                                // Add 2 new columns (V and W) about ediSON Company Code to detect duplicated
                                importSheet.Cells[2, 22, toRow, 22].FormulaR1C1 = "R[0]C[-8]";
                                importSheet.Cells[2, 23, toRow, 23].FormulaR1C1 = $"COUNTIF($V$2:$V${toRow},R[0]C[-1])";

                                // Add 2 new columns (X and Y) about Customer Prefix to detect duplicated
                                importSheet.Cells[2, 24, toRow, 24].FormulaR1C1 = "R[0]C[-9]";
                                importSheet.Cells[2, 25, toRow, 25].FormulaR1C1 = $"COUNTIF($X$2:$X${toRow},R[0]C[-1])";

                                importSheet.Workbook.Calculate();

                                #region to check Organization Codes
                                var toCheckOrganizationCodes = from findCell
                                                              in importSheet.Cells[2, 18, toRow, 18]
                                                               select findCell.Value?.ToString() ?? string.Empty;

                                toCheckOrganizationCodes = toCheckOrganizationCodes.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x).Distinct().ToList();

                                existingOrganizationCodes = GetExistingOrganizationCodes(toCheckOrganizationCodes);
                                #endregion

                                #region to check Organization Names
                                var toCheckOrganizationNames = from findCell
                                                               in importSheet.Cells[2, 20, toRow, 20]
                                                               select findCell.Value?.ToString() ?? String.Empty;

                                toCheckOrganizationNames = toCheckOrganizationNames.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x).Distinct().ToList();
                                existingOrganizationNames = GetExistingOrganizationNames(toCheckOrganizationNames);
                                #endregion

                                #region to check Edison Company Code
                                var toCheckEdisonCompanyCodes = from findCell
                                                               in importSheet.Cells[2, 22, toRow, 22]
                                                               select findCell.Value?.ToString() ?? String.Empty;

                                toCheckEdisonCompanyCodes = toCheckEdisonCompanyCodes.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(x => x).Distinct().ToList();
                                existingEdisonCompanyCodes = GetExistingEdisonCompanyCodes(toCheckEdisonCompanyCodes);
                                #endregion

                                availableLocations = GetAvailableLocations();
                            }

                            // Check row empty for the first 17 columns, exclude R -> Y
                            if (importSheet.IsRowEmpty(rowIndex, 17))
                            {
                                continue;
                            }

                            // Organization type
                            var organizationType = importSheet.Cells[$"A{rowIndex}"].Value?.ToString() ?? null;
                            if (organizationType != null)
                            {
                                organizationType = organizationType.Trim();
                            }
                            if (string.IsNullOrWhiteSpace(organizationType))
                            {
                                validationResult.Add(new ValidatorErrorInfo
                                {
                                    SheetName = importSheet.Name,
                                    Column = "label.organizationType",
                                    Row = rowIndex.ToString(),
                                    ObjectName = organizationType,
                                    ErrorMsg = "importLog.requiredField"
                                });
                            }
                            else if (!validOrganizationTypes.Contains(organizationType))
                            {
                                validationResult.Add(new ValidatorErrorInfo
                                {
                                    SheetName = importSheet.Name,
                                    Column = "label.organizationType",
                                    Row = rowIndex.ToString(),
                                    ObjectName = organizationType,
                                    ErrorMsg = $"importLog.valueMustBe;{string.Join(", ", validOrganizationTypes)}."
                                });
                            }
                            else
                            {
                                // Valid value -> add to new object
                                switch (organizationType.ToLowerInvariant())
                                {
                                    case "general":
                                        newOrganization.OrganizationType = 1;
                                        break;
                                    case "agent":
                                        newOrganization.OrganizationType = 2;
                                        break;
                                    case "principal":
                                        newOrganization.OrganizationType = 4;
                                        break;
                                    default:
                                        break;
                                }
                            }


                            // Organization code
                            string organizationCode = importSheet.Cells[$"B{rowIndex}"].Value?.ToString() ?? null;
                            if (organizationCode != null)
                            {
                                organizationCode = organizationCode.Trim();
                            }
                            if (orgTypesNeedOrgCode.Contains(organizationType))
                            {
                                if (string.IsNullOrWhiteSpace(organizationCode))
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.organizationCode",
                                        Row = rowIndex.ToString(),
                                        ObjectName = organizationCode,
                                        ErrorMsg = "importLog.requiredFieldForAgentAndPrincipal"
                                    });
                                }
                                else
                                {
                                    isValid = true;
                                    // Not 1 -> duplicated on file
                                    if ((importSheet.Cells[$"S{rowIndex}"].Value?.ToString() ?? null) != "1")
                                    {
                                        validationResult.Add(new ValidatorErrorInfo
                                        {
                                            SheetName = importSheet.Name,
                                            Column = "label.organizationCode",
                                            Row = rowIndex.ToString(),
                                            ObjectName = organizationCode,
                                            ErrorMsg = "importLog.duplicatedValue"
                                        });
                                        isValid = false;
                                    }

                                    if (existingOrganizationCodes.Contains(organizationCode, StringComparer.InvariantCultureIgnoreCase))
                                    {
                                        validationResult.Add(new ValidatorErrorInfo
                                        {
                                            SheetName = importSheet.Name,
                                            Column = "label.organizationCode",
                                            Row = rowIndex.ToString(),
                                            ObjectName = organizationCode,
                                            ErrorMsg = "importLog.valueIsExisting"
                                        });
                                        isValid = false;
                                    }

                                    if (organizationCode.Length > 30)
                                    {
                                        validationResult.Add(new ValidatorErrorInfo
                                        {
                                            SheetName = importSheet.Name,
                                            Column = "label.organizationCode",
                                            Row = rowIndex.ToString(),
                                            ObjectName = organizationCode,
                                            ErrorMsg = "importLog.longerThan30Characters"
                                        });
                                        isValid = false;
                                    }

                                    if (cjkCharRegex.IsMatch(organizationCode))
                                    {
                                        validationResult.Add(new ValidatorErrorInfo
                                        {
                                            SheetName = importSheet.Name,
                                            Column = "label.organizationCode",
                                            Row = rowIndex.ToString(),
                                            ObjectName = organizationCode,
                                            ErrorMsg = "importLog.notAllowChineseWording"
                                        });
                                        isValid = false;
                                    }

                                    if (isValid)
                                    {
                                        // Valid value -> add to new object
                                        newOrganization.Code = organizationCode;
                                    }
                                }
                            }

                            // Organization name
                            isValid = true;
                            var organizationName = importSheet.Cells[$"C{rowIndex}"].Value?.ToString() ?? null;
                            if (organizationName != null)
                            {
                                organizationName = Regex.Replace(organizationName, @"\s+", " ").Trim();
                            }
                            if (string.IsNullOrWhiteSpace(organizationName))
                            {
                                validationResult.Add(new ValidatorErrorInfo
                                {
                                    SheetName = importSheet.Name,
                                    Column = "label.organizationName",
                                    Row = rowIndex.ToString(),
                                    ObjectName = organizationName,
                                    ErrorMsg = "importLog.requiredField"
                                });
                            }
                            else
                            {
                                if (organizationName.Length > 50)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.organizationName",
                                        Row = rowIndex.ToString(),
                                        ObjectName = organizationName,
                                        ErrorMsg = "importLog.longerThan50Characters"
                                    });
                                    isValid = false;
                                }

                                // Not 1 -> duplicated on file
                                if ((importSheet.Cells[$"U{rowIndex}"].Value?.ToString() ?? null) != "1")
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.organizationName",
                                        Row = rowIndex.ToString(),
                                        ObjectName = organizationName,
                                        ErrorMsg = "importLog.duplicatedValue"
                                    });
                                    isValid = false;
                                }

                                // -> duplicated on database
                                if (existingOrganizationNames.Contains(organizationName, StringComparer.InvariantCultureIgnoreCase))
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.organizationName",
                                        Row = rowIndex.ToString(),
                                        ObjectName = organizationName,
                                        ErrorMsg = "importLog.valueIsExisting"
                                    });
                                    isValid = false;
                                }

                                if (isValid)
                                {
                                    // Valid value -> add to new object
                                    newOrganization.Name = organizationName;
                                }
                            }

                            // Address
                            cellValue = importSheet.Cells[$"D{rowIndex}"].Value?.ToString() ?? null;
                            if (cellValue != null)
                            {
                                cellValue = Regex.Replace(cellValue, @"\s+", " ").Trim();
                            }

                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 50)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.address",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.longerThan50Characters"
                                    });
                                }
                                else
                                {
                                    // Valid value -> add to new object
                                    newOrganization.Address = cellValue;
                                }
                            }

                            // Address Line 2
                            cellValue = importSheet.Cells[$"E{rowIndex}"].Value?.ToString() ?? null;
                            if (cellValue != null)
                            {
                                cellValue = Regex.Replace(cellValue, @"\s+", " ").Trim();
                            }

                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 50)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.addressLine2",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.longerThan50Characters"
                                    });
                                }
                                else
                                {
                                    // Valid value -> add to new object
                                    newOrganization.AddressLine2 = cellValue;
                                }
                            }


                            // Address Line 3
                            cellValue = importSheet.Cells[$"F{rowIndex}"].Value?.ToString() ?? null;
                            if (cellValue != null)
                            {
                                cellValue = Regex.Replace(cellValue, @"\s+", " ").Trim();
                            }

                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 50)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.addressLine3",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.longerThan50Characters"
                                    });
                                }
                                else
                                {
                                    // Valid value -> add to new object
                                    newOrganization.AddressLine3 = cellValue;
                                }
                            }

                            // Address Line 4
                            cellValue = importSheet.Cells[$"G{rowIndex}"].Value?.ToString() ?? null;
                            if (cellValue != null)
                            {
                                cellValue = Regex.Replace(cellValue, @"\s+", " ").Trim();
                            }

                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 50)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.addressLine4",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.longerThan50Characters"
                                    });
                                }
                                else
                                {
                                    // Valid value -> add to new object
                                    newOrganization.AddressLine4 = cellValue;
                                }
                            }

                            // Location Code
                            var locationCode = importSheet.Cells[$"H{rowIndex}"].Value?.ToString() ?? null;
                            if (locationCode != null)
                            {
                                locationCode = locationCode.Trim();
                            }

                            if (string.IsNullOrWhiteSpace(locationCode))
                            {
                                validationResult.Add(new ValidatorErrorInfo
                                {
                                    SheetName = importSheet.Name,
                                    Column = "label.locationCode",
                                    Row = rowIndex.ToString(),
                                    ObjectName = locationCode,
                                    ErrorMsg = "importLog.requiredField"
                                });
                            }
                            else if (!availableLocations.ContainsKey(locationCode))
                            {
                                validationResult.Add(new ValidatorErrorInfo
                                {
                                    SheetName = importSheet.Name,
                                    Column = "label.locationCode",
                                    Row = rowIndex.ToString(),
                                    ObjectName = locationCode,
                                    ErrorMsg = "importLog.valueIsNotExisting"
                                });
                            }
                            else
                            {
                                // Valid value -> add to new object
                                newOrganization.LocationId = availableLocations[locationCode];
                            }

                            // Contact Name
                            cellValue = importSheet.Cells[$"I{rowIndex}"].Value?.ToString() ?? null;
                            if (cellValue != null)
                            {
                                cellValue = Regex.Replace(cellValue, @"\s+", " ").Trim();
                            }
                            if (string.IsNullOrWhiteSpace(cellValue))
                            {
                                validationResult.Add(new ValidatorErrorInfo
                                {
                                    SheetName = importSheet.Name,
                                    Column = "label.contactName",
                                    Row = rowIndex.ToString(),
                                    ObjectName = cellValue,
                                    ErrorMsg = "importLog.requiredField"
                                });
                            }
                            else if (cellValue.Length > 30)
                            {
                                validationResult.Add(new ValidatorErrorInfo
                                {
                                    SheetName = importSheet.Name,
                                    Column = "label.contactName",
                                    Row = rowIndex.ToString(),
                                    ObjectName = cellValue,
                                    ErrorMsg = "importLog.longerThan30Characters"
                                });
                            }
                            else
                            {
                                // Valid value -> add to new object
                                newOrganization.ContactName = cellValue;
                            }


                            // Contact Email
                            isValid = true;
                            cellValue = importSheet.Cells[$"J{rowIndex}"].Value?.ToString() ?? null;
                            if (cellValue != null)
                            {
                                cellValue = Regex.Replace(cellValue, @"\s+", "");
                            }
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 40)
                                {
                                    isValid = false;
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.contactEmail",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.longerThan40Characters"
                                    });
                                }
                                if (!emailCheck.IsValid(cellValue))
                                {
                                    isValid = false;
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.contactEmail",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.wrongFormat"
                                    });
                                }
                                if (isValid)
                                {
                                    // Valid value -> add to new object
                                    newOrganization.ContactEmail = cellValue;
                                }
                            }


                            // Contact Number
                            cellValue = importSheet.Cells[$"K{rowIndex}"].Value?.ToString() ?? null;
                            if (cellValue != null)
                            {
                                cellValue = Regex.Replace(cellValue, @"\s+", " ").Trim();
                            }
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 30)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.contactNumber",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.longerThan30Characters"
                                    });
                                }
                                else
                                {
                                    // Valid value -> add to new object
                                    newOrganization.ContactNumber = cellValue;
                                }
                            }


                            // Web Domain
                            cellValue = importSheet.Cells[$"L{rowIndex}"].Value?.ToString().Trim() ?? null;
                            newOrganization.WebDomain = cellValue;

                            // ediSON Instance Id
                            cellValue = importSheet.Cells[$"M{rowIndex}"].Value?.ToString().Trim() ?? null;
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 32)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.ediSONInstance",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.longerThan30Characters"
                                    });
                                }
                                else
                                {
                                    // Valid value -> add to new object
                                    newOrganization.EdisonInstanceId = cellValue;
                                }
                            }


                            // ediSON Company Code Id
                            cellValue = importSheet.Cells[$"N{rowIndex}"].Value?.ToString().Trim() ?? null;
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 32)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.ediSONCompanyCode",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.longerThan32Characters"
                                    });
                                }
                                else if (importSheet.Cells[$"W{rowIndex}"].Value?.ToString() != "1")
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.ediSONCompanyCode",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.duplicatedValue"
                                    });
                                }
                                else if (existingEdisonCompanyCodes.Contains(cellValue, StringComparer.InvariantCultureIgnoreCase))
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.ediSONCompanyCode",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.valueIsExisting"
                                    });
                                }
                                else
                                {
                                    // Valid value -> add to new object
                                    newOrganization.EdisonCompanyCodeId = cellValue;
                                }
                            }

                            // Customer Prefix
                            cellValue = importSheet.Cells[$"O{rowIndex}"].Value?.ToString().Trim() ?? null;

                            if ("Principal".Equals(organizationType, StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrWhiteSpace(cellValue))
                            {
                                validationResult.Add(new ValidatorErrorInfo
                                {
                                    SheetName = importSheet.Name,
                                    Column = "label.customerPrefix",
                                    Row = rowIndex.ToString(),
                                    ObjectName = cellValue,
                                    ErrorMsg = "importLog.requiredForPrincipal"
                                });
                            }
                            else if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 5)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.customerPrefix",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.longerThan5Characters"
                                    });
                                }
                                else if (importSheet.Cells[$"Y{rowIndex}"].Value?.ToString() != "1")
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.customerPrefix",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.duplicatedValue"
                                    });
                                } else if (cjkCharRegex.IsMatch(cellValue))
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "label.customerPrefix",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "importLog.notAllowChineseWording"
                                    });
                                }
                                else
                                {
                                    // Valid value -> add to new object
                                    newOrganization.CustomerPrefix = cellValue;
                                }
                            }

                            // Taxpayer ID
                            cellValue = importSheet.Cells[$"P{rowIndex}"].Value?.ToString().Trim() ?? null;
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 50)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "Taxpayer ID",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "Not allow to longer than 50 characters."
                                    });
                                }
                                else
                                {
                                    newOrganization.TaxpayerId = cellValue;
                                }
                            }

                            // WeChat/WhatsApp
                            cellValue = importSheet.Cells[$"Q{rowIndex}"].Value?.ToString().Trim() ?? null;
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (cellValue.Length > 32)
                                {
                                    validationResult.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = importSheet.Name,
                                        Column = "WeChat/WhatsApp",
                                        Row = rowIndex.ToString(),
                                        ObjectName = cellValue,
                                        ErrorMsg = "Not allow to longer than 32 characters."
                                    });
                                }
                                else
                                {
                                    newOrganization.WeChatOrWhatsApp = cellValue;
                                }
                            }

                            // Add audit information
                            newOrganization.CreatedBy = userName;
                            newOrganization.CreatedDate = DateTime.UtcNow;

                            // Add to bulk
                            toImportOrganizations.Add(newOrganization);

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // Return exception message to GUI
                // Set 100/100 progress
                await _importDataProgressRepository.UpdateProgressAsync(importDataProgressId, 100, 100);
                await _importDataProgressRepository.UpdateStatusAsync(importDataProgressId, ImportDataProgressStatus.Failed, ex.Message);

                // Write full log on server.
                _logger.LogError(ex, $"Import Organizations from Excel by {userName} on import progress {importDataProgressId} failed.");
                return;
            }
            #endregion Validate data

            #region Import data
            // After validated finished successfully, set 50/100 progress values
            await _importDataProgressRepository.UpdateProgressAsync(importDataProgressId, 50, 100);
            if (validationResult.Any())
            {
                // Set 100/100 progress if validation is failed
                await _importDataProgressRepository.UpdateProgressAsync(importDataProgressId, 100, 100);
                await _importDataProgressRepository.UpdateStatusAsync(importDataProgressId, ImportDataProgressStatus.Failed, "importResult.validationFailed", JsonConvert.SerializeObject(validationResult, serializeObjectSetting));
                return;
            }

            // Start importing data
            await _importDataProgressRepository.UpdateStatusAsync(importDataProgressId, ImportDataProgressStatus.Started, "importResult.importingData");

            // Split data into every 500 rows
            var organizationInsertBatch = toImportOrganizations.Chunk(500);
            var organizationInsertBatchCount = organizationInsertBatch.Count();

            var chuckCounter = 1;
            var batchSuccess = true;
            foreach (var organizations in organizationInsertBatch)
            {
                // Update completed/overall progress values
                await _importDataProgressRepository.UpdateProgressAsync(importDataProgressId, 50 + (chuckCounter * rowChuck / rowLimit / 50), 100);
                try
                {
                    var result = await _csfeApiClient.BulkInsertOrganizationsAsync(organizations);
                }
                catch
                {
                    // Case there is error (very seldom), continue import -> notify end-user
                    batchSuccess = false;
                    continue;
                }
            }

            await _importDataProgressRepository.UpdateProgressAsync(importDataProgressId, 100, 100);

            if (batchSuccess)
            {
                await _importDataProgressRepository.UpdateStatusAsync(importDataProgressId, ImportDataProgressStatus.Success, $"{toImportOrganizations.Count};importResult.recordsImportedSuccessfully", null);
            }
            else
            {
                // Notify end-users for some insert failure
                await _importDataProgressRepository.UpdateStatusAsync(importDataProgressId, ImportDataProgressStatus.Warning, "importResult.recordsImportedPartially", null);
            }

            #endregion Import data

            return;
        }

        public async Task<byte[]> ExportErrorExcelAsync(long id, string userName, string lang = TranslationUtility.ENGLISH_KEY)
        {
            var importDataProgress = await _importDataProgressRepository.GetAsNoTrackingAsync(x => x.Id == id);

            if (importDataProgress == null)
            {
                throw new AppEntityNotFoundException($"Progress {id} not found!");
            }

            if (!string.IsNullOrWhiteSpace(importDataProgress.Log))
            {
                var errors = JsonConvert.DeserializeObject<List<ImportLogViewModel>>(importDataProgress.Log);
                if (errors.Any())
                {
                    var translations = await _translation.GetTranslationsByLanguageAsync(lang);
                    var excelErrors = errors
                        .GroupBy(x => new {x.Column, x.Row})
                        .OrderBy(x => x.Key.Row)
                        .Select((g, i) => {

                            var columnName = g.Key.Column;
                            if (translations.TryGetValue(columnName, out string value))
                            {
                                columnName = value;
                            }
                            return new ImportErrorExcelModel
                            {
                                Order = i + 1,
                                Column = columnName,
                                Row = g.Key.Row,
                                ErrorMsg = string.Join(Environment.NewLine, g.Select(i => TranslateError(i.ErrorMsg, translations)))
                            };

                        }).ToList();

                    var dt = excelErrors.ToDataTable();

                    // translate excel column name
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (translations.TryGetValue(dt.Columns[i].ColumnName, out string value))
                        {
                            dt.Columns[i].ColumnName = value;
                        }
                    }

                    var xlsx = _exportManager.ExportDataTableToXlsx(dt);
                    var sheet = xlsx.Workbook.Worksheets["sheet1"];
                    // excel custom styles
                    sheet.Cells.Style.WrapText = true;
                    sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                    return xlsx.GetAsByteArray();
                }
            }

            return null;
        }

        private string TranslateError(string error, Dictionary<string, string> translations)
        {
            var result = new List<string>();

            var splitError = error.Split(Seperator.SEMICOLON); 
            foreach (var item in splitError)
            {
                if (translations.TryGetValue(item, out string value))
                {
                    result.Add(value);
                }
                else
                {
                    result.Add(item);
                }
            }

            return string.Join(" ", result);
        }

        public async Task<byte[]> ExportPOErrorExcelAsync(long id, string userName, string lang = TranslationUtility.ENGLISH_KEY)
        {
            var importDataProgress = await _importDataProgressRepository.GetAsNoTrackingAsync(x => x.Id == id);

            if (importDataProgress == null)
            {
                throw new AppEntityNotFoundException($"Progress {id} not found!");
            }

            if (!string.IsNullOrWhiteSpace(importDataProgress.Log))
            {
                var errors = JsonConvert.DeserializeObject<List<ImportLogViewModel>>(importDataProgress.Log);
                if (errors.Any())
                {
                    var translations = await _translation.GetTranslationsByLanguageAsync(lang);
                    var excelErrors = errors
                        .GroupBy(x => new {x.ObjectName, x.SheetName, x.Row})
                        .Select((r, i) => {
                            var sheetName = r.Key.SheetName;
                            if (translations.TryGetValue(sheetName, out string value))
                            {
                                sheetName = value;
                            }
                            return new ImportPOErrorExcelModel
                            {
                                Order = i + 1,
                                ObjectName = r.Key.ObjectName,
                                SheetName = sheetName,
                                Row = r.Key.Row,
                                ErrorMsg = string.Join(Environment.NewLine, r.Select(i => TranslateError(i.ErrorMsg, translations)))
                            };
                        })
                        .ToList();

                    var dt = excelErrors.ToDataTable();

                    // translate excel column name
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (translations.TryGetValue(dt.Columns[i].ColumnName, out string value))
                        {
                            dt.Columns[i].ColumnName = value;
                        }
                    }
                    var xlsx = _exportManager.ExportDataTableToXlsx(dt);
                    var sheet = xlsx.Workbook.Worksheets["sheet1"];

                    // excel custom styles
                    sheet.Cells.Style.WrapText = true;
                    sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                    return xlsx.GetAsByteArray();
                }
            }

            return null;
        }

        /// <summary>
        /// To check whether provided organization codes are existing on database
        /// </summary>
        /// <param name="tocheckOrganizationCodes"></param>
        /// <returns></returns>
        private IEnumerable<string> GetExistingOrganizationCodes(IEnumerable<string> tocheckOrganizationCodes)
        {
            var sql = @"SELECT [Code] FROM Organizations WHERE [Code] IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] (@tocheckOrganizationCodes, ','))";

            var filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@tocheckOrganizationCodes",
                        Value = string.Join(',', tocheckOrganizationCodes),
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                };

            IEnumerable<string> mappingCallback(DbDataReader reader)
            {
                var mappedData = new List<string>();

                while (reader.Read())
                {
                    mappedData.Add(reader[0].ToString());
                }

                return mappedData;
            }

            return _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
        }

        private IEnumerable<string> GetExistingOrganizationNames(IEnumerable<string> toCheckOrganizationNames)
        {
            var sql = @"SELECT [Name] FROM Organizations WHERE [Name] IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] (@toCheckOrganizationNames, ','))";

            var filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@toCheckOrganizationNames",
                        Value = string.Join(',', toCheckOrganizationNames),
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                };

            IEnumerable<string> mappingCallback(DbDataReader reader)
            {
                var mappedData = new List<string>();

                while (reader.Read())
                {
                    mappedData.Add(reader[0].ToString());
                }

                return mappedData;
            }

            return _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
        }

        private IEnumerable<string> GetExistingEdisonCompanyCodes(IEnumerable<string> toCheckEdisonCompanyCodes)
        {
            var sql = @"SELECT [EdisonCompanyCodeId] FROM Organizations WHERE [EdisonCompanyCodeId] IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] (@toCheckEdisonCompanyCodes, ','))";

            var filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@toCheckEdisonCompanyCodes",
                        Value = string.Join(',', toCheckEdisonCompanyCodes),
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                };

            IEnumerable<string> mappingCallback(DbDataReader reader)
            {
                var mappedData = new List<string>();

                while (reader.Read())
                {
                    mappedData.Add(reader[0].ToString());
                }

                return mappedData;
            }

            return _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
        }

        /// <summary>
        /// To get all available locations from database
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, long> GetAvailableLocations()
        {
            var sql = @"SELECT Name, Id FROM Locations ORDER BY Name ";

            Dictionary<string, long> mappingCallback(DbDataReader reader)
            {
                var mappedData = new Dictionary<string, long>();

                while (reader.Read())
                {
                    mappedData.Add(reader[0].ToString(), (long)reader[1]);
                }

                return mappedData;
            }

            return _dataQuery.GetDataBySql(sql, mappingCallback);
        }

    }
}