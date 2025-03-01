﻿using ei8.Cortex.Subscriptions.Common;
using ei8.Cortex.Subscriptions.Common.Extensions;
using ei8.Cortex.Subscriptions.Common.Receivers;
using neurUL.Common.Http;
using NLog;
using Polly;
using Polly.Retry;
using Splat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Client.In
{
    public class HttpSubscriptionClient : ISubscriptionClient
    {
        private readonly IRequestProvider requestProvider;

        private static AsyncRetryPolicy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpSubscriptionClient.logger.Error(ex, "Error occurred while communicating with Neurul Cortex. " + ex.InnerException?.Message)
            );

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string subscriptionsPath = "nuclei/d23/subscriptions/receivers";

        public HttpSubscriptionClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task AddSubscriptionAsync<T>(string baseUrl, IAddSubscriptionReceiverRequest<T> request, string bearerToken, CancellationToken cancellationToken = default) where T : IReceiverInfo
        {
            var requestUrl = $"{baseUrl}{HttpSubscriptionClient.subscriptionsPath}/{request.ReceiverInfo.GetSubscriptionPath()}";

            await HttpSubscriptionClient.exponentialRetryPolicy.ExecuteAsync(async () =>
            {
                await this.requestProvider.PostAsync(requestUrl, request, token: bearerToken);
            });
        }
    }
}
