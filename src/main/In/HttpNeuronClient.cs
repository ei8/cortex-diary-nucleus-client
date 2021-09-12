/*
    This file is part of the d# project.
    Copyright (c) 2016-2018 ei8
    Authors: ei8
     This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    EI8. EI8 DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS
     This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    https://github.com/ei8/cortex-diary/blob/master/LICENSE
     The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
     You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the d# software without
    disclosing the source code of your own applications.
     For more information, please contact ei8 at this address: 
     support@ei8.works
 */

using NLog;
using neurUL.Common.Http;
using Polly;
using Splat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ei8.Cortex.Diary.Nucleus.Client.In;

namespace ei8.Cortex.Diary.Nucleus.Client.In
{
    public class HttpNeuronClient : INeuronClient
    {
        private readonly IRequestProvider requestProvider;

        private static Policy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpNeuronClient.logger.Error(ex, "Error occurred while communicating with Neurul Cortex. " + ex.InnerException?.Message)
            );

        private static readonly string neuronsPath = "nuclei/d23/neurons/";
        private static readonly string neuronsPathTemplate = neuronsPath + "{0}";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpNeuronClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }
        
        public async Task CreateNeuron(string avatarUrl, string id, string tag, string regionId, string externalReferenceUrl, string bearerToken, CancellationToken token = default(CancellationToken)) =>
            await HttpNeuronClient.exponentialRetryPolicy.ExecuteAsync(
                async () => await this.CreateNeuronInternal(avatarUrl, id, tag, regionId, externalReferenceUrl, bearerToken, token).ConfigureAwait(false));

        private async Task CreateNeuronInternal(string avatarUrl, string id, string tag, string regionId, string externalReferenceUrl, string bearerToken, CancellationToken token = default(CancellationToken))
        {
            var data = new
            {
                Id = id,
                Tag = HttpUtility.JavaScriptStringEncode(tag),
                RegionId = regionId,
                ExternalReferenceUrl = externalReferenceUrl
            };

            await this.requestProvider.PostAsync(
               $"{avatarUrl}{HttpNeuronClient.neuronsPath}",
               data,
               bearerToken
               );
        }

        public async Task ChangeNeuronTag(string avatarUrl, string id, string tag, int expectedVersion, string bearerToken, CancellationToken token = default(CancellationToken)) =>
            await HttpNeuronClient.exponentialRetryPolicy.ExecuteAsync(
                    async () => await this.ChangeNeuronTagInternal(avatarUrl, id, tag, expectedVersion, bearerToken, token).ConfigureAwait(false));

        private async Task ChangeNeuronTagInternal(string avatarUrl, string id, string tag, int expectedVersion, string bearerToken, CancellationToken token = default(CancellationToken))
        {
            var data = new
            {
                Tag = HttpUtility.JavaScriptStringEncode(tag)
            };

            await this.requestProvider.PatchAsync<object>(
               $"{avatarUrl}{string.Format(HttpNeuronClient.neuronsPathTemplate, id)}",
               data,
               bearerToken,
               token,
               new KeyValuePair<string, string>("ETag", expectedVersion.ToString())
               );
        }

        public async Task ChangeNeuronExternalReferenceUrl(string avatarUrl, string id, string externalReferenceUrl, int expectedVersion, string bearerToken, CancellationToken token = default(CancellationToken)) =>
            await HttpNeuronClient.exponentialRetryPolicy.ExecuteAsync(
                    async () => await this.ChangeNeuronExternalReferenceUrlInternal(avatarUrl, id, externalReferenceUrl, expectedVersion, bearerToken, token).ConfigureAwait(false));

        private async Task ChangeNeuronExternalReferenceUrlInternal(string avatarUrl, string id, string externalReferenceUrl, int expectedVersion, string bearerToken, CancellationToken token = default(CancellationToken))
        {
            var data = new
            {
                ExternalReferenceUrl = externalReferenceUrl
            };

            await this.requestProvider.PatchAsync<object>(
               $"{avatarUrl}{string.Format(HttpNeuronClient.neuronsPathTemplate, id)}",
               data,
               bearerToken,
               token,
               new KeyValuePair<string, string>("ETag", expectedVersion.ToString())
               );
        }

        public async Task DeactivateNeuron(string avatarUrl, string id, int expectedVersion, string bearerToken, CancellationToken token = default(CancellationToken)) =>
            await HttpNeuronClient.exponentialRetryPolicy.ExecuteAsync(
                async () => await this.DeactivateNeuronInternal(avatarUrl, id, expectedVersion, bearerToken, token));

        private async Task DeactivateNeuronInternal(string avatarUrl, string id, int expectedVersion, string bearerToken, CancellationToken token = default(CancellationToken))
        {
            await this.requestProvider.DeleteAsync<object>(
               $"{avatarUrl}{string.Format(HttpNeuronClient.neuronsPathTemplate, id)}",
               null,
               bearerToken,
               token,
               new KeyValuePair<string, string>("ETag", expectedVersion.ToString())
               );
        }
    }
}
