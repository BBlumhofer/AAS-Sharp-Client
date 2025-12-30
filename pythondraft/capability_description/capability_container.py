from typing import List
import aas_core3.types as aas_types
from aaspyclasses.submodels.capability_description.property_constraints import PropertyConditionalEnumType, PropertyConstraintEnumType
import aaspyclasses.utils as utils
from . import common_semantics as sem
from .elements_capability import CapabilityElement
from .property_elements import ListPropertyContainerElement, PropertySet
from .capability_relations import CapabilityRelations
from .constraint_set import ConstraintSet
from typing import TYPE_CHECKING, Dict
import copy


if TYPE_CHECKING:
    from .constraint_set import PropertyConstraint, PropertyConstraintContainer

class CapabilityComment(aas_types.MultiLanguageProperty):
    def __init__(self,value:Dict[str,str] = {"en":"Comment"}):
        language_elements = []
        for element in value:
            language_elements.append(aas_types.LangStringNameType(element,value[element]))
        super().__init__(id_short="Comment", display_name="Comment", semantic_id=utils.create_semantic_id(sem.SEM_COMMENT), value = language_elements)

class CapabilityReference(aas_types.Reference):
    def __init__(self, submodel_id, capability_id_short):
        type = aas_types.ReferenceTypes.MODEL_REFERENCE
        keytype_list = [aas_types.KeyTypes.SUBMODEL,aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION,aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION,aas_types.KeyTypes.CAPABILITY]
        key_values= [submodel_id, "CapabilitySet", capability_id_short+"Container",capability_id_short]
        keys = utils.create_keys_from_lists(keytype_list,key_values)
        super().__init__(type, keys)


class CapabilityContainer(aas_types.SubmodelElementCollection):
    @staticmethod
    def create_property_container(property_name: str, value, comment=None):
        from .property_elements import PropertyContainer, PropertyComment
        container = PropertyContainer(container_index=0, property_id_short=property_name, value=value)
        if comment:
            container.value.append(comment)
        return container
    
    
    def __init__(self,
                 capability_id_short: str,
                 submodel_id: str,
                 property_set: PropertySet | None = None,
                 relations: CapabilityRelations | None = None,
                 constraint_set: ConstraintSet | None = None,
                 comment: aas_types.MultiLanguageProperty | None = None,
                 ):
        super().__init__(id_short=f"{capability_id_short}Container", semantic_id=utils.create_semantic_id(sem.SEM_CAPABILITY_CONTAINER))
        self.capability = CapabilityElement(capability_id_short)
        self.capability_reference = CapabilityReference(submodel_id, capability_id_short)
        self.property_set = property_set or PropertySet()
        self.relations = relations or CapabilityRelations(capability_reference = copy.deepcopy(self.capability_reference))
        parts: list[aas_types.SubmodelElement] = [self.capability]
        if comment:
            parts.append(comment)
        if self.relations:
            parts.append(self.relations)
        if self.property_set:
            parts.append(self.property_set)
        if constraint_set:
            parts.append(constraint_set)
        self.value = parts

    def get_comment(self) -> 'CapabilityComment | None':
        for el in self.value:
            if isinstance(el, aas_types.MultiLanguageProperty) and getattr(el, 'id_short', None) == 'Comment':
                return el
        return None

    def set_comment(self, comment: 'CapabilityComment') -> None:
        found = False
        for i, el in enumerate(self.value):
            if isinstance(el, aas_types.MultiLanguageProperty) and getattr(el, 'id_short', None) == 'Comment':
                self.value[i] = comment
                found = True
        if not found:
            self.value.append(comment)

    def get_capability_relations(self) -> CapabilityRelations:
        for el in self.value:
            if isinstance(el, CapabilityRelations):
                return el
        return None

    def update_capability_relations(self, capability_relations: CapabilityRelations) -> None:
        for i, el in enumerate(self.value):
            if isinstance(el, CapabilityRelations):
                self.value[i] = capability_relations
                return
        self.value.append(capability_relations)

    def get_capability(self) -> CapabilityElement:
        for el in self.value:
            if isinstance(el, CapabilityElement):
                return el
        return None

    def get_property_set(self) -> PropertySet:
        for el in self.value:
            if isinstance(el, PropertySet):
                return el
        return None

    def get_property_container(self, id_short: str):
        pset = self.get_property_set()
        if pset:
            return pset.get_container(id_short)
        return None

    def add_property_constraint_container(self, constraint:"PropertyConstraint|None" = None, constraint_container:"PropertyConstraintContainer|None" = None, 
                                          constraint_type:PropertyConstraintEnumType|None = None, conditional_type:PropertyConditionalEnumType|None = None,capability_property_references:List[aas_types.Reference]|None=None ):
        index = self.relations.constraint_set.add_property_constraint_container(constraint=constraint, constraint_container=constraint_container,
                                                                                constraint_type=constraint_type, conditional_type=conditional_type, capability_property_references=capability_property_references)
        return index

    def add_transition_constraint_container(self, transition_constraint):
        self.relations.constraint_set.add_property_constraint_container(transition_constraint=transition_constraint)


    def get_property_reference(self,idx:int):
        property_reference = copy.deepcopy(self.capability_reference)
        property_reference.keys = property_reference.keys[:-1]
        index_str= f"{idx:03d}"
        containter_string = "PropertyContainer" + index_str
        container = self.property_set.value[idx-1]
        for element in container.value: 
            if element.id_short !="ConstraintPropertyRelations":
                property_object= element
                break

        property_reference.keys.append(aas_types.Key(aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION, "PropertySet"))
        property_reference.keys.append(aas_types.Key(aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION, containter_string))
        property_reference.keys.append(aas_types.Key(property_object.keytype, property_object.id_short))
        return property_reference

    def update_skill_reference(self,skill_sm_id:str, skill_idx:int):
        self.relations.update_skill_reference(skill_sm_id,skill_idx)

    def update_capability_reference(self,submodel_id):
        self.capability_reference.keys[0].value = submodel_id

class CapabilitySet(aas_types.SubmodelElementCollection):
    def __init__(self, containers: list[CapabilityContainer] | None = None):
        super().__init__(id_short="CapabilitySet", semantic_id=utils.create_semantic_id(sem.SEM_CAPABILITY_SET))
        self.value = containers or []

    def add_container(self, container: CapabilityContainer):
        self.value.append(container)
        index = len(self.value) -1
        return index,container.id_short 

    def get_container(self, idx: int|None=None, id_short:str|None=None) -> CapabilityContainer | None:
        if idx is None and id_short is None:
            raise ValueError("Invalid Arguments")
        if id_short is not None: 
            for c in self.value:
                if c.id_short == id_short:
                    return c
        if idx is not None:
            return self.value[idx]
        return None

    def remove_container(self, idx: int|None=None, id_short:str|None=None) -> bool:
        if idx is None and id_short is None:
            raise ValueError("Invalid Arguments")
        if id_short is not None:
            for i, c in enumerate(self.value):
                if c.id_short == id_short:
                    self.value.pop(i)
                    return True
            return False
        if idx is not None:
            if 0 <= idx < len(self.value):
                self.value.pop(idx)
                return True
            return False


    def update_container(self, container: CapabilityContainer, idx: int|None=None, id_short:str|None=None):
        if idx is None and id_short is None:
            raise ValueError("Invalid Arguments")
        if id_short is not None: 
            for c in self.value:
                if c.id_short == id_short:
                    c = container
        if idx is not None:
            self.value[idx] = container

    def clear(self):
        self.value=[]
