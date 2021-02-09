using System.Threading;
using System.Threading.Tasks;
using ei8.Cortex.Diary.Common;

namespace ei8.Cortex.Diary.Nucleus.Client.Out
{
    public interface INotificationClient
    {
        Task<NotificationLog> GetNotificationLog(string avatarUrl, string notificationLogId, string bearerToken, CancellationToken token = default(CancellationToken));
    }
}
