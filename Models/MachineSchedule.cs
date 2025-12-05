using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using BaSyx.Clients.AdminShell.Http;
using BaSyx.Utils.ResultHandling;
using BaSyx.Registry.Client.Http;

namespace AasSharpClient.Models;

public sealed class MachineScheduleSubmodel : Submodel
{
    private static readonly Reference SemanticReference = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/semantics/submodel/MachineSchedule#1/0"));

    private static readonly Reference LastUpdatedSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/semantics/submodel-element/MachineSchedule/LastTimeUpdated#1/0"));

    private static readonly Reference HasOpenTasksSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/semantics/submodel-element/MachineSchedule/HasOpenTasks#1/0"));

    private static readonly Reference ScheduleSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/semantics/submodel-element/MachineSchedule/Schedule#1/0"));

    public Property LastTimeUpdated { get; }
    public Property HasOpenTasks { get; }
    public SubmodelElementList Schedule { get; }
    private readonly List<SchedulingContainer> _managedSchedules = new();

    public MachineScheduleSubmodel(string? submodelIdentifier = null)
        : base("MachineSchedule", new Identifier(submodelIdentifier ?? Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        SemanticId = SemanticReference;

        LastTimeUpdated = CreateLastUpdatedProperty(null);
        HasOpenTasks = CreateHasOpenTasksProperty(false);
        Schedule = CreateScheduleList(null);

        SubmodelElements.Add(LastTimeUpdated);
        SubmodelElements.Add(HasOpenTasks);
        SubmodelElements.Add(Schedule);
    }

    public static MachineScheduleSubmodel CreateWithIdentifier(string submodelIdentifier) => new(submodelIdentifier);

    public Task<string> ToJsonAsync(CancellationToken cancellationToken = default) => SubmodelSerialization.SerializeAsync(this, cancellationToken);

    public void Apply(MachineScheduleData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        LastTimeUpdated.Value = data.LastTimeUpdated is null
            ? null
            : new PropertyValue<string>(data.LastTimeUpdated);
        HasOpenTasks.Value = new PropertyValue<bool>(data.HasOpenTasks);

        Schedule.Clear();
        if (data.ScheduleEntries != null)
        {
            foreach (var entry in data.ScheduleEntries)
            {
                Schedule.Add(entry);
            }
        }
        // populate managed schedules from any SchedulingContainer entries and ensure helper-derived state is consistent
        _managedSchedules.Clear();
        if (data.ScheduleEntries != null)
        {
            foreach (var entry in data.ScheduleEntries.OfType<SchedulingContainer>())
            {
                _managedSchedules.Add(entry);
            }
        }

        RefreshHasOpenTasks();
    }

    // Return schedule entries as strongly-typed SchedulingContainer instances when possible
    public IReadOnlyList<SchedulingContainer> GetSchedules()
    {
        return _managedSchedules.AsReadOnly();
    }

    // Add or update a scheduling container. Upsert is based on ReferredStep reference if present,
    // otherwise a new entry is appended.
    public void AddOrUpdateSchedule(SchedulingContainer container)
    {
        if (container == null) throw new ArgumentNullException(nameof(container));

        // try match via referred step identity
        string? matchKey = container.ReferredStep?.Value?.ToString();

        if (!string.IsNullOrEmpty(matchKey))
        {
            var existing = _managedSchedules
                .FirstOrDefault(c => string.Equals(c.ReferredStep?.Value?.ToString(), matchKey, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                // replace existing element in the managed list
                _managedSchedules.Remove(existing);
                _managedSchedules.Add(container);
                UpdateLastTimeUpdated();
                RefreshHasOpenTasks();
                return;
            }
        }

        // no match -> append
        // append to managed schedules (do not add directly to the SubmodelElementList to avoid BaSyx constraints)
        _managedSchedules.Add(container);
        UpdateLastTimeUpdated();
        RefreshHasOpenTasks();
    }

    public bool RemoveSchedule(SchedulingContainer container)
    {
        if (container == null) return false;

        var removed = _managedSchedules.Remove(container);
        if (removed)
        {
            UpdateLastTimeUpdated();
            RefreshHasOpenTasks();
        }

        return removed;
    }

    private void UpdateLastTimeUpdated()
    {
        LastTimeUpdated.Value = new PropertyValue<string>(DateTime.UtcNow.ToString("o"));
    }

    private static void EnsureUniqueIdShort(SchedulingContainer container)
    {
        if (container == null) return;

        // generate a unique idShort for list children to satisfy BaSyx constraints
        var newId = $"Scheduling_{Guid.NewGuid():N}";

        var prop = container.GetType().GetProperty("IdShort", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(container, newId);
            return;
        }

        var field = container.GetType().GetField("<IdShort>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(container, newId);
        }
    }

    private void RefreshHasOpenTasks()
    {
        // A simple heuristic: if there is any scheduling entry with no EndDateTime or an end in the future,
        // consider there are open tasks.
        var now = DateTime.UtcNow;
        var hasOpen = _managedSchedules.Any(sc =>
        {
            var end = sc.GetEndDateTime();
            if (!end.HasValue) return true;
            return end.Value > now;
        });

        HasOpenTasks.Value = new PropertyValue<bool>(hasOpen);
    }

    // Remote sync using the BaSyx Submodel HTTP client
    public async Task SyncToRemoteAsync(string remoteEndpoint, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(remoteEndpoint)) throw new ArgumentNullException(nameof(remoteEndpoint));

        var uri = new Uri(remoteEndpoint);
        var client = new SubmodelHttpClient(uri);

        IResult result = await client.ReplaceSubmodelAsync(this).ConfigureAwait(false);
        if (!result.Success)
        {
            var messages = result.Messages?.ToString() ?? string.Empty;
            throw new InvalidOperationException($"Failed to sync submodel to remote endpoint '{remoteEndpoint}': {messages}");
        }
    }

    public async Task SyncFromRemoteAsync(string remoteEndpoint, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(remoteEndpoint)) throw new ArgumentNullException(nameof(remoteEndpoint));

        var uri = new Uri(remoteEndpoint);
        var client = new SubmodelHttpClient(uri);

        var retrieveResult = await client.RetrieveSubmodelAsync().ConfigureAwait(false);
        if (!retrieveResult.Success || retrieveResult.Entity == null)
        {
            var messages = retrieveResult.Messages?.ToString() ?? string.Empty;
            throw new InvalidOperationException($"Failed to retrieve submodel from remote endpoint '{remoteEndpoint}': {messages}");
        }

        // Build MachineScheduleData from the retrieved ISubmodel
        if (retrieveResult.Entity is not ISubmodel remoteSubmodel)
        {
            throw new InvalidOperationException("Retrieved entity is not a submodel");
        }

        string submodelId = remoteSubmodel.Id?.Id ?? string.Empty;

        string? lastUpdated = null;
        bool hasOpen = false;
        IReadOnlyList<ISubmodelElement>? scheduleEntries = null;

        try
        {
            var lastProp = remoteSubmodel.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, LastTimeUpdated.IdShort, StringComparison.OrdinalIgnoreCase));
            if (lastProp != null)
            {
                lastUpdated = lastProp.Value?.Value?.ToString();
            }

