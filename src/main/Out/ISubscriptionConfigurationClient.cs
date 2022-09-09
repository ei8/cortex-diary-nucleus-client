using ei8.Cortex.Subscriptions.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Client.Out
{
    public interface ISubscriptionConfigurationClient
    {
        Task<SubscriptionConfiguration> GetServerConfigurationAsync(string baseUrl, CancellationToken token = default);
    }
}
