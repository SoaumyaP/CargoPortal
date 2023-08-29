using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Permissions.Services.Interfaces;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Hangfire;
using Groove.SP.Infrastructure.MicrosoftGraphAPI;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Data;
using AutoMapper.QueryableExtensions;

using System.Data.SqlClient;
using System.Data;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.Utilities;
using System.IO;
using OfficeOpenXml;
using Groove.SP.Infrastructure.Excel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System.Threading;
using System.Text.RegularExpressions;
using Groove.SP.Application.Providers.BlobStorage;
using static Groove.SP.Application.ApplicationBackgroundJob.SendMailBackgroundJobs;

namespace Groove.SP.Application.Users.Services
{
    public class UserProfileService : ServiceBase<UserProfileModel, UserProfileViewModel>, IUserProfileService
    {
        private const int LIMIT_EXCEL_IMPORT = 1000;

        private readonly AppConfig _appConfig;
        private readonly IPermissionService _permissionService;
        private readonly IB2CUserService _b2cUserService;
        private readonly IUserAuditLogRepository _userAuditLogRepository;
        private readonly IDataQuery _dataQuery;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IRoleRepository _roleRepository;
        private readonly IBlobStorage _blobStorage;
        private readonly IImportDataProgressRepository _importDataProgressRepository;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(IUnitOfWorkProvider uow,
            IOptions<AppConfig> appConfig,
            IPermissionService permissionService,
            IB2CUserService b2cUserService,
            IUserAuditLogRepository userAuditLogRepository,
            IDataQuery dataQuery,
            ICSFEApiClient csfeApiClient,
            IRoleRepository roleRepository,
            ILogger<UserProfileService> logger,
            IBlobStorage blobStorage,
            IImportDataProgressRepository importDataProgressRepository
            ) : base(uow)
        {
            _appConfig = appConfig.Value;
            _permissionService = permissionService;
            _b2cUserService = b2cUserService;
            _userAuditLogRepository = userAuditLogRepository;
            _dataQuery = dataQuery;
            _csfeApiClient = csfeApiClient;
            _roleRepository = roleRepository;
            _logger = logger;
            _importDataProgressRepository = importDataProgressRepository;
            _blobStorage = blobStorage;
        }

        #region Utils

