namespace Invoice.Application.ServiceContracts;

public interface IBlackListService
{
    bool IsTokenBlackListed(string token);
    void AddTokenToBlackList(string token);
}
