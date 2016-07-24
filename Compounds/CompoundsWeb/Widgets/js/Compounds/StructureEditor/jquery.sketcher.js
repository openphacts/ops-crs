(function ($) {
    $.widget("compound.sketcher", $.compound.chemdoodle_base, {

        options: {
            smiles: null,
            mol: null,
            width: 650,
            height: 350,
            specs: null,
            options: null
        },

        _onChemDoodleReady: function () {
            var oThis = this;

            this.element
                .addClass('cs-widget cs-widget-sketcher');

            $('body').loadScript($.Compounds.store.getUrl('Widgets/3rd/chemdoodle/6.0.0/uis/ChemDoodleWeb-uis.js'), function () {
                var options = $.extend({ useServices: false, oneMolecule: true }, oThis.options.options);

                oThis.sketcher = new ChemDoodle.SketcherCanvas($('canvas', oThis.element).attr('id'), oThis.options.width, oThis.options.height, options);

                $.extend(oThis.sketcher.specs, {
                    atoms_displayTerminalCarbonLabels_2D: true,
                    atoms_useJMOLColors: true,
                    bonds_clearOverlaps_2D: true,
                    shapes_color: 'c10000'
                }, oThis.options.specs);

                oThis.sketcher.repaint();

                if (oThis.options.mol != null)
                    oThis._setMolecule(oThis.options.mol);
                else if (oThis.options.smiles != null)
                    oThis._setSMILES(oThis.options.smiles);
            });
        },

        _setMolecule: function (mol) {
            var molecule = ChemDoodle.readMOL(mol);
            this.sketcher.loadMolecule(molecule);
        },

        _setSMILES: function (smiles) {
            var oThis = this;
            this.options.smiles = smiles;
            this.options.csid = null;
            this.options.inchi = null;
            this.element.loadProgress("");

            $.Compounds.store.convertSmiles2Mol(smiles, function (res) {
                oThis.options.mol = res.mol;
                oThis.element.hideProgress();

                oThis._setMolecule(res.mol);
            });
        },

        /*
        Function: smiles

        Get or set SMILES from/to the widget. If the 'value' parameter is NULL it returns SMILES of the chemical structure currently drawn in the widget otherwise it sets value to the widget and draw molecule there. 

        Parameters:
        value - SMILES

        (start code)
        var editor = $('#editorContainer').jchempaint({
        width: 900,
        height: 520
        }).data('jchempaint');

        var smiles = editor.smiles();
        (end code)
        */
        smiles: function (value) {
            if (value == null) {
                return 'Not implemented!';
            }
            else
                this._setSMILES(value);
        },

        /*
        Function: molecule

        Get or set MOL file from/to the widget. If the 'value' parameter is NULL it returns MOL file of the chemical structure currently drawn in the widget,
        otherwise it sets value to the widget and draw molecule there. 

        Parameters:
        value - MOL file

        (start code)
        var editor = $('#editorContainer').jchempaint({
        width: 900,
        height: 520
        }).data('jchempaint');

        var mol = editor.molecule();
        (end code)
        */
        molecule: function (value) {
            if (value == null) {
                var mol = this.sketcher.getMolecule();
                return ChemDoodle.writeMOL(mol)
            }
            else
                this._setMolecule(value);
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
            this.element.removeClass("cs-widget cs-widget-sketcher");
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
