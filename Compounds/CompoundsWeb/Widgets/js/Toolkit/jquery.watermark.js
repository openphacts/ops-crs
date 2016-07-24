(function ($) {
    $.widget("toolkit.watermark", {

        options: {
            hint: ''
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-watermark');

            $(this.element).focus(function () {
                if ($(this).val() == oThis.options.hint) {
                    $(this).removeClass('hint').val('');
                }
            });

            $(this.element).blur(function () {
                if (!$(this).val()) {
                    $(this).addClass('hint').val(oThis.options.hint);
                }
            });

            if(!$(this.element).val())
                $(this.element).addClass('hint').val(this.options.hint)
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
            this.element.removeClass("cs-widget-watermark");
        }
    });

}(jQuery));
