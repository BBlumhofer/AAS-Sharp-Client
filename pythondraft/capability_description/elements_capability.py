import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from . import common_semantics as sem

class CapabilityElement(aas_types.Capability):
    def __init__(self, id_short: str):
        super().__init__(
            id_short=id_short,
            semantic_id=utils.create_semantic_id(sem.SEM_CAPABILITY)
        )
