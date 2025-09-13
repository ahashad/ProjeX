using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LastMinute.Consultancy.Application.Client;
using LastMinute.Consultancy.Application.Client.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ProjeX.Blazor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IClientService clientService, ILogger<ClientsController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
        {
            try
            {
                var clients = await _clientService.GetAllAsync();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients");
                return Problem("An error occurred while retrieving clients.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ClientDto>> Get(Guid id)
        {
            try
            {
                var client = await _clientService.GetByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client {ClientId}", id);
                return Problem("An error occurred while retrieving the client.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientCommand command)
        {
            try
            {
                var created = await _clientService.CreateAsync(command);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return Problem("An error occurred while creating the client.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            try
            {
                await _clientService.UpdateAsync(command);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {ClientId}", id);
                return Problem("An error occurred while updating the client.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _clientService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {ClientId}", id);
                return Problem("An error occurred while deleting the client.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
