using AutoMapper;
using Invoice.API.Data;
using Invoice.API.DTOs;
using Invoice.API.Models;
using Invoice.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Invoice.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly IAsyncCustomerService _customerService;
    private readonly IMapper _mapper;

    public CustomerController(InvoiceContext context, IMapper mapper)
    {
        _customerService = new CustomerService(context);
        _mapper = mapper;
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        // Call the CustomerService to get the list of customers
        var customers = await _customerService.GetCustomersAsync(filterOn, filterQuery, sortBy,
            isAscending ?? true,
            pageNumber,
            pageSize);

        // Map and return the result
        return Ok(_mapper.Map<List<CustomerDto>>(customers));
    }


    [HttpGet("customers/{id:guid}")]
    public async Task<IActionResult> GetCustomerAsync(Guid id)
    {
        var customer = await _customerService.GetCustomerAsync(id);
        return Ok(_mapper.Map<CustomerDto>(customer));
    }


    [HttpPost("customers")]
    public async Task<IActionResult> AddCustomerAsync([FromBody] AddCustomerRequestDto requestDto)
    {
        var customer = _mapper.Map<Customer>(requestDto);
        var newCustomer = await _customerService.AddCustomerAsync(customer);

        return CreatedAtAction(nameof(GetCustomers), new { id = newCustomer.Id },
            _mapper.Map<CustomerDto>(newCustomer));
    }


    [HttpPut("customers/{id:guid}")]
    public async Task<IActionResult> UpdateCustomerAsync(Guid id, [FromBody] UpdateCustomerRequestDto requestDto)
    {
        if (id != requestDto.Id)
        {
            return BadRequest("Invalid customer id");
        }

        var customer = _mapper.Map<Customer>(requestDto);
        var updatedCustomer = await _customerService.UpdateCustomerAsync(customer);

        return Ok(_mapper.Map<CustomerDto>(updatedCustomer));
    }


    [HttpDelete("customers/{id:guid}")]
    public async Task<IActionResult> DeleteCustomerAsync(Guid id)
    {
        var deletedCustomer = await _customerService.DeleteCustomerAsync(id);
        return Ok(_mapper.Map<CustomerDto>(deletedCustomer));
    }


    [HttpPatch("customers/{id:guid}/archive")]
    public async Task<IActionResult> SoftDeleteCustomerAsync(Guid id)
    {
        var archivedCustomer = await _customerService.SoftDeleteCustomerAsync(id);
        return Ok(_mapper.Map<CustomerDto>(archivedCustomer));
    }
}