using System;
using System.Threading;
using System.Threading.Tasks;

namespace AasSharpClient.Models.Remote
{
    public interface IRemoteScheduleSyncService
    {
        Task SyncToAsync(MachineScheduleSubmodel submodel, Uri endpoint, CancellationToken cancellationToken = default);
        Task<MachineScheduleData> SyncFromAsync(Uri endpoint, CancellationToken cancellationToken = default);
    }
}
