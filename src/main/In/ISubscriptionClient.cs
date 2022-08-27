using ei8.Cortex.Subscriptions.Common;
using ei8.Cortex.Subscriptions.Common.Receivers;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Client.In
{
    public interface ISubscriptionClient<T> where T : IReceiverInfo
    {
        Task AddSubscriptionAsync(string baseUrl, IAddSubscriptionReceiverRequest<T> receiverInfo, string bearerToken, CancellationToken cancellationToken = default);
    }
}
