(function ($) {
    $.widget("toolkit.generalinfo", $.toolkit.toolkitbase, {

        options: {
            id: null,
            showImage: true,
            imgSize: 200,
            zoomSize: 500,
            properties: [],
            onDisplay: function (data) {
            }
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-generalinfo row');

            if (this.options.showImage) {
                this.image = $(document.createElement('div')).addClass('thumbnail');
                this._initImage();

                $(document.createElement('div'))
                    .addClass('picture col-md-4')
                    .append(this.image)
                    .appendTo(this.element);
            }

            this.title = $(document.createElement('strong'));

            this.properties = $(document.createElement('dl')).addClass('dl-horizontal');

            $(document.createElement('div'))
                .addClass('properties')
                .addClass(this.options.showImage ? 'col-md-8' : 'col-md-12')
                .append(this.title)
                .append(this.properties)
                .appendTo(this.element);

            if (oThis.options.id) {
                oThis.setID(oThis.options.id);
            }
        },

        _initImage: function() {
            this.image.molecule2d({
                width: this.options.imgSize,
                height: this.options.imgSize,
                zoomWidth: this.options.zoomSize,
                zoomHeight: this.options.zoomSize,
                allowSave: true
            });
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
            this.molA.remove();
            $('.cs-widget-cmp-props', this.element).remove();

            this.element.removeClass("cs-widget-cmp");
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

                oThis._setProperties(data);

                oThis.element.hideProgress();
            });
        },

        reload: function () {
            if (this.options.id) {
                this.setID(this.options.id);
            }
        },

        _loadData: function (id, callback) {
            $('body').showMessage('Error', '_loadData is not implemented!');

            //  override this method in order to load data and passing it back using callback
        },

        _setImage: function(data) {
            $('body').showMessage('Error', '_setImage is not implemented!');

            //  override this method in order to set proper molecule parameters. For example:
            //  this.image.molecule2d('setID', data.COMPOUND_ID);
        },

        _setTitle: function (data) {
            $('body').showMessage('Error', '_setTitle is not implemented!');

            //  override this method in order to set proper title. For example:
            //  this.title.html(data.Name);
        },

        _setProperties: function (data) {
            this.properties.empty();

            for (var i = 0; i < this.options.properties.length; i++) {
                var property = this.options.properties[i];

                var value = data[property.name];

                if (!value && typeof value !== 'boolean' && typeof value !== 'number') continue;

                if (typeof value === 'string' && value ||
                    typeof value === 'boolean' ||
                    typeof value === 'number' ||
                    typeof value === 'object' ||
                    value instanceof Array && value.length > 0) {

                    $(document.createElement('dt'))
                        .text(property.title)
                        .appendTo(this.properties);

                    $(document.createElement('dd'))
                        .append(
                            property.format != null ? property.format(value, data) : value
                        )
                        .appendTo(this.properties);
                }
            }

            //  calling Bootstrap tooltip...
            if ($.support.bootstrap())
                $('[data-toggle="tooltip"]', this.element).bootstrapTooltip({
                    delay: {
                        show: 500,
                        hide: 100
                    }
                });

            if (this.options.onDisplay) {
                this.options.onDisplay(data);
            }
        }
    });

}(jQuery));
