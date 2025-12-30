from ctypes import util
import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from aaspyclasses.utils.references import ReferenceTypes
from . import common_semantics as sem
from .generalized_by_set import CapabilityGeneralizedBySet
from .composed_of_set import ComposedOfSet
from .constraint_set import ConstraintSet
import copy

class CapabilityRealizedBy(aas_types.RelationshipElement):
    def __init__(self,capability_reference:aas_types.Reference, idx:int, skill_reference:aas_types.Reference|None= None):
        idx_str = f"{idx:03d}"
        self.first = capability_reference
        if skill_reference is None: 
            self.second = utils.create_empty_reference()
        else:
            self.second = skill_reference
        super().__init__(
            id_short ="CapabilityRealizedBy_"+idx_str,
            first=self.first,
            second = self.second
        )




    def update_skill_reference(self,skill_sm_id:str|None=None, skill_idx:int|None=None,skill_reference:aas_types.Reference|None =None):
        if skill_reference is None and skill_idx is None and skill_sm_id is None:
            raise ValueError
        if skill_reference is not None: 
            self.second = skill_reference
        if skill_sm_id is not None and skill_idx is not None:
            idx_str = f"{skill_idx:03d}"
            keylists = [aas_types.KeyTypes.SUBMODEL,aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION,aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION]
            keyvals = [skill_sm_id,"SkillSet",("Skill_"+idx_str)]
            self.second = utils.create_reference_from_lists(keylists,keyvals, ReferenceTypes.MODEL_REFERENCE)
        else:
            raise ValueError

class CapabilityRelations(aas_types.SubmodelElementCollection):
    def __init__(self, realizedby : CapabilityRealizedBy|None = None, generalized_by_set:CapabilityGeneralizedBySet|None = None,
                 composed_of_set: ComposedOfSet|None = None, constraint_set: ConstraintSet|None = None, capability_reference:str = None
                 ):
        super().__init__(id_short="CapabilityRelations", semantic_id=utils.create_semantic_id(sem.SEM_CAPABILITY_RELATIONS))
        self.value = []
        if realizedby is not None:
            self.realized_by = realizedby
        else:
            self.realized_by = CapabilityRealizedBy(capability_reference=capability_reference, idx = 1)


        if generalized_by_set is not None:
            self.generalized_by_set = generalized_by_set
        else:
            self.generalized_by_set = CapabilityGeneralizedBySet(capability_reference=copy.deepcopy(capability_reference))

        if composed_of_set is not None:
            self.composed_of_set = composed_of_set
        else:
            self.composed_of_set = ComposedOfSet(capability_reference=copy.deepcopy(capability_reference))

        if constraint_set is not None:
            self.constraint_set = constraint_set
        else:
            self.constraint_set = ConstraintSet(capability_reference=copy.deepcopy(capability_reference))
            
        self.value.append(self.realized_by)
        self.value.append(self.generalized_by_set)
        self.value.append(self.composed_of_set)
        self.value.append(self.constraint_set)
        




    def update_skill_reference(self,skill_sm_id:str|None=None, skill_idx:int|None=None,skill_reference:aas_types.Reference|None =None):
        self.realized_by.update_skill_reference(skill_reference=skill_reference, skill_idx=skill_idx, skill_sm_id=skill_sm_id)

