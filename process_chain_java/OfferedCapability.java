package submodel.ProcessChain;

import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.List;

import org.eclipse.digitaltwin.aas4j.v3.dataformat.core.DeserializationException;
import org.eclipse.digitaltwin.aas4j.v3.model.Property;
import org.eclipse.digitaltwin.aas4j.v3.model.Referable;
import org.eclipse.digitaltwin.aas4j.v3.model.ReferenceElement;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElement;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElementCollection;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElementList;
import org.eclipse.digitaltwin.aas4j.v3.model.impl.DefaultReferenceElement;

import shellWrapperFactory.ShellMetamodelFactory;
import browser.Browser;
import browser.ReferenceElementAccessor;
import shellWrapper.ReferableWrapper;
import shellWrapper.Exceptions.ReferableException;
import shellWrapper.Exceptions.SubmodelException;
import shellWrapper.URL.AAS_URL;
import slf4j_logger.slf4j_logger;

public class OfferedCapability extends ReferableWrapper<SubmodelElementCollection> {

	public static final String MODELTYPE = "OfferedCapability";
	public static final String OFFERED_CAPABILITY_REFERENCE = "OfferedCapabilityReference";
	public static final String INSTANCE_IDENTIFIER = "InstanceIdentifier";
	public static final String MATCHING_SCORE = "MatchingScore";
	public static final String SKILLS = "Skills";
	public static final String STATION = "Station";

	private ReferenceElement offered_cap_ref;
	private Property matching_score;
	private Property instance_identifier;
	private Property station;
	private List<Skill> skills = new ArrayList<Skill>();
	private SubmodelElementList _skills;

	public OfferedCapability(Referable off_cap) throws ReferableException, DeserializationException {
		super(off_cap, "");
		this.init();
	}

	private void init() throws ReferableException, DeserializationException {

		this.offered_cap_ref = Browser
				.browse_by_idShort(this.content, OFFERED_CAPABILITY_REFERENCE, ReferenceElement.class)
				.orElse(new DefaultReferenceElement());
		this._skills = Browser.browse_by_idShort(this.content, SKILLS, SubmodelElementList.class).get();
		this.matching_score = Browser.browse_by_idShort(this.content, MATCHING_SCORE, Property.class).get();
		this.station = Browser.browse_by_idShort(this.content, STATION, Property.class)
				.orElse(ShellMetamodelFactory.create_property(STATION, "None"));
		this.instance_identifier = Browser.browse_by_idShort(this.content, INSTANCE_IDENTIFIER, Property.class).get();

		for (SubmodelElement submodel_element : _skills.getValue()) {
			slf4j_logger.debug("Offered capabilty " + this.content.getIdShort() + " has skills");
			this.skills.add(new Skill(submodel_element));
		}
	}

	public List<Skill> get_skills() {
		return this.skills;
	}

	public Skill get_skill(ReferenceElement ref) {
		for (Skill _skill : this.skills) {
			if (this.compare_refs(_skill.get_skill_reference().getValue(), ref.getValue())) {
				return _skill;
			}
		}

		return null;
	}

	public void update_skill(Skill skill) {
		this.add_or_replace_in_sml(skill, this.skills, this._skills, Skill.SKILL_REFERENCE);
	}

	public Double get_matching_score() {
		return Double.parseDouble(this.matching_score.getValue());
	}

	public String get_instance_identifier() {
		return this.instance_identifier.getValue();
	}

	public String get_station() {
		return this.station.getValue();
	}

	public ReferenceElement get_offered_capability_ref() {
		return this.offered_cap_ref;
	}

	public submodel.Capabilities.Capability get_offered_capability(AAS_URL url)
			throws MalformedURLException, DeserializationException, SubmodelException, ReferableException {
		return ReferenceElementAccessor.get_smartfactory_element_by_ref(this.get_offered_capability_ref(), url,
				submodel.Capabilities.Capability.class);
	}

	public submodel.Capabilities.Capability get_offered_capability(List<AAS_URL> urls)
			throws MalformedURLException, DeserializationException, SubmodelException, ReferableException {
		return ReferenceElementAccessor.get_smartfactory_element_by_ref(this.get_offered_capability_ref(), urls,
				submodel.Capabilities.Capability.class);
	}
}
