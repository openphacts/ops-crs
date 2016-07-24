/*
Class: JSmol

Wrapper for the chemical structure viewer based on <JSmol at http://wiki.jmol.org/index.php/Jmol_JavaScript_Object ​/> HTML5.
This currently implements the lite version of JSmol

Simple demo application available <here at http://parts.chemspider.com/Example/JSmol>.
*/
(function ($) {
	$.widget("compound.jsmol", $.compound.compoundbase, {

		/*
		Constructor: jsmol
		Convert DIV panel into JSmol viewer.

		Parameters:
		options - Initialization <Options>

		(start code)
		$('#jsmolcontainer').jsmol({
			width: 300,
			height: 300,
			id: 236,
			color: "0xC0C0C0",
			bondWidth: 4,
			zoomScaling: 1.5,
			pinchScaling: 2.0,
			multipleBondSpacing: 2,
			spinRateX: 0.2,
			spinRateY: 0.5,
			spinFPS: 20,
			spin:true
		});
		(end code)

		Type: Options

		Properties:
		width - Width of the widget in pixels. Default value - 300
		height - Height of the widget in pixels. Default value - 300
		id - Compound ID to be displayed
		smiles - SMILES string of the molecule that should be displayed
		mol - string containing contents of mol connection file of the molecule to be displayed (with each new line delimited by \n)
				e.g. mol:""
						+"\n"
						+"\n"
						+"\n  2  1  0  0000  0  0  0  0  0999 V2000"
						+"\n    2.4000   -0.4625    0.0000 C   0  0  0  0  0  0  0  0  0  0  0"
						+"\n    3.7167   -0.4667    0.0000 C   0  0  0  0  0  0  0  0  0  0  0"
						+"\n  2  1  2  0"
						+"\nM  END"
						+"\n"
						+"\n$$$$\n"
		color - set background color of JSmol. Can be specified as decimal [red,green,blue] or hexadecimal [xRRGGBB] triplets - see http://jmol.sourceforge.net/jscolors/ for more details
		bondWidth - set bond width of JSmol
		zoomScaling - set zoom scaling factor of JSmol
		pinchScaling - set pinch scaling factor of JSmol
		multipleBondSpacing - set multiple Bond Spacing of JSmol
		spinRateX - set spin Rate of molecule in JSmol in x direction (if spin is set to true)
		spinRateY - set spin Rate of molecule in JSmol in y direction (if spin is set to true)
		spinFPS - set spin frames per second of molecule in JSmol (if spin is set to true)
		spin - set to true to make molecule spin when displayed, or false if not
		*/
		options: {
			width: 300,
			height: 300,
			id: null,
			smiles: null,
			mol: null,
			url: null,
			color: "white",
			type: 'JSMolLite', // [JMol | JSMol | JSMolLite]
			debug: false
		},

		// Set up the widget
		_create: function () {
			var oThis = this;

			this.element
				.addClass('cs-widget cs-widget-jsmol')
				.width(this.options.width)
				.height(this.options.height);

			if (this.options.debug) {
				$('body').loadScript($.Compounds.store.baseUrl + "/Widgets/3rd/jsmol/js/JSmol.full.lite.nojq.js", function () {
					$('body').loadScript($.Compounds.store.baseUrl + "/Widgets/3rd/jsmol/js/JSmol.nojq.js", function () {
						oThis._refresh();
					});
				});
			}
			else {
				$('body').loadScript($.Compounds.store.baseUrl + "/Widgets/3rd/jsmol/JSmol.lite.nojq.js", function () {
					$('body').loadScript($.Compounds.store.baseUrl + "/Widgets/3rd/jsmol/JSmol.min.nojq.js", function () {
						oThis._refresh();
					});
				});
			}
		},

		// called when created, and later when changing options
		_refresh: function () {
			var oThis = this;
			//var Info = {
			//    width: oThis.options.width,
			//    height: oThis.options.height,
			//    color: oThis.options.color,
			//    addSelectionOptions: false,
			//    use: "HTML5",
			//    readyFunction: null,
			//    defaultModel: "",
			//    bondWidth: oThis.options.bondWidth,
			//    zoomScaling: oThis.options.zoomScaling,
			//    pinchScaling: oThis.options.pinchScaling,
			//    mouseDragFactor: 0.5,
			//    touchDragFactor: 0.15,
			//    multipleBondSpacing: oThis.options.multipleBondSpacing,
			//    spinRateX: oThis.options.spinRateX,
			//    spinRateY: oThis.options.spinRateY,
			//    spinFPS: oThis.options.spinFPS,
			//    spin: oThis.options.spin,
			//    debug: false,
			//    j2sPath: $.ChemSpider.Database.BaseUrl + "/Library/0.2.0/3rd/jsmol/j2s"
			//};

			var Info = {
				width: this.options.width,
				height: this.options.height,
				color: this.options.color,
				readyFunction: function () {
					oThis._renderContent();
				},
				use: this.isJMol() ? "JAVA" : "HTML5",
				jarPath: $.Compounds.store.getUrl('Widgets/3rd/jsmol/java'),
				jarFile: "JmolApplet.jar",
				j2sPath: $.Compounds.store.getUrl('Widgets/3rd/jsmol/j2s'),
				serverURL: $.Compounds.store.getUrl('Widgets/3rd/jsmol/php/jsmol.php'),
				disableInitialConsole: !this.options.debug,
				disableJ2SLoadMonitor: !this.options.debug,
				addSelectionOptions: false,
				allowJavaScript: true,
				script: null
			};

			Jmol._document = null;

			if (this.isJSMolLite()) {
				this.jmol = Jmol.getTMApplet("jmol_" + $(this.element).attr('id'), Info);
			}
			else if(this.isJMol() || this.isJSMol()){
				this.jmol = Jmol.getApplet("jmol_" + $(this.element).attr('id'), Info);
			}

			if (this.options.debug) {
				this.jmol._showInfo(true);
			}

			this.element.empty();
			this.element.html(this.jmol._code);

			//  readyFunction event is not fired in case of JSMol Lite, so we have to call render function directly
			if (this.isJSMolLite()) {
				this._renderContent();
			}

			//  show "Loading" progress in release mode only...
			if (!this.options.debug)
				this.element.loadProgress("");
		},

		_renderContent: function() {
			if (this.options.id != null) {
				this.setID(this.options.id);
			}
			else if (this.options.smiles != null) {
				this.setSMILES(this.options.smiles);
			}
			else if (this.options.mol != null) {
				this.setMOL(this.options.mol);
			}
			else if (this.options.url != null) {
				this.setByUrl(this.options.url);
			}
			//else {
			//    this.element.empty();
			//    this.element.append(
			//        $(document.createElement('p'))
			//            .text("JSMol - no id, smiles or mol specified")
			//    );
			//}

			this.element.hideProgress();
		},

		// events bound via _bind are removed automatically
		// revert other modifications here
		_destroy: function () {
			var element = this;
			// the DOM element associated with this instance
			this.element.empty();
		},

		/*
		Function: setID

		Show molecule with this ChemSpider ID - retrieves mol from ChemSpider webservice.

		Parameters:
		id - Compound ID

		(start code)
		jsmol.setID(10368587);
		(end code)
		*/
		setID: function (id) {
			var oThis = this;

			this.options.id = id;
			this.options.smiles = null;
			this.options.mol = null;
			this.options.url = null;
			this.element.loadProgress("");

			$.get(
				$.Compounds.store.getUrl('/api/compounds/' + id + '/mol', { dimension: '3d' }),
				function (mol) {
					oThis.options.mol = mol;
					oThis.element.hideProgress();
					oThis.jmol._loadModel(mol);
				}
			);
		},

		/*
		Function: setSMILES
		
		Show molecule that has this smiles - converts smiles to mol using ChemSpider webservice.

		Parameters:
		smiles - SMILES string

		(start code)
		jsmol.setSMILES('CC(=O)Oc1ccccc1C(=O)O');
		(end code)
		*/
		setSMILES: function (smiles) {
			var oThis = this;
			this.options.smiles = smiles;
			this.options.id = null;
			this.options.inchi = null;
			this.options.url = null;
			$.ChemSpider.store.convertSmiles2Mol(smiles, function (res) {
				oThis.jmol._loadModel(res.mol);
			});
		},

		/*
		Function: setMOL

		Show molecule with this mol.

		Parameters:
		mol - contents of mol connection file defining structure

		(start code)
		jsmol.setMOL(""
						+"\n"
						+"\n"
						+"\n  2  1  0  0000  0  0  0  0  0999 V2000"
						+"\n    2.4000   -0.4625    0.0000 C   0  0  0  0  0  0  0  0  0  0  0"
						+"\n    3.7167   -0.4667    0.0000 C   0  0  0  0  0  0  0  0  0  0  0"
						+"\n  2  1  2  0"
						+"\nM  END"
						+"\n"
						+"\n$$$$\n");
		(end code)
		*/
		setMOL: function (mol) {
			this.options.mol = mol;
			this.options.smiles = null;
			this.options.id = null;
			this.options.url = null;

			this.jmol._loadModel(mol, '');
		},

		setByUrl: function (url) {
			var oThis = this;

			this.options.url = url;
			this.options.mol = null;
			this.options.smiles = null;
			this.options.id = null;

			$.get(url, function (mol) {
				oThis.options.mol = mol;
				oThis.setMOL(mol);
			});
		},

		/*
		Function: toggleSpin

		If JSMol spin is on then it sets it to off, and vice-versa

		(start code)
		jsmol.toggleSpin();
		(end code)
		*/
		toggleSpin: function () {
			var oThis = this;
			if (this.options.spin) {
				this.options.spin = false;
				this.jmol.spin(false);
			} else {
				this.options.spin = true;
				this.jmol.spin(true);
			}
		},

		/*
		Function: setOptions

		Set new options to update the widget's view

		(start code)
		jsmol.setOptions({
			bondWidth: 2,
			spinRateX: 0.1,
			spinRateY: 0.1
		});
		(end code)
		*/

		setOptions: function (options) {
			$.extend(this.options, options);

			this._refresh();
		},

		isJMol: function () {
			return this.options.type == 'JMol';
		},

		isJSMol: function() {
			return this.options.type == 'JSMol';
		},

		isJSMolLite: function() {
			return this.options.type == 'JSMolLite';
		}

	});

}(jQuery));

$(document).ready(function () {
	$('div[data-type="jsmol"]').livequery(function () {
		var options = {
		};

		if ($(this).is('[data-id]'))
			options.id = $(this).data('id');

		if ($(this).is('[data-width]'))
			options.width = $(this).data('width');

		if ($(this).is('[data-height]'))
			options.height = $(this).data('height');

		if ($(this).is('[data-smiles]'))
			options.smiles = $(this).data('smiles');

		//if ($(this).is('[data-inchi]'))
		//	options.inchi = $(this).data('inchi');

		if ($(this).is('[data-mol-url]'))
			options.url = $(this).data('mol-url');

		if ($('input.mol-file', this).length == 1)
			options.mol = $('input.mol-file', this).val();

		if ($(this).is('[data-jsmol-type]'))
			options.type = $(this).data('jsmol-type');

		$(this).jsmol(options);
	});
});
