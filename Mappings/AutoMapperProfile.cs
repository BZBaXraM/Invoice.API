using AutoMapper;
using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;

namespace InvoiceManager.API.Mappings;

/// <summary>
/// AutoMapper profile
/// </summary>
public class AutoMapperProfile : Profile
{
    /// <inheritdoc />
    public AutoMapperProfile()
    {
        CreateMap<InvoiceDto, Invoice>().ReverseMap();
        CreateMap<CreateInvoiceDto, Invoice>().ReverseMap();
        CreateMap<UpdateInvoiceDto, Invoice>().ReverseMap();
        CreateMap<InvoiceRowDto, InvoiceRow>().ReverseMap();
        CreateMap<CreateInvoiceRowDto, InvoiceRow>().ReverseMap();
        CreateMap<UpdateInvoiceRowDto, InvoiceRow>().ReverseMap();
        CreateMap<CustomerDto, Customer>().ReverseMap();
        CreateMap<CreateCustomerDto, Customer>().ReverseMap();
        CreateMap<UpdateCustomerDto, Customer>().ReverseMap();
    }
}