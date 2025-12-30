import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from . import common_semantics as sem
from abc import ABC, abstractmethod

# Abstract base for all property constraints
class PropertyConstraint(ABC):
    @abstractmethod
    def __init__(self, id_short: str, semantic_id: str):
        pass

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

# OCLConstraint
class OCLConstraint(PropertyConstraint, aas_types.File):
    def __init__(self, path: str):
        super().__init__(self, id_short="OCLConstraint", semantic_id=utils.create_semantic_id(sem.SEM_OCL_FILE), content_type="text/plain",value=path)


# BasicConstraint
class BasicConstraint(PropertyConstraint, aas_types.Property):
    def __init__(self, value: str, qualifier:aas_types.Qualifier):
        aas_types.Property.__init__(self, id_short="BasicConstraint", semantic_id=utils.create_semantic_id(sem.SEM_CONSTRAINT_COLLECTION), value=value, value_type=aas_types.DataTypeDefXSD.STRING, qualifiers=[qualifier])
        self.keytype = aas_types.KeyTypes.PROPERTY

# CustomConstraint
class CustomConstraint(PropertyConstraint):
    def __init__(self, id_short: str = "CustomConstraint",aas_element_type=aas_types.SubmodelElementCollection, value = [], **kwargs):
        aas_element_type.__init__(self, id_short=id_short, semantic_id=utils.create_semantic_id(sem.SEM_CUSTOM_CONSTRAINT))
        self.value = []

# TransitionConstraintContainer
class TransitionConstraintContainer(aas_types.SubmodelElementCollection):
    def __init__(self, id_short: str = "TransitionConstraintContainer"):
        super().__init__(id_short=id_short, semantic_id=utils.create_semantic_id(sem.SEM_TRANSITION_CONSTRAINT_CONTAINER))
        self.value: list[aas_types.SubmodelElement] = []

# TransitionConstrainedBy
class TransitionConstrainedBy(aas_types.RelationshipElement):
    def __init__(self, first: aas_types.Reference, second: aas_types.Reference):
        super().__init__(
            id_short="TransitionConstrainedBy",
            semantic_id=utils.create_semantic_id(sem.SEM_TRANSITION_RELATION),
            first=first,
            second=second
        )

# ConstraintSet bleibt als Container für PropertyConstraintContainer und TransitionConstraintContainer
class PropertyConstraintContainer(aas_types.SubmodelElementCollection):
    def __init__(self, id_short: str = "PropertyConstraintContainer"):
        super().__init__(id_short=id_short, semantic_id=utils.create_semantic_id(sem.SEM_PROPERTY_CONSTRAINT_CONTAINER))
        self.value: list[PropertyConstraint] = []

class ConstraintSet(aas_types.SubmodelElementCollection):
    def __init__(self,
                 property_constraint_containers: list[PropertyConstraintContainer] | None = None,
                 transition_constraint_containers: list[TransitionConstraintContainer] | None = None):
        super().__init__(id_short="ConstraintSet", semantic_id=utils.create_semantic_id(sem.SEM_CONSTRAINT_SET))
        self.value: list[aas_types.SubmodelElement] = []
        if property_constraint_containers:
            self.value.extend(property_constraint_containers)
        else:
            self.value.append(PropertyConstraintContainer())
        if transition_constraint_containers:
            self.value.extend(transition_constraint_containers)
        else:
            self.value.append(TransitionConstraintContainer())

    def list_property_constraint_containers(self):
        return [v for v in self.value if isinstance(v, PropertyConstraintContainer)]

    def list_transition_constraint_containers(self):
        return [v for v in self.value if isinstance(v, TransitionConstraintContainer)]

    def add_property_constraint_container(self) -> PropertyConstraintContainer:
        idx = len(self.list_property_constraint_containers())
        pc = PropertyConstraintContainer(id_short=f"PropertyConstraintContainer{idx:03d}")
        self.value.append(pc)
        return pc

    def add_transition_constraint_container(self) -> TransitionConstraintContainer:
        idx = len(self.list_transition_constraint_containers())
        tc = TransitionConstraintContainer(id_short=f"TransitionConstraintContainer{idx:03d}")
        self.value.append(tc)
        return tc

