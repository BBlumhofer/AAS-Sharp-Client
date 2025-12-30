import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from aaspyclasses.utils.exceptions import InvalidArguments
from . import common_semantics as sem

class PropertyComment(aas_types.MultiLanguageProperty):
    def __init__(self, texts: list[tuple[str,str]] | None = None):
        texts = texts or [("en", " ")]
        super().__init__(
            id_short="PropertyComment",
            value=[aas_types.LangStringTextType(language=lang, text=txt) for lang, txt in texts]
        )

class SamePropertyRel(aas_types.RelationshipElement):
    def __init__(self, first: aas_types.Reference | None = None, second: aas_types.Reference | None = None):
        super().__init__(
            id_short="SameProperty",
            semantic_id=utils.create_semantic_id(sem.SEM_GENERIC_REL),
            first=first or aas_types.Reference(type=aas_types.ReferenceTypes.MODEL_REFERENCE, keys=[]),
            second=second or aas_types.Reference(type=aas_types.ReferenceTypes.MODEL_REFERENCE, keys=[])
        )

class GenericCapabilityProperty(aas_types.Property):
    def __init__(self, id_short: str, value: str = "", value_type: aas_types.DataTypeDefXSD = aas_types.DataTypeDefXSD.STRING, semantic: str | None = None):
        super().__init__(
            id_short=id_short,
            semantic_id=utils.create_semantic_id(semantic or sem.SEM_PROPERTY_GENERIC),
            value=value,
            value_type=value_type
        )

class RangePropertyContainerElement(aas_types.Range):
    def __init__(self,id_short, min=None, max_val=None):
        if min is not None:
            datatype = utils.datatypes.get_datatype_from_value(min)
        else:
            datatype = utils.datatypes.get_datatype_from_value(max_val)

        super().__init__(value_type=datatype, id_short=id_short, min=min, max=max_val)
        self.keytype = aas_types.KeyTypes.RANGE
        

class ValuePropertyContainerElement(aas_types.Property):
    def __init__(self,id_short, value=None):
        if value is not None:
            value_type = utils.datatypes.get_datatype_from_value(value)
        else:
            value_type = aas_types.DataTypeDefXSD.ANY_URI
        super().__init__(value_type=value_type, id_short=id_short, value=value),
        self.keytype = aas_types.KeyTypes.PROPERTY


        

class ListPropertyContainerElement(aas_types.SubmodelElementList):
    def __init__(self,id_short, values):
        if values is not None:
            datatype: None | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD | aas_types.DataTypeDefXSD = utils.datatypes.get_datatype_from_value(values[0])
        super().__init__(type_value_list_element = aas_types.AASSubmodelElements.PROPERTY,
                         id_short=id_short, value_type_list_element=datatype)
        self.value =[]
        self.keytype = aas_types.KeyTypes.SUBMODEL_ELEMENT_LIST
        for value in values:
            self.value.append(aas_types.Property(value_type=datatype, value=value))
        




class PropertyContainer(aas_types.SubmodelElementCollection):
    def __init__(self, container_index: int, property_id_short: str, value=None, value_list=None, min=None, max=None, same_rel: SamePropertyRel | None = None, comment: PropertyComment | None = None):
        # Prüfe, dass nur ein Typ von Wert gesetzt ist
        arg_count = sum([value is not None, value_list is not None, (min is not None or max is not None)])
        if arg_count != 1:
            raise InvalidArguments("PropertyContainer: Es darf nur einer von value, value_list oder min/max gesetzt werden!")
        semantic_dynamic = f"{sem.BASE}/CapabilitySet/CapabilityContainer/PropertySet/PropertyContainer/{property_id_short}#1/0"
        super().__init__(id_short=f"PropertyContainer{container_index:03d}", semantic_id=utils.create_semantic_id(semantic_dynamic))
        parts = []
        if same_rel:
            parts.append(same_rel)
        if comment:
            parts.append(comment)
        # Erzeuge das passende SubmodelElement
        if value is not None:
            property_element = ValuePropertyContainerElement(property_id_short, value)
        elif value_list is not None:
            property_element = ListPropertyContainerElement(property_id_short, value_list)
        elif min is not None or max is not None:
            property_element = RangePropertyContainerElement(property_id_short, min, max)
        else:
            property_element = None
        if property_element:
            parts.append(property_element)
        self.value = parts

class PropertySet(aas_types.SubmodelElementCollection):
    def add_container(self, container: PropertyContainer):
        self.value.append(container)

    def update_container(self, idx: int, new_container: PropertyContainer):
        self.value[idx] = new_container

    def get_container_by_index(self, idx: int) -> PropertyContainer:
        return self.value[idx]

    def get_containers(self):
        return self.value[:]

    def clear(self):
        self.value.clear()

    def find_container_by_id_short(self, id_short: str):
        return self.get_container(id_short)
    def update_container(self, idx: int, new_container: PropertyContainer):
        self.value[idx] = new_container

    def get_container_by_index(self, idx: int) -> PropertyContainer:
        return self.value[idx]

    def clear(self):
        self.value.clear()

    def find_container_by_id_short(self, id_short: str):
        return self.get_container(id_short)
    def __init__(self, containers: list[PropertyContainer] | None = None):
        super().__init__(id_short="PropertySet", semantic_id=utils.create_semantic_id(sem.SEM_PROPERTY_SET))
        self.value = containers or []

    def add_container(self, container: PropertyContainer):
        self.value.append(container)

    def get_container(self, id_short: str) -> PropertyContainer | None:
        for c in self.value:
            if c.id_short == id_short:
                return c
        return None

    def remove_container(self, id_short: str) -> bool:
        for i,c in enumerate(self.value):
            if c.id_short == id_short:
                del self.value[i]
                return True
        return False
