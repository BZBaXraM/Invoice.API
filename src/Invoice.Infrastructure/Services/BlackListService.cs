using System.Collections.Concurrent;
using Invoice.Application.ServiceContracts;

namespace Invoice.Infrastructure.Services;

public class BlackListService : IBlackListService
{
    private ConcurrentBag<string> BlackList { get; } = [];

    public bool IsTokenBlackListed(string token) =>
        !string.IsNullOrWhiteSpace(token) && BlackList.Contains(token);

    public void AddTokenToBlackList(string token)
    {
        if (!string.IsNullOrWhiteSpace(token) && !BlackList.Contains(token))
        {
            BlackList.Add(token);
        }
    }
}
