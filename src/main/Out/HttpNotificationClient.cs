using Newtonsoft.Json;
using NLog;
using org.neurul.Common;
using org.neurul.Common.Constants;
using org.neurul.Common.Domain.Model;
using org.neurul.Common.Http;
using Polly;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using works.ei8.Cortex.Diary.Common;

namespace works.ei8.Cortex.Diary.Nucleus.Client.Out
{
    public class HttpNotificationClient : INotificationClient
    {
        private readonly IRequestProvider requestProvider;
        private readonly ITokenService tokenService;

        private static Policy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpNotificationClient.logger.Error(ex, "Error occurred while querying Event Store. " + ex.InnerException?.Message)
            );

        private static string getEventsPathTemplate = "{0}nuclei/d23/notifications/{1}";
        private static readonly Dictionary<string, HttpClient> clients = new Dictionary<string, HttpClient>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpNotificationClient(IRequestProvider requestProvider = null, ITokenService tokenService = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
            this.tokenService = tokenService ?? Locator.Current.GetService<ITokenService>();
        }

        public async Task<NotificationLog> GetNotificationLog(string avatarUrl, string notificationLogId, CancellationToken token = default(CancellationToken)) =>
            await HttpNotificationClient.exponentialRetryPolicy.ExecuteAsync(
                async () => await this.GetNotificationLogInternal(avatarUrl, notificationLogId, token).ConfigureAwait(false));
        
        public async Task<NotificationLog> GetNotificationLogInternal(string avatarUrl, string notificationLogId, CancellationToken token = default(CancellationToken))
        {
            return await requestProvider.GetAsync<NotificationLog>(
                           string.Format(HttpNotificationClient.getEventsPathTemplate, avatarUrl, notificationLogId),
                           tokenService.GetAccessToken(),
                           token
                           );
        }
    }
}
