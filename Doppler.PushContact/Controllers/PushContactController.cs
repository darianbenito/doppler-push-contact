using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Doppler.PushContact.Models;
using Doppler.PushContact.DopplerSecurity;
using System;
using Doppler.PushContact.Services;
using System.Linq;
using System.Collections.Generic;

namespace Doppler.PushContact.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    [Route("[controller]")]
    public class PushContactController : ControllerBase
    {
        private readonly IPushContactService _pushContactService;

        public PushContactController(IPushContactService pushContactService)
        {
            _pushContactService = pushContactService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] PushContactModel pushContactModel)
        {
            var added = await _pushContactService.AddAsync(pushContactModel);

            if (!added)
            {
                return StatusCode(500);
            }

            return Ok();
        }

        [HttpGet("{domain}")]
        public async Task<IActionResult> Get([FromRoute] string domain)
        {
            var pushContactFilter = new PushContactFilter(domain);

            var pushContacts = await _pushContactService.GetAsync(pushContactFilter);

            if (pushContacts == null || !pushContacts.Any())
            {
                return NotFound();
            }

            return Ok(pushContacts);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] IEnumerable<string> deviceTokens)
        {
            var deletedCount = await _pushContactService.DeleteByDeviceTokenAsync(deviceTokens);

            return Ok(deletedCount);
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] IEnumerable<PushContactHistoryEvent> pushContactHistoryEvents)
        {
            await _pushContactService.AddHistoryEventsAsync(pushContactHistoryEvents);

            return Ok();
        }
    }
}
