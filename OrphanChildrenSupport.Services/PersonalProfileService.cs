using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OrphanChildrenSupport.DataContracts;
using OrphanChildrenSupport.DataContracts.Resources;
using OrphanChildrenSupport.HttpClientFactory.Libraries;
using OrphanChildrenSupport.Services.Contracts;
using OrphanChildrenSupport.Services.Models;
using OrphanChildrenSupport.Tools.Encryptions;
using OrphanChildrenSupport.Tools.HttpContextExtensions;
using OrphanChildrenSupport.Infrastructure.Repositories;
using OrphanChildrenSupport.Infrastructure.Repositories.Specifications;
using Microsoft.EntityFrameworkCore;

namespace OrphanChildrenSupport.Servicess
{
    public class PersonalProfileService : IPersonalProfileService
    {

        private string _connectionString;
        private string _folderid;
        private string _type;
        private ICryptoEncryptionHelper _cryptoEncryptionHelper;
        private IHttpContextHelper _httpContextHelper;
        private readonly IMapper _mapper;
        private readonly ILogger<PersonalProfileService> _logger;

        public PersonalProfileService(IMapper mapper, ILogger<PersonalProfileService> logger, IConfiguration config,
            ICryptoEncryptionHelper cryptoEncryptionHelper, IHttpContextHelper httpContextHelper)
        {
            _mapper = mapper;
            _logger = logger;
            _connectionString = config.GetValue<string>("ConnectionStrings:OrphanChildrenSupportConnection") ?? "";
            _folderid = config.GetValue<string>("LibraryApi:PersonalProfileAvatarFolderId") ?? "";
            _type = config.GetValue<string>("LibraryApi:Type") ?? "";
            _cryptoEncryptionHelper = cryptoEncryptionHelper;
            _httpContextHelper = httpContextHelper;
        }

