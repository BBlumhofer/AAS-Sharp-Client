# Semantic ID constants for CapabilityDescription Submodel (pattern: hierarchical path + #1/0)
BASE = "https://smartfactory.de/aas/submodel/CapabilityDescription"

# Top-level
SEM_CAPABILITY_DESCRIPTION = f"{BASE}#1/0"
SEM_CAPABILITY_SET = f"{BASE}/CapabilitySet#1/0"
SEM_CAPABILITY_CONTAINER = f"{BASE}/CapabilitySet/CapabilityContainer#1/0"  # generic container semantic

# Direktes Capability Element (kein Wrapper mehr)
SEM_CAPABILITY = f"{BASE}/Capability#1/0"

# PropertySet (enthält PropertyContainer*)
SEM_PROPERTY_SET = f"{BASE}/CapabilitySet/CapabilityContainer/PropertySet#1/0"
# PropertyContainer selbst erhält dynamische Semantik: {BASE}/CapabilitySet/CapabilityContainer/PropertySet/PropertyContainer/<PropertyName>#1/0
# Einzel-Property generisch (Fallback)
SEM_COMMENT = f"{BASE}/CapabilitySet/CapabilityContainer/Comment#1/0"
SEM_PROPERTY_GENERIC = f"{BASE}/CapabilitySet/CapabilityContainer/PropertySet/Property#1/0"

# CapabilityRelations root
SEM_CAPABILITY_RELATIONS = f"{BASE}/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0"

# ConstraintSet
SEM_CONSTRAINT_SET = f"{SEM_CAPABILITY_RELATIONS}/ConstraintSet#1/0"
SEM_PROPERTY_CONSTRAINT_CONTAINER = f"{SEM_CONSTRAINT_SET}/PropertyConstraintContainer#1/0"
SEM_TRANSITION_CONSTRAINT_CONTAINER = f"{SEM_CONSTRAINT_SET}/TransitionConstraintContainer#1/0"
SEM_TRANSITION_CONDITION_TYPE = f"{SEM_CONSTRAINT_SET}/TransitionConstraintContainer/TransitionConditionType#1/0"
SEM_CUSTOM_CONSTRAINT = f"{SEM_PROPERTY_CONSTRAINT_CONTAINER}/CustomConstraint#1/0"
SEM_CUSTOM_CONSTRAINT_NAME = f"{SEM_CUSTOM_CONSTRAINT}/ConstraintName#1/0"
SEM_CUSTOM_CONSTRAINT = f"{SEM_CUSTOM_CONSTRAINT}/ConstraintValue#1/0"
SEM_CUSTOM_CONSTRAINT_STORAGE = f"{SEM_CUSTOM_CONSTRAINT}/SotrageConstraint#1/0"
SEM_STORAGE_CONSTRAINT_PRODUCT_ID = f"{SEM_CUSTOM_CONSTRAINT_STORAGE}/ProductID#1/0"
SEM_STORAGE_CONSTRAINT_PRODUCT_TYPE = f"{SEM_CUSTOM_CONSTRAINT_STORAGE}/ProductType#1/0"
# Property Constraint Unterstrukturen
SEM_CONSTRAINT_PROPERTY_SET = f"{SEM_PROPERTY_CONSTRAINT_CONTAINER}/ConstraintPropertySet#1/0"
SEM_CONSTRAINT_COLLECTION = f"{SEM_PROPERTY_CONSTRAINT_CONTAINER}/Constraint#1/0"
SEM_CONSTRAINT_TYPE = f"{SEM_PROPERTY_CONSTRAINT_CONTAINER}/ConstraintTypeProperty#1/0"
SEM_TRANSITION_CONDITIONAL_TYPE = f"{SEM_TRANSITION_CONSTRAINT_CONTAINER}/TransitionConditionalType#1/0"
SEM_TRANSITION_RELATION = f"{SEM_TRANSITION_CONSTRAINT_CONTAINER}/CapabilityTransitionRel#1/0"

# Generische Fallbacks
SEM_GENERIC_REL = f"{SEM_CAPABILITY_RELATIONS}/Relationship#1/0"
SEM_OCL_FILE = f"{SEM_CONSTRAINT_COLLECTION}/OCLFile#1/0"

