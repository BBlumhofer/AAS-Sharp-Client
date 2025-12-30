import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from . import common_semantics as sem
from abc import ABC, abstractmethod
from .property_constraints import  PropertyConstraint, PropertyConstraintContainer, PropertyConstraintEnumType, PropertyConditionalEnumType
from .transition_constraints import TransitionConstrainedBy, TransitionConstraintContainer
from typing import List

class ConstraintSet(aas_types.SubmodelElementCollection):
    def add_property_constraint_container(self, constraint:PropertyConstraint|None = None, constraint_container:PropertyConstraintContainer|None = None, constraint_type:PropertyConstraintEnumType|None = None, conditional_type:PropertyConditionalEnumType|None = None, capability_property_references:List[aas_types.Reference]|None = None):
        """
        Fügt einen PropertyConstraintContainer hinzu. Falls ein Constraint übergeben wird, wird ein Container erzeugt.
        Gibt den Index des hinzugefügten Containers in self.value zurück.
        """
        if constraint is None and constraint_container is None:
            raise ValueError
        if constraint_container is not None:
            container = constraint_container
        else:
            idx = len([v for v in self.value if isinstance(v, PropertyConstraintContainer)])
            container = PropertyConstraintContainer(property_constraint=constraint, id_short=f"PropertyConstraintContainer{idx:03d}", constraint_set_reference=self.constraint_set_reference, constraint_type=constraint_type,conditional_type=conditional_type, capability_property_references=capability_property_references)
        self.property_constraint_containers.append(container)
        self.update_constraint_set()
        return len(self.property_constraint_containers) - 1

    def add_transition_constraint_container(self, obj:TransitionConstraintContainer|TransitionConstrainedBy):
        """
        Fügt einen TransitionConstraintContainer hinzu. Falls ein Constraint übergeben wird, wird ein Container erzeugt.
        Gibt den Index des hinzugefügten Containers in self.value zurück.
        """
        if isinstance(obj, TransitionConstraintContainer):
            container = obj
        else:
            idx = len([v for v in self.value if isinstance(v, TransitionConstraintContainer)])
            container = TransitionConstraintContainer(obj, id_short=f"TransitionConstraintContainer{idx:03d}", capability_reference=self.capability_reference)
        self.transition_constraint_containers.append(container)
        self.update_constraint_set()
        return len(self.transition_constraint_containers) - 1

    def update_property_constraint_container(self, idx: int, new_container):
        self.property_constraint_containers[idx] = new_container
        self.update_constraint_set()

    def get_property_constraint_container(self, idx: int):
        return self.property_constraint_containers[idx]

    def get_property_constraint_containers(self):
        return self.property_constraint_containers

    def clear_property_constraint_containers(self):
        self.property_constraint_containers = []
        self.update_constraint_set()

    def remove_property_constraint_container(self, idx: int):
        self.property_constraint_containers.pop(idx)
        self.update_constraint_set()

    def update_transition_constraint_container(self, idx: int, new_container):
        self.transition_constraint_containers[idx] = new_container
        self.update_constraint_set()

    def get_transition_constraint_container(self, idx: int):
        return self.transition_constraint_containers[idx] 

    def get_transition_constraint_containers(self):
        return self.transition_constraint_containers

    def clear_transition_constraint_containers(self):
        self.transition_constraint_containers = [] 

    def remove_transition_constraint_container(self, idx: int):
        self.transition_constraint_containers.pop(idx)
        self.update_constraint_set()

    def update_constraint_set(self):
        self.value = []
        if self.property_constraint_containers:
            self.value.extend(self.property_constraint_containers)
        if self.transition_constraint_containers:
            self.value.extend(self.transition_constraint_containers)

    def __init__(self, capability_reference,
                 property_constraint_containers: list[PropertyConstraintContainer] | None = None,
                 transition_constraint_containers: list[TransitionConstraintContainer] | None = None):
        super().__init__(id_short="ConstraintSet", semantic_id=utils.create_semantic_id(sem.SEM_CONSTRAINT_SET))
        self.capability_reference = capability_reference
        self.constraint_set_reference = capability_reference
        self.constraint_set_reference.keys = self.constraint_set_reference.keys[:-1]
        self.constraint_set_reference.keys.append(aas_types.Key(aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION,"CapabilityRelations"))
        self.constraint_set_reference.keys.append(aas_types.Key(aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION, "ConstraintSet"))
        self.value = []

        
        
        if property_constraint_containers:
            self.value.extend(property_constraint_containers)
            self.property_constraint_containers = property_constraint_containers
        else:
            self.property_constraint_containers=[]
        if transition_constraint_containers:
            self.value.extend(transition_constraint_containers)
            self.transition_constraint_containers= transition_constraint_containers
        else:
            self.transition_constraint_containers=[]

    def list_property_constraint_containers(self):
        return [v for v in self.value if isinstance(v, PropertyConstraintContainer)]

    def list_transition_constraint_containers(self):
        return [v for v in self.value if isinstance(v, TransitionConstraintContainer)]














