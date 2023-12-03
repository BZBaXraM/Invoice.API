using AutoMapper;
using Invoice.API.DTOs;
using Invoice.API.Models;

namespace Invoice.API.Mappings;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<UserDto, User>().ReverseMap();
        CreateMap<CustomerDto, Customer>().ReverseMap();
        CreateMap<InvoiceDto, Models.Invoice>().ReverseMap();
        CreateMap<AddCustomerRequestDto, Customer>().ReverseMap();
        CreateMap<UpdateCustomerRequestDto, Customer>().ReverseMap();
        CreateMap<AddInvoiceRequestDto, Models.Invoice>().ReverseMap();
    }
}