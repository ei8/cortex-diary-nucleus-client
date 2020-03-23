using System.Threading;
using System.Threading.Tasks;
using works.ei8.Cortex.Diary.Common;

namespace works.ei8.Cortex.Diary.Nucleus.Client.Out
{
    public interface INotificationClient
    {
        Task<NotificationLog> GetNotificationLog(string avatarUrl, string notificationLogId, CancellationToken token = default(CancellationToken));
    }
}
