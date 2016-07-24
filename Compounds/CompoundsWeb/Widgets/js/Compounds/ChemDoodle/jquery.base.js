(function ($) {
    $.widget("compound.chemdoodle_base", $.compound.compoundbase, {

        store: $.Compounds.store,

        // Set up the widget
        _create: function () {
            var oThis = this;

            if ($('meta[http-equiv="X-UA-Compatible"]', $('head')).length == 0) {
                $(document.createElement('meta'))
                    .attr({
                        'http-equiv': 'X-UA-Compatible',
                        content: 'chrome=1'
                    })
                    .appendTo($('head'));
            }

            //this.canvas = $(document.createElement('canvas'))
            //                .attr({
            //                    id: 'chemdoodle_' + Math.floor((Math.random() * 10000) + 1)
            //                })
            //                .appendTo(this.element);

            $('body').loadScript($.Compounds.store.baseUrl + '/Widgets/3rd/chemdoodle/6.0.0/ChemDoodleWeb.js', function () {
                oThis._onChemDoodleReady();
            });
        },

        _onChemDoodleReady: function () {
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
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
