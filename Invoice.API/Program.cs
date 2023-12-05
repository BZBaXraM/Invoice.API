using FluentValidation.AspNetCore;
using Invoice.API.Data;
using Invoice.API.Mappings;
using Invoice.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<InvoiceContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IAsyncCustomerService, CustomerService>();
builder.Services.AddScoped<IAsyncInvoiceService, InvoiceService>();
builder.Services.AddScoped<IAsyncUserService, UserService>();
// builder.Services.AddScoped<IAsyncReportService, IAsyncReportService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
builder.Services.AddFluentValidationAutoValidation();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await app.RunAsync();