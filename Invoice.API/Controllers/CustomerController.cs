using AutoMapper;
using Invoice.API.Data;
using Invoice.API.Models;
using Invoice.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Invoice.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController(InvoiceContext context, IMapperBase mapper) : ControllerBase
{
    private readonly IAsyncCustomerService _customerService = new CustomerService(context);

    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        var customers = await _customerService.GetCustomersAsync(filterOn, filterQuery, sortBy,
            isAscending ?? true,
            pageNumber,
            pageSize);
        
        return Ok(mapper.Map<List<Customer>>(customers));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomerAsync(Guid id)
    {
        var customer = await _customerService.GetCustomerAsync(id);
        return Ok(mapper.Map<Customer>(customer));
    }
    
    [HttpPost]
    public async Task<IActionResult> AddCustomerAsync([FromBody] Customer customer)
    {
        var newCustomer = await _customerService.AddCustomerAsync(mapper.Map<Customer>(customer));
        return CreatedAtAction(nameof(GetCustomers), new {id = newCustomer.Id}, mapper.Map<Customer>(newCustomer));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomerAsync(Guid id, [FromBody] Customer customer)
    {
        if (id != customer.Id)
        {
            return BadRequest();
        }

        var updatedCustomer = await _customerService.UpdateCustomerAsync(mapper.Map<Invoice.API.Models.Customer>(customer));
        return Ok(mapper.Map<Customer>(updatedCustomer));
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomerAsync(Guid id)
    {
        var customer = await _customerService.DeleteCustomerAsync(id);
        return Ok(mapper.Map<Customer>(customer));
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> SoftDeleteCustomerAsync(Guid id)
    {
        var customer = await _customerService.SoftDeleteCustomerAsync(id);
        return Ok(mapper.Map<Customer>(customer));
    }
}