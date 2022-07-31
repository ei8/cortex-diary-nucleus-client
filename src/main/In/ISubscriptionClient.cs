using ei8.Cortex.Subscriptions.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Client.In
{
    public interface ISubscriptionClient
    {
        Task AddSubscriptionAsync(string baseUrl, BrowserSubscriptionInfo subscriptionInfo, string bearerToken, CancellationToken cancellationToken = default);
    }
}
