package submodel.ProcessChain;

import java.net.MalformedURLException;

import org.eclipse.digitaltwin.aas4j.v3.dataformat.core.DeserializationException;
import org.eclipse.digitaltwin.aas4j.v3.dataformat.json.JsonDeserializer;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElement;

import shellWrapper.ShellWrapper;
import shellWrapper.Exceptions.ShellException;
import shellWrapper.Exceptions.ReferableException;
import shellWrapper.Exceptions.SubmodelException;
import shellWrapper.Identifiers.Identification;
import shellWrapper.URL.RepositoryURL;
import slf4j_logger.slf4j_logger;

public class Test2 {
	
	public static void main(String[] args) throws MalformedURLException, ShellException, SubmodelException, ReferableException, DeserializationException, InterruptedException, NoSuchFieldException, SecurityException, IllegalArgumentException, IllegalAccessException {
		
	
		String a = "{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://smartfactory.de/aas/submodel/processchain/requiredcapability#1/0\"}]},\"qualifiers\":[{\"kind\":\"ValueQualifier\",\"type\":\"SequenceID\",\"valueType\":\"xs:int\",\"value\":\"1\"},{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://admin-shell.io/SubmodelTemplates/Cardinality/1/0\"}]},\"kind\":\"TemplateQualifier\",\"type\":\"SMT/Cardinality/1\",\"valueType\":\"xs:string\",\"value\":\"One\",\"valueId\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://example/ide320948322\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"RequiredCapability\"},{\"type\":\"ReferenceElement\",\"value\":\"RequiredCapabilityReference\"}]}},{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://admin-shell.io/SubmodelTemplates/Cardinality/1/0\"}]},\"kind\":\"TemplateQualifier\",\"type\":\"SMT/Cardinality/2\",\"valueType\":\"xs:string\",\"value\":\"OneToMany\",\"valueId\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://example/ide320948322\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"RequiredCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"OfferedCapability\"}]}}],\"value\":[{\"category\":\"PARAMETER\",\"idShort\":\"RequiredCapabilityReference\",\"description\":[{\"language\":\"en-US\",\"text\":\"ReferenceElementobject\"},{\"language\":\"de\",\"text\":\"ReferenceElementElement\"}],\"value\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://smartfactory.de/sm/3d6442d9-2077-46ee-a782-db1fb9e096d7\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"CapabilitySet\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"ManuallyAssemblyContainer\"}]},\"modelType\":\"ReferenceElement\"},{\"idShort\":\"OfferedCapabilities\",\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://smartfactory.de/aas/submodel/processchain/offeredcapabilities#1/0\"}]},\"orderRelevant\":false,\"typeValueListElement\":\"SubmodelElementCollection\",\"value\":[{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://smartfactory.de/aas/submodel/processchain/offeredcapability#1/0\"}]},\"qualifiers\":[{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://admin-shell.io/SubmodelTemplates/Cardinality/1/0\"}]},\"kind\":\"TemplateQualifier\",\"type\":\"SMT/Cardinality/2\",\"valueType\":\"xs:string\",\"value\":\"OneToMany\",\"valueId\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://example/ide320948322\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"RequiredCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"OfferedCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"Skill\"}]}},{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://admin-shell.io/SubmodelTemplates/Cardinality/1/0\"}]},\"kind\":\"TemplateQualifier\",\"type\":\"SMT/Cardinality/1\",\"valueType\":\"xs:string\",\"value\":\"One\",\"valueId\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://example/ide320948322\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"RequiredCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"OfferedCapability\"},{\"type\":\"ReferenceElement\",\"value\":\"OfferedCapabilityReference\"}]}}],\"value\":[{\"category\":\"PARAMETER\",\"idShort\":\"OfferedCapabilityReference\",\"description\":[{\"language\":\"en-US\",\"text\":\"ReferenceElementobject\"},{\"language\":\"de\",\"text\":\"ReferenceElementElement\"}],\"value\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://smartfactory.de/sm/a1d18a9e-6423-45ab-8ece-4ea899bcf048\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"CapabilitySet\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"CheckAttendanceContainer\"}]},\"modelType\":\"ReferenceElement\"},{\"idShort\":\"Skills\",\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://smartfactory.de/aas/submodel/processchain/skills#1/0\"}]},\"orderRelevant\":true,\"typeValueListElement\":\"SubmodelElementCollection\",\"value\":[{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://smartfactory.de/aas/submodel/processchain/skill#1/0\"}]},\"qualifiers\":[{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://admin-shell.io/SubmodelTemplates/Cardinality/1/0\"}]},\"kind\":\"TemplateQualifier\",\"type\":\"SMT/Cardinality/1\",\"valueType\":\"xs:string\",\"value\":\"One\",\"valueId\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://example/ide320948322\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"RequiredCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"OfferedCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"Skill\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"FeasibilityCheckData\"}]}},{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://admin-shell.io/SubmodelTemplates/Cardinality/1/0\"}]},\"kind\":\"TemplateQualifier\",\"type\":\"SMT/Cardinality/2\",\"valueType\":\"xs:string\",\"value\":\"One\",\"valueId\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://example/ide320948322\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"RequiredCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"OfferedCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"Skill\"},{\"type\":\"ReferenceElement\",\"value\":\"SkillReference\"}]}},{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://admin-shell.io/SubmodelTemplates/Cardinality/1/0\"}]},\"kind\":\"TemplateQualifier\",\"type\":\"SMT/Cardinality/3\",\"valueType\":\"xs:string\",\"value\":\"One\",\"valueId\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://example/ide320948322\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"RequiredCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"OfferedCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"Skill\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"Parameters\"}]}}],\"value\":[{\"idShort\":\"FeasibilityCheckData\",\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://smartfactory.de/aas/submodel/processchain/skill/feasibilitycheckdata#1/0\"}]},\"value\":[{\"idShort\":\"ExpectedProcessTime\",\"valueType\":\"xs:integer\",\"value\":\"10\",\"modelType\":\"Property\"},{\"idShort\":\"Price\",\"valueType\":\"xs:double\",\"value\":\"4.0\",\"modelType\":\"Property\"},{\"idShort\":\"CO2Limit\",\"valueType\":\"xs:double\",\"value\":\"3.0\",\"modelType\":\"Property\"},{\"idShort\":\"Success\",\"valueType\":\"xs:string\",\"value\":\"true\",\"modelType\":\"Property\"}],\"modelType\":\"SubmodelElementCollection\"},{\"category\":\"PARAMETER\",\"idShort\":\"SkillReference\",\"description\":[{\"language\":\"en-US\",\"text\":\"ReferenceElementobject\"},{\"language\":\"de\",\"text\":\"ReferenceElementElement\"}],\"value\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://smartfactory.de/sm/e70c0aa2-2a58-43a9-aa3d-cd81bcbedd3b\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"Skill_0001\"}]},\"modelType\":\"ReferenceElement\"},{\"idShort\":\"RequiredInputParameters\",\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://smartfactory.de/aas/submodel/processchain/skill/parameters#1/0\"}]},\"qualifiers\":[{\"semanticId\":{\"type\":\"ExternalReference\",\"keys\":[{\"type\":\"GlobalReference\",\"value\":\"https://admin-shell.io/SubmodelTemplates/Cardinality/1/0\"}]},\"kind\":\"TemplateQualifier\",\"type\":\"SMT/Cardinality/1\",\"valueType\":\"xs:string\",\"value\":\"OneToMany\",\"valueId\":{\"type\":\"ModelReference\",\"keys\":[{\"type\":\"Submodel\",\"value\":\"https://example/ide320948322\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"RequiredCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"OfferedCapability\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"Skill\"},{\"type\":\"SubmodelElementCollection\",\"value\":\"Parameters\"},{\"type\":\"Property\",\"value\":\"Parameter\"}]}}],\"value\":[{\"idShort\":\"Parameter\",\"valueType\":\"xs:string\",\"value\":\"test\",\"modelType\":\"Property\"}],\"modelType\":\"SubmodelElementCollection\"}],\"modelType\":\"SubmodelElementCollection\"}],\"modelType\":\"SubmodelElementList\"}],\"modelType\":\"SubmodelElementCollection\"}],\"modelType\":\"SubmodelElementList\"}],\"modelType\":\"SubmodelElementCollection\"}";

		System.out.println(new RequiredCapability(new JsonDeserializer().read(a, SubmodelElement.class))  );
		
		Thread.sleep(10000000);		
		RepositoryURL url = new RepositoryURL("http://localhost:8081/");
		
		ShellWrapper sf_aas = new ShellWrapper(
				url, new Identification("https://example.com/ids/aas/0013_6160_2132_6068"));
		
		ProcessChain pc = sf_aas.get_submodel(ProcessChain.MODELTYPE, ProcessChain.class);
	
		slf4j_logger.info("Process chain ID: " + pc.get_content().getId());
				
		slf4j_logger.info("--------------------------------------------");

		for(RequiredCapability rc : pc.get_required_capabilities()) {
			slf4j_logger.info("required cap: " +  rc.get_required_capability_ref().getValue().getKeys().get(0).getValue().toString());

			for(OfferedCapability oc : rc.get_offered_capabilities()) {
				slf4j_logger.info("offered cap: " +  oc.get_offered_capability_ref().getValue().getKeys().get(0).getValue().toString());
				
				for(Skill skill : oc.get_skills()) {
					Skill _skill = new Skill(new JsonDeserializer().read(skill.toString(), SubmodelElement.class) );
					_skill.set_feasibility_check_data("aa", "bb");
					oc.update_skill(_skill);
					slf4j_logger.info("skill param " + skill.get_parameters());
					slf4j_logger.info("skill feas " + skill.get_feasibility_check_data());

					
				}
			}
		}
		
		slf4j_logger.info("--------------------------------------------");
		
		for(RequiredCapability rc : pc.get_required_capabilities()) {
			slf4j_logger.info("required cap: " +  rc.get_required_capability_ref().getValue().getKeys().get(0).getValue().toString());

			for(OfferedCapability oc : rc.get_offered_capabilities()) {
				slf4j_logger.info("offered cap: " +  oc.get_offered_capability_ref().getValue().getKeys().get(0).getValue().toString());
				
				for(Skill skill : oc.get_skills()) {
					Skill _skill = new Skill(new JsonDeserializer().read(skill.toString(), SubmodelElement.class) );
					_skill.set_feasibility_check_data("aa", "bb");
					oc.update_skill(_skill);
					slf4j_logger.info("skill param " + skill.get_parameters());
					slf4j_logger.info("skill feas " + skill.get_feasibility_check_data());	
				}
			}
		}
		
	}

}
