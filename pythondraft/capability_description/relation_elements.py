import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from . import common_semantics as sem

class CapabilityRelations(aas_types.SubmodelElementCollection):
    def __init__(self, relations: list[aas_types.SubmodelElement] | None = None):
        super().__init__(id_short="CapabilityRelations", semantic_id=utils.create_semantic_id(sem.SEM_CAPABILITY_RELATIONS))
        self.value = relations or []

    def add_relation(self, rel: aas_types.RelationshipElement):
        self.value.append(rel)

    def remove_relation(self, id_short: str) -> bool:
        for i,r in enumerate(self.value):
            if isinstance(r, aas_types.RelationshipElement) and r.id_short == id_short:
                del self.value[i]
                return True
        return False

