(function ($) {
    $.widget("compound.chemdoodle_molecule_base", $.compound.chemdoodle_base, {

        store: $.Compounds.store,

        options: {
            id: null,
            inchi: null,
            smiles: null,
            mol: null,
            width: 200,
            height: 200,
            viewerSpecs: null,
            representation3D: 'Ball and Stick', //  Ball and Stick|Stick|van der Waals Spheres|Wireframe|Line
            mode: '2D'  //  2D|3D
        },

        // Set up the widget
        _onChemDoodleReady: function () {
            var oThis = this;

            this.element
                .width(this.options.width)
                .height(this.options.height)
                .addClass('cs-widget cs-widget-chemdoodle-molecule');

            this._onChemDoodleInit();

            if (this.options.id != null)
                this.setID(this.options.id);
            else if (this.options.smiles != null)
                this.setSMILES(this.options.smiles);
            else if (this.options.mol != null)
                this.setMolecule(this.options.mol);
            else if (this.options.url != null)
                this.setByUrl(this.options.url);
        },

        _onChemDoodleInit: function() {
        },

        _widget2D: function () {
            this.widget2D = null;

            this._initWidget2D();

            return this.widget2D;
        },

        _initWidget2D: function() {
            alert('2D widget is not initialized!');
        },

        _widget3D: function () {
            this.widget3D = null;

            this._initWidget3D();

            return this.widget3D;
        },

        _initWidget3D: function () {
            alert('3D widget is not initialized!');
        },

        setMode: function (mode) {
            if (this.options.mode != mode) {
                this.options.mode = mode;

                if (this.is2D()) {
                    if (this.canvas3D != null)
                        this.canvas3D.hide();
                    if (this.canvas2D != null)
                        this.canvas2D.show();
                }

                if (this.is3D()) {
                    if (this.canvas2D != null)
                        this.canvas2D.hide();
                    if (this.canvas3D != null)
                        this.canvas3D.show();
                }

                if (this.options.id != null)
                    this.setID(this.options.id);
                else if (this.options.smiles != null)
                    this.setSMILES(this.options.smiles);
                else if (this.options.mol != null)
                    this.setMolecule(this.options.mol);
                else if (this.options.url != null)
                        this.setByUrl(this.options.url);
            }
        },

        setMolecule: function (mol) {
            if (this.is2D()) {
                var viewer = this._widget2D();
                var molecule = ChemDoodle.readMOL(mol);
                viewer.loadMolecule(molecule);
            }
            else if (this.is3D()) {
                var viewer = this._widget3D();
                var molecule = ChemDoodle.readMOL(mol, 1);
                //var molecule = ChemDoodle.readCIF(mol, 1, 2, 3);
                viewer.loadMolecule(molecule);
            }
        },

        setID: function (id) {
            var oThis = this;

            this.options.id = id;
            this.options.smiles = null;
            this.options.mol = null;
            this.element.loadProgress("");

            $.get(
                $.Compounds.store.getUrl('api/compounds/' + id + '/mol', { 'dimension': this.is3D() ? '3d' : '2d' }),
                function (mol) {
                    oThis.options.mol = mol;
                    oThis.element.hideProgress();
                    oThis.setMolecule(mol);
                });
        },

        setSMILES: function (smiles) {
            //$('body').showMessage('Not supported', 'Unfortunately we do not support SMILES for chemdoodle_viewer widget yet!');
            var oThis = this;
            this.options.smiles = smiles;
            this.options.id = null;
            this.options.inchi = null;
            this.element.loadProgress("");

            $.Compounds.store.convertSmiles2Mol(smiles, function (res) {
                oThis.options.mol = res.mol;
                oThis.element.hideProgress();

                oThis.setMolecule(res.mol);
            });
        },

        setInChI: function (inchi) {
            //$('body').showMessage('Not supported', 'Unfortunately we do not support InChI for chemdoodle_viewer widget yet!');
            var oThis = this;
            this.options.smiles = smiles;
            this.options.id = null;
            this.options.inchi = null;
            this.element.loadProgress("");

            this.store.convertSmiles2Mol(smiles, function (res) {
                oThis.options.mol = res.mol;
                oThis.element.hideProgress();

                oThis.setMolecule(res.mol);
            });
        },

        setByUrl: function (url) {
            var oThis = this;

            this.options.url = url;
            this.options.mol = null;
            this.options.smiles = null;
            this.options.id = null;

            $.get(url, function (mol) {
                oThis.options.mol = mol;
                oThis.setMolecule(mol);
            });
        },

        set3DRepresentation: function (representation) {
            if (this.options.representation3D != representation) {
                this.options.representation3D = representation;

                if (this.is3DInitialized()) {
                    this.widget3D.specs.set3DRepresentation(this.options.representation3D);
                    this.widget3D.repaint();
                }
            }
        },

        setViewerSpecs: function (specs) {
            $.extend(this.options.viewerSpecs, specs);

            if (this.is2DInitialized()) {
                $.extend(this.widget2D.specs, this.options.viewerSpecs);
                this.widget2D.repaint();
            }

            if (this.is3DInitialized()) {
                $.extend(this.widget3D.specs, this.options.viewerSpecs);
                this.widget3D.repaint();
            }
        },

        is2D: function () {
            return this.options.mode == '2D';
        },

        is3D: function () {
            return this.options.mode == '3D';
        },

        is2DInitialized: function () {
            return this.widget2D != null;
        },

        is3DInitialized: function () {
            return this.widget3D != null;
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
            this.element.removeClass("cs-widget cs-widget-chemdoodle2d");
        },

        // called when created, and later when changing options
        _refresh: function () {
            var oThis = this;
        },

        // Use the _setOption method to respond to changes to options
        _setOption: function (key, value) {
            if (key === 'mode') {
                this.setMode(value);
            }
            else if (key === 'id') {
                this.setID(value);
            }
            else if (key === 'mol') {
                this.setMolecule(value);
            }
            else if (key === 'inchi') {
                this.setInChI(value);
            }
            else if (key === 'smiles') {
                this.setSMILES(value);
            }
            else if (key === 'representation3D') {
                this.set3DRepresentation(value);
            }
            else if (key === 'viewerSpecs') {
                this.setViewerSpecs(value);
            }

            $.Widget.prototype._setOption.apply(this, arguments);
        },

        _setOptions: function () {
            $.Widget.prototype._setOptions.apply(this, arguments);
            this._refresh();
        }
    });
}(jQuery));
