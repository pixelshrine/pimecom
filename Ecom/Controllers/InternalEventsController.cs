using Ecom.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging;

namespace Ecom.Application.Controllers;

[ApiController]
[Route("internal/events")]
public class InternalEventsController : ControllerBase
{
    private readonly EventRouter _router;

    public InternalEventsController(EventRouter router)
    {
        _router = router;
    }

    [HttpPost]
    public async Task<IActionResult> Route(EventEnvelope envelope)
    {
        await _router.Route(envelope);

        return Accepted();
    }
}
