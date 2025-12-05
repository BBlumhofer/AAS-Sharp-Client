using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using BaSyx.Utils;
using BaSyx.Models.Extensions;

namespace AasSharpClient.Models;

public class ProductionPlan : Submodel
{
    [JsonIgnore]
    public Property<string> IsFinished { get; }
    [JsonIgnore]
    public QuantityInformation QuantityInformation { get; }
    [JsonIgnore]
    public List<Step> Steps { get; }

    public ProductionPlan(bool isFinished, int totalNumberOfPieces, Step? initialStep = null)
        : base("ProductionPlan", new Identifier($"https://smartfactory.de/submodels/{Guid.NewGuid()}"))
    {
        Kind = ModelingKind.Instance;
        SemanticId = SemanticReferences.ProductionPlanSemanticId;

        IsFinished = new Property<string>("IsFinished", isFinished ? "true" : "false");
        IsFinished.SemanticId = SemanticReferences.IsFinished;
        QuantityInformation = new QuantityInformation(totalNumberOfPieces);
        Steps = new List<Step>();

        SubmodelElements.Add(IsFinished);
        SubmodelElements.Add(QuantityInformation);

        if (initialStep != null)
        {
            append_step(initialStep);
        }
    }

    public void append_step(Step step)
    {
        Steps.Add(step);
        SubmodelElements.Add(step);
    }

