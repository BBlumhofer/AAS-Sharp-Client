using System.Collections.Generic;

namespace AasSharpClient.Messages;

/// <summary>
/// Message DTO for process chain proposals exchanged via messaging.
/// </summary>
public class ProcessChainProposal
{
    public string ProcessChainId { get; set; } = string.Empty;
    public List<ProcessChainStep> Steps { get; set; } = new();
}

public class ProcessChainStep
{
    public string Capability { get; set; } = string.Empty;
    public List<string> CandidateModules { get; set; } = new();
}
