using DidActivation.Application.Dtos;
using DidActivation.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DidActivation.Api.Controllers
{
    [ApiController]
    [Route("activations")]
    public sealed class ActivationsController : ControllerBase
    {
        private readonly IActivationService _activationService;

        public ActivationsController(IActivationService activationService)
        {
            _activationService = activationService;
        }

        [HttpPost]
        public async Task<ActionResult<ActivationCreatedResponseDto>> CreateAsync(
            [FromBody] RequestActivationDto request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Did) || string.IsNullOrWhiteSpace(request.CustomerId))
                return BadRequest(new { error = "INVALID_REQUEST", message = "DID e customerId são obrigatórios." });

            var response = await _activationService.RequestActivationAsync(request, cancellationToken);
            return Accepted(response);
        }

        [HttpGet("{activationId:guid}")]
        public async Task<ActionResult<ActivationDetailResponseDto>> GetAsync(
            Guid activationId,
            CancellationToken cancellationToken)
        {
            var response = await _activationService.GetActivationAsync(activationId, cancellationToken);
            if (response is null)
                return NotFound(new { error = "NOT_FOUND", message = "Activation not found." });

            return Ok(response);
        }
    }
}