        public async Task<ApiResponse<PersonalProfileResource>> CreatePersonalProfile(PersonalProfileResource personalProfileResource)
        {
            const string loggerHeader = "CreatePersonalProfile";

            var apiResponse = new ApiResponse<PersonalProfileResource>();
            PersonalProfile personalProfile = _mapper.Map<PersonalProfileResource, PersonalProfile>(personalProfileResource);

            _logger.LogDebug($"{loggerHeader} - Start to add PersonalProfile: {JsonConvert.SerializeObject(personalProfile)}");
            using (var unitOfWork = new UnitOfWork(_connectionString))
            {
                try
                {
                    personalProfile.CreatedBy = _httpContextHelper.GetCurrentUser();
                    personalProfile.CreatedTime = DateTime.UtcNow;
                    personalProfile.AccountName = personalProfile.Email.Split("@")[0];
                    await unitOfWork.PersonalProfileRepository.Add(personalProfile);
                    await unitOfWork.SaveChanges();
                    _logger.LogDebug($"{loggerHeader} - Add new PersonalProfile successfully with Id: {personalProfile.Id}");
                    personalProfile = await unitOfWork.PersonalProfileRepository.FindFirst(predicate: d => d.Id == personalProfile.Id);
                    apiResponse.Data = _mapper.Map<PersonalProfile, PersonalProfileResource>(personalProfile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{loggerHeader} have error: {ex.Message}");
                    apiResponse.IsError = true;
                    apiResponse.Message = ex.Message;
                    await unitOfWork.SaveErrorLog(ex);
                }
                finally
                {
                    unitOfWork.Dispose();
                }
            }

            return apiResponse;
        }

        public async Task<ApiResponse<PersonalProfileResource>> UpdatePersonalProfile(long id, PersonalProfileResource personalProfileResource)
        {
            const string loggerHeader = "UpdatePersonalProfile";
            var apiResponse = new ApiResponse<PersonalProfileResource>();

            using (var unitOfWork = new UnitOfWork(_connectionString))
            {
                try
                {
                    var personalProfile = await unitOfWork.PersonalProfileRepository.FindFirst(predicate: d => d.Id == id);
                    personalProfile = _mapper.Map<PersonalProfileResource, PersonalProfile>(personalProfileResource, personalProfile);
                    _logger.LogDebug($"{loggerHeader} - Start to update PersonalProfile: {JsonConvert.SerializeObject(personalProfile)}");
                    personalProfile.AccountName = personalProfile.Email.Split("@")[0];
                    personalProfile.ModifiedBy = _httpContextHelper.GetCurrentUser();
                    personalProfile.LastModified = DateTime.UtcNow;
                    unitOfWork.PersonalProfileRepository.Update(personalProfile);
                    await unitOfWork.SaveChanges();
                    _logger.LogDebug($"{loggerHeader} - Update PersonalProfile successfully with Id: {personalProfile.Id}");

                    personalProfile = await unitOfWork.PersonalProfileRepository.FindFirst(predicate: d => d.Id == personalProfile.Id);
                    apiResponse.Data = _mapper.Map<PersonalProfile, PersonalProfileResource>(personalProfile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{loggerHeader} have error: {ex.Message}");
                    apiResponse.IsError = true;
                    apiResponse.Message = ex.Message;
                    await unitOfWork.SaveErrorLog(ex);
                }
                finally
                {
                    unitOfWork.Dispose();
                }
            }

            return apiResponse;
        }

        public async Task<ApiResponse<PersonalProfileResource>> DeletePersonalProfile(long id, bool removeFromDB = false)
        {
            const string loggerHeader = "DeletePersonalProfile";

            var apiResponse = new ApiResponse<PersonalProfileResource>();

            _logger.LogDebug($"{loggerHeader} - Start to delete PersonalProfile with Id: {id}");
            using (var unitOfWork = new UnitOfWork(_connectionString))
            {
                try
                {
                    var personalProfile = await unitOfWork.PersonalProfileRepository.FindFirst(d => d.Id == id);
                    if (removeFromDB)
                    {
                        unitOfWork.PersonalProfileRepository.Remove(personalProfile);
                    }
                    else
                    {
                        personalProfile.ModifiedBy = _httpContextHelper.GetCurrentUser();
                        personalProfile.IsDeleted = true;
                        personalProfile.LastModified = DateTime.UtcNow;
                        unitOfWork.PersonalProfileRepository.Update(personalProfile);
                    }

                    await unitOfWork.SaveChanges();

                    _logger.LogDebug($"{loggerHeader} - Delete PersonalProfile successfully with Id: {personalProfile.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{loggerHeader} have error: {ex.Message}");
                    apiResponse.IsError = true;
                    apiResponse.Message = ex.Message;
                    await unitOfWork.SaveErrorLog(ex);
                }
                finally
                {
                    unitOfWork.Dispose();
                }
            }

            return apiResponse;
        }

        public async Task<ApiResponse<PersonalProfileResource>> GetPersonalProfile(long id)
        {
            const string loggerHeader = "GetPersonalProfile";

            var apiResponse = new ApiResponse<PersonalProfileResource>();

            _logger.LogDebug($"{loggerHeader} - Start to get PersonalProfile with Id: {id}");

            using (var unitOfWork = new UnitOfWork(_connectionString))
            {
                try
                {
                    var personalProfile = await unitOfWork.PersonalProfileRepository.FindFirst(predicate: d => d.Id == id);
                    apiResponse.Data = _mapper.Map<PersonalProfile, PersonalProfileResource>(personalProfile);
                    _logger.LogDebug($"{loggerHeader} - Get PersonalProfile successfully with Id: {apiResponse.Data.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{loggerHeader} have error: {ex.Message}");
                    apiResponse.IsError = true;
                    apiResponse.Message = ex.Message;
                    await unitOfWork.SaveErrorLog(ex);
                }
                finally
                {
                    unitOfWork.Dispose();
                }
            }

            return apiResponse;
        }

        public async Task<ApiResponse<QueryResultResource<PersonalProfileResource>>> GetPersonalProfiles(QueryResource queryObj)
        {
            const string loggerHeader = "GetPersonalProfiles";

            var apiResponse = new ApiResponse<QueryResultResource<PersonalProfileResource>>();
            var pagingSpecification = new PagingSpecification(queryObj);

            _logger.LogDebug($"{loggerHeader} - Start to get PersonalProfiles with");

            using (var unitOfWork = new UnitOfWork(_connectionString))
            {

                try
                {
                    var columnsMap = new Dictionary<string, Expression<Func<PersonalProfile, object>>>()
                    {
                        ["accountname"] = s => s.AccountName,
                        ["fullname"] = s => s.FullName,
                    };

                    var query = await unitOfWork.PersonalProfileRepository.FindAll(predicate: d => d.IsDeleted == false
                                                                                                && ((String.IsNullOrEmpty(queryObj.AccountName) || EF.Functions.Like(d.AccountName, $"%{queryObj.AccountName}%")))
                                                                                                && ((String.IsNullOrEmpty(queryObj.FullName) || EF.Functions.Like(d.FullName, $"%{queryObj.FullName}%")))
                                                                                                && (
                                                                                                        (
                                                                                                            (String.IsNullOrEmpty(queryObj.FullNameOrEmail) || EF.Functions.Like(d.FullName, $"%{queryObj.FullNameOrEmail}%"))
                                                                                                        )
                                                                                                        ||
                                                                                                        (
                                                                                                            (String.IsNullOrEmpty(queryObj.FullNameOrEmail) || EF.Functions.Like(d.Email, $"%{queryObj.FullNameOrEmail}%"))
                                                                                                        )
                                                                                                    )
                                                                                                ,
                                                                        include: null,
                                                                        orderBy: source => String.IsNullOrEmpty(queryObj.SortBy) ? source.OrderByDescending(d => d.Id)
                                                                                                                                 : queryObj.IsSortAscending ?
                                                                                                                                   source.OrderBy(columnsMap[queryObj.SortBy]) :
                                                                                                                                   source.OrderByDescending(columnsMap[queryObj.SortBy]),
                                                                        disableTracking: true,
                                                                        pagingSpecification: pagingSpecification);
                    apiResponse.Data = _mapper.Map<QueryResult<PersonalProfile>, QueryResultResource<PersonalProfileResource>>(query);
                    _logger.LogDebug($"{loggerHeader} - Get PersonalProfiles successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{loggerHeader} have error: {ex.Message}");
                    apiResponse.IsError = true;
                    apiResponse.Message = ex.Message;
                    await unitOfWork.SaveErrorLog(ex);
                }
                finally
                {
                    unitOfWork.Dispose();
                }
            }

            return apiResponse;
        }

        public async Task<string> GetAccountNameByContext()
        {
            const string loggerHeader = "Get Account Name";

            var apiResponse = new ApiResponse<string>();
            var currentEmail = _httpContextHelper.GetCurrentUser();
            if (!String.IsNullOrEmpty(currentEmail))
            {
                _logger.LogDebug($"{loggerHeader} - Start to get PersonalProfile with email: {currentEmail}");

                using (var unitOfWork = new UnitOfWork(_connectionString))
                {
                    try
                    {
                        var personalProfile = await unitOfWork.PersonalProfileRepository.FindFirst(d => d.Email == currentEmail);
                        if (personalProfile == null)
                        {
                            throw new InvalidOperationException("Lỗi: Không thể tìm thấy người dùng với email: " + currentEmail);
                        }
                        apiResponse.Data = personalProfile.FullName;
                        _logger.LogDebug($"{loggerHeader} - Get PersonalProfile successfully with Id: {apiResponse.Data}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{loggerHeader} have error: {ex.Message}");
                        apiResponse.IsError = true;
                        apiResponse.Message = ex.Message;
                        await unitOfWork.SaveErrorLog(ex);

                        return "";
                    }
                    finally
                    {
                        unitOfWork.Dispose();
                    }
                }

                return apiResponse.Data;
            }
            else
            {
                return "";
            }
        }
       }
}