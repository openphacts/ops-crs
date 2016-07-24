(function ($) {
    $.widget("compound.chemdoodle3d", $.compound.chemdoodle_base, {

        store: $.Compounds.store,

        options: {
            csid: null,
            inchi: null,
            smiles: null,
            mol: null,
            width: 200,
            height: 200,
            viewerSpecs: null
        },

        // Set up the widget
        _onChemDoodleReady: function () {
            var oThis = this;

            this.element
                .width(this.options.width)
                .height(this.options.height)
                .addClass('cs-widget cs-widget-chemdoodle3d');

            //if (!this.libsLoaded) {
            //    if ($('meta[http-equiv="X-UA-Compatible"]', $('head')).length == 0) {
            //        $(document.createElement('meta'))
            //            .attr({
            //                'http-equiv': 'X-UA-Compatible',
            //                content: 'chrome=1'
            //            })
            //            .appendTo($('head'));
            //    }

            //    $('body').loadScript(this.store.getUrl('Widgets/CS/3rd/chemdoodle/ChemDoodleWeb-libs.js'), function () {
            //        $('body').loadScript($.ChemSpider.Database.BaseUrl + '/Widgets/CS/3rd/chemdoodle/ChemDoodleWeb.js', function () {
            //            oThis.libsLoaded = true;

            //            if (oThis.options.mol != null)
            //                oThis.setMolecule(oThis.options.mol);
            //            else if (oThis.options.csid != null)
            //                oThis.setCSID(oThis.options.csid);
            //            else if (oThis.options.smiles != null)
            //                oThis.setSMILES(oThis.options.smiles);
            //        });
            //    });
            //}
            //else {
                if (this.options.mol != null)
                    this.setMolecule(this.options.mol);
                else if (this.options.csid != null)
                    this.setCSID(this.options.csid);
                else if (this.options.smiles != null)
                    this.setSMILES(this.options.smiles);
            //}
        },

        setMolecule: function (mol) {
            if (this.viewer == null) {
                $(document.createElement('canvas'))
                    .attr({
                        id: 'chemdoodle3d_' + Math.floor((Math.random() * 10000) + 1)
                    })
                    .appendTo(this.element);

                //this.viewer = new ChemDoodle.ViewerCanvas($('canvas', this.element).attr('id'), this.options.width, this.options.height);
                //$.extend(this.viewer.specs, {
                //    bonds_width_2D: 2,
                //    atoms_displayTerminalCarbonLabels_2D: true,
                //    atoms_useJMOLColors: true,
                //    bonds_useJMOLColors: true,
                //    atoms_font_bold_2D: true,
                //    atoms_font_size_2D: 15
                //}, this.options.viewerSpecs);

                //this.viewer = new ChemDoodle.ViewerCanvas3D($('canvas', this.element).attr('id'), this.options.width, this.options.height);
                //this.viewer.specs.set3DRepresentation('Ball and Stick');
                //this.viewer.specs.backgroundColor = 'black';
                //this.viewer.specs.lightDirection_3D = [0, 0, -1];
                //this.viewer.specs.atoms_resolution_3D = 3;
                //this.viewer.specs.bonds_resolution_3D = 3;
                //this.viewer.specs.bonds_useJMOLColors = true;
                //this.viewer.specs.bonds_color = '#00FF00';
                //this.viewer.specs.atoms_useJMOLColors = true;
                //this.viewer.specs.atoms_color = '#0000FF';
                //this.viewer.specs.atoms_materialAmbientColor_3D = viewerLowRes.specs.bonds_materialAmbientColor_3D = '#333333';

                this.viewer = new ChemDoodle.TransformCanvas3D($('canvas', this.element).attr('id'), this.options.width, this.options.height);
                this.viewer.specs.set3DRepresentation('Ball and Stick');
                //this.viewer.specs.bonds_useJMOLColors = true;
                //this.viewer.specs.bonds_color = '#00FF00';
                //this.viewer.specs.atoms_useJMOLColors = true;
                //this.viewer.specs.atoms_sphereDiameter_3D = .4;
                //this.viewer.specs.bonds_cylinderDiameter_3D = .4;
                //this.viewer.specs.atoms_useVDWDiameters_3D = true;
                //this.viewer.specs.atoms_color = '#0000FF';
                //this.viewer.specs.atoms_displayLabels_3D = true;
                //this.viewer.specs.bonds_resolution_3D = 30;
            }
            //var mol2 = '3036\n  CHEMDOOD12280913053D\n\n 28 29  0     0  0  0  0  0  0999 V2000\n    0.0456    1.0544   -1.9374 Cl  0  0  0  0  0  0  0  0  0  0  0  0\n   -0.7952   -1.7026   -1.7706 Cl  0  0  0  0  0  0  0  0  0  0  0  0\n    0.6447   -0.8006   -4.1065 Cl  0  0  0  0  0  0  0  0  0  0  0  0\n    1.8316   -0.9435    4.4004 Cl  0  0  0  0  0  0  0  0  0  0  0  0\n    6.9949    1.1239   -3.9007 Cl  0  0  0  0  0  0  0  0  0  0  0  0\n    1.9032   -1.0692   -1.6001 C   0  0  0  0  0  0  0  0  0  0  0  0\n    1.8846   -1.0376   -0.1090 C   0  0  0  0  0  0  0  0  0  0  0  0\n    3.2176   -0.5035   -2.1949 C   0  0  0  0  0  0  0  0  0  0  0  0\n    0.5585   -0.6223   -2.3126 C   0  0  0  0  0  0  0  0  0  0  0  0\n    2.2670    0.1198    0.5688 C   0  0  0  0  0  0  0  0  0  0  0  0\n    4.3480   -1.2638   -2.0859 C   0  0  0  0  0  0  0  0  0  0  0  0\n    1.4856   -2.1660    0.6075 C   0  0  0  0  0  0  0  0  0  0  0  0\n    3.1719    0.7242   -2.7939 C   0  0  0  0  0  0  0  0  0  0  0  0\n    2.2506    0.1490    1.9633 C   0  0  0  0  0  0  0  0  0  0  0  0\n    5.5313   -0.7541   -2.6203 C   0  0  0  0  0  0  0  0  0  0  0  0\n    1.4691   -2.1369    2.0020 C   0  0  0  0  0  0  0  0  0  0  0  0\n    4.3552    1.2340   -3.3284 C   0  0  0  0  0  0  0  0  0  0  0  0\n    1.8515   -0.9793    2.6800 C   0  0  0  0  0  0  0  0  0  0  0  0\n    5.5350    0.4948   -3.2417 C   0  0  0  0  0  0  0  0  0  0  0  0\n    1.9777   -2.1366   -1.8749 H   0  0  0  0  0  0  0  0  0  0  0  0\n    2.5727    1.0177    0.0401 H   0  0  0  0  0  0  0  0  0  0  0  0\n    4.3513   -2.2356   -1.6034 H   0  0  0  0  0  0  0  0  0  0  0  0\n    1.1951   -3.0814    0.0991 H   0  0  0  0  0  0  0  0  0  0  0  0\n    2.3077    1.3562   -2.8879 H   0  0  0  0  0  0  0  0  0  0  0  0\n    2.5491    1.0585    2.4783 H   0  0  0  0  0  0  0  0  0  0  0  0\n    6.4431   -1.3411   -2.5451 H   0  0  0  0  0  0  0  0  0  0  0  0\n    1.1584   -3.0244    2.5473 H   0  0  0  0  0  0  0  0  0  0  0  0\n    4.3449    2.2098   -3.8075 H   0  0  0  0  0  0  0  0  0  0  0  0\n  1  9  1  0  0  0  0\n  2  9  1  0  0  0  0\n  3  9  1  0  0  0  0\n  4 18  1  0  0  0  0\n  5 19  1  0  0  0  0\n  6  7  1  0  0  0  0\n  6  8  1  0  0  0  0\n  6  9  1  0  0  0  0\n  6 20  1  0  0  0  0\n  7 10  2  0  0  0  0\n  7 12  1  0  0  0  0\n  8 11  2  0  0  0  0\n  8 13  1  0  0  0  0\n 10 14  1  0  0  0  0\n 10 21  1  0  0  0  0\n 11 15  1  0  0  0  0\n 11 22  1  0  0  0  0\n 12 16  2  0  0  0  0\n 12 23  1  0  0  0  0\n 13 17  2  0  0  0  0\n 13 24  1  0  0  0  0\n 14 18  2  0  0  0  0\n 14 25  1  0  0  0  0\n 15 19  2  0  0  0  0\n 15 26  1  0  0  0  0\n 16 18  1  0  0  0  0\n 16 27  1  0  0  0  0\n 17 19  1  0  0  0  0\n 17 28  1  0  0  0  0\nM  END\n';

            var molecule = ChemDoodle.readMOL(mol, 1);

            this.viewer.loadMolecule(molecule);

            var oThis = this;
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
