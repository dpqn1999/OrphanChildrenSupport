using System.Threading.Tasks;
using OrphanChildrenSupport.DataContracts;
using OrphanChildrenSupport.DataContracts.Resources;

namespace OrphanChildrenSupport.Services.Contracts
{
    public interface IPersonalProfileService
    {
        Task<ApiResponse<PersonalProfileResource>> CreatePersonalProfile(PersonalProfileResource personalProfileResource);
        Task<ApiResponse<PersonalProfileResource>> UpdatePersonalProfile(long id, PersonalProfileResource personalProfileResource);
        Task<ApiResponse<PersonalProfileResource>> DeletePersonalProfile(long id, bool removeFromDB = false);
        Task<ApiResponse<PersonalProfileResource>> GetPersonalProfile(long id);
        Task<ApiResponse<QueryResultResource<PersonalProfileResource>>> GetPersonalProfiles(QueryResource queryObj);
    }
}
