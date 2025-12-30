from unittest.mock import Base
import aas_core3.types as aas_types
import aaspyclasses.utils as utils
from . import common_semantics as sem
from aaspyclasses.submodels.capability_description._constraint_elements import PropertyConstraint, CustomConstraint
from aaspyclasses.utils.exceptions import ValueMissing


# CustomConstraint
class StorageConstraint(aas_types.SubmodelElementCollection, CustomConstraint):
    def __init__(self, product_type="", product_id=""):
        if product_type == "" and product_id == "":
            raise ValueMissing("ProductType and ProductID")
        CustomConstraint.__init__(self, id_short="CustomConstraint", semantic_id=utils.create_semantic_id(sem.SEM_CUSTOM_CONSTRAINT_STORAGE))
        self.constraint_name = ConstraintName("StorageConstraint")
        self.product_id = None
        self.product_type = None
        self.keytype = aas_types.KeyTypes.SUBMODEL_ELEMENT_COLLECTION
        self.value = [self.constraint_name]
        if product_id != "":
            self.product_id = ProductID(product_id)
            self.value.append(self.product_id)
        if product_type != "":
            self.product_type = ProductType(product_type)
            self.value.append(self.product_type)

        



class ConstraintName(aas_types.Property):
    def __init__(self, value: str = ""):
        super().__init__(
            id_short="ConstraintName",
            semantic_id=utils.create_semantic_id(sem.SEM_CUSTOM_CONSTRAINT_STORAGE),
            value=value,
            value_type=aas_types.DataTypeDefXSD.STRING,
        )

class ProductID(aas_types.Property):
    def __init__(self, value: str = ""):
        super().__init__(
            id_short="ProductID",
            semantic_id=utils.create_semantic_id(sem.SEM_STORAGE_CONSTRAINT_PRODUCT_ID),
            value=value,
            value_type=aas_types.DataTypeDefXSD.STRING,
        )

class ProductType(aas_types.Property):
    def __init__(self, value: str = ""):
        super().__init__(
            id_short="ProductType",
            semantic_id=utils.create_semantic_id(sem.SEM_STORAGE_CONSTRAINT_PRODUCT_TYPE),
            value=value,
            value_type=aas_types.DataTypeDefXSD.STRING,
        )