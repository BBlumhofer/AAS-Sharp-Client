package submodel.ProcessChain;

import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.List;

import org.eclipse.digitaltwin.aas4j.v3.dataformat.core.DeserializationException;
import org.eclipse.digitaltwin.aas4j.v3.model.Referable;
import org.eclipse.digitaltwin.aas4j.v3.model.ReferenceElement;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElement;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElementCollection;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElementList;
import org.eclipse.digitaltwin.aas4j.v3.model.impl.DefaultReferenceElement;

import referable.Scheduling.Scheduling;
import browser.Browser;
import browser.ReferenceElementAccessor;
import shellWrapper.ReferableWrapper;
import shellWrapper.Exceptions.ReferableException;
import shellWrapper.Exceptions.SubmodelException;
import shellWrapper.URL.AAS_URL;
import slf4j_logger.slf4j_logger;

public class RequiredCapability extends ReferableWrapper<SubmodelElementCollection> {
	// Optionales Element InitialPlannedScheduling
	private referable.Scheduling.Scheduling initialPlannedScheduling;

	public static final String MODELTYPE = "RequiredCapability";

	public static final String OFFERED_CAPABILITIES = "OfferedCapabilities";
	public static final String REQUIRED_CAPABILITY_REFERENCE = "RequiredCapabilityReference";

	private ReferenceElement required_cap_ref;
	private List<OfferedCapability> offered_capabilities = new ArrayList<OfferedCapability>();
	SubmodelElementList _offered_capabilities;

	public RequiredCapability(Referable req_cap) throws ReferableException, DeserializationException {
		super(req_cap, "");
		this.init();
		// InitialPlannedScheduling optional auslesen
		try {
			java.util.Optional<org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElementCollection> smcOpt = Browser
					.browse_layered(this.content, "InitialPlannedScheduling",
							org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElementCollection.class);
			if (smcOpt.isPresent()) {
				this.initialPlannedScheduling = new Scheduling(smcOpt.get());
			} else {
				this.initialPlannedScheduling = null;
			}
		} catch (Exception e) {
			this.initialPlannedScheduling = null;
		}
	}

	public Scheduling get_initial_planned_scheduling() {
		return this.initialPlannedScheduling;
	}

	public void set_initial_planned_scheduling(Scheduling scheduling) {
		this.initialPlannedScheduling = scheduling;
	}

	private void init() throws ReferableException, DeserializationException {

		this.required_cap_ref = Browser
				.browse_by_idShort(this.content, REQUIRED_CAPABILITY_REFERENCE, ReferenceElement.class)
				.orElse(new DefaultReferenceElement());
		this._offered_capabilities = Browser
				.browse_by_idShort(this.content, OFFERED_CAPABILITIES, SubmodelElementList.class).get();

		for (SubmodelElement submodel_element : this._offered_capabilities.getValue()) {
			slf4j_logger.debug("Required capability " + this.content.getIdShort() + " has offered capability");
			this.offered_capabilities.add(new OfferedCapability(submodel_element));
		}
	}

	public List<OfferedCapability> get_offered_capabilities() {
		return this.offered_capabilities;
	}

	public ReferenceElement get_required_capability_ref() {
		return this.required_cap_ref;
	}

	public submodel.Capabilities.Capability get_required_capability(AAS_URL url)
			throws MalformedURLException, DeserializationException, SubmodelException, ReferableException {
		return ReferenceElementAccessor.get_smartfactory_element_by_ref(this.get_required_capability_ref(), url,
				submodel.Capabilities.Capability.class);
	}

	public submodel.Capabilities.Capability get_required_capability(List<AAS_URL> urls)
			throws MalformedURLException, DeserializationException, SubmodelException, ReferableException {
		return ReferenceElementAccessor.get_smartfactory_element_by_ref(this.get_required_capability_ref(), urls,
				submodel.Capabilities.Capability.class);
	}

	public OfferedCapability get_offered_capability(ReferenceElement ref) {
		for (OfferedCapability _offered_cap : this.offered_capabilities) {
			if (this.compare_refs(_offered_cap.get_offered_capability_ref().getValue(), ref.getValue())) {
				return _offered_cap;
			}
		}

		return null;
	}

	public void update_offered_capability(OfferedCapability offered_cap) {
		this.add_or_replace_in_sml(offered_cap, this.offered_capabilities, this._offered_capabilities,
				OfferedCapability.INSTANCE_IDENTIFIER);
	}

}
