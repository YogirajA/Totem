using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Totem.Infrastructure;

namespace Totem.Features.Contracts
{
    public class ContractsController : Controller
    {
        private readonly TotemContext _context;
        private readonly IMediator _mediator;

        public ContractsController(TotemContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(Index.Query query) => View(await _mediator.Send(query));

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(Details.Query query) => View(await _mediator.Send(query));

        public async Task<FileStreamResult> Download(Download.Query query) => await _mediator.Send(query);

        // GET: Contracts/Create
        [Authorize]
        public IActionResult Create() => View();

        // POST: Contracts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Create.Command command)
        {
            if (!ModelState.IsValid) return View(command);

            await _mediator.Send(command);

            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Edit.Query query) => View(await _mediator.Send(query));

        // POST: Contracts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(Edit.Command command)
        {
            if (!ModelState.IsValid) return View(command);

            await _mediator.Send(command);

            return RedirectToAction(nameof(Index));
        }


        //Get Contracts/1/TestMessage
        public async Task<IActionResult> TestMessage(TestMessage.Query query) => View(await _mediator.Send(query));

        // POST: Used by GUI to test a single message and return the view
        [HttpPost]
        public async Task<IActionResult> TestMessage(TestMessage.Command command) => View(await _mediator.Send(command));

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var contract = await _context.Contract.FindAsync(id);
            _context.Contract.Remove(contract);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: SchemaObjects
        public async Task<JsonResult> SchemaObjects()
        {
            var schemaObjects = await _context.ContractSchema.OrderBy(x => x.SchemaName).ToListAsync();
            return Json(schemaObjects);
        }
    }
}
