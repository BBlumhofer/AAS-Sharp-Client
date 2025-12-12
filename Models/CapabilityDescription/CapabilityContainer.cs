using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BaSyx.Models.AdminShell;
using RangeElement = BaSyx.Models.AdminShell.Range;

namespace AasSharpClient.Models
{
    /// <summary>
    /// Wrapper class for a CapabilityContainer (SubmodelElementCollection) that provides
    /// helper constructors and (de)serialization helpers similar to other model classes.
    /// </summary>
    public class CapabilityContainer : SubmodelElementCollection
    {
        private Capability? _capability;
        private MultiLanguageProperty? _comment;
        private CapabilityRelationsSection? _relations;
        private CapabilityPropertySetSection? _propertySet;

        public CapabilityContainer(string idShort) : base(idShort)
        {
        }

        public CapabilityContainer(SubmodelElementCollection source) : base(source.IdShort)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            SemanticId = source.SemanticId;
            Description = source.Description;
            Qualifiers = source.Qualifiers;

            if (source.Values != null)
            {
                foreach (var child in source.Values)
                {
                    Add(child);
                }
            }
        }

        public Capability? Capability => _capability ??= Values?.OfType<Capability>().FirstOrDefault();

        public MultiLanguageProperty? Comment => _comment ??= Values?.OfType<MultiLanguageProperty>().FirstOrDefault();

        public CapabilityRelationsSection? Relations =>
            _relations ??= CapabilityRelationsSection.TryCreate(
                FindCollection(CapabilityDescriptionSemantics.CapabilityRelations, "Relations"));

        public CapabilityPropertySetSection? PropertySet =>
            _propertySet ??= CapabilityPropertySetSection.TryCreate(
                FindCollection(CapabilityDescriptionSemantics.PropertySet, "PropertySet"));

        public IEnumerable<PropertyConstraintContainerSection> Constraints =>
            Relations?.ConstraintSet?.ConstraintContainers ?? Enumerable.Empty<PropertyConstraintContainerSection>();

        public IReadOnlyDictionary<string, PropertyConstraintContainerSection> ConstraintDictionary =>
            Relations?.ConstraintSet?.ConstraintContainerMap ?? CapabilityConstraintSetSection.EmptyConstraintMap;

        public IEnumerable<RelationshipElement> RealizedBy => Relations?.RealizedBy ?? Enumerable.Empty<RelationshipElement>();

        public IEnumerable<CapabilityPropertyContainerSection> PropertyContainers =>
            PropertySet?.Containers ?? Enumerable.Empty<CapabilityPropertyContainerSection>();

        public IReadOnlyDictionary<string, CapabilityPropertyContainerSection> PropertyContainerDictionary =>
            PropertySet?.ContainerMap ?? CapabilityPropertySetSection.EmptyContainerMap;

        public CapabilityRelationsSection EnsureRelations(string idShort = "Relations")
        {
            if (Relations != null)
            {
                return Relations;
            }

            var relations = CapabilityDescriptionElementFactory.CreateEmptyCollection(
                string.IsNullOrWhiteSpace(idShort) ? "Relations" : idShort,
                CapabilityDescriptionSemantics.CapabilityRelations);

            Add(relations);
            _relations = CapabilityRelationsSection.TryCreate(relations);
            return _relations!;
        }

        public CapabilityPropertySetSection EnsurePropertySet(string idShort = "PropertySet")
        {
            if (PropertySet != null)
            {
                return PropertySet;
            }

            var propertySet = CapabilityDescriptionElementFactory.CreateEmptyCollection(
                string.IsNullOrWhiteSpace(idShort) ? "PropertySet" : idShort,
                CapabilityDescriptionSemantics.PropertySet);

            Add(propertySet);
            _propertySet = CapabilityPropertySetSection.TryCreate(propertySet);
            return _propertySet!;
        }

        public static CapabilityContainer FromSubmodelElement(ISubmodelElement element)
        {
            if (element is SubmodelElementCollection collection)
            {
                return new CapabilityContainer(collection);
            }

            throw new ArgumentException("Element is not a SubmodelElementCollection", nameof(element));
        }

        public static CapabilityContainer FromDefinition(AasSharpClient.Models.CapabilityContainerDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            var smc = CapabilityDescriptionElementFactory.CreateCapabilityContainer(definition);
            return new CapabilityContainer(smc);
        }

        public string ToJson()
        {
            // Serialize this single element using SubmodelSerialization helper
            return SubmodelSerialization.SerializeElements(new[] { this });
        }

        public string GetCapabilityName()
        {
            var capabilityElem = Capability;
            if (!string.IsNullOrWhiteSpace(capabilityElem?.IdShort))
            {
                return capabilityElem!.IdShort!;
            }

            if (!string.IsNullOrWhiteSpace(IdShort))
            {
                const string suffix = "Container";
                if (IdShort!.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return IdShort[..^suffix.Length];
                }

            }

            return string.IsNullOrWhiteSpace(IdShort) ? "Capability" : IdShort;
        }

