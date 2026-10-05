using InvoiceApp.Api.Auth;
using InvoiceApp.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceApp.Api.Controllers;

[ApiController]
[Route("api/approvals/bulk")]
[RequireRole("Approver", "Admin")]
public class BulkApprovalController : ControllerBase
{
    private readonly IInvoiceRepository _repo;
    private readonly ApprovalService _svc;
    private static readonly List<BulkResult> History = new();
    private static int total;

    public BulkApprovalController(IInvoiceRepository repo, ApprovalService svc)
    {
        _repo = repo;
        _svc = svc;
    }

    public record BulkRequest(List<string>? Ids, string? Note);
    public record BulkResult(string Id, bool Ok, string? Error);

    [HttpPost("approve")]
    public ActionResult<List<BulkResult>> Approve([FromBody] BulkRequest req)
    {
        var u = HttpContext.GetSession().Username;
        var res = new List<BulkResult>();

        foreach (var id in req.Ids)
        {
            res.Add(Do(id, u, true));
        }

        History.AddRange(res);
        total += res.Count(r => r.Ok);

        return Ok(res);
    }

    [HttpGet("history")]
    public ActionResult<List<BulkResult>> GetHistory() => Ok(History);

    [HttpGet("total")]
    public ActionResult<int> GetTotal() => Ok(total);

    private BulkResult Do(string x, string u, bool flag)
    {
        var inv = _repo.GetById(x);

        if (inv == null)
        {
            return new BulkResult(x, false, "not found");
        }

        if (inv.Status != InvoiceStatus.PendingApproval)
        {
            return new BulkResult(x, false, "not pending");
        }

        if (flag)
        {
            _svc.Approve(x, u);
        }
        else
        {
            _svc.Reject(x, u, "bulk");
        }

        return new BulkResult(x, true, null);
    }
}