        public string CreateSelfIssuedToken(
            string issuer,
            string audience,
            TimeSpan expiration,
            string signingSecret,
            ICollection<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var nowUtc = DateTime.UtcNow;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingSecret));
            var signingCredentials = new SigningCredentials(key, "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = audience,
                Expires = nowUtc.Add(expiration),
                IssuedAt = nowUtc,
                Issuer = issuer,
                NotBefore = nowUtc,
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(claims)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string BuildIdToken(string emailAddress)
        {
            var policyTokenLifetime = new TimeSpan(_appConfig.B2C.PolicyTokenLifeTime, 0, 0, 0);
            var policyClaims = new List<Claim>();
            var emailClaim = new Claim("email", emailAddress);
            policyClaims.Add(emailClaim);

            // Create the JWT containing the list of claims and signed by the client secret.
            var selfIssuedToken = CreateSelfIssuedToken(
                $"{_appConfig.B2C.Tenant}.onmicrosoft.com",
                _appConfig.B2C.RedirectUri,
                policyTokenLifetime,
                _appConfig.B2C.ClientSecret,
                policyClaims);

            return selfIssuedToken;
        }


        private string BuildAccountActivateUrl(string token)
        {
            string nonce = Guid.NewGuid().ToString("n");

            return string.Format(this._appConfig.B2C.PasswordResetUrl,
                    _appConfig.B2C.Tenant,
                    _appConfig.B2C.Policy,
                    _appConfig.B2C.ClientId,
                    nonce,
                    Uri.EscapeDataString(this._appConfig.B2C.RedirectUri)
                    )
                    + "&client_assertion_type=" + Uri.EscapeDataString("urn:ietf:params:oauth:client-assertion-type:jwt-bearer")
                    + "&client_assertion=" + token;
        }

        private string ForgotPasswordUrl
        {
            get
            {
                string nonce = Guid.NewGuid().ToString("n");

                return string.Format(this._appConfig.B2C.PasswordResetUrl,
                        _appConfig.B2C.Tenant,
                        _appConfig.B2C.ResetPasswordPolicyId,
                        _appConfig.B2C.ClientId,
                        nonce,
                        Uri.EscapeDataString(this._appConfig.B2C.RedirectUri));
            }
        }
        #endregion

        protected override IDictionary<string, string> SortMap { get; } = new Dictionary<string, string>() {
            { "statusName", "status"},
            { "organizationTypeName", "organizationType" }
        };

        protected override Func<IQueryable<UserProfileModel>, IQueryable<UserProfileModel>> FullIncludeProperties
        {
            get
            {
                return x => x.Include(m => m.UserRoles).ThenInclude(m => m.Role);
            }
        }

        public async Task<IEnumerable<UserProfileViewModel>> GetUsersByOrganizationIdAsync(long id)
        {
            var models = await this.Repository.Query(x => x.OrganizationId == id
                    && (x.Status == UserStatus.Active || x.Status == UserStatus.Inactive || x.Status == UserStatus.WaitForConfirm),
                null,
                FullIncludeProperties).ToListAsync();
            return Mapper.Map<IEnumerable<UserProfileViewModel>>(models);
        }

        public async Task<UserProfileViewModel> GetByUsernameAsync(string username)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Username == (username ?? ""));
            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }
            return Mapper.Map<UserProfileViewModel>(model);
        }

        public async Task<UserProfileViewModel> GetAsync(string userName)
        {
            var model = await this.Repository.GetAsync(x => x.Username == userName, null, FullIncludeProperties);
            var viewModel = Mapper.Map<UserProfileViewModel>(model);

            return viewModel;
        }

        public async Task<UserProfileViewModel> GetAsync(IdentityInfo currentUser)
        {
            var userName = currentUser.Username;
            var isUserRoleSwitch = currentUser.IsUserRoleSwitch;
            var userRoleId = currentUser.UserRoleId;

            // User role switch mode, get data by role
            if (isUserRoleSwitch)
            {
                var user = await Repository.GetAsync(x => x.Username == userName);
                var viewModel = Mapper.Map<UserProfileViewModel>(user);
                if (viewModel != null)
                {
                    var organization = await _csfeApiClient.GetOrganizationByIdAsync(currentUser.OrganizationId);
                    viewModel.IsInternal = false;
                    viewModel.OrganizationId = organization.Id;
                    viewModel.OrganizationName = organization.Name;
                    viewModel.IsUserRoleSwitch = true;

                    viewModel.UserRoles = new[]
                    {
                        new UserRoleViewModel
                        {
                            UserId = user.Id,
                            RoleId = userRoleId,
                            Role = new RoleViewModel
                            {
                                Id = userRoleId,
                                Name = EnumHelper<Role>.GetDisplayDescription((Role)userRoleId),
                                IsInternal = false,
                                IsOfficial = true,
                                Activated = true,
                                Status = RoleStatus.Active
                            }
                        }
                    };
                }

                return viewModel;
            }
            // Normal mode, get data by username/email
            else
            {
                var model = await Repository.GetAsync(x => x.Username == userName, null, FullIncludeProperties);
                var viewModel = Mapper.Map<UserProfileViewModel>(model);
                return viewModel;
            }
        }

        public async Task<UserProfileViewModel> GetAsync(long id)
        {
            var model = await this.Repository.GetAsync(x => x.Id == id, null, FullIncludeProperties);
            var viewModel = Mapper.Map<UserProfileViewModel>(model);
            viewModel.IdentityType = model.IsInternal ? _appConfig.InternalIdentityType : _appConfig.ExternalIdentityType;
            viewModel.IdentityTenant = model.IsInternal ? _appConfig.InternalIdentityTenant : _appConfig.ExternalIdentityTenant;

            return viewModel;
        }

        public async Task<IEnumerable<DropDownListItem<long>>> GetSelectionAsync(string searchEmail = "")
        {
            var query = Repository.QueryAsNoTracking(x => x.Status == UserStatus.Active);

            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                query = query.Where(x => x.Username.Contains(searchEmail));
            }

            var users = await query.Select(x => new DropDownListItem<long>
            {
                Text = x.Username,
                Value = x.Id
            }).ToListAsync();

            return users ?? new List<DropDownListItem<long>>();
        }

        public async Task<UserProfileViewModel> CreateUserAsync(UserProfileViewModel model, bool isInternal)
        {
            var user = new UserProfileModel
            {
                AccountNumber = Guid.NewGuid().ToString().ToUpper().Substring(0, AppConstant.ACCOUNT_NUMBER_LENGTH),
                Username = model.Username,
                Email = model.Email,
                Name = model.Name,
                Phone = model.Phone,
                CompanyName = isInternal ? _appConfig.InternalOrganization : model.CompanyName,
                CompanyAddressLine1 = model.CompanyAddressLine1,
                CompanyAddressLine2 = model.CompanyAddressLine2,
                CompanyAddressLine3 = model.CompanyAddressLine3,
                CompanyAddressLine4 = model.CompanyAddressLine4,
                CompanyWeChatOrWhatsApp = model.CompanyWeChatOrWhatsApp,
                Customer = model.Customer,
                Status = UserStatus.Pending,
                IsInternal = isInternal,
                OrganizationId = model.OrganizationId,
                OrganizationName = isInternal ? _appConfig.InternalOrganization : model.OrganizationName,
                OrganizationRoleId = model.OrganizationRoleId,
                OPContactEmail = model.OPContactEmail,
                OPContactName = model.OPContactName,
                OPCountryId = model.OPCountryId,
                OPLocationName = model.OPLocationName,
                TaxpayerId = model.TaxpayerId,
                LastSignInDate = DateTime.UtcNow,
                UserRoles = new List<UserRoleModel>
                {
                    new UserRoleModel
                    {
                        UserId = model.Id,
                        RoleId = (long) (isInternal ? Role.Pending : Role.Guest)
                    }
                }
            };

            user.Audit();

            // Create app account
            await Repository.AddAsync(user);
            await this.UnitOfWork.SaveChangesAsync();

            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"Account Registration Wait-For-Approve (User) {user.Email}",
                "AccountRegistrationWaitForApprove_User",
                 new AccountRegistrationEmailTemplateViewModel()
                 {
                     Name = model.Name,
                     SupportEmail = _appConfig.SupportEmail
                 },
                 model.Username, $" Shipment Portal: Email App Account Registration Wait-For-Approve (User)"));


            
            if (!string.IsNullOrWhiteSpace(user.OPContactEmail))
            {
                List<string> cc = new();
                var userOffice = await _csfeApiClient.GetUserOfficeByLocationAsync(user.OPLocationName, user.OPCountryId ?? 0);
                if (!string.IsNullOrWhiteSpace(userOffice?.CorpMarketingContactEmail))
                {
                    cc.Add(userOffice.CorpMarketingContactEmail);
                }
                if (!string.IsNullOrWhiteSpace(userOffice?.OPManagementContactEmail))
                {
                    cc.Add(userOffice.OPManagementContactEmail);
                }
                var attachmentContent = await GenerateOrganizationImportAttachmentAsync(user);

                BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync($"Account Registration Wait-For-Approve (Admin) {user.Email}",
                "AccountRegistrationWaitForApprove_Admin", model, user.OPContactEmail, cc,
                $"Shipment Portal: Email App Account Registration Wait-For-Approve (Admin)", new SPEmailAttachment
                {
                    AttachmentContent = attachmentContent,
                    AttachmentName = "CSP_OrganizationImportTemplate.xlsx"
                }));

            }



            return Mapper.Map<UserProfileViewModel>(user);
        }

        /// <summary>
        /// Generate the organization import attachment by user information.
        /// </summary>
        /// <returns></returns>
        private async Task<byte[]> GenerateOrganizationImportAttachmentAsync(UserProfileModel user)
        {
            byte[] fileContent = await _blobStorage.GetBlobByRelativePathAsync($"template:organization:OrganizationImportTemplate.xlsx");

            using (Stream fileStream = new MemoryStream(fileContent))
            {
                using (var xlPackage = new ExcelPackage(fileStream))
                {
                    var sheet = xlPackage.Workbook.Worksheets[0];

                    ExcelRange cell;
                    string cellName = "";

                    // OrganizationType
                    cellName = "A2";
                    cell = sheet.Cells[cellName];
                    cell.Value = OrganizationType.General.ToString();

                    // OrganizationName
                    //< Your Company Name>
                    cellName = "C2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.CompanyName;

                    // Address
                    //< Your Company Address>
                    cellName = "D2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.CompanyAddressLine1 ?? "";

                    // AddressLine2
                    //< Your Company Address>
                    cellName = "E2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.CompanyAddressLine2 ?? "";

                    // AddressLine3
                    //< Your Company Address>
                    cellName = "F2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.CompanyAddressLine3 ?? "";

                    // AddressLine4
                    //< Your Company Address>
                    cellName = "G2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.CompanyAddressLine4 ?? "";

                    // Location Code
                    //< Retrieve from OP Contact Contry and OP Contact City>
                    var location = await _csfeApiClient.GetLocationByDescriptionAsync(user.OPLocationName, user.OPCountryId ?? 0);
                    cellName = "H2";
                    cell = sheet.Cells[cellName];
                    cell.Value = location?.Name ?? "";

                    // ContactName
                    //<Display Name>
                    cellName = "I2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.Name;

                    // ContactEmail
                    //< Email >
                    cellName = "J2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.Email;

                    // ContactNumber
                    //<Phone Number>
                    cellName = "K2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.Phone;

                    // TaxpayerID
                    //<Taxpayer ID>
                    cellName = "P2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.TaxpayerId;

                    // WeChat/WhatsApp
                    //<YourCompanyWeChat/WhatsApp>
                    cellName = "Q2";
                    cell = sheet.Cells[cellName];
                    cell.Value = user.CompanyWeChatOrWhatsApp;

                    sheet.DeleteRow(3, 2);

                    fileContent = xlPackage.GetAsByteArray();
                }
            }

            return fileContent;
        }

        /// <summary>
        /// Add new an external user 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<UserProfileViewModel> CreateExternalUserAsync(UserProfileViewModel model)
        {
            var user = new UserProfileModel
            {
                AccountNumber = Guid.NewGuid().ToString().ToUpper().Substring(0, AppConstant.ACCOUNT_NUMBER_LENGTH),
                Username = model.Username,
                Email = model.Email,
                Name = model.Name,
                Phone = model.Phone,
                CompanyName = model.CompanyName,
                Title = model.Title,
                Department = model.Department,
                Status = UserStatus.WaitForConfirm,
                IsInternal = false,
                OrganizationId = model.OrganizationId,
                OrganizationName = model.OrganizationName,
                OrganizationCode = model.OrganizationCode,
                OrganizationRoleId = model.OrganizationRoleId,
                OrganizationType = model.OrganizationType,
                LastSignInDate = DateTime.UtcNow,
                UserRoles = new List<UserRoleModel>()
            };

            foreach (var userRole in model.UserRoles)
            {
                user.UserRoles.Add(
                   new UserRoleModel
                   {
                       User = user,
                       RoleId = userRole.RoleId
                   });
            }

            user.Audit();

            var b2cUser = await _b2cUserService.GetUserBySignInEmailAsync(model.Username);

            if (b2cUser == null) // Create B2C account
            {
                await _b2cUserService.CreateUserAsync(user.Name,
                    user.Username,
                    user.Title,
                    user.Department,
                    user.CompanyName);

                string token = BuildIdToken(user.Username);
                string activationLink = BuildAccountActivateUrl(token);

                BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"New Account is Created {user.Email}",
                    "AccountCreation_Activation_ExternalUser", new AccountCreationEmailTemplateViewModel()
                    {
                        ActivationLink = activationLink,
                        ForgotPasswordLink = ForgotPasswordUrl,
                        ActivationExpireIn = _appConfig.B2C.PolicyTokenLifeTime,
                    },
                    model.Username, $" Shipment Portal: Email App New Account is Created")
                );
            }
            else
            {
                BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"New Account is Created {user.Email}",
                    "AccountCreation_Inform_ExternalUser", new AccountCreationEmailTemplateViewModel()
                    {
                        SigninLink = $"{_appConfig.ClientUrl}/login?type=ex"
                    },
                    model.Username, $" Shipment Portal: Email App New Account is Created")
                );
            }

            // Create app account
            await Repository.AddAsync(user);
            await this.UnitOfWork.SaveChangesAsync();

            // Include Role in result
            var result = await Repository.GetAsync(x => x.Id == user.Id, null, FullIncludeProperties);
            return Mapper.Map<UserProfileViewModel>(result);
        }

        public async Task<bool> UpdateStatusUsersAsync(long organizationId, UserStatus status, string username)
        {
            try
            {
                IEnumerable<UserProfileModel> userList = null;
                if (status == UserStatus.Active)
                {
                    userList = await this.Repository.Query(x => x.OrganizationId == organizationId && x.Status == UserStatus.Inactive).ToListAsync();
                }
                else
                {
                    if (status == UserStatus.Inactive)
                    {
                        userList = await this.Repository.Query(x => x.OrganizationId == organizationId && x.Status == UserStatus.Active).ToListAsync();
                    }
                }
                foreach (var user in userList)
                {
                    user.Status = status;
                    user.Audit(username);
                }

                await this.UnitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new AppException(ex.ToString());
            }
        }

        public async Task<UserProfileViewModel> UpdateUserStatusAsync(long id, UpdateUserStatusViewModel viewModel, string username)
        {
            var userProfile = await Repository.GetAsync(x => x.Id == id);

            if (userProfile == null)
            {
                throw new AppEntityNotFoundException($"Object with Id {id} not found!");
            }

            userProfile.Status = viewModel.Status;
            userProfile.Audit(username);

            Repository.Update(userProfile);
            await UnitOfWork.SaveChangesAsync();

            return Mapper.Map<UserProfileViewModel>(userProfile);
        }

        public async Task<bool> CheckExistsUser(string email)
        {
            var user = await this.Repository.GetAsync(x => x.Email == email);
            return user != null;
        }

        public async Task<bool> UpdateOrganizationAsync(long organizationId, string organizationName, OrganizationType organizationType, string username)
        {
            try
            {
                IEnumerable<UserProfileModel> userList = null;
                userList = await this.Repository.Query(x => x.OrganizationId == organizationId).ToListAsync();
                foreach (var user in userList)
                {
                    user.OrganizationName = organizationName;
                    user.OrganizationType = organizationType;
                    user.Audit(username);
                }

                await this.UnitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new AppException(ex.ToString());
            }
        }

        public async Task UpdateLastSignInDateAsync(string username)
        {
            try
            {
                var userList = await this.Repository.Query(x => x.Username == username).ToListAsync();
                foreach (var user in userList)
                {
                    user.LastSignInDate = DateTime.UtcNow;
                }
                await this.UnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new AppException(ex.ToString());
            }
        }

        public async Task SyncUserTracking(string username, IEnumerable<UserAuditLogViewModel> viewModels)
        {
            try
            {
                if (viewModels == null || !viewModels.Any())
                {
                    return;
                }

                var models = Mapper.Map<IEnumerable<UserAuditLogModel>>(viewModels);
                foreach (var item in models)
                {
                    item.Email = username;
                    item.Audit(username);
                }
                await _userAuditLogRepository.AddRangeAsync(models.ToArray());
                await UnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new AppException(ex.ToString());
            }
        }

        public override async Task<DataSourceResult> ListAsync(DataSourceRequest request)
        {
            CompositeFilterDescriptor descriptor = new CompositeFilterDescriptor()
            {
                FilterDescriptors = new FilterDescriptorCollection
                {
                    new FilterDescriptor("status", FilterOperator.IsEqualTo, UserStatus.Active),
                    new FilterDescriptor("status", FilterOperator.IsEqualTo, UserStatus.Inactive),
                    new FilterDescriptor("status", FilterOperator.IsEqualTo, UserStatus.WaitForConfirm)
                },
                LogicalOperator = FilterCompositionLogicalOperator.Or
            };
            if (request.Filters == null)
            {
                request.Filters = new FilterDescriptorCollection();
            }
            request.Filters.Add(descriptor);

            IQueryable<UserListQueryModel> query;
            string sql =
                        @"SELECT
                            USP.Id,
	                        USP.AccountNumber,
	                        USP.Email,
	                        USP.[Name],
							USP.IsInternal,
                            USP.OrganizationId,
	                        USP.OrganizationName,
	                        USP.OrganizationType,
							CASE WHEN USP.OrganizationType = 1 THEN 'label.general'
								 WHEN USP.OrganizationType = 2 THEN 'label.agent'
								 WHEN USP.OrganizationType = 4 THEN 'label.principal'
							ELSE '' END AS OrganizationTypeName,
	                        USP.LastSignInDate,
	                        USP.[Status],
							CASE WHEN USP.[Status] = -1 THEN 'label.deleted'
								 WHEN USP.[Status] = 0 THEN 'label.rejected'
								 WHEN USP.[Status] = 1 THEN 'label.pending'
								 WHEN USP.[Status] = 2 THEN 'label.active'
								 WHEN USP.[Status] = 3 THEN 'label.inactive'
								 WHEN USP.[Status] = 4 THEN 'label.waitForConfirm'
							ELSE '' END AS StatusName,
	                        IIF(T.[Name] IS NULL, '', T.[Name]) AS RoleName
                        FROM UserProfiles USP (NOLOCK)
                        OUTER APPLY (
	                        SELECT TOP 1 R.*
	                        FROM UserRoles USR (NOLOCK) INNER JOIN Roles R (NOLOCK) ON USR.RoleId = R.Id
	                        WHERE USR.UserId = USP.Id
						) T
                   ";

            query = _dataQuery.GetQueryable<UserListQueryModel>(sql);

            var data = await query.ToDataSourceResultAsync(request);
            return data;
        }

        public async Task<UserProfileViewModel> UpdateCurrentUserAsync(UserProfileViewModel viewModel, string username)
        {
            try
            {
                viewModel.ValidateAndThrow();
                var currentModel = await this.Repository.GetAsync(x => x.Username == username);
                currentModel.Audit(username);
                currentModel.Name = viewModel.Name;
                currentModel.Title = viewModel.Title;
                currentModel.Department = viewModel.Department;
                currentModel.ProfilePicture = viewModel.ProfilePicture;
                currentModel.Phone = viewModel.Phone;
                currentModel.CompanyName = viewModel.CompanyName;
                currentModel.CountryId = viewModel.CountryId;
                await this.UnitOfWork.SaveChangesAsync();
                return viewModel;
            }
            catch (Exception ex)
            {
                throw new AppException(ex.ToString());
            }
        }

        public async Task<UserProfileViewModel> UpdateAsync(long id, UserProfileViewModel viewModel, string username)
        {
            try
            {
                UnitOfWork.BeginTransaction();

                viewModel.ValidateAndThrow();
                var model = await Repository.GetAsync(x => x.Id == id, null, FullIncludeProperties);
                var role = model.UserRoles?.Select(ur => ur.Role).FirstOrDefault();
                var isChangeRole = role != null && role.Id != viewModel.Role.Id;

                // update UserRoles
                if (viewModel.Role != null)
                {
                    var userRole = model.UserRoles?.FirstOrDefault();
                    if (userRole != null)
                    {
                        model.UserRoles.Remove(userRole);
                    }
                    await UnitOfWork.SaveChangesAsync();
                    userRole = new UserRoleModel
                    {
                        UserId = model.Id,
                        RoleId = viewModel.Role.Id
                    };
                    userRole.Audit(username);
                    model.UserRoles?.Add(userRole);
                }

                model.Name = viewModel.Name;
                model.Phone = viewModel.Phone;
                model.CountryId = viewModel.CountryId;
                model.OrganizationId = viewModel.OrganizationId;
                model.OrganizationName = viewModel.OrganizationName;
                model.OrganizationCode = viewModel.OrganizationCode;
                model.OrganizationType = viewModel.OrganizationType;
                model.CompanyName = viewModel.CompanyName;
                model.Title = viewModel.Title;
                model.Department = viewModel.Department;
                model.Status = viewModel.Status;

                model.Audit(username);
                this.Repository.Update(model);
                await this.UnitOfWork.SaveChangesAsync();

                if (isChangeRole)
                {
                    await _permissionService.InvalidatePermissionCache(username);
                    BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"Your role in Shipment Portal has been changed {model.Email}",
                        "EmailUserRoleChanged", new UserRoleChangedEmailTemplateViewModel() {
                            Name = viewModel.Name,
                            RoleName = viewModel.Role.Name,
                            SupportEmail = _appConfig.SupportEmail
                        }, model.Username, $"Shipment Portal: Your role in Shipment Portal has been changed"));
                }

                UnitOfWork.CommitTransaction();
                return Mapper.Map<UserProfileModel, UserProfileViewModel>(model);
            }
            catch
            {
                UnitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task ActivateUserAsync(string username)
        {
            var user = await this.Repository.GetAsync(x => x.Username == username);

            if (user.Status == UserStatus.WaitForConfirm)
            {
                user.Status = UserStatus.Active;
            }

            await this.UnitOfWork.SaveChangesAsync();
        }

        public async Task<DataSourceResult> TraceUserByEmailSearchingAsync(DataSourceRequest request, string email)
        {
            IQueryable<UserAuditLogQueryModel> query;
            string sql = @"
                        SELECT Id, OperatingSystem, Browser, ScreenSize, Feature, AccessDateTime
                        FROM UserAuditLogs WITH (NOLOCK)
                        WHERE Email = {0}";
            query = _dataQuery.GetQueryable<UserAuditLogQueryModel>(sql, email);
            return await query.ProjectTo<UserAuditLogViewModel>(Mapper.ConfigurationProvider)
                .ToDataSourceResultAsync(request);
        }

        public async Task<long> TraceUserByEmailTotalCountAsync(string email)
        {
            string sql = @"
                         -- Default is zero
                        SET @result = 0;
                        
		                SELECT @result = COUNT(1)
                        FROM UserAuditLogs WITH (NOLOCK)
                        WHERE Email = @email";


            var param = new SqlParameter
            {
                ParameterName = "@email",
                Value = email,
                DbType = DbType.String,
                Direction = ParameterDirection.Input
            };
            var totalRowCount = long.Parse(_dataQuery.GetValueFromVariable(sql, new[] { param }));
            return totalRowCount;
        }

        public async Task RemoveExternalUserCompletelyAsync(long id, IdentityInfo currenrUser)
        {
            var userProfile = await Repository.FindAsync(id);

            // Only work for external user
            if (userProfile == null || userProfile.IsInternal)
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");

            }

            if (!currenrUser.IsInternal)
            {
                // Must be in the same org
                if (currenrUser.OrganizationId != userProfile.OrganizationId)
                {
                    throw new AppAuthorizationException($"No permission to delete!");
                }

                // Must be org admin
                var organization = await _csfeApiClient.GetOrganizationByIdAsync(currenrUser.OrganizationId);
                if (organization?.AdminUser != currenrUser.Email)
                {
                    throw new AppAuthorizationException($"No permission to delete!");
                }
            }

            Repository.RemoveByKeys(userProfile.Id);
            await _b2cUserService.RemoveUserAsync(userProfile.Email);
            await UnitOfWork.SaveChangesAsync();

        }

        public async Task ValidateExcelImportAsync(byte[] file, string fileName, string userName, long importDataProgressId)
        {
            using (var bjUoW = UnitOfWorkProvider.CreateUnitOfWorkForBackgroundJob())
            {
                var importDataProgressRepository = (IImportDataProgressRepository)bjUoW.GetRepository<ImportDataProgressModel>();

                if (file == null)
                {
                    await importDataProgressRepository.UpdateStatusAsync(
                        importDataProgressId,
                        ImportDataProgressStatus.Failed,
                        "importResult.fileIsNull");

                    return;
                }

                try
                {
                    var emailCheck = new EmailAddressAttribute();
                    var propertyManager = new PropertyManager<UserExcelViewModel>();
                    var validationErrorLogs = new List<ValidatorErrorInfo>();
                    var excelUsers = new List<UserExcelViewModel>();

                    ReadExcelUserImportContent(file, propertyManager, out excelUsers);
                    if (excelUsers.Count > LIMIT_EXCEL_IMPORT)
                    {
                        await importDataProgressRepository.UpdateStatusAsync(
                                   importDataProgressId,
                                   ImportDataProgressStatus.Failed,
                                   "importResult.limit1000User");  

                        return;
                    }

                    // progess starting...                 
                    await importDataProgressRepository.UpdateProgressAsync(importDataProgressId, 0, excelUsers.Count);
                    // update progress status
                    await importDataProgressRepository.UpdateStatusAsync(importDataProgressId, ImportDataProgressStatus.Started, "importResult.validatingData");

                    var excelOrgCodes = excelUsers.Select(x => x.OrganizationCode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct();
                    var organizations = await _csfeApiClient.GetOrganizationsByCodesAsync(excelOrgCodes);

                    var completedStep = 0;
                    foreach (var excelUser in excelUsers)
                    {
                        if (!string.IsNullOrWhiteSpace(excelUser.Email))
                        {
                            // check duplicate in excel file
                            var existingInExcel = excelUsers.Any(user => user.Email == excelUser.Email && user.Row != excelUser.Row);
                            // check duplicate in system
                            var existingInSystem = await Repository.AnyAsync(user => user.Email == excelUser.Email);

                            if (existingInExcel)
                            {
                                validationErrorLogs.Add(
                                   new ValidatorErrorInfo
                                   {
                                       ObjectName = excelUser.Email,
                                       Row = excelUser.Row,
                                       Column = "label.email",
                                       ErrorMsg = "importLog.duplicatedValue"
                                   });
                            }
                            if (existingInSystem)
                            {
                                validationErrorLogs.Add(
                                   new ValidatorErrorInfo
                                   {
                                       ObjectName = excelUser.Email,
                                       Row = excelUser.Row,
                                       Column = "label.email",
                                       ErrorMsg = "importLog.valueIsExisting"
                                   });
                            }

                            var checkingEmail = Regex.Replace(excelUser.Email, @"\s+", "");

                            // check max length
                            if (checkingEmail.Length > 40)
                            {
                                validationErrorLogs.Add(new ValidatorErrorInfo
                                {
                                    Column = "label.email",
                                    Row = excelUser.Row,
                                    ObjectName = excelUser.Email,
                                    ErrorMsg = "importLog.longerThan40Characters"
                                });
                            }
                            // check email format
                            if (!emailCheck.IsValid(checkingEmail))
                            {
                                validationErrorLogs.Add(
                                   new ValidatorErrorInfo
                                   {
                                       ObjectName = excelUser.Email,
                                       Row = excelUser.Row,
                                       Column = "label.email",
                                       ErrorMsg = "importLog.wrongFormat"
                                   });
                            }
                        }
                        else
                        {
                            // missing email
                            validationErrorLogs.Add(
                                new ValidatorErrorInfo
                                {
                                    ObjectName = excelUser.Email,
                                    Row = excelUser.Row,
                                    Column = "label.email",
                                    ErrorMsg = "importLog.requiredField"
                                });
                        }

                        // Validate Role
                        var validRole = new Role[]
                        {
                                    Role.Shipper,
                                    Role.Agent,
                                    Role.Principal,
                                    Role.CruisePrincipal,
                                    Role.CruiseAgent,
                                    Role.Warehouse,
                                    Role.Factory
                        };
                        if (!string.IsNullOrWhiteSpace(excelUser.Role))
                        {
                            var isValidImportRole = validRole.Any(x => x.GetAttributeValue<DisplayAttribute, string>(x => x.Description).ToLower() == excelUser.Role.ToLower());
                            if (!isValidImportRole)
                            {
                                validationErrorLogs.Add(new ValidatorErrorInfo
                                {
                                    Row = excelUser.Row,
                                    Column = "label.role",
                                    ObjectName = excelUser.Role,
                                    ErrorMsg = "importLog.valueIsExisting"
                                });
                            }
                        }
                        else
                        {
                            validationErrorLogs.Add(new ValidatorErrorInfo
                            {
                                Row = excelUser.Row,
                                Column = "label.role",
                                ObjectName = excelUser.Role,
                                ErrorMsg = "importLog.requiredField"
                            });
                        }

                        // Validate Name
                        if (string.IsNullOrWhiteSpace(excelUser.Name))
                        {
                            validationErrorLogs.Add(new ValidatorErrorInfo
                            {
                                Row = excelUser.Row,
                                Column = "label.name",
                                ObjectName = excelUser.Name,
                                ErrorMsg = "importLog.requiredField"
                            });
                        }

                        // Validate Phone
                        if (!string.IsNullOrEmpty(excelUser.Phone))
                        {
                            if (excelUser.Phone.Length >= 128)
                            {
                                validationErrorLogs.Add(new ValidatorErrorInfo
                                {
                                    Row = excelUser.Row,
                                    Column = "label.phone",
                                    ObjectName = excelUser.Phone,
                                    ErrorMsg = "importLog.tooLongField"
                                });
                            }
                        }

                        // Validate CountryCode
                        if (!string.IsNullOrWhiteSpace(excelUser.CountryCode))
                        {
                            var country = await _csfeApiClient.GetCountryByCodeAsync(excelUser.CountryCode);
                            if (country == null)
                            {
                                validationErrorLogs.Add(new ValidatorErrorInfo
                                {
                                    Row = excelUser.Row,
                                    Column = "label.countryCode",
                                    ObjectName = excelUser.CountryCode,
                                    ErrorMsg = "importLog.valueIsNotExisting"
                                });
                            }
                        }

                        // Validate Department
                        if (!string.IsNullOrEmpty(excelUser.Department))
                        {
                            if (excelUser.Department.Length >= 128)
                            {
                                validationErrorLogs.Add(new ValidatorErrorInfo
                                {
                                    Row = excelUser.Row,
                                    Column = "label.department",
                                    ObjectName = excelUser.Department,
                                    ErrorMsg = "importLog.tooLongField"
                                });
                            }
                        }

                        // Validate OrganizationCode
                        if (!string.IsNullOrWhiteSpace(excelUser.OrganizationCode))
                        {
                            var isFactoryUser = excelUser.Role.ToLower() == Role.Factory.GetAttributeValue<DisplayAttribute, string>(x => x.Description).ToLower();

                            var validOrgCode = organizations.FirstOrDefault(organization
                                => organization.Code == excelUser.OrganizationCode && (!isFactoryUser || organization.OrganizationType == (int)OrganizationType.General)) != null;
                            if (!validOrgCode)
                            {
                                validationErrorLogs.Add(new ValidatorErrorInfo
                                {
                                    Row = excelUser.Row,
                                    Column = "label.organizationCode",
                                    ObjectName = excelUser.OrganizationCode,
                                    ErrorMsg = "importLog.valueIsNotExisting"
                                });
                            }
                        }
                        else
                        {
                            validationErrorLogs.Add(new ValidatorErrorInfo
                            {
                                Row = excelUser.Row,
                                Column = "label.organizationCode",
                                ObjectName = excelUser.OrganizationCode,
                                ErrorMsg = "importLog.requiredField"
                            });
                        }

                        // Validate Title
                        if (!string.IsNullOrEmpty(excelUser.Title))
                        {
                            if (excelUser.Title.Length >= 128)
                            {
                                validationErrorLogs.Add(new ValidatorErrorInfo
                                {
                                    Row = excelUser.Row,
                                    Column = "label.title",
                                    ObjectName = excelUser.Title,
                                    ErrorMsg = "importLog.tooLongField"
                                });
                            }
                        }

                        // update progress...
                        completedStep++;
                        await importDataProgressRepository.UpdateProgressAsync(importDataProgressId, completedStep);
                    }

                    if (validationErrorLogs.Any())
                    {
                        string log = JsonConvert.SerializeObject(
                           validationErrorLogs,
                           new JsonSerializerSettings
                           {
                               ContractResolver = new CamelCasePropertyNamesContractResolver()
                           });

                        await importDataProgressRepository.UpdateStatusAsync(
                           importDataProgressId,
                           ImportDataProgressStatus.Failed,
                           "importResult.fileValidationFailed",
                           log);
                    }
                    else
                    {
                        await UnitOfWork.SaveChangesAsync();
                        await importDataProgressRepository.UpdateStatusAsync(
                           importDataProgressId,
                           ImportDataProgressStatus.Success,
                           "importResult.validationUserImportSuccessfully");
                    }
                }
                catch (InvalidDataException ex)
                {
                    await importDataProgressRepository.UpdateStatusAsync(
                        importDataProgressId,
                        ImportDataProgressStatus.Failed,
                        "importResult.invalidExcelFile",
                        ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot validate User from Excel", ex.Message);
                    await importDataProgressRepository.UpdateStatusAsync(
                        importDataProgressId,
                        ImportDataProgressStatus.Failed,
                        "importResult.cannotValidate",
                         AppException.GetTrueExceptionMessage(ex));
                }
            }
        }

        public async Task ImportExcelSilentAsync(byte[] file, string fileName, string userName, string email, string name, long importDataProgressId)
        {
            var propertyManager = new PropertyManager<UserExcelViewModel>();
            var excelUsers = new List<UserExcelViewModel>();

            ReadExcelUserImportContent(file, propertyManager, out excelUsers);

            if ((excelUsers.Count > LIMIT_EXCEL_IMPORT) || !excelUsers.Any())
            {
                return;
            }

            var importedUsers = new List<UserProfileModel>();
            var failedUsers = new List<FailedUserImportViewModel>();

            var orgCodes = excelUsers.Select(u => u.OrganizationCode).Distinct();
            var organizations = await _csfeApiClient.GetOrganizationsByCodesAsync(orgCodes);

            var countryCodes = excelUsers.Where(u => !string.IsNullOrWhiteSpace(u.CountryCode)).Select(u => u.CountryCode).Distinct();


            foreach (var excelUser in excelUsers)
            {
                using (var bjUoW = UnitOfWorkProvider.CreateUnitOfWorkForBackgroundJob())
                {
                    var importDataProgressRepository = (IImportDataProgressRepository)bjUoW.GetRepository<ImportDataProgressModel>();
                    var userProfileRepository = (IUserProfileRepository)bjUoW.GetRepository<UserProfileModel>();

                    try
                    {
                        bjUoW.BeginTransaction();

                        var organization = organizations.FirstOrDefault(org => org.Code == excelUser.OrganizationCode);
                        var user = new UserProfileModel
                        {
                            AccountNumber = Guid.NewGuid().ToString().ToUpper().Substring(0, AppConstant.ACCOUNT_NUMBER_LENGTH),
                            Username = excelUser.Email,
                            Email = excelUser.Email,
                            Name = excelUser.Name,
                            Phone = excelUser.Phone,
                            CompanyName = organization?.Name,
                            Title = excelUser.Title,
                            Department = excelUser.Department,
                            Status = UserStatus.WaitForConfirm,
                            IsInternal = false,
                            OrganizationId = organization.Id,
                            OrganizationName = organization?.Name,
                            OrganizationCode = organization?.Code,
                            OrganizationType = organization?.OrganizationType != null ? (OrganizationType)organization.OrganizationType : 0,
                            LastSignInDate = DateTime.UtcNow,
                            UserRoles = new List<UserRoleModel>()
                        };

                        // Country id
                        if (!string.IsNullOrWhiteSpace(excelUser.CountryCode))
                        {
                            var countryId = (await _csfeApiClient.GetCountryByCodeAsync(excelUser.CountryCode)).Id;
                            user.CountryId = countryId;
                        }

                        // Add user role
                        var roleId = (await _roleRepository.GetAsNoTrackingAsync(x => x.Name == excelUser.Role)).Id;
                        user.UserRoles.Add(new UserRoleModel
                        {
                            User = user,
                            RoleId = roleId
                        });

                        user.Audit();

                        await userProfileRepository.AddAsync(user);
                        await bjUoW.SaveChangesAsync();

                        var b2cUser = await _b2cUserService.GetUserBySignInEmailAsync(user.Username);

                        if (b2cUser == null) // Create B2C account
                        {
                            await _b2cUserService.CreateUserAsync(user.Name,
                                user.Username,
                                !string.IsNullOrWhiteSpace(user.Title) ? user.Title : null,
                                !string.IsNullOrWhiteSpace(user.Department) ? user.Department : null,
                                user.CompanyName);

                            string token = BuildIdToken(user.Username);
                            string activationLink = BuildAccountActivateUrl(token);

                            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"New Account is Created {user.Email}",
                                "AccountCreation_Activation_ExternalUser", new AccountCreationEmailTemplateViewModel()
                                {
                                    ActivationLink = activationLink,
                                    ForgotPasswordLink = ForgotPasswordUrl,
                                    ActivationExpireIn = _appConfig.B2C.PolicyTokenLifeTime,
                                },
                                user.Username, $" Shipment Portal: Email App New Account is Created")
                            );
                        }
                        else
                        {
                            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"New Account is Created {user.Email}",
                                "AccountCreation_Inform_ExternalUser", new AccountCreationEmailTemplateViewModel()
                                {
                                    SigninLink = $"{_appConfig.ClientUrl}/login?type=ex"
                                },
                                user.Username, $" Shipment Portal: Email App New Account is Created")
                            );
                        }

                        bjUoW.CommitTransaction();
                        importedUsers.Add(user);
                    }
                    catch (Exception ex)
                    {
                        bjUoW.RollbackTransaction();

                        if (ex.InnerException != null && ex.InnerException.Message.Contains("AK_UserProfiles_Username"))
                        {
                            failedUsers.Add(new FailedUserImportViewModel
                            {
                                UserName = excelUser.Email,
                                Remark = "Duplicated email found.",
                                Log = AppException.GetTrueExceptionMessage(ex)
                            });
                        }
                        else
                        {
                            failedUsers.Add(new FailedUserImportViewModel
                            {
                                UserName = excelUser.Email,
                                Remark = "Please retry import.",
                                Log = AppException.GetTrueExceptionMessage(ex)
                            });
                        }
                        _logger.LogError(ex, $"Cannot import User {excelUser.Email} from Excel", ex.Message);

                        continue;
                    }
                }
            }
            // write log silent
            try
            {
                await _importDataProgressRepository.UpdateProgressAsync(importDataProgressId, importedUsers.Count, excelUsers.Count);

                await _importDataProgressRepository.UpdateStatusAsync(
                                    importDataProgressId,
                                    ImportDataProgressStatus.Success,
                                    $"{importedUsers.Count} record(s) imported",
                                    JsonConvert.SerializeObject(failedUsers));
            }
            catch
            {
                // try to log file...
                _logger.LogError(JsonConvert.SerializeObject(failedUsers));
            }

            // send mail to importer
            var emailModel = new ImportUserResultEmailModel
            {
                Name = name,
                FileName = fileName,
                CompletedOn = DateTime.UtcNow,
                FailCount = failedUsers.Count,
                SuccessCount = importedUsers.Count,
                TotalImport = excelUsers.Count,
                FailedUser = failedUsers
            };
            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"User excel import result", "User_ExcelImportResult",
                emailModel, email, $"Shipment Portal: User import result"));
        }

        public async Task SendActivationEmail(long id)
        {
            var user = await Repository.GetAsNoTrackingAsync(x => x.Id == id);

            if (user == null)
            {
                throw new AppEntityNotFoundException($"Object with id {id} not found!");
            }

            string token = BuildIdToken(user.Username);
            string activationLink = BuildAccountActivateUrl(token);
            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"New Account is Created {user.Email}",
                "AccountCreation_Activation_ExternalUser", new AccountCreationEmailTemplateViewModel()
                {
                    ActivationLink = activationLink,
                    ForgotPasswordLink = ForgotPasswordUrl,
                    ActivationExpireIn = _appConfig.B2C.PolicyTokenLifeTime,
                },
                user.Username, $" Shipment Portal: Email App New Account is Created")
            );
        }

        private void ReadExcelUserImportContent(byte[] file, PropertyManager<UserExcelViewModel> propetyManager, out List<UserExcelViewModel> outputModel)
        {
            using (Stream fileStream = new MemoryStream(file))
            {
                using (var xlPackage = new ExcelPackage(fileStream))
                {
                    var excelUsers = new List<UserExcelViewModel>();

                    var workSheet = xlPackage.Workbook.Worksheets[0];
                    int rowCount = workSheet.Dimension.Rows;

                    for (var row = 2; row <= rowCount; row++)
                    {
                        if (workSheet.IsRowEmpty(row)) continue;
                        var userExcelViewModel = propetyManager.ReadExcelByRowWithoutValidation(workSheet, row);
                        userExcelViewModel.Row = row.ToString();
                        // This line code will make sure the Role is always UPPERCASE at first, unless it will cause some issues.
                        userExcelViewModel.Role = StringHelper.FirstCharToUpperCase(userExcelViewModel.Role);
                        excelUsers.Add(userExcelViewModel);
                    }
                    outputModel = excelUsers;
                }
            }
        }
    }
    public class FailedUserImportViewModel
    {
        public string UserName { get; set; }
        public string Remark { get; set; }
        public string Log { get; set; }
    }
    public class ImportUserResultEmailModel
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public DateTime CompletedOn { get; set; }
        public int TotalImport { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<FailedUserImportViewModel> FailedUser { get; set; }
    }
}