from enum import Enum
import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from . import common_semantics as sem
from abc import ABC, abstractmethod
from typing import List
# Abstract base for all property constraints
class PropertyConstraint(ABC):
    @abstractmethod
    def __init__(self, id_short: str, semantic_id: str):
        self.id_short =id_short
        self.keytype = aas_types.KeyTypes.PROPERTY
        pass

class PropertyConditionalEnumType(Enum):
    PRE = "Pre"
    POST = "Post"
    INVARIANT = "Invariant"
    PREPOST = "PrePost"
    PREINVARIANT = "PreInvariant"
    INVARIANTPOST = "InvariantPost"
    PREPOSTINVARIANT = "PrePostInvariant"



class PropertyConstraintEnumType(Enum):
    OPERATION_CONSTRAINT = "OperationConstraint"
    OCL_CONSTRAINT = "OCLConstraint"
    BASIC_CONSTRAINT = "BasicConstraint"
    CUSTOM_CONSTRAINT = "CustomConstraint"

class PropertyConditionalType(aas_types.Property):
    def __init__(self, conditional_type:PropertyConditionalEnumType):
        super().__init__(value_type=aas_types.DataTypeDefXSD.STRING,
                         id_short="PropertyConditionalType",
                         value=conditional_type.value)
        self.keytype = aas_types.KeyTypes.PROPERTY

class PropertyConstraintType(aas_types.Property):
    def __init__(self, constraint_type:PropertyConstraintEnumType):
        super().__init__(value_type=aas_types.DataTypeDefXSD.STRING,
                         id_short="ConstraintType",
                         value=constraint_type.value)
        self.keytype = aas_types.KeyTypes.PROPERTY


# ConstraintSet bleibt als Container für PropertyConstraintContainer und TransitionConstraintContainer
class PropertyConstraintContainer(aas_types.SubmodelElementCollection):
    def update_constraint(self, new_constraint):
        self.property_constraint= new_constraint

    def get_constraint(self):
        return self.property_constraint


    def __init__(self, property_constraint:PropertyConstraint, constraint_set_reference, id_short: str = "PropertyConstraintContainer", capability_property_references:List[aas_types.Reference]|None=[utils.create_empty_reference()],
                 constraint_type:PropertyConstraintEnumType = PropertyConstraintEnumType.BASIC_CONSTRAINT, conditional_type:PropertyConditionalEnumType = PropertyConditionalEnumType.PREPOSTINVARIANT ):
        super().__init__(id_short=id_short, semantic_id=utils.create_semantic_id(sem.SEM_PROPERTY_CONSTRAINT_CONTAINER))
        self.value = []
        self.reference = constraint_set_reference
        self.reference.keys.append(aas_types.Key(aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION, id_short))
        self.property_constraint = property_constraint
        self.constraint_reference = constraint_set_reference
        self.constraint_reference.keys.append(aas_types.Key(self.property_constraint.keytype, self.property_constraint.id_short))
        
        if capability_property_references is None:
            capability_property_references = [utils.create_empty_reference()]
        self.constraint_property_relations = ConstraintPropertyRelations(property_constraint_reference=self.reference,capability_property_reference=capability_property_references)
        self.property_conidital_type = PropertyConditionalType(conditional_type)
        self.property_constraint_type = PropertyConstraintType(constraint_type)

        self.value.append(self.property_conidital_type)
        self.value.append(self.property_constraint_type)    
        self.value.append(self.property_constraint)
        self.value.append(self.constraint_property_relations)



# ConstraintSet bleibt als Container für PropertyConstraintContainer und TransitionConstraintContainer
class ConstraintPropertyRelations(aas_types.SubmodelElementCollection):
    def __init__(self, property_constraint_reference:aas_types.Reference, capability_property_reference:List[aas_types.Reference], id_short: str = "ConstraintPropertyRelations"):
        super().__init__(id_short=id_short, semantic_id=utils.create_semantic_id(sem.SEM_PROPERTY_CONSTRAINT_CONTAINER))
        self.value=[]
        index = 0
        if isinstance(capability_property_reference, aas_types.Reference):
            capability_property_reference = [capability_property_reference]
        for cap_ref in capability_property_reference:
            index +=1
            idx_str = f"{index:03d}"
            id_short = "ConstraintHasProperty_" + idx_str
            self.value.append(aas_types.RelationshipElement(id_short=id_short, first=property_constraint_reference, second=cap_ref))


# Relationship: ConstraintHasProperty
class ConstraintHasProperty(aas_types.RelationshipElement):
    def __init__(self, first: PropertyConstraint, second: aas_types.Property):
        super().__init__(
            id_short="ConstraintHasProperty",
            semantic_id=utils.create_semantic_id(sem.SEM_CONSTRAINT_PROPERTY_SET),
            first=first,
            second=second
        )

# OperationConstraintRelation
class OperationConstraintRelation(aas_types.ReferenceElement):
    def __init__(self, value: aas_types.Reference):
        super().__init__(
            id_short="OperationConstraintRelation",
            semantic_id=utils.create_semantic_id(sem.SEM_CONSTRAINT_COLLECTION),
            value=value
        )
        self.keytype = aas_types.KeyTypes.REFERENCE_ELEMENT

# OCLConstraint
class OCLConstraint(aas_types.File, PropertyConstraint):
    def __init__(self, path: str):
        super().__init__(self, id_short="OCLConstraint", semantic_id=utils.create_semantic_id(sem.SEM_OCL_FILE), content_type="text/plain",value=path)
        self.keytype = aas_types.KeyTypes.FILE

# BasicConstraint
class BasicConstraint(aas_types.Property, PropertyConstraint):
    def __init__(self, value: str, qualifier: str = ""):
        aas_types.Property.__init__(self, id_short="BasicConstraint", semantic_id=utils.create_semantic_id(sem.SEM_CONSTRAINT_COLLECTION), value=value, value_type=aas_types.DataTypeDefXSD.STRING)
        self.qualifier = qualifier
        self.keytype = aas_types.KeyTypes.PROPERTY

# CustomConstraint
class CustomConstraint(aas_types.SubmodelElement, PropertyConstraint):
    def __init__(self, id_short: str = "CustomConstraint",aas_element_type=aas_types.SubmodelElementCollection, value = [], **kwargs):
        aas_element_type.__init__(self, id_short=id_short, semantic_id=utils.create_semantic_id(sem.SEM_CUSTOM_CONSTRAINT))
        self.value = []
