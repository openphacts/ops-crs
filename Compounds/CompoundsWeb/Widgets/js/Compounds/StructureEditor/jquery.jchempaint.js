/*
Class: JChemPaint

Wrapper for the chemical 2D structure editor based on <JChemPaint at http://jchempaint.github.com/> Java applet.

Simple demo application available <here at http://parts.chemspider.com/Example/JChemPaintEditor>.
*/
(function ($) {
    $.widget("compound.jchempaint", $.compound.compoundbase, {

        /*
        Constructor: jchempaint
        Convert DIV panel into JChemPaint widget.

        Parameters:
        options - Initialization <Options>

        (start code)
        $('#drawer').jchempaint({
        width: 900,
        height: 500
        });
        (end code)

        Type: Options

        Properties:
        width - Width of the widget in pixels. Default value - 650
        height - Height of the widget in pixels. Default value - 350
        */
        options: {
            smiles: null,
            mol: null,
            width: 650,
            height: 350
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element
                .addClass('cs-widget cs-widget-jchempaint')
                .width(this.options.width)
                .height(this.options.height);

            this.applet = $(document.createElement('applet'))
                            .attr({
                                "width": this.options.width,
                                "height": this.options.height,
                                "archive": $.Compounds.store.getUrl("Widgets/3rd/jchempaint/3.1.3/jchempaint-applet-core.jar"),
                                "code": "org.openscience.jchempaint.applet.JChemPaintEditorApplet"
                            })
                            .append(
                                $(document.createElement('param')).attr({ "name": "implicitHs", "value": "true" })
                            )
                            .append(
                                $(document.createElement('param')).attr({ "name": "codebase_lookup", "value": "false" })
                            )
                            .append(
                                $(document.createElement('param')).attr({ "name": "image", "value": "hourglass.gif" })
                            )
                            .append(
                                $(document.createElement('param')).attr({ "name": "centerImage", "value": "true" })
                            )
                            .append(
                                $(document.createElement('param')).attr({ "name": "boxBorder", "value": "false" })
                            )
                            .append(
                                $(document.createElement('param')).attr({ "name": "language", "value": "en" })
                            );

            this.element.append(this.applet);

            //this.applet.get(0).onLoad = function () {
            //    alert('applet loaded!')
            //};

            if (this.options.smiles != null) {
                this.smiles(this.options.smiles);
            }
            else if (this.options.mol != null) {
                this.molecule(this.options.mol);
            }

        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
            this.element.removeClass("cs-widget-jchempaint");
            $(this.element).empty();
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
            if (value == null)
                return this.applet.get(0).getSmiles();
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
            if (value == null)
                return this.applet.get(0).getMolFile();
            else
                this._setMolecule(value);
        },

        _setSMILES: function (smiles) {
            var oThis = this;

            this.element.loadProgress("Converting...");

            $.Compounds.store.convertSmiles2Mol(smiles, function (res) {
                oThis._setMolecule(res.mol);
                oThis.element.hideProgress();
            });
        },

        _setMolecule: function (mol) {
            if (this.applet != null && this.applet.get(0).setMolFile != null)
                this.applet.get(0).setMolFile(mol);
        }
    });

} (jQuery));
