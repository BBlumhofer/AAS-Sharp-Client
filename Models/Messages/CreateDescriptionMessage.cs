using System.Collections.Generic;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages
{
    /// <summary>
    /// Minimal typed representation for CreateDescription responses used in messaging.
    /// Creates a SubmodelElementCollection containing a single `Description_Result` property.
    /// </summary>
    public class CreateDescriptionMessage : SubmodelElementCollection
    {
        public CreateDescriptionMessage(string description) : base("CreateDescriptionResponse")
        {
            Add(SubmodelElementFactory.CreateStringProperty("Description_Result", description));
        }

        public static List<ISubmodelElement> CreateInteractionElements(string description)
        {
            return new List<ISubmodelElement>
            {
                new CreateDescriptionMessage(description)
            };
        }
    }
}
