using neurUL.Common.Http;
using NLog;
using Polly;
using Splat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Client.In
{
    public class HttpAccessClient : IAccessClient
    {
        private readonly IRequestProvider requestProvider;

        private static Policy ExponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpAccessClient.Logger.Error(ex, "Error occurred while communicating with Neurul Cortex. " + ex.InnerException?.Message)
            );

        private static readonly string AccessModulePath = "nuclei/d23/access/";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HttpAccessClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task CreateNeuronAccessRequest(string avatarUrl, string neuronId, int expectedVersion, string bearerToken, CancellationToken token = default)
        {
            await HttpAccessClient.ExponentialRetryPolicy.ExecuteAsync(
                   async () => await this.CreateNeuronAccessRequestInternal(avatarUrl, neuronId, expectedVersion, bearerToken, token)).ConfigureAwait(false);
        }

        private async Task CreateNeuronAccessRequestInternal(string avatarUrl, string neuronId, int expectedVersion, string bearerToken, CancellationToken token = default(CancellationToken))
        {
            await this.requestProvider.PostAsync<object>(
               $"{avatarUrl}/{HttpAccessClient.AccessModulePath}/neuron/{neuronId}",
               null,
               bearerToken,
               token,
               new KeyValuePair<string, string>("ETag", expectedVersion.ToString())
               );
        }
    }
}
