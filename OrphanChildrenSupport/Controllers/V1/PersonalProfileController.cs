using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OrphanChildrenSupport.DataContracts;
using OrphanChildrenSupport.DataContracts.Resources;
using OrphanChildrenSupport.Services.Contracts;

namespace OrphanChildrenSupport.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/personalProfiles")]//required for default versioning
    [Route("api/v{version:apiVersion}/personalProfiles")]
    [ApiController]
    public class PersonalProfileController : Controller
    {
        private IPersonalProfileService _personalProfileService;

        public PersonalProfileController(IPersonalProfileService personalProfileService)
        {
            this._personalProfileService = personalProfileService;
        }

        // GET: api/personalProfiles
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] QueryResource queryResource)
        {
            var apiResponse = await _personalProfileService.GetPersonalProfiles(queryResource);
            return apiResponse.IsError ? BadRequest(apiResponse.Message) : Ok(apiResponse.Data);
        }

        // GET api/personalProfiles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var apiResponse = await _personalProfileService.GetPersonalProfile(id);
            return apiResponse.IsError ? BadRequest(apiResponse.Message) : Ok(apiResponse.Data);
        }

        // POST api/personalProfiles
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PersonalProfileResource PersonalProfileResource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Lỗi dữ liệu, vui lòng kiểm tra và thử lại.");
            }

            var apiResponse = await _personalProfileService.CreatePersonalProfile(PersonalProfileResource);
            return apiResponse.IsError ? BadRequest(apiResponse.Message) : Ok(apiResponse.Data);
        }

        // PUT api/personalProfiles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] PersonalProfileResource PersonalProfileResource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Lỗi dữ liệu, vui lòng kiểm tra và thử lại.");
            }

            var apiResponse = await _personalProfileService.UpdatePersonalProfile(id, PersonalProfileResource);
            return apiResponse.IsError ? BadRequest(apiResponse.Message) : Ok(apiResponse.Data);
        }

        // DELETE api/personalProfiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var apiResponse = await _personalProfileService.DeletePersonalProfile(id);
            return apiResponse.IsError ? BadRequest(apiResponse.Message) : Ok(apiResponse.Data);
        }
    }
}
