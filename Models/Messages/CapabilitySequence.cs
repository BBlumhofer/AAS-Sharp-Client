using System;
using System.Collections.Generic;
using System.Linq;
using AasSharpClient.Models.ProcessChain;
using AasSharpClient.Models.Helpers;
using BaSyx.Models.AdminShell;
using System;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// Typed wrapper around a BaSyx <see cref="SubmodelElementList"/> that is expected to contain
/// <see cref="OfferedCapability"/> elements.
/// </summary>
public class CapabilitySequence : SubmodelElementList
{
    public CapabilitySequence(string idShort = TransportRequestMessage.CapabilitiesSequenceIdShort) : base(idShort)
    {
        OrderRelevant = false;
    }

    /// <summary>
    /// Returns the contained capabilities as a typed list of <see cref="OfferedCapability"/>.
    /// This materializes a snapshot list from the underlying elements.
    /// </summary>
    public List<OfferedCapability> Capabilities
    {
        get
        {
            var list = new List<OfferedCapability>();
            foreach (var element in this)
            {
                if (element is OfferedCapability oc)
                {
                    list.Add(oc);
                    continue;
                }

                if (element is SubmodelElementCollection coll)
                {
                    try
                    {
                        var materialized = MaterializeFromCollection(coll);
                        list.Add(materialized);
                    }
                    catch
                    {
                        // ignore malformed children
                    }
                }
            }

            return list;
        }
    }

