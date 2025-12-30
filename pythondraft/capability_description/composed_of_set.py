import aas_core3.types as aas_types


class ComposedOfSet(aas_types.SubmodelElementCollection):
    def get_reference(self, index):
        return self.relationships.get(index)

    def clear_references(self):
        self.relationships.clear()
        self.value = [v for v in self.value if isinstance(v, CapabilityComposedOfComment)]
    def __init__(self, capability_reference, composed_references=None, comment=None):
        super().__init__(id_short="ComposedOfSet")
        self.value = []
        self.relationships = []
        self.capability_reference = capability_reference
        self.comment = CapabilityComposedOfComment(comment) if comment else None
        if composed_references:
            for idx, composed_reference in enumerate(composed_references, start=1):
                rel = CapabilityComposedOf(capability_reference, composed_reference, idx)
                self.relationships.append(rel)
                self.value.append(rel)
        if self.comment:
            self.value.append(self.comment)

    def update_reference(self, index, reference):
        self.relationships[index] = CapabilityComposedOf(self.capability_reference, reference, index)

    def add_reference(self, reference) -> int:
        idx = len(self.relationships)
        rel = CapabilityComposedOf(self.capability_reference, reference, idx)
        self.relationships.append(rel)
        self.value.append(rel)
        return idx

    def remove_reference(self, index):
        if len(self.relationships)>index:
            rel = self.relationships.pop(index)
            self.value.pop(index)

    def get_reference(self, index):
        return self.relationships[index]

    def get_references(self):
        return list(self.relationships.values())

    def clear_references(self):
        self.relationships.clear()
        self.value = [v for v in self.value if isinstance(v, CapabilityComposedOfComment)]

class ComposedOfContainer(aas_types.SubmodelElementCollection):
    def __init__(self, capability_reference, composed_reference, comment=None):
        super().__init__(id_short="ComposedOfContainer")
        self.value = []
        rel = CapabilityComposedOf(capability_reference, composed_reference, 1)
        self.value.append(rel)
        if comment:
            self.value.append(CapabilityComposedOfComment(comment))

class CapabilityComposedOf(aas_types.RelationshipElement):
    def __init__(self, capability_reference, composed_reference, index):
        self.index = index
        idx_str = f"{index:03d}"
        id_short = "CapabilityComposedOf_" + idx_str
        super().__init__(first=capability_reference, second=composed_reference, id_short=id_short)

class CapabilityComposedOfComment(aas_types.MultiLanguageProperty):
    def __init__(self, text):
        super().__init__(value={"en": text})