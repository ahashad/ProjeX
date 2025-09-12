using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LastMinute.Consultancy.Application.Client;
using LastMinute.Consultancy.Application.Client.Commands;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetAllAsync();
            return View(clients);
        }

        // GET: Client/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _clientService.GetByIdAsync(id.Value);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClientCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _clientService.CreateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating client: {ex.Message}");
                }
            }
            return View(command);
        }

        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _clientService.GetByIdAsync(id.Value);
            if (client == null)
            {
                return NotFound();
            }

            var command = new UpdateClientCommand
            {
                Id = client.Id,
                ClientName = client.ClientName,
                ContactPerson = client.ContactPerson,
                Email = client.Email,
                Phone = client.Phone,
                Address = client.Address,
                Status = client.Status
            };

            return View(command);
        }

        // POST: Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateClientCommand command)
        {
            if (id != command.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _clientService.UpdateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating client: {ex.Message}");
                }
            }
            return View(command);
        }

        // GET: Client/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _clientService.GetByIdAsync(id.Value);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _clientService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting client: {ex.Message}");
                var client = await _clientService.GetByIdAsync(id);
                return View(client);
            }
        }
    }
}

