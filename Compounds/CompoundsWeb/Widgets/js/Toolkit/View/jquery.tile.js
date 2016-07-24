(function ($) {
    $.widget("toolkit.tile", $.toolkit.toolkitbase, {

        options: {
            width: 200,
            zoomSize: 500,
            item: null,
            properties: []
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-tile').width(this.options.width + 4);

            this.tile = $(document.createElement('div'))
                            .appendTo(this.element);

            this._initTile();

            this.properties = $(document.createElement('ul'));

            $(document.createElement('div'))
                .addClass('tile-properties')
                .append(this.properties)
                .appendTo(this.element);

            if (this.options.item != null) {
                this.drawTile(this.options.item);
                this._setProperties(this.options.item);
            }

            this._init();
        },

        _initTile: function() {
        },

        _init: function() {
            //  *********************************************************************
            //  override this method in if you want to add some functionality on init
            //  *********************************************************************
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
            $(this.element).empty();

            this.element.removeClass("cs-widget-tile");
        },

        drawTile: function (item) {
            //  ***********************************************************
            //  override this method in order to implement tile's rendering
            //  ***********************************************************

            $('body').showMessage('Error: toolkit.tile', 'drawTile is not implemented!');
        },

        _setProperties: function (item) {
            this.properties.empty();

            for (var i = 0; i < this.options.properties.length; i++) {
                var property = this.options.properties[i];

                var value = item[property.name];

                if (typeof value === 'string' && value ||
                    typeof value === 'boolean' ||
                    typeof value === 'number' ||
                    value instanceof Array && value.length > 0) {

                    $(document.createElement('li'))
                        .append(
                            property.title + ': '
                        )
                        .append(
                            property.format != null ? property.format(value) : value
                        )
                        .appendTo(this.properties);
                }
            }
        }
    });

}(jQuery));