    private static OfferedCapability MaterializeFromCollection(SubmodelElementCollection source)
    {
        var offer = new OfferedCapability(string.Empty);
        if (source == null) return offer;

        offer.SemanticId = source.SemanticId;
        offer.Description = source.Description;
        offer.Qualifiers = source.Qualifiers;

        var children = source.Values ?? Array.Empty<ISubmodelElement>();
        foreach (var child in children)
        {
            switch (child)
            {
                case ReferenceElement reference when string.Equals(reference.IdShort, OfferedCapability.OfferedCapabilityReferenceIdShort, StringComparison.OrdinalIgnoreCase):
                    offer.OfferedCapabilityReference.Value = reference.Value;
                    break;
                case Property prop when string.Equals(prop.IdShort, OfferedCapability.InstanceIdentifierIdShort, StringComparison.OrdinalIgnoreCase):
                    offer.InstanceIdentifier.SetText(AasValueUnwrap.UnwrapToString(prop.Value) ?? string.Empty);
                    break;
                case Property prop when string.Equals(prop.IdShort, OfferedCapability.StationIdShort, StringComparison.OrdinalIgnoreCase):
                    offer.Station.SetText(AasValueUnwrap.UnwrapToString(prop.Value) ?? string.Empty);
                    break;
                case Property prop when string.Equals(prop.IdShort, OfferedCapability.MatchingScoreIdShort, StringComparison.OrdinalIgnoreCase):
                    offer.MatchingScore.Value = new PropertyValue<double>(double.TryParse(AasValueUnwrap.UnwrapToString(prop.Value), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0d);
                    break;
                case Property prop when string.Equals(prop.IdShort, OfferedCapability.CostIdShort, StringComparison.OrdinalIgnoreCase):
                    offer.SetCost(double.TryParse(AasValueUnwrap.UnwrapToString(prop.Value), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var c) ? c : 0d);
                    break;
                case Property prop when string.Equals(prop.IdShort, OfferedCapability.SequencePlacementIdShort, StringComparison.OrdinalIgnoreCase):
                    offer.SequencePlacement.SetText(AasValueUnwrap.UnwrapToString(prop.Value) ?? string.Empty);
                    break;
                case SubmodelElementCollection scm when string.Equals(scm.IdShort, OfferedCapability.EarliestSchedulingInformationIdShort, StringComparison.OrdinalIgnoreCase):
                    var props = scm.Values?.OfType<Property>() ?? Array.Empty<Property>();
                    DateTime? start = null; DateTime? end = null; TimeSpan? setup = null; TimeSpan? cycle = null;
                    foreach (var property in props)
                    {
                        var value = AasValueUnwrap.UnwrapToString(property.Value);
                        if (string.IsNullOrWhiteSpace(value)) continue;
                        switch (property.IdShort)
                        {
                            case "StartDateTime":
                                if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var ps)) start = ps;
                                break;
                            case "EndDateTime":
                                if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var pe)) end = pe;
                                break;
                            case "SetupTime": if (TimeSpan.TryParse(value, out var s)) setup = s; break;
                            case "CycleTime": if (TimeSpan.TryParse(value, out var cy)) cycle = cy; break;
                        }
                    }
                    if (start.HasValue && end.HasValue && setup.HasValue && cycle.HasValue) offer.SetEarliestScheduling(start.Value, end.Value, setup.Value, cycle.Value);
                    break;
                case SubmodelElementList list when string.Equals(list.IdShort, OfferedCapability.ActionsIdShort, StringComparison.OrdinalIgnoreCase):
                    foreach (var act in list)
                    {
                        if (act is AasSharpClient.Models.Action a)
                        {
                            offer.AddAction(a);
                        }
                        else if (act is SubmodelElementCollection acoll)
                        {
                            var title = string.Empty;
                            var machine = string.Empty;
                            foreach (var p in acoll.OfType<Property>())
                            {
                                if (string.Equals(p.IdShort, "ActionTitle", StringComparison.OrdinalIgnoreCase)) title = AasValueUnwrap.UnwrapToString(p.Value) ?? string.Empty;
                                if (string.Equals(p.IdShort, "MachineName", StringComparison.OrdinalIgnoreCase)) machine = AasValueUnwrap.UnwrapToString(p.Value) ?? string.Empty;
                            }

                            AasSharpClient.Models.InputParameters? inputParameters = null;
                            try
                            {
                                var ip = acoll.Values?.OfType<SubmodelElementCollection>()
                                    .FirstOrDefault(c => string.Equals(c.IdShort, "InputParameters", StringComparison.OrdinalIgnoreCase));
                                if (ip != null)
                                {
                                    inputParameters = new AasSharpClient.Models.InputParameters();
                                    foreach (var prop in ip.OfType<Property>())
                                    {
                                        var key = prop.IdShort ?? string.Empty;
                                        if (string.IsNullOrWhiteSpace(key)) continue;
                                        var value = AasValueUnwrap.UnwrapToString(prop.Value) ?? string.Empty;
                                        inputParameters.SetParameter(key, value);
                                    }
                                }
                            }
                            catch { }

                            var mat = new AasSharpClient.Models.Action(idShort: "", actionTitle: title, status: AasSharpClient.Models.ActionStatusEnum.PLANNED, inputParameters: inputParameters, finalResultData: null, preconditions: null, skillReference: null, machineName: machine);
                            offer.AddAction(mat);
                        }
                    }
                    break;
                case SubmodelElementCollection coll:
                    try
                    {
                        var id = coll.IdShort ?? string.Empty;
                        if (string.Equals(id, "Preconditions", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var el in coll.Values ?? Array.Empty<ISubmodelElement>())
                                try { offer.Preconditions.Add(el); } catch { }
                            break;
                        }

                        if (string.Equals(id, "FinalResultData", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var el in coll.Values ?? Array.Empty<ISubmodelElement>())
                                try { offer.FinalResultData.Add(el); } catch { }
                            break;
                        }

                        if (string.Equals(id, "Effects", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var el in coll.Values ?? Array.Empty<ISubmodelElement>())
                                try { offer.Effects.Add(el); } catch { }
                            break;
                        }

                        if (string.Equals(id, "Postconditions", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var el in coll.Values ?? Array.Empty<ISubmodelElement>())
                                try { offer.Postconditions.Add(el); } catch { }
                            break;
                        }
                    }
                    catch { }
                    // unknown collection: ignore
                    break;
                case SubmodelElementList listUnknown:
                    try
                    {
                        var id = listUnknown.IdShort ?? string.Empty;
                        if (string.Equals(id, "Preconditions", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var el in listUnknown) try { offer.Preconditions.Add(el); } catch { }
                            break;
                        }
                        if (string.Equals(id, "FinalResultData", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var el in listUnknown) try { offer.FinalResultData.Add(el); } catch { }
                            break;
                        }
                        if (string.Equals(id, "Effects", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var el in listUnknown) try { offer.Effects.Add(el); } catch { }
                            break;
                        }
                        if (string.Equals(id, "Postconditions", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var el in listUnknown) try { offer.Postconditions.Add(el); } catch { }
                            break;
                        }
                    }
                    catch { }
                    break;
            }
        }

        return offer;
    }

    public IEnumerable<OfferedCapability> OfferedCapabilities
    {
        get
        {
            foreach (var element in this)
            {
                if (element is OfferedCapability cap)
                {
                    yield return cap;
                }
            }
        }
    }

    public void AddCapability(OfferedCapability capability)
    {
        if (capability == null)
        {
            return;
        }

        // Keep list entries anonymous; consumers typically rely on InstanceIdentifier.
        if (!string.IsNullOrEmpty(capability.IdShort))
        {
            capability.IdShort = string.Empty;
        }

        Add(capability);
    }

    /// <summary>
    /// Insert items at the beginning of the sequence.
    /// </summary>
    public void InsertRangeBefore(IEnumerable<OfferedCapability> capabilities)
    {
        if (capabilities == null) return;
        var toInsert = capabilities.Where(c => c != null).Cast<ISubmodelElement>().ToList();
        if (toInsert.Count == 0) return;

        var existing = this.Cast<ISubmodelElement>().ToList();
        var combined = new List<ISubmodelElement>(toInsert.Count + existing.Count);
        combined.AddRange(toInsert);
        combined.AddRange(existing);

        Clear();
        AddRange(combined);
    }

    /// <summary>
    /// Append items to the end of the sequence.
    /// </summary>
    public void InsertRangeAfter(IEnumerable<OfferedCapability> capabilities)
    {
        if (capabilities == null) return;
        foreach (var c in capabilities)
        {
            if (c != null) Add(c);
        }
    }

    /// <summary>
    /// Insert items at the specified zero-based index.
    /// </summary>
    public void InsertRangeAt(int index, IEnumerable<OfferedCapability> capabilities)
    {
        if (capabilities == null) return;
        var toInsert = capabilities.Where(c => c != null).Cast<ISubmodelElement>().ToList();
        if (toInsert.Count == 0) return;

        var existing = this.Cast<ISubmodelElement>().ToList();
        if (index <= 0)
        {
            InsertRangeBefore(toInsert.Cast<OfferedCapability>());
            return;
        }

        if (index >= existing.Count)
        {
            InsertRangeAfter(toInsert.Cast<OfferedCapability>());
            return;
        }

        var combined = new List<ISubmodelElement>(existing.Count + toInsert.Count);
        combined.AddRange(existing.Take(index));
        combined.AddRange(toInsert);
        combined.AddRange(existing.Skip(index));

        Clear();
        AddRange(combined);
    }
}
