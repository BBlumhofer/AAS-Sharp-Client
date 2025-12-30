from .property_elements import PropertyContainer
import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from . import common_semantics as sem
from .elements_capability import CapabilityElement
from .property_elements import PropertySet
from .relation_elements import CapabilityRelations
from .constraint_set import ConstraintSet
from typing import Dict




class CapabilityComment(aas_types.MultiLanguageProperty):
    def __init__(self,value:Dict[str,str] = {"en":"Comment"}):
        language_elements = []
        for element in value:
            language_elements.append(aas_types.LangStringNameType(element,value[element]))
        super().__init__(id_short="Comment", display_name="Comment", semantic_id=utils.create_semantic_id(sem.SEM_COMMENT), value = language_elements)


class CapabilityContainer(aas_types.SubmodelElementCollection):
    def add_property_container(self, container: PropertyContainer):
        self.property_set.add_container(container)

    def remove_property_container(self, id_short: str):
        self.property_set.remove_container(id_short)

    def update_property_container(self, id_short: str, new_container: PropertyContainer):
        for i, c in enumerate(self.property_set.value):
            if c.id_short == id_short:
                self.property_set.value[i] = new_container
                break

    def get_property_container(self, id_short: str):
        return self.property_set.get_container(id_short)

    def clear_property_containers(self):
        self.property_set.value.clear()

    def find_property_container_by_id_short(self, id_short: str):
        return self.property_set.get_container(id_short)

    @staticmethod
    def create_property_container(property_name, realized_by_reference=None, value=None, value_list=None, min=None, max=None, comment=None):
        idx = 1  # Index kann dynamisch vergeben werden
        return PropertyContainer(
            container_index=idx,
            property_id_short=property_name,
            value=value,
            value_list=value_list,
            min=min,
            max=max,
            same_rel=None,
            comment=comment
        )
    def __init__(self,
                 capability_id_short: str,
                 property_set: PropertySet | None = None,
                 relations: CapabilityRelations | None = None,
                 constraint_set: ConstraintSet | None = None,
                 comment: aas_types.MultiLanguageProperty | None = None):
        super().__init__(id_short=f"{capability_id_short}Container", semantic_id=utils.create_semantic_id(sem.SEM_CAPABILITY_CONTAINER))
        capability = CapabilityElement(capability_id_short)
        self.property_set = property_set or PropertySet()
        self.relations = relations or CapabilityRelations()
        parts: list[aas_types.SubmodelElement] = [capability]
        if comment:
            parts.append(comment)
        if self.relations:
            parts.append(self.relations)
        if self.property_set:
            parts.append(self.property_set)
        if constraint_set:
            parts.append(constraint_set)
        self.value = parts

    def get_comment()->CapabilityComment:
        pass
    def set_comment()->None:
        pass
    def get_capability_relations() -> CapabilityRelations:
        pass
    def update_capability_relations(capability_relations:CapabilityRelations)->None:
        pass
    def get_capability() -> CapabilityElement:
        pass
    def get_property_set() ->PropertySet:
        pass
    def get_property_container(id_short):
        pass





class CapabilitySet(aas_types.SubmodelElementCollection):
    def __init__(self, containers: list[CapabilityContainer] | None = None):
        super().__init__(id_short="CapabilitySet", semantic_id=utils.create_semantic_id(sem.SEM_CAPABILITY_SET))
        self.value = containers or []

