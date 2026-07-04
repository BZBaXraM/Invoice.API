namespace Invoice.Application.ServiceContracts;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
