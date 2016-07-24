$(function () {
    $.widget("layout.areastats", {
        _create: function () {
            var oThis = this;

            this.console = $(document.createElement('div'))
                .css({
                    position: 'absolute',
                    bottom: '0px',
                    right: '0px',
                    color: 'gray'
                })
                .text('some text just to trigger livequery')
                .appendTo(this.element);

            this.element.closest('.layout-area').on('area-resize', function (event, params) {
                oThis._dumpStats();

                event.stopPropagation();
            });

            $(':visible', this.element).livequery(function () {
                oThis._dumpStats();
            });
        },

        _dumpStats: function() {
            this.console.text(this.element.outerWidth() + 'x' + this.element.outerHeight());
        }
    });
}(jQuery));
