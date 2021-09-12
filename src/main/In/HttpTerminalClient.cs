using NLog;
using neurUL.Common.Http;
using neurUL.Cortex.Common;
using Polly;
using Splat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ei8.Cortex.Diary.Nucleus.Client.In;

namespace ei8.Cortex.Diary.Nucleus.Client.In
{
    public class HttpTerminalClient : ITerminalClient
    {
        private readonly IRequestProvider requestProvider;

        private static Policy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpTerminalClient.logger.Error(ex, "Error occurred while communicating with Neurul Cortex. " + ex.InnerException?.Message)
            );

        private static readonly string terminalsPath = "nuclei/d23/terminals/";
        private static readonly string terminalsPathTemplate = terminalsPath + "{0}";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpTerminalClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task CreateTerminal(string avatarUrl, string id, string presynapticNeuronId, string postsynapticNeuronId, NeurotransmitterEffect effect, float strength, string externalReferenceUrl, string bearerToken, CancellationToken token = default(CancellationToken)) =>
            await HttpTerminalClient.exponentialRetryPolicy.ExecuteAsync(
                async () => await this.CreateTerminalInternal(avatarUrl, id, presynapticNeuronId, postsynapticNeuronId, effect, strength, externalReferenceUrl, bearerToken, token).ConfigureAwait(false));

        private async Task CreateTerminalInternal(string avatarUrl, string id, string presynapticNeuronId, string postsynapticNeuronId, NeurotransmitterEffect effect, float strength, string externalReferenceUrl, string bearerToken, CancellationToken token = default(CancellationToken))
        {
            var data = new
            {
                Id = id,
                PresynapticNeuronId = presynapticNeuronId,
                PostsynapticNeuronId = postsynapticNeuronId,
                Effect = effect.ToString(),
                Strength = strength.ToString(),
                ExternalReferenceUrl = externalReferenceUrl
            };

            await this.requestProvider.PostAsync(
               $"{avatarUrl}{HttpTerminalClient.terminalsPath}",
               data,
               bearerToken
               );
        }
        
        public async Task DeactivateTerminal(string avatarUrl, string id, int expectedVersion, string bearerToken, CancellationToken token = default(CancellationToken)) =>
            await HttpTerminalClient.exponentialRetryPolicy.ExecuteAsync(
                    async () => await this.DeactivateTerminalInternal(avatarUrl, id, expectedVersion, bearerToken, token).ConfigureAwait(false));

        private async Task DeactivateTerminalInternal(string avatarUrl, string id, int expectedVersion, string bearerToken, CancellationToken token = default(CancellationToken))
        {
            await this.requestProvider.DeleteAsync<object>(
               $"{avatarUrl}{string.Format(HttpTerminalClient.terminalsPathTemplate, id)}",
               null,
               bearerToken,
               token,
               new KeyValuePair<string, string>("ETag", expectedVersion.ToString())
               );
        }
    }
}