    public Step? GetStep(string idShort)
    {
        if (string.IsNullOrWhiteSpace(idShort))
        {
            return null;
        }

        return Steps.FirstOrDefault(step => string.Equals(step.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<Step> GetStepsByStatus(StepStatusEnum status)
    {
        return Steps.Where(step => step.State == status).ToList();
    }

    public void UpdateQuantity(int totalNumberOfPieces)
    {
        QuantityInformation.TotalNumberOfPieces.Value = new PropertyValue<string>(totalNumberOfPieces.ToString());
    }

    public void SetFinished(bool finished)
    {
        IsFinished.Value = new PropertyValue<string>(finished ? "true" : "false");
    }

    public bool RemoveStep(string idShort)
    {
        var step = GetStep(idShort);
        if (step is null)
        {
            return false;
        }

        Steps.Remove(step);
        SubmodelElements.Remove(step);
        return true;
    }

    public void InsertStep(int index, Step step)
    {
        if (step is null)
        {
            throw new ArgumentNullException(nameof(step));
        }

        Steps.Remove(step);
        index = Math.Clamp(index, 0, Steps.Count);
        Steps.Insert(index, step);
        SyncStepElements();
    }

    public IEnumerable<Action> GetPendingActions()
    {
        return Steps.SelectMany(s => s.Actions).Where(a => a.State != ActionStatusEnum.DONE);
    }

    public bool ResetStep(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.Reset());
    public bool ScheduleStep(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.Schedule());
    public bool StartStepProduction(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.StartProduction());
    public bool SuspendStep(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.Suspend());
    public bool ResumeStep(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.Resume());
    public bool CompleteStep(string stepIdShort) => ApplyStepTransition(
        stepIdShort,
        step => step.State == StepStatusEnum.DONE || step.EndProduction());
    public bool ErrorStep(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.Error());
    public bool AbortStep(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.Abort());
    public bool ReturnStepToCreated(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.ReturnToCreated());
    public bool ReturnStepToPlanned(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.ReturnToPlanned());
    public bool ReturnStepToExecuting(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.ReturnToExecuting());
    public bool ReturnStepToSuspended(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.ReturnToSuspended());
    public bool ReturnStepToCompleted(string stepIdShort) => ApplyStepTransition(stepIdShort, step => step.ReturnToCompleted());

    public bool ResetAction(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.Reset());
    public bool ScheduleAction(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.Schedule());
    public bool StartActionProduction(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.StartProduction());
    public bool SuspendAction(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.Suspend());
    public bool ResumeAction(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.Resume());
    public bool CompleteAction(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.EndProduction());
    public bool ErrorAction(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.Error());
    public bool AbortAction(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.Abort());
    public bool ReturnActionToCreated(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.ReturnToCreated());
    public bool ReturnActionToPlanned(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.ReturnToPlanned());
    public bool ReturnActionToExecuting(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.ReturnToExecuting());
    public bool ReturnActionToSuspended(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.ReturnToSuspended());
    public bool ReturnActionToCompleted(string stepIdShort, string actionIdShort) => ApplyActionTransition(stepIdShort, actionIdShort, action => action.ReturnToCompleted());

    public void Complete() => IsFinished.Value = new PropertyValue<string>("true");
    public bool IsCompleted() => IsFinished.Value.Value?.ToString() == "true";

    public Task<string> ToJsonAsync(CancellationToken cancellationToken = default)
    {
        return SubmodelSerialization.SerializeAsync(this, cancellationToken);
    }

    public static ProductionPlan Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        bool isFinished = false;
        int totalPieces = 0;
        var steps = new List<Step>();

        if (root.TryGetProperty("submodelElements", out var submodelElements) && submodelElements.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in submodelElements.EnumerateArray())
            {
                if (!element.TryGetProperty("idShort", out var idShortProp))
                {
                    continue;
                }

                var idShort = idShortProp.GetString() ?? string.Empty;
                switch (idShort)
                {
                    case "IsFinished":
                        isFinished = string.Equals(element.GetProperty("value").GetString(), "true", StringComparison.OrdinalIgnoreCase);
                        break;
                    case "QuantityInformation":
                        totalPieces = ParseQuantityInformation(element);
                        break;
                    default:
                        if (idShort.StartsWith("Step", StringComparison.OrdinalIgnoreCase))
                        {
                            steps.Add(Step.FromJson(element));
                        }
                        break;
                }
            }
        }

        var plan = new ProductionPlan(isFinished, totalPieces);
        plan.SubmodelElements.Clear();
        plan.Steps.Clear();
        plan.SubmodelElements.Add(plan.IsFinished);
        plan.SubmodelElements.Add(plan.QuantityInformation);
        plan.IsFinished.Value = new PropertyValue<string>(isFinished ? "true" : "false");
        plan.QuantityInformation.TotalNumberOfPieces.Value = new PropertyValue<string>(totalPieces.ToString());

        foreach (var step in steps)
        {
            plan.append_step(step);
        }

        return plan;
    }

    /// <summary>
    /// Fill steps and action parameters from referenced submodels and normalize scheduling.
    /// This mirrors the Java logic from referable/ProductionPlan.fill_steps
    /// </summary>
    /// <param name="submodels">Optional list of submodels to search for referenced values.</param>
    public void FillSteps(params Submodel[] submodels)
    {
        foreach (var step in Steps)
        {
            foreach (var action in step.Actions)
            {
                FillAction(action, submodels);
            }

            try
            {
                step.Scheduling?.NormalizeToAbsoluteDates();
            }
            catch
            {
                // ignore scheduling normalization failures
            }
        }
    }

    private void FillAction(Action action, params Submodel[] submodels)
    {
        if (action == null) return;

        // set planned status
        try
        {
            action.SetStatus(ActionStatusEnum.PLANNED);
        }
        catch
        {
            // ignore
        }

        foreach (var kv in action.InputParameters.Parameters)
        {
            var key = kv.Key;
            var prop = kv.Value;
            FillParameter(key, prop, action, submodels);
        }
    }

    private void FillParameter(string key, Property property, Action action, params Submodel[] submodels)
    {
        if (property?.Value?.Value == null) return;

        // unwrap BaSyx IValue if present
        object? raw = property.Value.Value;
        if (raw is BaSyx.Models.AdminShell.IValue iv)
        {
            raw = iv.Value;
        }

        // only handle string references or lists of references represented as comma-separated or single path with '/'
        if (raw is string s && (s.Contains(",") || s.Contains("/")))
        {
            string[] locations;
            if (s.Contains(","))
            {
                locations = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
            else
            {
                locations = new[] { s };
            }
            var resolvedValues = new List<object>();

            foreach (var location in locations)
            {
                // expected format: <submodelIdentifier>,<path/to/property>
                var parts = location.Split('/', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string submodelIdent = parts.Length > 0 ? parts[0] : location;
                string path = parts.Length > 1 ? parts[1] : string.Empty;

                // search provided submodels first
                Submodel? found = null;
                if (submodels != null)
                {
                    foreach (var sm in submodels)
                    {
                        if (sm == null) continue;
                        if (string.Equals(sm.IdShort, submodelIdent, StringComparison.OrdinalIgnoreCase))
                        {
                            found = sm; break;
                        }

                        if (sm.SemanticId != null && sm.SemanticId.Keys != null)
                        {
                            foreach (var k in sm.SemanticId.Keys)
                            {
                                if (k?.Value != null && k.Value.Contains(submodelIdent, StringComparison.OrdinalIgnoreCase))
                                {
                                    found = sm; break;
                                }
                            }
                        }

                        if (found != null) break;
                    }
                }

                if (found == null) continue;

                // traverse path within submodel
                ISubmodelElement? current = null;
                if (string.IsNullOrEmpty(path))
                {
                    // no path: use first property found with matching idShort (submodel-level)
                    current = found.SubmodelElements.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, submodelIdent, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var container = (IEnumerable<ISubmodelElement>)found.SubmodelElements;
                    ISubmodelElement? traversed = null;
                    foreach (var seg in segments)
                    {
                        traversed = container.OfType<ISubmodelElement>().FirstOrDefault(e => string.Equals(e.IdShort, seg, StringComparison.OrdinalIgnoreCase));
                        if (traversed == null) break;

                        if (traversed is SubmodelElementCollection col)
                        {
                            container = col;
                        }
                        else
                        {
                            container = Array.Empty<ISubmodelElement>();
                        }
                    }

                    current = traversed;
                }

                if (current is Property targetProp)
                {
                    // extract underlying value
                    var valueObj = targetProp.Value?.Value;
                    if (valueObj is BaSyx.Models.AdminShell.IValue inner)
                    {
                        resolvedValues.Add(inner.Value);
                    }
                    else if (valueObj != null)
                    {
                        resolvedValues.Add(valueObj);
                    }
                }
            }

            if (resolvedValues.Count == 1)
            {
                action.InputParameters.SetParameter(key, resolvedValues[0]);
            }
            else if (resolvedValues.Count > 1)
            {
                action.InputParameters.SetParameter(key, resolvedValues);
            }
        }
    }

    private void SyncStepElements()
    {
        foreach (var step in Steps)
        {
            SubmodelElements.Remove(step);
        }

        foreach (var step in Steps)
        {
            SubmodelElements.Add(step);
        }
    }

    private static int ParseQuantityInformation(JsonElement element)
    {
        if (element.TryGetProperty("value", out var valueArray) && valueArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in valueArray.EnumerateArray())
            {
                if (entry.TryGetProperty("idShort", out var idShort) && idShort.GetString() == "TotalNumberOfPieces")
                {
                    if (entry.TryGetProperty("value", out var value))
                    {
                        string? valueStr = value.GetString();
                        if (!string.IsNullOrEmpty(valueStr) && int.TryParse(valueStr, out int parsed))
                        {
                            return parsed;
                        }
                    }
                }
            }
        }

        return 0;
    }

    private bool ApplyStepTransition(string stepIdShort, Func<Step, bool> transition)
    {
        var step = GetStep(stepIdShort);
        return step is not null && transition(step);
    }

    private bool ApplyActionTransition(string stepIdShort, string actionIdShort, Func<Action, bool> transition)
    {
        var step = GetStep(stepIdShort);
        var action = step?.GetAction(actionIdShort);
        return action is not null && transition(action);
    }
}
