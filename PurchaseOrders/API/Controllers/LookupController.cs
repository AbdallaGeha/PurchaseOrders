using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseOrders.Application.Dtos.Setup;
using PurchaseOrders.Application.Services.Setup;

namespace PurchaseOrders.API.Controllers
{
    [Route("api/lookup")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _service;

        public LookupController(ILookupService lookupService)
        {
            _service = lookupService;
        }

        /// <summary>
        /// Retrieves a list of projects key value pairs
        /// </summary>
        /// <response code="200">a list of suppliers</response>
        [HttpGet("projects")]
        [ProducesResponseType(typeof(List<KeyValueDto>), 200)]
        public async Task<ActionResult<List<KeyValueDto>>> GetProjects()
        {
            var projects = await _service.GetProjectsLookup();
            return Ok(projects);
        }

        /// <summary>
        /// Retrieves a list of suppliers key value pairs
        /// </summary>
        /// <response code="200">a list of suppliers</response>
        [HttpGet("suppliers")]
        [ProducesResponseType(typeof(List<KeyValueDto>), 200)]
        public async Task<ActionResult<List<KeyValueDto>>> GetSuppliers()
        {
            var suppliers = await _service.GetSuppliersLookup();
            return Ok(suppliers);
        }

        /// <summary>
        /// Retrieves a list of expenses items key value pairs
        /// </summary>
        /// <response code="200">a list of items </response>
        [HttpGet("items")]
        [ProducesResponseType(typeof(List<KeyValueDto>), 200)]
        public async Task<ActionResult<List<KeyValueDto>>> GetItems()
        {
            var items = await _service.GetItemsLookup();
            return Ok(items);
        }

        /// <summary>
        /// Retrieves a list of expenses Units key value pairs
        /// </summary>
        /// <response code="200">a list of Units </response>
        [HttpGet("Units/{id}")]
        [ProducesResponseType(typeof(List<KeyValueDto>), 200)]
        public async Task<ActionResult<List<KeyValueDto>>> GetUnits(Guid id)
        {
            var Units = await _service.GetUnitsLookup(id);
            return Ok(Units);
        }

        /// <summary>
        /// Retrieves a list of expenses currencies key value pairs
        /// </summary>
        /// <response code="200">a list of currencies </response>
        [HttpGet("currencies")]
        [ProducesResponseType(typeof(List<KeyValueDto>), 200)]
        public async Task<ActionResult<List<KeyValueDto>>> GetCurrencies()
        {
            var currencies = await _service.GetCurrenciesLookup();
            return Ok(currencies);
        }


    }
}
