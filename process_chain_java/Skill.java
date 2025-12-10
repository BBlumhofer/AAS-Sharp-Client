package submodel.ProcessChain;

import java.io.IOException;
import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.eclipse.digitaltwin.aas4j.v3.dataformat.core.DeserializationException;
import org.eclipse.digitaltwin.aas4j.v3.model.Key;
import org.eclipse.digitaltwin.aas4j.v3.model.KeyTypes;
import org.eclipse.digitaltwin.aas4j.v3.model.Referable;
import org.eclipse.digitaltwin.aas4j.v3.model.Reference;
import org.eclipse.digitaltwin.aas4j.v3.model.ReferenceElement;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElement;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElementCollection;
import org.eclipse.digitaltwin.aas4j.v3.model.impl.DefaultReferenceElement;

import referable.KeyValueContainer.KeyValueContainer;
import browser.Browser;
import browser.ReferenceElementAccessor;
import shellWrapper.ReferableWrapper;
import shellWrapper.Exceptions.ReferableException;
import shellWrapper.Exceptions.SubmodelException;
import shellWrapper.Identifiers.Identification;
import shellWrapper.URL.AAS_URL;
import shellWrapperFactory.ShellMetamodelFactory;

public class Skill extends ReferableWrapper<SubmodelElementCollection>{
	
	public static final String MODELTYPE ="Skills";
	
	public static final String FEASIBILITY_CHECK_DATA = "FeasibilityCheckData";
	public static final String SKILL_REFERENCE = "SkillReference";
	public static final String PARAMETERS = "RequiredInputParameters";
	public static final String EXPECTED_TIME = "ExpectedProcessTime";
	
	private ReferenceElement skill_ref;
	private KeyValueContainer feasibility_check_data;
	private KeyValueContainer parameter_set;
	
	public Skill(Referable skills) throws ReferableException, DeserializationException {
		super(skills, "");
		this.init();
	}
	
	public Skill(Reference skill) throws ReferableException, DeserializationException, IOException {
		super(FEASIBILITY_CHECK_DATA);
		this.init();
		ReferenceElement ref = this.get_skill_reference();
		ref.getValue().setKeys(skill.getKeys());
	}
	
	private void init() {
		this.skill_ref = Browser.browse_by_idShort(this.content, SKILL_REFERENCE, ReferenceElement.class).orElse(new DefaultReferenceElement());
		this.feasibility_check_data = new KeyValueContainer(
				Browser.browse_layered(this.content, Skill.FEASIBILITY_CHECK_DATA, SubmodelElement.class),
				Skill.FEASIBILITY_CHECK_DATA,
				this.content);
		this.parameter_set = new KeyValueContainer(
				Browser.browse_layered(this.content, Skill.PARAMETERS, SubmodelElement.class),
				Skill.PARAMETERS,
				this.content);
	}
	
	public ReferenceElement get_skill_reference() {
		return this.skill_ref;
	}
	
	public void set_skill_reference(Identification sm_id, String skill_idShort) {
		List<Key> _keys = new ArrayList<Key>();
		_keys.add(ShellMetamodelFactory.create_key(KeyTypes.SUBMODEL, sm_id.get_id()));
		_keys.add(ShellMetamodelFactory.create_key(KeyTypes.SUBMODEL_ELEMENT_COLLECTION, skill_idShort));
		this.get_skill_reference().getValue().setKeys(_keys);
	}
	
	public submodel.Skills.Skill get_skill(AAS_URL url) throws MalformedURLException, DeserializationException, SubmodelException, ReferableException {
		return ReferenceElementAccessor.get_smartfactory_element_by_ref(this.get_skill_reference(), url, submodel.Skills.Skill.class);
	}
	
	public submodel.Skills.Skill get_skill(List<AAS_URL> urls) throws MalformedURLException, DeserializationException, SubmodelException, ReferableException {
		return ReferenceElementAccessor.get_smartfactory_element_by_ref(this.get_skill_reference(), urls, submodel.Skills.Skill.class);
	}
	
	public Map<String,Object> get_parameters() {
		return this.parameter_set.get_params();
	}
	
	public void set_parmeter_set(String key, String value) {
		this.parameter_set.set_param(key, value);
	}
	
	public Map<String,Object> get_feasibility_check_data() {
		return this.feasibility_check_data.get_params();
	}
	
	public void set_feasibility_check_data(String key, String value) {
		this.feasibility_check_data.set_param(key, value);
	}
}