        private SubmodelElementCollection? FindCollection(Reference semanticId, string? fallbackIdShort = null)
        {
            var match = Values?
                .OfType<SubmodelElementCollection>()
                .FirstOrDefault(collection => CapabilityDescriptionReferenceComparer.Equals(collection.SemanticId, semanticId));

            if (match != null || string.IsNullOrWhiteSpace(fallbackIdShort))
            {
                return match;
            }

            return Values?
                .OfType<SubmodelElementCollection>()
                .FirstOrDefault(collection => string.Equals(collection.IdShort, fallbackIdShort, StringComparison.OrdinalIgnoreCase));
        }
    }

    public sealed class CapabilityRelationsSection
    {
        private CapabilityConstraintSetSection? _constraintSet;

        internal CapabilityRelationsSection(SubmodelElementCollection source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public SubmodelElementCollection Source { get; }

        public IEnumerable<RelationshipElement> Relationships =>
            Source.Values?.OfType<RelationshipElement>() ?? Enumerable.Empty<RelationshipElement>();

        public IEnumerable<RelationshipElement> RealizedBy => RelationshipsById("RealizedBy");

        public IEnumerable<RelationshipElement> Requires => RelationshipsById("Requires");

        public IEnumerable<RelationshipElement> Provides => RelationshipsById("Provides");

        public CapabilityConstraintSetSection? ConstraintSet =>
            _constraintSet ??= CapabilityConstraintSetSection.TryCreate(
                CapabilityDescriptionElementLookup.Find(Source, CapabilityDescriptionSemantics.ConstraintSet, "ConstraintSet"));

        public IEnumerable<RelationshipElement> RelationshipsById(string idShort)
        {
            if (string.IsNullOrWhiteSpace(idShort))
            {
                return Enumerable.Empty<RelationshipElement>();
            }

            return Relationships.Where(rel => string.Equals(rel.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
        }

        internal static CapabilityRelationsSection? TryCreate(SubmodelElementCollection? collection)
        {
            return collection == null ? null : new CapabilityRelationsSection(collection);
        }
    }

    public sealed class CapabilityConstraintSetSection
    {
        private IReadOnlyDictionary<string, PropertyConstraintContainerSection>? _constraintMap;

        internal CapabilityConstraintSetSection(SubmodelElementCollection source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public SubmodelElementCollection Source { get; }

        public IEnumerable<PropertyConstraintContainerSection> ConstraintContainers =>
            Source.Values?.OfType<SubmodelElementCollection>()
                .Select(PropertyConstraintContainerSection.TryCreate)
                .Where(section => section != null)
                .Select(section => section!)
            ?? Enumerable.Empty<PropertyConstraintContainerSection>();

        public IReadOnlyDictionary<string, PropertyConstraintContainerSection> ConstraintContainerMap =>
            _constraintMap ??= BuildConstraintMap();

        private IReadOnlyDictionary<string, PropertyConstraintContainerSection> BuildConstraintMap()
        {
            var map = new Dictionary<string, PropertyConstraintContainerSection>(StringComparer.OrdinalIgnoreCase);
            foreach (var constraint in ConstraintContainers)
            {
                if (!string.IsNullOrWhiteSpace(constraint.Source.IdShort))
                {
                    map[constraint.Source.IdShort!] = constraint;
                }
            }

            return map;
        }

        internal static CapabilityConstraintSetSection? TryCreate(SubmodelElementCollection? collection)
        {
            return collection == null ? null : new CapabilityConstraintSetSection(collection);
        }

        internal static IReadOnlyDictionary<string, PropertyConstraintContainerSection> EmptyConstraintMap { get; } =
            new Dictionary<string, PropertyConstraintContainerSection>(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class PropertyConstraintContainerSection
    {
        private CustomConstraintSection? _customConstraint;

        internal PropertyConstraintContainerSection(SubmodelElementCollection source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public SubmodelElementCollection Source { get; }

        public Property? ConditionalType => GetPropertyById("ConditionalType");

        public Property? ConstraintType => GetPropertyById("ConstraintType");

        public CustomConstraintSection? CustomConstraint =>
            _customConstraint ??= CustomConstraintSection.TryCreate(
                CapabilityDescriptionElementLookup.Find(Source, CapabilityDescriptionSemantics.CustomConstraint, "CustomConstraint"));

        public IEnumerable<RelationshipElement> PropertyRelations =>
            Source.Values?
                .OfType<SubmodelElementCollection>()
                .FirstOrDefault(c => string.Equals(c.IdShort, "ConstraintPropertyRelations", StringComparison.OrdinalIgnoreCase))?
                .OfType<RelationshipElement>()
            ?? Enumerable.Empty<RelationshipElement>();

        private Property? GetPropertyById(string idShort)
        {
            return Source.Values?.OfType<Property>()
                .FirstOrDefault(prop => string.Equals(prop.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
        }

        internal static PropertyConstraintContainerSection? TryCreate(SubmodelElementCollection? collection)
        {
            return collection == null ? null : new PropertyConstraintContainerSection(collection);
        }
    }

    public sealed class CustomConstraintSection
    {
        internal CustomConstraintSection(SubmodelElementCollection source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public SubmodelElementCollection Source { get; }

        public IEnumerable<Property> Properties => Source.Values?.OfType<Property>() ?? Enumerable.Empty<Property>();

        public Property? GetProperty(string idShort)
        {
            if (string.IsNullOrWhiteSpace(idShort))
            {
                return null;
            }

            return Properties.FirstOrDefault(prop => string.Equals(prop.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
        }

        internal static CustomConstraintSection? TryCreate(SubmodelElementCollection? collection)
        {
            return collection == null ? null : new CustomConstraintSection(collection);
        }
    }

    public sealed class CapabilityPropertySetSection
    {
        private IReadOnlyDictionary<string, CapabilityPropertyContainerSection>? _containerMap;

        internal CapabilityPropertySetSection(SubmodelElementCollection source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public SubmodelElementCollection Source { get; }

        public IEnumerable<CapabilityPropertyContainerSection> Containers =>
            Source.Values?.OfType<SubmodelElementCollection>()
                .Select(CapabilityPropertyContainerSection.TryCreate)
                .Where(container => container != null)
                .Select(container => container!)
            ?? Enumerable.Empty<CapabilityPropertyContainerSection>();

        public CapabilityPropertyContainerSection? GetContainer(string idShort)
        {
            if (string.IsNullOrWhiteSpace(idShort))
            {
                return null;
            }

            return Containers.FirstOrDefault(container => string.Equals(container.Source.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyDictionary<string, CapabilityPropertyContainerSection> ContainerMap =>
            _containerMap ??= BuildContainerMap();

        private IReadOnlyDictionary<string, CapabilityPropertyContainerSection> BuildContainerMap()
        {
            var map = new Dictionary<string, CapabilityPropertyContainerSection>(StringComparer.OrdinalIgnoreCase);
            foreach (var container in Containers)
            {
                if (!string.IsNullOrWhiteSpace(container.Source.IdShort))
                {
                    map[container.Source.IdShort!] = container;
                }
            }

            return map;
        }

        internal static CapabilityPropertySetSection? TryCreate(SubmodelElementCollection? collection)
        {
            return collection == null ? null : new CapabilityPropertySetSection(collection);
        }

        internal static IReadOnlyDictionary<string, CapabilityPropertyContainerSection> EmptyContainerMap { get; } =
            new Dictionary<string, CapabilityPropertyContainerSection>(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class CapabilityPropertyContainerSection
    {
        internal CapabilityPropertyContainerSection(SubmodelElementCollection source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public SubmodelElementCollection Source { get; }

        public MultiLanguageProperty? Comment => Source.Values?.OfType<MultiLanguageProperty>().FirstOrDefault();

        public RangeElement? Range => Source.Values?.OfType<RangeElement>().FirstOrDefault();

        public Property? Property => Source.Values?.OfType<Property>().FirstOrDefault();

        public SubmodelElementList? PropertyList => Source.Values?.OfType<SubmodelElementList>().FirstOrDefault();

        public string? FixedValue => Property?.Value?.Value?.ToString();

        internal static CapabilityPropertyContainerSection? TryCreate(SubmodelElementCollection? collection)
        {
            return collection == null ? null : new CapabilityPropertyContainerSection(collection);
        }
    }

    internal static class CapabilityDescriptionReferenceComparer
    {
        public static bool Equals(Reference? left, Reference? right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            var leftKeys = left.Keys?.ToList();
            var rightKeys = right.Keys?.ToList();

            if (leftKeys == null || rightKeys == null || leftKeys.Count != rightKeys.Count)
            {
                return false;
            }

            for (var i = 0; i < leftKeys.Count; i++)
            {
                var leftKey = leftKeys[i];
                var rightKey = rightKeys[i];

                if (leftKey.Type != rightKey.Type)
                {
                    return false;
                }

                if (!string.Equals(leftKey.Value, rightKey.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal static class CapabilityDescriptionElementLookup
    {
        public static SubmodelElementCollection? Find(SubmodelElementCollection? parent, Reference semanticId, string fallbackIdShort)
        {
            if (parent == null)
            {
                return null;
            }

            var match = parent.Values?
                .OfType<SubmodelElementCollection>()
                .FirstOrDefault(collection => CapabilityDescriptionReferenceComparer.Equals(collection.SemanticId, semanticId));

            if (match != null)
            {
                return match;
            }

            if (string.IsNullOrWhiteSpace(fallbackIdShort))
            {
                return null;
            }

            return parent.Values?
                .OfType<SubmodelElementCollection>()
                .FirstOrDefault(collection => string.Equals(collection.IdShort, fallbackIdShort, StringComparison.OrdinalIgnoreCase));
        }
    }
}
