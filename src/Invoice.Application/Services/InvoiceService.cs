namespace Invoice.Application.Services;

public class InvoiceService(
    IUnitOfWork uow,
    CreateInvoiceRequestValidator createValidator,
    UpdateInvoiceRequestValidator updateValidator,
    IRealtimeNotifier realtimeNotifier) : IInvoiceService
{
    public async Task<ResponseModel<InvoiceResponse>> CreateAsync(Guid ownerUserId, CreateInvoiceRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<InvoiceResponse>("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var customer = await uow.CustomerRepository.GetByIdAsync(request.CustomerId, ownerUserId);
        if (customer is null)
        {
            return ResponseModel.Failure<InvoiceResponse>("Customer not found", 404);
        }

        var invoice = new Domain.Entities.Invoice
        {
            UserId = ownerUserId,
            CustomerId = request.CustomerId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Comment = request.Comment,
            Status = InvoiceStatus.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Rows = request.Rows.Select(r => new InvoiceRow
            {
                Service = r.Service,
                Quantity = r.Quantity,
                Rate = r.Rate,
                Sum = r.Quantity * r.Rate
            }).ToList()
        };

        invoice.TotalSum = invoice.Rows.Sum(r => r.Sum);

        uow.InvoiceRepository.AddInvoice(invoice);
        await uow.CommitAsync();

        var response = invoice.ToInvoiceResponse();
        await realtimeNotifier.InvoiceCreatedAsync(ownerUserId, response);

        return ResponseModel.Success(response);
    }

    public async Task<ResponseModel<InvoiceResponse>> UpdateAsync(Guid ownerUserId, Guid id, UpdateInvoiceRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ResponseModel.Failure<InvoiceResponse>("Validation failed", 400,
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var invoice = await uow.InvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (invoice is not { Status: InvoiceStatus.Created })
        {
            return ResponseModel.Failure<InvoiceResponse>("Invoice not found or cannot be updated", 409);
        }

        var customer = await uow.CustomerRepository.GetByIdAsync(request.CustomerId, ownerUserId);
        if (customer is null)
        {
            return ResponseModel.Failure<InvoiceResponse>("Customer not found", 404);
        }

        invoice.CustomerId = request.CustomerId;
        invoice.StartDate = request.StartDate;
        invoice.EndDate = request.EndDate;
        invoice.Comment = request.Comment;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;

        invoice.Rows.Clear();
        foreach (var row in request.Rows)
        {
            invoice.Rows.Add(new InvoiceRow
            {
                Service = row.Service,
                Quantity = row.Quantity,
                Rate = row.Rate,
                Sum = row.Quantity * row.Rate
            });
        }

        invoice.TotalSum = invoice.Rows.Sum(r => r.Sum);

        await uow.CommitAsync();

        var response = invoice.ToInvoiceResponse();
        await realtimeNotifier.InvoiceUpdatedAsync(ownerUserId, response);

        return ResponseModel.Success(response);
    }

    public async Task<ResponseModel> UpdateStatusAsync(Guid ownerUserId, Guid id, InvoiceStatus status)
    {
        var invoice = await uow.InvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (invoice is null)
        {
            return ResponseModel.Failure("Invoice not found", 404);
        }

        invoice.Status = status;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;
        await uow.CommitAsync();

        await realtimeNotifier.InvoiceStatusChangedAsync(ownerUserId, id, status);

        return ResponseModel.Success("Invoice status updated successfully");
    }

    public async Task<ResponseModel<InvoiceResponse>> GetByIdAsync(Guid ownerUserId, Guid id)
    {
        var invoice = await uow.InvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (invoice is null)
        {
            return ResponseModel.Failure<InvoiceResponse>("Invoice not found", 404);
        }

        return ResponseModel.Success(invoice.ToInvoiceResponse());
    }

    public async Task<ResponseModel<PagedResult<InvoiceResponse>>> GetListAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        Guid? customerId,
        InvoiceStatus? status,
        string? sortBy,
        bool sortDescending)
    {
        var (items, totalCount) = await uow.InvoiceRepository.GetPagedAsync(ownerUserId, pageNumber, pageSize,
            customerId, status, sortBy, sortDescending);

        return ResponseModel.Success(new PagedResult<InvoiceResponse>
        {
            Items = items.Select(i => i.ToInvoiceResponse()).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public async Task<ResponseModel> DeleteAsync(Guid ownerUserId, Guid id)
    {
        var invoice = await uow.InvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (invoice is not { Status: InvoiceStatus.Created })
        {
            return ResponseModel.Failure("Invoice not found or cannot be deleted", 409);
        }

        uow.InvoiceRepository.RemoveInvoice(invoice);
        await uow.CommitAsync();

        await realtimeNotifier.InvoiceDeletedAsync(ownerUserId, id);

        return ResponseModel.Success("Invoice deleted successfully");
    }

    public async Task<ResponseModel> ArchiveAsync(Guid ownerUserId, Guid id)
    {
        var invoice = await uow.InvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (invoice is null)
        {
            return ResponseModel.Failure("Invoice not found", 404);
        }

        invoice.DeletedAt = DateTimeOffset.UtcNow;
        await uow.CommitAsync();

        await realtimeNotifier.InvoiceArchivedAsync(ownerUserId, id);

        return ResponseModel.Success("Invoice archived successfully");
    }

    public async Task<ResponseModel<byte[]>> ExportToPdfAsync(Guid ownerUserId, Guid id)
    {
        var invoice = await uow.InvoiceRepository.GetByIdWithRowsAsync(id, ownerUserId);
        if (invoice is null)
        {
            return ResponseModel.Failure<byte[]>("Invoice not found", 404);
        }

        var customer = await uow.CustomerRepository.GetByIdAsync(invoice.CustomerId, ownerUserId);
        var biller = await uow.UserRepository.GetByIdAsync(ownerUserId);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => ComposeHeader(header, invoice, biller));
                page.Content().Element(content => ComposeContent(content, invoice, customer));

                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium));
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return ResponseModel.Success(document.GeneratePdf());
    }

    private static void ComposeHeader(IContainer container, Domain.Entities.Invoice invoice, User? biller)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(biller is not null ? $"{biller.FirstName} {biller.LastName}" : "Company").FontSize(16).SemiBold();
                if (!string.IsNullOrWhiteSpace(biller?.Address))
                {
                    column.Item().Text(biller.Address);
                }

                if (biller is not null)
                {
                    column.Item().Text(biller.Email);
                    if (!string.IsNullOrWhiteSpace(biller.PhoneNumber))
                    {
                        column.Item().Text(biller.PhoneNumber);
                    }
                }
            });

            row.ConstantItem(180).Column(column =>
            {
                column.Item().AlignRight().Text("INVOICE").FontSize(22).SemiBold().FontColor(Colors.Blue.Medium);
                column.Item().AlignRight().Text($"# {invoice.Id.ToString()[..8].ToUpperInvariant()}");
                column.Item().AlignRight().Text($"Issued: {invoice.CreatedAt:yyyy-MM-dd}");
                column.Item().AlignRight().Text($"Period: {invoice.StartDate:yyyy-MM-dd} – {invoice.EndDate:yyyy-MM-dd}");
                column.Item().AlignRight().Text($"Status: {invoice.Status}").SemiBold();
            });
        });
    }

    private static void ComposeContent(IContainer container, Domain.Entities.Invoice invoice, Customer? customer)
    {
        container.PaddingTop(20).Column(column =>
        {
            column.Spacing(12);

            column.Item().Element(c => ComposeBillTo(c, customer));
            column.Item().Element(c => ComposeTable(c, invoice));

            column.Item().AlignRight().Width(220).Row(row =>
            {
                row.RelativeItem().Text("Total").SemiBold();
                row.ConstantItem(100).AlignRight().Text($"{invoice.TotalSum:N2} AZN").SemiBold().FontSize(12);
            });

            if (!string.IsNullOrWhiteSpace(invoice.Comment))
            {
                column.Item().PaddingTop(10).Text("Notes").SemiBold().FontColor(Colors.Grey.Darken2);
                column.Item().Text(invoice.Comment);
            }
        });
    }

    private static void ComposeBillTo(IContainer container, Customer? customer)
    {
        container.Column(column =>
        {
            column.Item().Text("Bill To").SemiBold().FontColor(Colors.Grey.Darken2);
            column.Item().Text(customer?.Name ?? "Unknown customer").FontSize(12).SemiBold();
            if (!string.IsNullOrWhiteSpace(customer?.Address))
            {
                column.Item().Text(customer.Address);
            }

            if (customer is not null)
            {
                column.Item().Text(customer.Email);
                if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
                {
                    column.Item().Text(customer.PhoneNumber);
                }
            }
        });
    }

    private static void ComposeTable(IContainer container, Domain.Entities.Invoice invoice)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);
                columns.RelativeColumn(3);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("#");
                header.Cell().Element(HeaderCellStyle).Text("Service");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Quantity");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Rate");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Amount");

                static IContainer HeaderCellStyle(IContainer c) => c
                    .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                    .Background(Colors.Blue.Medium)
                    .PaddingVertical(6)
                    .PaddingHorizontal(5);
            });

            var index = 1;
            foreach (var row in invoice.Rows)
            {
                var alternate = index % 2 == 0;

                table.Cell().Element(c => BodyCellStyle(c, alternate)).Text(index.ToString());
                table.Cell().Element(c => BodyCellStyle(c, alternate)).Text(row.Service);
                table.Cell().Element(c => BodyCellStyle(c, alternate)).AlignRight().Text(row.Quantity.ToString("N2"));
                table.Cell().Element(c => BodyCellStyle(c, alternate)).AlignRight().Text($"{row.Rate:N2} AZN");
                table.Cell().Element(c => BodyCellStyle(c, alternate)).AlignRight().Text($"{row.Sum:N2} AZN");

                index++;
            }

            static IContainer BodyCellStyle(IContainer c, bool alternate) => c
                .Background(alternate ? Colors.Grey.Lighten4 : Colors.White)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5)
                .PaddingHorizontal(5);
        });
    }
}
