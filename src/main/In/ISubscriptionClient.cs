using ei8.Cortex.Subscriptions.Common;
using ei8.Cortex.Subscriptions.Common.Receivers;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Client.In
{
    public interface ISubscriptionClient
    {
        Task AddSubscriptionAsync(string baseUrl, AddSubscriptionWebReceiverRequest receiverRequest, string bearerToken, CancellationToken cancellationToken = default);
    }
}