            var hasOpenProp = remoteSubmodel.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, HasOpenTasks.IdShort, StringComparison.OrdinalIgnoreCase));
            if (hasOpenProp != null)
            {
                var maybeValue = hasOpenProp.Value?.Value as BaSyx.Models.AdminShell.IValue;
                if (maybeValue != null)
                {
                    try
                    {
                        hasOpen = maybeValue.ToObject<bool>();
                    }
                    catch
                    {
                        var s = maybeValue.ToString();
                        if (!bool.TryParse(s, out var parsed)) parsed = false;
                        hasOpen = parsed;
                    }
                }
            }

            var scheduleList = remoteSubmodel.SubmodelElements.OfType<SubmodelElementList>().FirstOrDefault(l => string.Equals(l.IdShort, Schedule.IdShort, StringComparison.OrdinalIgnoreCase));
            if (scheduleList != null)
            {
                // copy elements into a temporary list
                var temp = new List<ISubmodelElement>();
                foreach (var e in scheduleList)
                {
                    if (e is ISubmodelElement sme)
                    {
                        temp.Add(sme);
                    }
                }

                scheduleEntries = temp;
            }
        }
        catch
        {
            // best effort: if parsing fails continue with whatever we collected
        }

        var data = new MachineScheduleData(submodelId, lastUpdated, hasOpen, scheduleEntries);
        Apply(data);
    }

    // Convenience: Sync using an AssetAdministrationShell endpoint and a submodel Identifier
    public async Task SyncToAasAsync(Uri aasEndpoint, Identifier submodelIdentifier, CancellationToken token = default)
    {
        if (aasEndpoint == null) throw new ArgumentNullException(nameof(aasEndpoint));
        if (submodelIdentifier == null) throw new ArgumentNullException(nameof(submodelIdentifier));

        var aasClient = new AssetAdministrationShellHttpClient(aasEndpoint);
        var result = await aasClient.ReplaceSubmodelAsync(submodelIdentifier, this).ConfigureAwait(false);
        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to replace submodel on AAS endpoint: {result.Messages}");
        }
    }

    public async Task SyncFromAasAsync(Uri aasEndpoint, Identifier submodelIdentifier, CancellationToken token = default)
    {
        if (aasEndpoint == null) throw new ArgumentNullException(nameof(aasEndpoint));
        if (submodelIdentifier == null) throw new ArgumentNullException(nameof(submodelIdentifier));

        var aasClient = new AssetAdministrationShellHttpClient(aasEndpoint);
        var retrieve = await aasClient.RetrieveSubmodelAsync(submodelIdentifier).ConfigureAwait(false);
        if (!retrieve.Success || retrieve.Entity == null)
        {
            throw new InvalidOperationException($"Failed to retrieve submodel from AAS endpoint: {retrieve.Messages}");
        }

        var remote = retrieve.Entity;
        var submodelId = remote.Id?.Id ?? string.Empty;
        var last = remote.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, LastTimeUpdated.IdShort, StringComparison.OrdinalIgnoreCase))?.Value?.Value?.ToString();
        var hasOpen = false;
        var hasOpenProp = remote.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, HasOpenTasks.IdShort, StringComparison.OrdinalIgnoreCase));
        if (hasOpenProp != null)
        {
            var maybe = hasOpenProp.Value?.Value as BaSyx.Models.AdminShell.IValue;
            if (maybe != null)
            {
                try { hasOpen = maybe.ToObject<bool>(); } catch { bool.TryParse(maybe.ToString(), out hasOpen); }
            }
        }
        var scheduleList = remote.SubmodelElements.OfType<SubmodelElementList>().FirstOrDefault(l => string.Equals(l.IdShort, Schedule.IdShort, StringComparison.OrdinalIgnoreCase));
        IReadOnlyList<ISubmodelElement>? scheduleEntries = null;
        if (scheduleList != null)
        {
            var temp = new List<ISubmodelElement>();
            foreach (var e in scheduleList)
            {
                if (e is ISubmodelElement sme) temp.Add(sme);
            }

            scheduleEntries = temp;
        }

        var data = new MachineScheduleData(submodelId, last, hasOpen, scheduleEntries);
        Apply(data);
    }

    // Convenience: Sync using a Submodel Repository endpoint
    public async Task SyncToSubmodelRepositoryAsync(Uri repositoryEndpoint, Identifier submodelIdentifier, CancellationToken token = default)
    {
        if (repositoryEndpoint == null) throw new ArgumentNullException(nameof(repositoryEndpoint));
        if (submodelIdentifier == null) throw new ArgumentNullException(nameof(submodelIdentifier));

        var repoClient = new SubmodelRepositoryHttpClient(repositoryEndpoint);
        var result = await repoClient.UpdateSubmodelAsync(submodelIdentifier, this).ConfigureAwait(false);
        if (!result.Success) throw new InvalidOperationException($"Failed to update submodel in repository: {result.Messages}");
    }

    public async Task SyncFromSubmodelRepositoryAsync(Uri repositoryEndpoint, Identifier submodelIdentifier, CancellationToken token = default)
    {
        if (repositoryEndpoint == null) throw new ArgumentNullException(nameof(repositoryEndpoint));
        if (submodelIdentifier == null) throw new ArgumentNullException(nameof(submodelIdentifier));

        var repoClient = new SubmodelRepositoryHttpClient(repositoryEndpoint);
        var retrieve = await repoClient.RetrieveSubmodelAsync(submodelIdentifier).ConfigureAwait(false);
        if (!retrieve.Success || retrieve.Entity == null) throw new InvalidOperationException($"Failed to retrieve submodel from repository: {retrieve.Messages}");

        var remote = retrieve.Entity;
        var scheduleList = remote.SubmodelElements.OfType<SubmodelElementList>().FirstOrDefault(l => string.Equals(l.IdShort, Schedule.IdShort, StringComparison.OrdinalIgnoreCase));
        IReadOnlyList<ISubmodelElement>? scheduleEntries = null;
        if (scheduleList != null)
        {
            var temp = new List<ISubmodelElement>();
            foreach (var e in scheduleList)
            {
                if (e is ISubmodelElement sme) temp.Add(sme);
            }
            scheduleEntries = temp;
        }

        var last = remote.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, LastTimeUpdated.IdShort, StringComparison.OrdinalIgnoreCase))?.Value?.Value?.ToString();
        var hasOpen = false;
        var hasOpenProp = remote.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, HasOpenTasks.IdShort, StringComparison.OrdinalIgnoreCase));
        if (hasOpenProp != null)
        {
            var maybe = hasOpenProp.Value?.Value as BaSyx.Models.AdminShell.IValue;
            if (maybe != null)
            {
                try { hasOpen = maybe.ToObject<bool>(); } catch { bool.TryParse(maybe.ToString(), out hasOpen); }
            }
        }

        var data = new MachineScheduleData(remote.Id?.Id ?? string.Empty, last, hasOpen, scheduleEntries);
        Apply(data);
    }

    // Convenience: use a Registry to resolve the submodel endpoint, then sync
    public async Task SyncToRegistryAsync(string registryUrl, string aasIdentifier, string submodelIdentifier, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(registryUrl)) throw new ArgumentNullException(nameof(registryUrl));
        if (string.IsNullOrWhiteSpace(aasIdentifier)) throw new ArgumentNullException(nameof(aasIdentifier));
        if (string.IsNullOrWhiteSpace(submodelIdentifier)) throw new ArgumentNullException(nameof(submodelIdentifier));

        var settings = new RegistryClientSettings();
        settings.RegistryConfig.RegistryUrl = registryUrl.TrimEnd('/');
        var registry = new RegistryHttpClient(settings);
        var regResult = await registry.RetrieveSubmodelRegistrationAsync(aasIdentifier, submodelIdentifier).ConfigureAwait(false);
        if (!regResult.Success || regResult.Entity == null) throw new InvalidOperationException($"Failed to retrieve submodel descriptor from registry: {regResult.Messages}");

        var descriptor = regResult.Entity;
        var endpoint = descriptor.Endpoints?.FirstOrDefault(e => e.ProtocolInformation?.EndpointProtocol == Uri.UriSchemeHttps) ?? descriptor.Endpoints?.FirstOrDefault(e => e.ProtocolInformation?.EndpointProtocol == Uri.UriSchemeHttp);
        if (endpoint == null || string.IsNullOrEmpty(endpoint.ProtocolInformation?.EndpointAddress)) throw new InvalidOperationException("No HTTP endpoint found in submodel descriptor retrieved from registry");

        var submodelUri = new Uri(endpoint.ProtocolInformation.EndpointAddress);
        var client = new SubmodelHttpClient(submodelUri);
        var result = await client.ReplaceSubmodelAsync(this).ConfigureAwait(false);
        if (!result.Success) throw new InvalidOperationException($"Failed to replace submodel via registry-resolved endpoint: {result.Messages}");
    }

    public async Task SyncFromRegistryAsync(string registryUrl, string aasIdentifier, string submodelIdentifier, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(registryUrl)) throw new ArgumentNullException(nameof(registryUrl));
        if (string.IsNullOrWhiteSpace(aasIdentifier)) throw new ArgumentNullException(nameof(aasIdentifier));
        if (string.IsNullOrWhiteSpace(submodelIdentifier)) throw new ArgumentNullException(nameof(submodelIdentifier));

        var settings = new RegistryClientSettings();
        settings.RegistryConfig.RegistryUrl = registryUrl.TrimEnd('/');
        var registry = new RegistryHttpClient(settings);
        var regResult = await registry.RetrieveSubmodelRegistrationAsync(aasIdentifier, submodelIdentifier).ConfigureAwait(false);
        if (!regResult.Success || regResult.Entity == null) throw new InvalidOperationException($"Failed to retrieve submodel descriptor from registry: {regResult.Messages}");

        var descriptor = regResult.Entity;
        var endpoint = descriptor.Endpoints?.FirstOrDefault(e => e.ProtocolInformation?.EndpointProtocol == Uri.UriSchemeHttps) ?? descriptor.Endpoints?.FirstOrDefault(e => e.ProtocolInformation?.EndpointProtocol == Uri.UriSchemeHttp);
        if (endpoint == null || string.IsNullOrEmpty(endpoint.ProtocolInformation?.EndpointAddress)) throw new InvalidOperationException("No HTTP endpoint found in submodel descriptor retrieved from registry");

        var submodelUri = new Uri(endpoint.ProtocolInformation.EndpointAddress);
        var client = new SubmodelHttpClient(submodelUri);
        var retrieve = await client.RetrieveSubmodelAsync().ConfigureAwait(false);
        if (!retrieve.Success || retrieve.Entity == null) throw new InvalidOperationException($"Failed to retrieve submodel via registry-resolved endpoint: {retrieve.Messages}");

        var remote = retrieve.Entity;
        var last = remote.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, LastTimeUpdated.IdShort, StringComparison.OrdinalIgnoreCase))?.Value?.Value?.ToString();
        var hasOpen = false;
        var hasOpenProp = remote.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, HasOpenTasks.IdShort, StringComparison.OrdinalIgnoreCase));
        if (hasOpenProp != null)
        {
            var maybe = hasOpenProp.Value?.Value as BaSyx.Models.AdminShell.IValue;
            if (maybe != null)
            {
                try { hasOpen = maybe.ToObject<bool>(); } catch { bool.TryParse(maybe.ToString(), out hasOpen); }
            }
        }
        var scheduleList = remote.SubmodelElements.OfType<SubmodelElementList>().FirstOrDefault(l => string.Equals(l.IdShort, Schedule.IdShort, StringComparison.OrdinalIgnoreCase));
        IReadOnlyList<ISubmodelElement>? scheduleEntries = null;
        if (scheduleList != null)
        {
            var temp = new List<ISubmodelElement>();
            foreach (var e in scheduleList)
            {
                if (e is ISubmodelElement sme) temp.Add(sme);
            }
            scheduleEntries = temp;
        }

        var data = new MachineScheduleData(remote.Id?.Id ?? string.Empty, last, hasOpen, scheduleEntries);
        Apply(data);
    }

    private static Property CreateLastUpdatedProperty(string? timestamp)
    {
        return SubmodelElementFactory.CreateProperty("LastTimeUpdated", timestamp, LastUpdatedSemantic, "xs:dateTime");
    }

    private static Property CreateHasOpenTasksProperty(bool hasOpenTasks)
    {
        return SubmodelElementFactory.CreateProperty("HasOpenTasks", hasOpenTasks, HasOpenTasksSemantic, "xs:boolean");
    }

    private static SubmodelElementList CreateScheduleList(IReadOnlyList<ISubmodelElement>? entries)
    {
        var list = new SubmodelElementList("Schedule")
        {
            SemanticId = ScheduleSemantic,
            OrderRelevant = true,
            TypeValueListElement = ModelType.SubmodelElementCollection
        };

        if (entries != null)
        {
            foreach (var entry in entries)
            {
                list.Add(entry);
            }
        }

        return list;
    }
}

public sealed record MachineScheduleData(
    string SubmodelIdentifier,
    string? LastTimeUpdated,
    bool HasOpenTasks,
    IReadOnlyList<ISubmodelElement>? ScheduleEntries);
