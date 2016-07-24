(function ($) {
    $.widget("toolkit.generalinfo_update", $.toolkit.toolkitbase, {

        options: {
            id: null,
            showImage: true,
            imgSize: 200,
            properties: [],
            onCancel: function () { },  //  callback on cancel
            onSave: function () { }     //  callback on save
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-info-update');

            if (this.options.showImage) {
                this.image = $(document.createElement('div')).addClass('thumbnail');

                this.uploadImage = $(document.createElement('input'))
                    .addClass('filestyle')
                    .attr({
                        accept: 'image/*',
                        type: 'file'
                    });

                this._initImage();
            }

            this.title = $(document.createElement('strong'));

            this.form = $(document.createElement('div')).form({
                properties: this.options.properties
            });

            var row = $(document.createElement('div'))
                .addClass('row')
                .appendTo(this.element);

            if(this.options.showImage) {
                row.append(
                    $(document.createElement('div'))
                        .addClass('picture col-md-4')
                        .append(this.image)
                        .append(this.uploadImage)
                );
            }

            row.append(
                $(document.createElement('div'))
                    .addClass('properties')
                    .addClass(this.options.showImage ? 'col-md-8' : 'col-md-12')
                    .append(this.title)
                    .append(this.form)
            );

            $(document.createElement('div'))
                .append(
                    $(document.createElement('button'))
                        .addClass('btn btn-default pull-right cancel-btn')
                        .attr({
                            type: 'button'
                        })
                        .append(
                            $(document.createElement('span'))
                                .addClass('glyphicon glyphicon-remove')
                                .attr({ 'aria-hidden': true })
                        )
                        .append(
                            $(document.createTextNode(' Cancel'))
                        )
                        .click(function () {
                            oThis._cancel();
                        })
                )
                .append(
                    $(document.createElement('button'))
                        .addClass('btn btn-default pull-right save-btn')
                        .attr({
                            type: 'button'
                        })
                        .append(
                            $(document.createElement('span'))
                                .addClass('glyphicon glyphicon-ok')
                                .attr({ 'aria-hidden': true })
                        )
                        .append(
                            $(document.createTextNode(' Save'))
                        )
                        .click(function () {
                            oThis._save();
                        })
                )
                .appendTo(this.element);

            if (this.options.showImage) {
                $('body').loadScript($.Toolkit.store.getUrl('Widgets/3rd/bootstrap.filestyle.js'), function () {
                    oThis.uploadImage.filestyle({ buttonText: "Find file" });
                });
            }

            if (oThis.options.id) {
                oThis.setID(oThis.options.id);
            }
        },

        reload: function () {
            if (this.options.id) {
                this.setID(this.options.id);
            }
        },

        _initImage: function () {
        },

        // called when created, and later when changing options
        _refresh: function () {
        },

        // Use the _setOption method to respond to changes to options
        _setOption: function (key, value) {
            $.Widget.prototype._setOption.apply(this, arguments);
        },

        _setOptions: function () {
            $.Widget.prototype._setOptions.apply(this, arguments);
            this._refresh();
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
        },

        /*
        Function: setID

        Function that loads data from ChemSpider website and display them in the component.

        Parameters:
        csid - Compound ChemSpider ID that should be loaded

        (start code)
        $('#compoundInfo').compoundinfo("setCSID", 2157);
        (end code)
        */
        setID: function (id) {
            var oThis = this;

            this.options.id = id;

            this.element.loadProgress();

            this._loadData(this.options.id, function (data) {
                oThis._setImage(data);

                oThis._setTitle(data);

                oThis.form.form('setData', data);

                oThis.element.hideProgress();
            });
        },

        _loadData: function (id, callback) {
            $('body').showMessage('Error', '_loadData is not implemented!');

            //  override this method in order to load data and passing it back using callback
        },

        _setImage: function (data) {
            $('body').showMessage('Error', '_setImage is not implemented!');

            //  override this method in order to set proper molecule parameters. For example:
            //  this.image.molecule2d('setID', data.COMPOUND_ID);
        },

        _setTitle: function (text) {
            //  override this method if you want to change the functionality

            this.title.html(text);
        },

        _save: function () {
        },

        _cancel: function () {
            this._trigger("onCancel", null, {});
        }
    });

}(jQuery));
