import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from . import common_semantics as sem

# TransitionConditionType
class TransitionConditionType(aas_types.Property):
    def __init__(self, value: str = "", id_short: str = "TransitionConditionType"):
        super().__init__(
            id_short=id_short,
            semantic_id=utils.create_semantic_id(sem.SEM_TRANSITION_CONDITION_TYPE),
            value=value,
            value_type=aas_types.DataTypeDefXSD.STRING
        )

# TransitionConstraintContainer
class TransitionConstraintContainer(aas_types.SubmodelElementCollection):
    def add_element(self, element):
        self.value.append(element)

    def update_element(self, idx: int, new_element):
        self.value[idx] = new_element

    def get_element(self, idx: int):
        return self.value[idx]

    def get_elements(self):
        return self.value[:]

    def clear_elements(self):
        self.value.clear()
    def add_element(self, element):
        self.value.append(element)

    def remove_element(self, idx: int):
        self.value.pop(idx)

    def update_element(self, idx: int, new_element):
        self.value[idx] = new_element

    def get_element(self, idx: int):
        return self.value[idx]

    def clear_elements(self):
        self.value.clear()
    def __init__(self, capability_reference, id_short: str = "TransitionConstraintContainer", constrained_by: 'TransitionConstrainedBy' = None, condition_type: TransitionConditionType = None):
        super().__init__(id_short=id_short, semantic_id=utils.create_semantic_id(sem.SEM_TRANSITION_CONSTRAINT_CONTAINER))
        self.value: list[aas_types.SubmodelElement] = []
        if constrained_by:
            self.value.append(constrained_by)
        if condition_type:
            self.value.append(condition_type)

# TransitionConstrainedBy
class TransitionConstrainedBy(aas_types.RelationshipElement):
    def __init__(self, first: aas_types.Reference, second: aas_types.Reference):
        super().__init__(
            id_short="TransitionConstrainedBy",
            semantic_id=utils.create_semantic_id(sem.SEM_TRANSITION_RELATION),
            first=first,
            second=second
        )

