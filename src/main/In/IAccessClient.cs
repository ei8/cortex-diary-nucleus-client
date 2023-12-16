using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Client.In
{
    public interface IAccessClient
    {
        Task CreateNeuronAccessRequest(string avatarUrl, string neuronId, string bearerToken, CancellationToken token = default(CancellationToken));
    }
}
