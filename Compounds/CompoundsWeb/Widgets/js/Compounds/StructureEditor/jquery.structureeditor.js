/*
Class: StructureEditor

Component for editing chemical structure by converting it (from term, SMILES, InChi or CSID etc.), edit it later and get entered structure as SMILES string. 
This component based on some conversion services provided by ChemSpider and structure editors components.

*/
$(function () {
    $.widget("compound.structureeditor", $.toolkit.editor_base, {

        /*
        Constructor: structureeditor
        Convert DIV panel into StructureEditor widget.

        Parameters:
        options - Initialization <Options>

        (start code)
        $('#drawer').structureeditor({
            width: 300,
            height: 300,
            smiles: 'CC(=O)Oc1ccccc1C(=O)O'
        });
        (end code)

        Type: Options

        Properties:
        smiles - SMILES string of the structure that sshould be rendered during the initialization
        width - Width of the widget in pixels. Default value - 300
        height - Height of the widget in pixels. Default value - 300
        jsdraw - Bool value that enable/disable JSDraw widget for entering structure. Enabled by default
        jchempaint - Bool value that enable/disable JChemPaint widget for entering structure. Enabled by default
        onChange - Callback function that will be called every time when the structure changed
        */
        options: {
            smiles: null,
            mol: null,
            inchi: null,
            url: null,
            ketcher: false,
            jsdraw: true,
            jchempaint: false,
            sketcher: false
        },

        _onInit: function () {
            var oThis = this;

            var id = this.element.attr('id');

            this.element
                .addClass("cs-widget-structure-editor")

            this.smilesIF = $(document.createElement('input'))
                .attr({
                    type: 'hidden',
                    name: this.element.attr('id') + 'Smiles'
                }).appendTo(this.element);

            this.moleculeIF = $(document.createElement('input'))
                .attr({
                    type: 'hidden',
                    name: this.element.attr('id') + 'Molecule'
                })
                .appendTo(this.element);

            this._buildConvertDialog();

            if (this.options.smiles != null) {
                this._setSmiles(this.options.smiles);
            }
            else if (this.options.mol != null) {
                this._setMolecule(this.options.mol);
            }
            else if (this.options.inchi != null) {
                this._setInChI(this.options.inchi);
            }
            else if (this.options.url != null) {
                this._setUrl(this.options.url);
            }
        },

        _onEditDialogOpen: function () {
            if (this.moleculeIF.val() != '') {
                this.activeEditor.molecule(this.moleculeIF.val());
            }
            else if (this.smilesIF.val() != '') {
                this.activeEditor.smiles(this.smilesIF.val());
            }
        },

        _onEditDialogOk: function () {
            var mol = this.activeEditor.molecule();
            this._setMolecule(mol);
        },

        _loadState: function (value) {
            this.options.mol = value;
        },

        _buildConvertDialog: function () {
            var oThis = this;

            this.convertDlg = $(document.createElement('div'))
                    .attr({ title: 'Convert to Structure' })
                    .css({ display: 'none' })
                    .append(
                        $(document.createElement('input'))
                            .width('95%')
                            .attr({ 'type': 'text' })
                            .keypress(function (e) {
                                if (e.which == 13) {
                                    oThis._convertDlgOnOk();
                                }
                            })
                    )
                    .append(
                        $(document.createElement('em')).text("Name, SMILES or InChI")
                    )
                    .appendTo(this.element);

            this.convertDlg.dialog({
                width: 350,
                //height: 180,
                resizable: false,
                autoOpen: false,
                buttons: {
                    Ok: function () {
                        oThis._convertDlgOnOk();
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            });
        },

        _convertButton: function () {
            var oThis = this;

            $(document.createElement('button'))
                .addClass('cs-convert-button')
                .text('Convert')
                .button({
                    icons: {
                        primary: this.options.showBtnIcon ? "ui-icon-transfer-e-w" : ""
                    },
                    text: this.options.showBtnText
                })
                .click(function (event) {
                    oThis.convertDlg.dialog("open");

                    event.stopPropagation();
                    return false;
                })
                .appendTo(this.topToolbar);
        },

        _buildTopToolbar: function () {
            var editButton = this._editButton();
            var convertButton = this._convertButton();

            this.topToolbar.append(convertButton);
            this.topToolbar.append(editButton);
        },

        _buildEditors: function () {
            var oThis = this;

            var id = this.element.attr('id') || 'structure_editor';

            if(this.options.ketcher) {
                $('ul', this.editDlg).append(
                    $(document.createElement('li'))
                        .attr({ 'class': 'ketcher' })
                        .append(
                            $(document.createElement('a'))
                                .attr({ 'href': '#' + id + '_ketcher' })
                                .text('Ketcher')
                        )
                );

                this.editDlg.append(
                    $(document.createElement('div'))
                        .attr({
                            'class': 'ketcher',
                            'id': id + '_ketcher'
                        })
                );
            }

            if(this.options.jsdraw) {
                $('ul', this.editDlg)
                    .append(
                        $(document.createElement('li'))
                            .attr({ 'class': 'jsdraw' })
                            .append(
                                $(document.createElement('a'))
                                    .attr({ 'href': '#' + id + '_jsdraw' })
                                    .text('JSDraw')
                            )
                    );

                this.editDlg.append(
                    $(document.createElement('div'))
                        .attr({
                            'id': id + '_jsdraw',
                            'class': 'jsdraw',
                        })
                        .append(
                            $(document.createElement('div'))
                        )
                );
            }

            if(this.options.jchempaint) {
                $('ul', this.editDlg)
                    .append(
                        $(document.createElement('li'))
                            .attr({ 'class': 'jchempaint' })
                            .append(
                                $(document.createElement('a'))
                                    .attr({ 'href': '#' + id + '_jchempaint' })
                                    .text('JChemPaint')
                            )
                    );

                this.editDlg.append(
                    $(document.createElement('div'))
                        .attr({
                            'id': id + '_jchempaint',
                            'class': 'jchempaint',
                        })
                );
            }

            if (this.options.sketcher) {
                $('ul', this.editDlg)
                    .append(
                        $(document.createElement('li'))
                            .attr({ 'class': 'sketcher' })
                            .append(
                                $(document.createElement('a'))
                                    .attr({ 'href': '#' + id + '_sketcher' })
                                    .text('ChemDoole Sketcher')
                            )
                    );

                this.editDlg.append(
                    $(document.createElement('div'))
                        .attr({
                            'id': id + '_sketcher',
                            'class': 'sketcher',
                        })
                );
            }

            this.editDlg.tabs({
                active: 0,
                activate: function (event, ui) {
                    var mol = oThis.activeEditor.molecule();

                    if(ui.newTab.hasClass('ketcher')) {
                        oThis.activeEditor = oThis.ketcherEditor;
                    }
                    else if(ui.newTab.hasClass('jsdraw')) {
                        oThis.activeEditor = oThis.jsdrawEditor;
                    }
                    else if(ui.newTab.hasClass('jchempaint')) {
                        oThis.activeEditor = oThis.jchempaintEditor;
                    }
                    else if (ui.newTab.hasClass('sketcher')) {
                        oThis.activeEditor = oThis.sketcherEditor;
                    }

                    oThis.activeEditor.molecule(mol);
                }
            });

            //if(this.options.ketcher) {
            //    this.ketcherEditor = $('div.ketcher', this.editDlg).ketcher({
            //        width: 800,
            //        height: 490
            //    }).data('cs-ketcher');
            //}

            if(this.options.jsdraw) {
                this.jsdrawEditor = $('div.jsdraw > div', this.editDlg).jsdraw({
                    width: 800,
                    height: 480
                }).data('compound-jsdraw');
            }

            if(this.options.jchempaint) {
                this.jchempaintEditor = $('div.jchempaint', this.editDlg).jchempaint({
                    width: 770,
                    height: 470
                }).data('compound-jchempaint');
            }

            if (this.options.sketcher) {
                this.sketcherEditor = $('div.sketcher', this.editDlg).sketcher({
                    width: 770,
                    height: 450
                }).data('compound-sketcher');
            }

            var active = oThis.editDlg.tabs('option', 'active');
            var activeTab = $('ul.ui-tabs-nav > li', this.editDlg).get(active);
            if($(activeTab).hasClass('ketcher')) {
                this.activeEditor = this.ketcherEditor;
            }
            else if($(activeTab).hasClass('jsdraw')) {
                this.activeEditor = this.jsdrawEditor;
            }
            else if($(activeTab).hasClass('jchempaint')) {
                this.activeEditor = this.jchempaintEditor;
            }
            else if ($(activeTab).hasClass('sketcher')) {
                this.activeEditor = this.sketcherEditor;
            }
        },

        _onDestroy: function () {
            this.element.removeClass("cs-widget-structure-editor");
        },

        // _setOption is called for each individual option that is changing
        _setOption: function (key, value) {
            if (key == "smiles") {
                if (value == null)
                    return this.smiles();
                else
                    this.smiles(value);
            }
            else if (key == "molecule") {
                if (value == null)
                    return this.molecule();
                else
                    this.molecule(value);
            }

            // In jQuery UI 1.8, you have to manually invoke the _setOption method from the base widget
            $.Widget.prototype._setOption.apply(this, arguments);
            // In jQuery UI 1.9 and above, you use the _super method instead
            this._super("_setOption", key, value);
        },

        _setSmiles: function (smiles) {
            var oThis = this;

            if (smiles == null) {
                this._set(null, null);

                this.emptySpan.show();
                this.molImg.hide();
            }
            else {
                this.emptySpan.hide();

                $.Compounds.store.convertSmiles2Mol(smiles, function (res) {
                    if (res.confidence == 0) {
                        $('body').showMessage('Error', 'Cannot convert SMILES to MOL: ' + res.message);
                    }
                    else {
                        oThis._set(smiles, res.mol);
                    }
                })
            }
        },

        _setMolecule: function (mol) {
            var oThis = this;

            if (mol == null) {
                this._set(null, null);

                this.emptySpan.show();
                this.molImg.hide();
            }
            else {
                this.emptySpan.hide();

                $.Compounds.store.convertMol2Smiles(mol, function (res) {
                    oThis._set(res.mol, mol);
                });
            }
        },

        _setInChI: function (inchi) {
            var oThis = this;

            if (inchi == null) {
                this._set(null, null);

                this.emptySpan.show();
                this.molImg.hide();
            }
            else {
                this.emptySpan.hide();

                $.Compounds.store.convertInChI2Mol(inchi, function (res) {
                    oThis._setMolecule(res.mol);
                });
            }
        },

        _setUrl: function (url) {
            var oThis = this;

            if (url == null) {
                this._set(null, null);

                this.emptySpan.show();
                this.molImg.hide();
            }
            else {
                this.emptySpan.hide();

                $.get(url, function (mol) {
                    oThis._setMolecule(mol);
                });
            }
        },

        _set: function (smiles, mol) {
            var oThis = this;

            if ((smiles == null || smiles == '') && (mol == null || mol == '')) {
                this.smilesIF.val('');
                this.moleculeIF.val('');
            }
            else {
                this.smilesIF.val(smiles);
                this.moleculeIF.val(mol);

                $.Compounds.store.molToBase64Image(mol, this.options.width, this.options.height, function (base64) {
                    oThis.molImg.attr('src', 'data:image/png;base64,' + base64);
                })

            }

            this._trigger("onChange", null, { smiles: smiles, mol: mol });

            this._saveState(mol);
        },

        _convertDlgOnOk: function() {
            var oThis = this;
            this.convertDlg.dialog("close");

            this.element.loadProgress('');

            var text = $('input', this.convertDlg).val();
            $.ChemSpider.store.convertTerm2Smiles(text, function (res) {
                oThis.element.hideProgress();
 
                if(res.confidence == 0) {
                    $('body').showMessage('Warning', 'Cannot convert entered text to structure.<br\>Please enter another name and try again!');
                }
                else {
                    oThis._setSmiles(res.mol);

                    if(res.message != '')
                        $('body').showMessage('Warning', res.message);
                }
            });
        },

        /*
        Function: smiles

        Get or set SMILES from/to the widget. If the 'smiles' parameter is NULL then it returns SMILES of the chemical structure currently drawn in the widget, 
        otherwise it sets value to the widget and draw molecule there. 

        Parameters:
        smiles - SMILES string

        (start code)
        var editor = $('#editorContainer').structureeditor({
            width: 300,
            height: 300
        }).data('cs-structureeditor');

        var smiles = editor.smiles();
        (end code)
        */
        smiles: function (smiles) {
            if (smiles != null)
                this._setSmiles(smiles);
            else
                return this.smilesIF.val();
        },

        /*
        Function: molecule

        Returns MOL file of the structure entered in the widget. 

        (start code)
        var editor = $('#editorContainer').structureeditor({
            width: 300,
            height: 300
        }).data('cs-structureeditor');

        var mol = editor.molecule();
        (end code)
        */
        molecule: function (mol) {
            if (mol == null)
                return this.moleculeIF.val();
            else
                this._setMolecule(mol);
        },

        clear: function () {
            this._setMolecule(null);
        }
    });

}(jQuery));

$(document).ready(function () {
    $('div[data-type="structure-editor"]').livequery(function () {
        var options = {
        };

        if ($(this).is('[data-smiles]'))
            options.smiles = $(this).data('smiles');

        if ($(this).is('[data-inchi]'))
            options.inchi = $(this).data('inchi');

        if ($(this).is('[data-mol-url]'))
            options.url = $(this).data('mol-url');

        if ($('input.mol-file', this).length == 1)
            options.mol = $('input.mol-file', this).val();

        if ($(this).is('[data-jsdraw]'))
            options.jsdraw = $(this).data('jsdraw');

        if ($(this).is('[data-jchempaint]'))
            options.jchempaint = $(this).data('jchempaint');

        if ($(this).is('[data-save-state]'))
            options.saveState = $(this).data('save-state');

        if ($(this).is('[data-width]'))
            options.width = $(this).data('width');

        if ($(this).is('[data-height]'))
            options.height = $(this).data('height');

        if ($(this).is('[data-dialog-width]'))
            options.width = $(this).data('dialog-width');

        if ($(this).is('[data-dialog-height]'))
            options.height = $(this).data('dialog-height');

        $(this).structureeditor(options);
    });
});
