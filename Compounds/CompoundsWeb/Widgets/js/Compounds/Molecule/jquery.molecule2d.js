/*
Class: Molecule 2D

Display chemical structure as 2D image by ChemSpider ID, InChi os SMILES strings.
*/

(function ($) {
    $.widget("compound.molecule2d", $.compound.compoundbase, {

        /*
        Constructor: molecule2d
        Convert DIV panel into Molecule2D widget.

        Parameters:
        options - Initialization <Options>

        (start code)
        var mol2d = $("#molecule2d").molecule2d({
            width: 200,
            height: 200,
            id: 2157
        });
        (end code)

        Type: Options

        Properties:
        id - ChemSpider ID of compound that image should be presented
        inchi - InChi string of the molecule that should be presented
        smiles - SMILES string of the molecule that should be presented
        width - Width of the widget in pixels. Default value - 200
        height - Height of the widget in pixels. Default value - 200
        zoomWidth - Width of zoomed image in pixels. Default value - 500
        zoomHeight - Height of zoomed image in pixels. Default value - 500
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
        */
        options: {
            id: null,
            inchi: null,
            smiles: null,
            mol: null,
            molUrl: null,
            width: 200,
            height: 200,
            zoomWidth: 0,
            zoomHeight: 0,
            allowSave: false,
            click: null,
            saveDialog: {
                title: 'Save MOL',
                label: 'MOL file',
                filetype: 'chemical/x-mdl-molfile;charset=utf-8',
                extension: '.mol',
                defaultfile: 'molecule.mol'
            }
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-mol2d');

            this.molImg = $(document.createElement('img'))
                    .attr({ src: $.Compounds.store.getLibraryUrl('css/images/busy.gif') })
                    .addClass('cs-widget-cmp-image')
                    .appendTo(this.element);

            this.molImgLoader = $(document.createElement('img'))
                    .addClass('image-loader')
                    .load(function () {
                        oThis.molImg.attr({ src: $(this).attr('src') })
                    })
                    .hide()
                    .appendTo(this.element);

            this._createToolbar();

            //  if molecule size set as percentage...
            if (typeof this.options.width == 'string' && this.options.width.endsWith('%')) {
                this.options.width = this._getAbsValue(this.options.width, this.element.width());
            }
            if (typeof this.options.height == 'string' && this.options.height.endsWith('%')) {
                this.options.height = this._getAbsValue(this.options.height, this.element.height());
            }

            oThis._refresh();
        },

        _getAbsValue: function (val, range) {
            if (typeof val == 'string' && val.endsWith('%')) {
                var percent = parseFloat(val);
                if (!isNaN(percent))
                    return Math.round(range * percent / 100);
            }
            else
                return val;
        },

        _createToolbar: function () {
            var oThis = this;

            var toolbar = $(document.createElement('div'))
                .addClass('mol2d-toolbar')
                .appendTo(this.element);

            if (this.isAllowSave()) {
                this.zoomBtn = $(document.createElement('a'))
                                    .addClass('btn')
                                    .attr({
                                        title: 'Save',
                                        'data-toggle': 'tooltip',
                                        'data-placement': 'right',
                                        'data-original-title': 'Save'
                                    })
                                    .click(function () {
                                        oThis._showSaveDialog();
                                        return false;
                                    })
                                    .append(
                                        $(document.createElement('span'))
                                            .addClass('glyphicon glyphicon-floppy-save')
                                    )
                                    .appendTo(toolbar);
            }

            if (this.isAllowZoom()) {
                this.zoomBtn = $(document.createElement('a'))
                                    .addClass('btn')
                                    .attr({
                                        'data-lightbox': 'molecule' + this.options.id,
                                        href: $.Compounds.store.getImageUrl(this.options.id, this.options.zoomWidth, this.options.zoomHeight),
                                        title: 'Zoom',
                                        'data-toggle': 'tooltip',
                                        'data-placement': 'right',
                                        'data-original-title': 'Zoom'
                                    })
                                    .append(
                                        $(document.createElement('span'))
                                            .addClass('glyphicon glyphicon-zoom-in')
                                    )
                                    .appendTo(toolbar);
            }
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        destroy: function () {
            this.element.empty();
            $.Widget.prototype.destroy.call(this);
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
            this.element.removeClass("cs-widget-2dmol");
        },

        // called when created, and later when changing options
        _refresh: function () {
            var oThis = this;

            this.element
                .css('width', this.options.width + 4)
                .css('height', this.options.height + 4);

            if (this.options.click) {
                this.molImg.css({ cursor: 'pointer' })
                this.molImg.click(function () {
                    oThis.options.click({
                        id: oThis.options.id,
                        smiles: oThis.options.smiles,
                        inchi: oThis.options.inchi
                    });

                    return false;
                });
            }

            if (this.options.id != null) {
                this.setID(oThis.options.id);
            }
            else if (this.options.smiles != null) {
                this.setSMILES(oThis.options.smiles);
            }
            else if (this.options.inchi != null) {
                this.setInchi(oThis.options.inchi);
            }
            else if (this.options.mol != null) {
                this.setMol(oThis.options.mol);
            }
            else if (this.options.molUrl != null) {
                this.setUrl(this.options.molUrl);
            }
        },

        // Use the _setOption method to respond to changes to options
        _setOption: function (key, value) {
            $.Widget.prototype._setOption.apply(this, arguments);
        },

        _setOptions: function () {
            $.Widget.prototype._setOptions.apply(this, arguments);
            this._refresh();
        },

        isAllowZoom: function () {
            return this.options.zoomHeight > 0 && this.options.zoomWidth > 0;
        },

        isAllowSave: function () {
            return this.options.allowSave;
        },

        _loadMolFile: function (callback) {
            var oThis = this;

            if (this.options.id != null) {
                this._loadMolFileById(this.options.id, callback);
            }
            else if (this.options.smiles) {
                this._loadMolFileBySmiles(this.options.smiles, callback);
            }
            else if (this.options.inchi) {
                $.ChemSpider.store.convertTerm2Mol(this.options.inchi, function (res) {
                    if (callback)
                        callback(res.mol);
                });
            }
            else if (this.options.mol) {
                if (callback)
                    callback(this.options.mol);
            }
            else if (this.options.molUrl) {
                $.ajax({
                    type: 'GET',
                    url: this.options.molUrl
                })
                .done(function (mol) {
                    if (callback != null)
                        callback(mol);
                })
                .fail(function (xhr, error, thrownError) {
                    alert('cannot load file: ' + oThis.options.molUrl);
                });
            }
        },

        _loadMolFileById: function (id, callback) {
            $.Compounds.store.getCompound({ id: this.options.id }, function (compound) {
                if (callback)
                    callback(compound.Mol);
            });
        },

        _loadMolFileBySmiles: function (smiles, callback) {
            $.ChemSpider.store.convertSmiles2Mol(this.options.smiles, function (res) {
                if (callback)
                    callback(res.mol);
            });
        },

        /*
        Function: setID

        Set ChemSpider ID and load molecule's image from the compound.

        Parameters:
        id - ChemSpider ID

        (start code)
        mol2d.setID(10368587);
        (end code)
        */
        setID: function (id) {
            var oThis = this;

            this.options.id = id;
            this.options.smiles = null;
            this.options.inchi = null;

            this.molImg.attr({
                'data-id': id
            });

            if (this.isAllowZoom()) {
                this.zoomBtn.attr({
                    'data-lightbox': 'molecule' + this.options.id,
                });
            }

            this._setUrlById(id);
        },

        _setUrls: function (url, zoomUrl) {
            var oThis = this;

            if (this.isAllowZoom() && zoomUrl) {
                this.zoomBtn.attr('href', zoomUrl);
            }

            this.molImgLoader.attr({ src: url });
        },

        _setUrlById: function(id) {
            this._setUrls(
                $.Compounds.store.getImageUrl(id, this.options.width, this.options.height),
                $.Compounds.store.getImageUrl(id, this.options.zoomWidth, this.options.zoomHeight)
            );
        },

        /*
        Function: setSMILES

        Set SMILES and convert it to image.

        Parameters:
        smiles - SMILES string

        (start code)
        mol2d.setSMILES('CC(=O)Oc1ccccc1C(=O)O');
        (end code)
        */
        setSMILES: function (smiles) {
            var oThis = this;

            this.options.smiles = smiles;
            this.options.id = null;
            this.options.inchi = null;

            if (this.isAllowZoom())
                this.zoomBtn.attr('href', $.Compounds.store.getImageUrlBySmiles(smiles, this.options.zoomWidth, this.options.zoomHeight));

            this.molImg
                .attr('src', $.Compounds.store.getImageUrlBySmiles(smiles, this.options.width, this.options.height))
                .attr('smiles', smiles);
        },

        /*
        Function: setInchi

        Set InChi and convert it to image.

        Parameters:
        inchi - InChi string

        (start code)
        mol2d.setInchi('InChI=1S/C9H8O4/c1-6(10)13-8-5-3-2-4-7(8)9(11)12/h2-5H,1H3,(H,11,12)');
        (end code)
        */
        setInchi: function (inchi) {
            var oThis = this;

            this.options.smiles = null;
            this.options.id = null;
            this.options.inchi = inchi;

            //this.element.loadProgress("");
            
            if (this.isAllowZoom())
                this.zoomBtn.attr('href', $.ChemSpider.store.getMolImageUrlByInchi(inchi, this.options.zoomWidth, this.options.zoomHeight));

            this.molImg
                .attr('src', $.ChemSpider.store.getMolImageUrlByInchi(inchi, this.options.width, this.options.height))
                .attr('inchi', inchi);

            //this.element.hideProgress();
        },

        setUrl: function (url) {
            var oThis = this;

            $.get(url, function (mol) {
                oThis.setMol(mol);
            });

            //this._setUrls(
            //    $.Compounds.store.getImageUrlByMolUrl(this.options.molUrl, this.options.width, this.options.height),
            //    $.Compounds.store.getImageUrlByMolUrl(this.options.molUrl, this.options.zoomWidth, this.options.zoomHeight)
            //);
        },

        setMol: function (mol) {
            var oThis = this;

            this.options.smiles = null;
            this.options.id = null;
            this.options.inchi = null;
            this.options.mol = mol;

            if (this.isAllowZoom()) {
                $.Compounds.store.molToBase64Image(mol, oThis.options.zoomWidth, oThis.options.zoomHeight, function (base64) {
                    oThis.zoomBtn.attr('href', 'data:image/png;base64,' + base64);
                })
            }

            $.Compounds.store.molToBase64Image(mol, this.options.width, this.options.height, function (base64) {
                oThis.molImg.attr('src', 'data:image/png;base64,' + base64);
            })
        },

        /*
       Function: setOptions

       Set new options to update the widget's view

       (start code)
       mol2d.setOptions({
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

        setSize: function (width, height) {
            this.setOptions({
                width: width,
                height: height
            });
        },

        /*
        Function: toggleSpin

        Placeholder function

        (start code)
        mol2d.toggleSpin();
        (end code)
        */
        toggleSpin: function () {
        },

        _showSaveDialog: function () {
            var oThis = this;

            if (this.saveDlg == null) {
                this.molFile = $(document.createElement('textarea'))
                        .attr({readonly: 'readonly'})
                        .css({'min-height': '400px'})
                        .addClass('form-control');

                this.molFilename = $(document.createElement('input'))
                        .attr({ type: 'text' })
                        .addClass('form-control');

                this.saveDlg = $(document.createElement('div'))
                    .hide()
                    .addClass('modal fade')
                    .attr({
                        role: 'dialog',
                        'aria-hidden': false
                    })
                    .append(
                        $(document.createElement('div'))
                            .addClass('modal-dialog')
                            .append(
                                $(document.createElement('div'))
                                    .addClass('modal-content')
                                    .append(
                                        $(document.createElement('div'))
                                            .addClass('modal-header')
                                            .append(
                                                $(document.createElement('button'))
                                                    .addClass('close')
                                                    .attr({
                                                        type: 'button',
                                                        'data-dismiss': 'modal',
                                                        'aria-hidden': true
                                                    })
                                                    .html('&times;')
                                            )
                                            .append(
                                                $(document.createElement('h4'))
                                                    .addClass('modal-title')
                                                    .text(this.options.saveDialog.title)
                                            )
                                    )
                                    .append(
                                        $(document.createElement('div'))
                                            .addClass('modal-body')
                                            .append(
                                                $(document.createElement('form'))
                                                    .attr({
                                                        role: 'form'
                                                    })
                                                    .append(
                                                        $(document.createElement('div'))
                                                            .addClass('form-group')
                                                            .append(
                                                                $(document.createElement('label'))
                                                                    .text(this.options.saveDialog.label)
                                                            )
                                                            .append(this.molFile)
                                                    )
                                                    .append(
                                                        $(document.createElement('div'))
                                                            .addClass('form-group')
                                                            .append(
                                                                $(document.createElement('label'))
                                                                    .text('Filename')
                                                            )
                                                            .append(this.molFilename)
                                                    )
                                            )
                                    )
                                    .append(
                                        $(document.createElement('div'))
                                            .addClass('modal-footer')
                                            .append(
                                                $(document.createElement('button'))
                                                    .addClass('btn btn-primary')
                                                    .attr({
                                                        type: 'button',
                                                        'data-dismiss': 'modal',
                                                        'aria-hidden': true
                                                    })
                                                    .click(function () {
                                                        var blob = new Blob([oThis.molFile.text()], { type: oThis.options.saveDialog.filetype });
                                                        saveAs(blob, oThis.molFilename.val());
                                                    })
                                                    .text('Save')
                                            )
                                            .append(
                                                $(document.createElement('button'))
                                                    .addClass('btn btn-default')
                                                    .attr({
                                                        type: 'button',
                                                        'data-dismiss': 'modal',
                                                        'aria-hidden': true
                                                    })
                                                    .text('Close')
                                            )
                                    )
                            )
                    )
                    .appendTo(this.element);
            }

            this.saveDlg.modal('show');

            oThis._loadMolFile(function (mol) {
                if (oThis.options.id != null) {
                    oThis.molFilename.val(oThis.options.id + oThis.options.saveDialog.extension);
                }
                else {
                    oThis.molFilename.val(oThis.options.saveDialog.defaultfile);
                }
                oThis.molFile.text(mol);
            });
        }
    });

}(jQuery));

$(document).ready(function () {
    $('div[data-type="molecule2d"]').livequery(function () {
        var options = {
        };

        if ($(this).is('[data-id]'))
            options.id = $(this).data('id');

        if ($(this).is('[data-inchi]'))
            options.inchi = $(this).data('inchi');

        if ($(this).is('[data-smiles]'))
            options.smiles = $(this).data('smiles');

        if ($(this).is('[data-mol-url]'))
            options.molUrl = $(this).data('mol-url');

        if ($('input.mol-file', this).length == 1)
            options.mol = $('input.mol-file', this).val();

        if ($(this).is('[data-width]'))
            options.width = $(this).data('width');

        if ($(this).is('[data-height]'))
            options.height = $(this).data('height');

        if ($(this).is('[data-zoom-width]'))
            options.zoomWidth = $(this).data('zoom-width');

        if ($(this).is('[data-zoom-height]'))
            options.zoomHeight = $(this).data('zoom-height');

        if ($(this).is('[data-allow-save]'))
            options.allowSave = $(this).data('allowSave');

        $(this).molecule2d(options);
    });
});
