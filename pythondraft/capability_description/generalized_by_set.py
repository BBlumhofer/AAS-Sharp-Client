import aas_core3.types as aas_types

from typing import TYPE_CHECKING, List

if TYPE_CHECKING:
    from aaspyclasses.submodels.capability_description.capability_container import CapabilityReference

class CapabilityGeneralizedBySet(aas_types.SubmodelElementCollection):
    def __init__(self,capability_reference: 'CapabilityReference', generalized_references: List[aas_types.Reference]|None = None):
        super().__init__(id_short="GeneralizedBySet")
        self.value = []
        self.capability_reference = capability_reference
        if generalized_references is not None:
            index = 0 
            self.relationships =[]
            for generalized_reference in generalized_references: 
                index += 1
                self.relationships.append(CapabilityGeneralizedBy(capability_reference,generalized_reference, index))
                self.value.append(self.relationships[-1])

    def update_reference(self,index, reference: aas_types.Reference):
        self.relationships[index] = CapabilityGeneralizedBy(self.capability_reference,reference, index)

    def add_reference(self,reference: aas_types.Reference)-> int:
        old_len = len(self.relationships)
        index = old_len +1 
        relationship = CapabilityGeneralizedBy(self.capability_reference, reference, index)
        self.relationships.append(relationship)
        index = len(self.relationships)-1
        return index

    def remove_reference(self, index):
        self.relationships[index].pop

    def get_reference(self, index):
        return self.relationships[index]

    def clear_references(self):
        self.relationships.clear()
        self.value.clear()

class CapabilityGeneralizedBy(aas_types.RelationshipElement):
    def __ini__(self,capability_reference,generalized_reference, index:int):
        self.index = index
        idx_str = f"{index:03d}"
        id_short = "CapabilityGeneralizedBy_" + idx_str
        super().__init__(first=capability_reference, second=generalized_reference, id_short=id_short)

