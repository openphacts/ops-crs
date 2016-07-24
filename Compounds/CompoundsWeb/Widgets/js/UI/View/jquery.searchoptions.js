$(function () {
    $.widget("ui.searchoptions", {

        options: {
            searchOptions: null,
            settingsUrl: '/Search/Settings'
        },

        // the constructor
        _create: function () {
            var oThis = this;

            this.element
                .addClass("cs-widget cs-widget-searchoptions");

            if (this.options.searchOptions != null) {
                this._displayHeader();
                this._displayOptions(this.options.searchOptions);
                this._displayFooter();
            }
        },

        _displayHeader: function() {
            $(document.createElement('div'))
                .addClass('header')
                .text('Options')
                .appendTo(this.element);
        },

        _displayOptions: function (options) {
            this._addOption('Hits Limit', options.getHitsLimit());

            if (options.hasDatasources()) {
                this._addOption('Datasources', options.getDatasourcesNames().join(', '));
            }
            if (options.options.searchScopes.realOnly) {
                this._addOption('Real Only', 'True');
            }
        },

        _displayFooter: function () {
            $(document.createElement('div'))
                .addClass('footer')
                .append(
                    $(document.createElement('a'))
                        .attr({
                            href: this.options.settingsUrl
                        })
                        .text('more...')
                )
                .appendTo(this.element);
        },

        _addOption: function (title, value) {
            $(document.createElement('div'))
                .addClass('row')
                .append(
                    $(document.createElement('div'))
                        .addClass('col-md-2 title')
                        .text(title)
                )
                .append(
                    $(document.createElement('div'))
                        .addClass('col-md-10 value')
                        .text(value)
                )
                .appendTo(this.element);
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
            this.element.empty();

            this.element.removeClass("cs-widget-searchoptions");
            this.element.removeClass("cs-widget");
        },

        // _setOption is called for each individual option that is changing
        _setOption: function (key, value) {
            // In jQuery UI 1.8, you have to manually invoke the _setOption method from the base widget
            $.Widget.prototype._setOption.apply(this, arguments);
            // In jQuery UI 1.9 and above, you use the _super method instead
            this._super("_setOption", key, value);
        }
    });

}(jQuery));