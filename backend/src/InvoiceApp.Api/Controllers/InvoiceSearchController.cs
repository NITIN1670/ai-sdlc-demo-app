using InvoiceApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceApp.Api.Controllers;

[ApiController]
[Route("api/invoice-search")]
public class InvoiceSearchController : ControllerBase
{
    private readonly InvoiceSearchService _search;

    public InvoiceSearchController(InvoiceSearchService search)
    {
        _search = search;
    }

    [HttpGet]
    public ActionResult<List<InvoiceSummary>> Search([FromQuery] string q) => Ok(_search.Search(q));

    [HttpGet("{id}/export-link")]
    public IActionResult ExportLink(string id) =>
        Ok(new { url = $"/exports/{id}?sig={_search.CreateExportSignature(id)}" });

    [HttpDelete("customer/{customerName}")]
    public IActionResult DeleteCustomer(string customerName) =>
        Ok(new { deleted = _search.DeleteByCustomer(customerName) });
}
