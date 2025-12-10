package submodel.ProcessChain;

import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.List;

import org.eclipse.digitaltwin.aas4j.v3.dataformat.core.DeserializationException;
import org.eclipse.digitaltwin.aas4j.v3.model.Submodel;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElement;
import org.eclipse.digitaltwin.aas4j.v3.model.SubmodelElementList;

import browser.Browser;
import shellWrapper.Identifiers.Identifier;
import shellWrapper.ShellWrapper;
import shellWrapper.SubmodelWrapper;
import shellWrapper.Exceptions.ReferableException;
import shellWrapper.Exceptions.SubmodelException;
import shellWrapper.Identifiers.SemanticId;
import shellWrapper.URL.AAS_URL;

public class ProcessChain extends SubmodelWrapper {
	
	public static final SemanticId MODELTYPE = new SemanticId(Identifier.PATTERN +"/semantics/submodel/ProcessChain#1/02");
	
	public static final String REQUIRED_CAPABILITIES = "RequiredCapabilities";
	
	private List<RequiredCapability> req_capabilities = new ArrayList<RequiredCapability>();
	
	public ProcessChain() {
		// for serviceLoader
	}
	
	public ProcessChain(AAS_URL url) throws MalformedURLException, SubmodelException, ReferableException, DeserializationException {
		super(url);
		this.init();
	}

	public ProcessChain(Submodel submodel) throws MalformedURLException, ReferableException, DeserializationException {
		super(submodel, ProcessChain.MODELTYPE);
		this.init();
	}

	public ProcessChain(SemanticId submodel_semantic, ShellWrapper aas) throws MalformedURLException, SubmodelException, ReferableException, DeserializationException {
		super(submodel_semantic, aas);
		this.init();
	}

	private void init() throws ReferableException, DeserializationException {
		
		for(SubmodelElement submodel_element : Browser.browse_by_idShort(this.content, REQUIRED_CAPABILITIES, SubmodelElementList.class).get().getValue()) {
			this.req_capabilities.add(new RequiredCapability(submodel_element));
		}
	}
	
	public List<RequiredCapability> get_required_capabilities() throws ReferableException, DeserializationException{
		return this.req_capabilities;
	}
}
