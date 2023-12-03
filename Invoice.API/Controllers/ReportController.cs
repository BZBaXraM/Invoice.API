using Invoice.API.Data;
using Invoice.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Invoice.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly IAsyncReportService _reportService;

    public ReportController(InvoiceContext context)
    {
        _reportService = new ReportService(context);
    }
}