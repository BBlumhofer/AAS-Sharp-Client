using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Clients.AdminShell.Http;
using BaSyx.Utils.ResultHandling;
using Microsoft.Extensions.Logging;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Remote
{
    // Small DI-friendly service that uses a factory to create BaSyx SubmodelHttpClient instances.
    // This intentionally avoids a heavy adapter: tests can inject a custom factory that returns
    // a mocked client or a client configured with a special HttpMessageHandler.
    public class RemoteScheduleSyncService : IRemoteScheduleSyncService
    {
        private readonly Func<Uri, SubmodelHttpClient> _clientFactory;
        private readonly ILogger<RemoteScheduleSyncService>? _logger;

        public RemoteScheduleSyncService(Func<Uri, SubmodelHttpClient>? clientFactory = null, ILogger<RemoteScheduleSyncService>? logger = null)
        {
            _clientFactory = clientFactory ?? (u => new SubmodelHttpClient(u));
            _logger = logger;
        }

        public async Task SyncToAsync(MachineScheduleSubmodel submodel, Uri endpoint, CancellationToken cancellationToken = default)
        {
            if (submodel == null) throw new ArgumentNullException(nameof(submodel));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            using var client = _clientFactory(endpoint);
            IResult result = await client.ReplaceSubmodelAsync(submodel).ConfigureAwait(false);
            if (!result.Success)
            {
                _logger?.LogError("SyncToAsync failed: {0}", result.Messages?.ToString());
                throw new InvalidOperationException($"Failed to sync submodel to {endpoint}: {result.Messages}");
            }
        }

        public async Task<MachineScheduleData> SyncFromAsync(Uri endpoint, CancellationToken cancellationToken = default)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            using var client = _clientFactory(endpoint);
            var retrieveResult = await client.RetrieveSubmodelAsync().ConfigureAwait(false);
            if (!retrieveResult.Success || retrieveResult.Entity == null)
            {
                _logger?.LogError("SyncFromAsync failed: {0}", retrieveResult.Messages?.ToString());
                throw new InvalidOperationException($"Failed to retrieve submodel from {endpoint}: {retrieveResult.Messages}");
            }

            if (retrieveResult.Entity is not ISubmodel remoteSubmodel)
                throw new InvalidOperationException("Retrieved entity is not a submodel");

            string submodelId = remoteSubmodel.Id?.Id ?? string.Empty;
            string? lastUpdated = null;
            bool hasOpen = false;
            IReadOnlyList<ISubmodelElement>? scheduleEntries = null;

            try
            {
                var lastProp = remoteSubmodel.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, "LastTimeUpdated", StringComparison.OrdinalIgnoreCase));
                if (lastProp != null)
                    lastUpdated = lastProp.Value?.Value?.ToString();

                var hasOpenProp = remoteSubmodel.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, "HasOpenTasks", StringComparison.OrdinalIgnoreCase));
                if (hasOpenProp != null)
                {
                    var maybe = hasOpenProp.Value?.Value as BaSyx.Models.AdminShell.IValue;
                    if (maybe != null)
                    {
                        try { hasOpen = maybe.ToObject<bool>(); } catch { bool.TryParse(maybe.ToString(), out hasOpen); }
                    }
                }

                var scheduleList = remoteSubmodel.SubmodelElements.OfType<SubmodelElementList>().FirstOrDefault(l => string.Equals(l.IdShort, "Schedule", StringComparison.OrdinalIgnoreCase));
                if (scheduleList != null)
                {
                    var temp = new List<ISubmodelElement>();
                    foreach (var e in scheduleList)
                    {
                        if (e is ISubmodelElement sme) temp.Add(sme);
                    }
                    scheduleEntries = temp;
                }
            }
            catch { /* best-effort mapping */ }

            return new MachineScheduleData(submodelId, lastUpdated, hasOpen, scheduleEntries);
        }
    }
}
