using InvoiceManager.API.CustomFilters;
using InvoiceManager.API.DTOs;
using InvoiceManager.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManager.API.Controllers;

/// <summary>
/// Controller for customer management
/// </summary>
/// <param name="service"></param>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CustomerController(IAsyncCustomerService service) : ControllerBase
{
    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateModel]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        var customer = await service.CreateCustomerAsync(dto);

        return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
    }

    /// <summary>
    /// Update a customer
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
    {
        var customer = await service.UpdateCustomerAsync(id, dto);
        return Ok(customer);
    }

    /// <summary>
    /// Get a customer by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
    {
        var customer = await service.GetCustomerByIdAsync(id);
        return Ok(customer);
    }

    /// <summary>
    /// Get a list of customers
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetCustomers([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var customers = await service.GetCustomersAsync(pageNumber, pageSize);
        return Ok(customers);
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        await service.DeleteCustomerAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Archive a customer
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/archive")]
    public async Task<IActionResult> ArchiveCustomer(int id)
    {
        await service.ArchiveCustomerAsync(id);
        return NoContent();
    }
}