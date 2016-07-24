/*
Class: JSDraw

Wrapper for the chemical 2D structure editor based on <JSDraw at http://www.scilligence.com/web/jsdraw.aspx/> HTML5 widget.

Simple demo application available <here at http://parts.chemspider.com/Example/JSDrawEditor>.
*/
(function ($) {
    $.widget("compound.jsdraw", $.compound.compoundbase, {

        /*
        Constructor: jsdraw
        Convert DIV panel into JSDraw widget.

        Parameters:
        options - Initialization <Options>

        (start code)
        $('#drawer').jsdraw({
        width: 900,
        height: 500
        });
        (end code)

        Type: Options

        Properties:
        width - Width of the widget in pixels. Default value - 650
        height - Height of the widget in pixels. Default value - 350
        mode - Mode that should be used. Possible values are: Mobile|Classic. Default value - Mobile.
        */
        options: {
            smiles: null,
            mol: null,
            width: 650,
            height: 350,
            mode: 'Classic'
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element
                .addClass('cs-widget cs-widget-jsdraw')
                .width(this.options.width)
                .height(this.options.height);

            this.jsdrawDiv = $(document.createElement('div'))
                .width(this.options.width)
                .height(this.options.height)
                .appendTo(this.element);

            if (this.options.mode == 'Mobile')
                this.jsdrawDiv.attr({ skin: 'w8' })

            oThis.element.loadProgress("Loading...");

            $('body').loadScript("http://ajax.googleapis.com/ajax/libs/dojo/1.8.3/dojo/dojo.js", function () {
                setTimeout(function () {
                    require(["dojo/parser", "dojox/gfx"], function (parser) {
                        parser.parse();

                        $('body').loadScript($.Compounds.store.baseUrl + "/Widgets/3rd/jsdraw/JSDraw3.1.9/Scilligence.JSDraw2.Pro.js", function () {
                            oThis.jsDraw = new JSDraw(oThis.jsdrawDiv.get(0));

                            oThis._onInit();
                        })
                    });
                }, 300);
            });

            oThis.element.hideProgress();
        },

        _onInit: function () {
            //  ************************************************************
            //  override this method in order to add some more functionality
            //  ************************************************************

            if (this.options.smiles != null) {
                this.smiles(this.options.smiles);
            }
            else if (this.options.mol != null) {
                this.molecule(this.options.mol);
            }

            if (this._molecule != null)
                this.jsDraw.setMolfile(this._molecule);
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
            this.element.removeClass("cs-widget-jsdraw");
        },

        /*
        Function: smiles

        Get or set SMILES from/to the widget. If the 'value' parameter is NULL it returns SMILES of the chemical structure currently drawn in the widget otherwise it sets value to the widget and draw molecule there. 

        Parameters:
        value - SMILES

        (start code)
        var editor = $('#editorContainer').jsdraw({
        width: 900,
        height: 520
        }).data('jsdraw');

        var smiles = editor.smiles();
        (end code)
        */
        smiles: function (value) {
            if (value == null)
                return this.jsDraw.getSmiles();
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
        var editor = $('#editorContainer').jsdraw({
        width: 900,
        height: 520
        }).data('jsdraw');

        var mol = editor.molecule();
        (end code)
        */
        molecule: function (value) {
            if (value == null)
                return this.jsDraw.getMolfile();
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
            this._molecule = mol;
            if (this.jsDraw != null) {
                this.jsDraw.setMolfile(mol);
            }
        }
    });

} (jQuery));
