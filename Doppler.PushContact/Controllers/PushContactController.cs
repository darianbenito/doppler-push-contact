using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Doppler.PushContact.Models;
using Doppler.PushContact.DopplerSecurity;
using System;
using Doppler.PushContact.Services;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Doppler.PushContact.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    public class PushContactController : ControllerBase
    {
        private readonly IPushContactService _pushContactService;

        public PushContactController(IPushContactService pushContactService)
        {
            _pushContactService = pushContactService;
        }

        [HttpPost]
        [Route("push-contacts")]
        public async Task<IActionResult> Add([FromBody] PushContactModel pushContactModel)
        {
            await _pushContactService.AddAsync(pushContactModel);

            return Ok();
        }

        [HttpGet]
        [Route("push-contacts/{domain}")]
        public async Task<IActionResult> GetBy([FromRoute] string domain, [FromQuery] string email)
        {
            var pushContactFilter = new PushContactFilter(domain, email);

            var pushContacts = await _pushContactService.GetAsync(pushContactFilter);

            if (pushContacts == null || !pushContacts.Any())
            {
                return NotFound();
            }

            return Ok(pushContacts);
        }

        [HttpDelete]
        [Route("push-contacts/_bulk")]
        public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<string> deviceTokens)
        {
            var deletedCount = await _pushContactService.DeleteByDeviceTokenAsync(deviceTokens);

            return Ok(deletedCount);
        }

        [HttpPut]
        [Route("push-contacts/{deviceToken}/email")]
        public async Task<IActionResult> UpdateEmail([FromRoute] string deviceToken, [FromBody] string email)
        {
            await _pushContactService.UpdateEmailAsync(deviceToken, email);

            return Ok();
        }
    }
}
