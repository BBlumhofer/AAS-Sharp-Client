import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from ...base.submodel import AASSubmodel
from . import common_semantics as sem
from .capability_container import CapabilitySet, CapabilityContainer
class CapabilityDescriptionSubmodel(AASSubmodel):

    SEMANTIC_ID = sem.SEM_CAPABILITY_DESCRIPTION

    def __init__(self, id_short="CapabilityDescription", submodel_id: str | None = None, capability_containers: list[CapabilityContainer] | None = None):
        super().__init__(semantic_id_value=self.SEMANTIC_ID, submodel_id=submodel_id)
        self.id_short = id_short
        self.kind = aas_types.ModellingKind.INSTANCE
        self.capability_set = CapabilitySet(capability_containers)
        self.submodel_elements = [self.capability_set]
        self.value = self.submodel_elements

    def add_capability_container(self, container: CapabilityContainer)-> int:
        container.update_capability_reference(self.id)
        index, id_short = self.capability_set.add_container(container)
        return index, id_short
    
    def remove_capability_container(self, idx: int|None=None, id_short:str|None=None):
        self.capability_set.remove_container(idx, id_short)

    def update_capability_container(self, container: CapabilityContainer, idx: int|None=None, id_short:str|None=None):
        container.update_capability_reference(self.id)
        self.capability_set.update_container(container=container,idx=idx,id_short=id_short)

    def get_capability_container(self, idx: int|None=None, id_short:str|None=None):
        return self.capability_set.get_container(idx, id_short)

    def find_capability_container_by_id_short(self, id_short: str):
        return self.capability_set.find_container_by_id_short(id_short)

    def clear_capability_containers(self):
        self.capability_set.clear()

    @staticmethod
    def container(capability_id_short: str) -> CapabilityContainer:
        return CapabilityContainer(capability_id_short=capability_id_short)
