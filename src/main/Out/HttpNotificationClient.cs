using ei8.Cortex.Diary.Common;
using neurUL.Common.Http;
using NLog;
using Polly;
using Polly.Retry;
using Splat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Client.Out
{
    public class HttpNotificationClient : INotificationClient
    {
        private readonly IRequestProvider requestProvider;

        private static AsyncRetryPolicy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpNotificationClient.logger.Error(ex, "Error occurred while querying Event Store. " + ex.InnerException?.Message)
            );

        private static string getEventsPathTemplate = "{0}nuclei/un8y/notifications/{1}";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpNotificationClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task<NotificationLog> GetNotificationLog(string avatarUrl, string notificationLogId, string bearerToken, CancellationToken token = default(CancellationToken)) =>
            await HttpNotificationClient.exponentialRetryPolicy.ExecuteAsync(
                async () => await this.GetNotificationLogInternal(avatarUrl, notificationLogId, bearerToken, token).ConfigureAwait(false));
        
        private async Task<NotificationLog> GetNotificationLogInternal(string avatarUrl, string notificationLogId, string bearerToken, CancellationToken token = default(CancellationToken))
        {
            return await requestProvider.GetAsync<NotificationLog>(
                           string.Format(HttpNotificationClient.getEventsPathTemplate, avatarUrl, notificationLogId),
                           bearerToken,
                           token
                           );
        }
    }
}
