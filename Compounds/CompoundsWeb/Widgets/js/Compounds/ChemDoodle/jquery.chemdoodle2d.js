(function ($) {
    $.widget("compound.chemdoodle2d", $.compound.chemdoodle_base, {

        store: $.Compounds.store,

        options: {
            csid: null,
            inchi: null,
            smiles: null,
            mol: null,
            width: 200,
            height: 200,
            viewerSpecs: null,
            mode: '2D'  //  2D|3D
        },

        // Set up the widget
        _onChemDoodleReady: function () {
            var oThis = this;

            this.element
                .width(this.options.width)
                .height(this.options.height)
                .addClass('cs-widget cs-widget-chemdoodle2d');

            $(document.createElement('canvas'))
                .attr({
                    id: 'chemdoodle3d_' + Math.floor((Math.random() * 10000) + 1)
                })
                .appendTo(this.element);

            if (this.options.mol != null)
                this.setMolecule(this.options.mol);
            else if (this.options.csid != null)
                this.setCSID(this.options.csid);
            else if (this.options.smiles != null)
                this.setSMILES(this.options.smiles);

            $(document.createElement('button'))
                .text('3D')
                .button({
                })
                .click(function (event) {
                    oThis.setMode('3D');

                    event.stopPropagation();
                    return false;
                })
                .appendTo(this.element)
        },

        _viewerCanvas: function () {
            if (this.viewerCanvas == null) {
                this.viewerCanvas = new ChemDoodle.ViewerCanvas($('canvas', this.element).attr('id'), this.options.width, this.options.height);
                $.extend(this.viewerCanvas.specs, {
                    bonds_width_2D: 2,
                    atoms_displayTerminalCarbonLabels_2D: true,
                    atoms_useJMOLColors: true,
                    bonds_useJMOLColors: true,
                    atoms_font_bold_2D: true,
                    atoms_font_size_2D: 15
                }, this.options.viewerSpecs);
            }

            return this.viewerCanvas;
        },

        _transformCanvas: function () {
            if (this.transformCanvas == null) {
                this.transformCanvas = new ChemDoodle.TransformCanvas($('canvas', this.element).attr('id'), this.options.width, this.options.height, true);
                $.extend(this.transformCanvas.specs, {
                    bonds_width_2D: 2,
                    atoms_displayTerminalCarbonLabels_2D: true,
                    atoms_useJMOLColors: true,
                    bonds_useJMOLColors: true,
                    atoms_font_bold_2D: true,
                    atoms_font_size_2D: 15,
                    atoms_display: false,
                    bonds_clearOverlaps_2D: true
                }, this.options.viewerSpecs);
            }

            return this.transformCanvas;
        },

        setMolecule: function (mol) {
            var molecule = ChemDoodle.readMOL(mol);

            if (this.options.mode == '2D') {
                var viewer = this._viewerCanvas();
                viewer.loadMolecule(molecule);
            }
            else if (this.options.mode == '3D') {
                var viewer = this._transformCanvas();
                viewer.loadMolecule(molecule);
            }
        },

        setMode: function (mode) {
            if (this.options.mode != mode) {
                this.options.mode = mode;
                this.setMolecule(this.options.mol);
            }
        },

        setCSID: function (csid) {
            var oThis = this;

            this.options.csid = csid;
            this.options.smiles = null;
            this.options.mol = null;
            this.element.loadProgress("");
            $.Compounds.store.getCompoundInfo(csid, "Compound[CSID|Mol]", function (compound) {
                oThis.options.mol = compound.Mol;
                oThis.options.csid = compound.CSID;
                oThis.element.hideProgress();

                oThis.setMolecule(compound.Mol);
            });
        },

        setSMILES: function (smiles) {
            var oThis = this;
            this.options.smiles = smiles;
            this.options.csid = null;
            this.options.inchi = null;
            this.element.loadProgress("");

            $.Compounds.store.convertSmiles2Mol(smiles, function (res) {
                oThis.options.mol = res.mol;
                oThis.element.hideProgress();

                oThis.setMolecule(res.mol);
            });
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
            $.Widget.prototype._setOption.apply(this, arguments);
        },

        _setOptions: function () {
            $.Widget.prototype._setOptions.apply(this, arguments);
            this._refresh();
        }
    });
}(jQuery));
