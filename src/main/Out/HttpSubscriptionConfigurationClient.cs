using ei8.Cortex.Subscriptions.Common;
using neurUL.Common.Http;
using NLog;
using Polly;
using Splat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Client.Out
{
    public class HttpSubscriptionConfigurationClient : ISubscriptionConfigurationClient
    {
        private readonly IRequestProvider requestProvider;

        private static Policy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpSubscriptionConfigurationClient.logger.Error(ex, "Error occurred while communicating with Neurul Cortex. " + ex.InnerException?.Message)
            );

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string subscriptionsConfigurationPath = "nuclei/d23/subscriptions/config";

        public HttpSubscriptionConfigurationClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task<SubscriptionConfiguration> GetServerConfigurationAsync(string baseUrl, CancellationToken token = default)
        {
            var url = $"{baseUrl}{HttpSubscriptionConfigurationClient.subscriptionsConfigurationPath}";
            return await HttpSubscriptionConfigurationClient.exponentialRetryPolicy.ExecuteAsync(async () =>
            {
                return await this.requestProvider.GetAsync<SubscriptionConfiguration>(url, "", token);
            });
        }
    }
}
