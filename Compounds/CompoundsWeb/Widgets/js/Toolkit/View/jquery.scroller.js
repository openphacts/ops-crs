(function ($) {
    $.widget("toolkit.scroller", $.toolkit.toolkitbase, {

        options: {
            items: [],
            size: 100,
            visibleItems: 4,
            speed: 200,
            drawCell: function (item) {
                //  **********************************************************
                //  override this method in order to implement cells rendering
                //  **********************************************************

                $('body').showMessage('Error: toolkit.scroller', 'drawCell is not implemented!');
            }
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.wrapper = $(document.createElement('ul')).addClass('als-wrapper');

            this.element
                .addClass('cs-widget cs-widget-scroller als-container')
                .append(
                    $(document.createElement('span'))
                        .addClass('als-prev')
                        .append(
                            $(document.createElement('img'))
                                .attr({
                                    src: $.Toolkit.store.getLibraryUrl('css/images/thin-left-arrow.png'),
                                    alt: 'prev',
                                    title: 'previous'
                                })
                        )
                        .css({
                            top: (this.options.size - 15) / 2
                        })
                )
                .append(
                    $(document.createElement('div'))
                        .addClass('als-viewport')
                        .append(this.wrapper)
                )
                .append(
                    $(document.createElement('span'))
                        .addClass('als-next')
                        .append(
                            $(document.createElement('img'))
                                .attr({
                                    src: $.Toolkit.store.getLibraryUrl('css/images/thin-right-arrow.png'),
                                    alt: 'next',
                                    title: 'next'
                                })
                        )
                        .css({
                            top: (this.options.size - 15) / 2
                        })
                );

            this.setItems(this.options.items);
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

        _destroy: function () {
            this.element.removeClass("cs-widget cs-widget-scroller");
        },

        setItems: function (items) {
            var oThis = this;

            this.wrapper.empty();

            if (items != null && items.length > 0) {
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];

                    var li = $(document.createElement('li'))
                        .addClass('als-item thumbnail')
                        .css({
                            'min-width': this.options.size,
                            'min-height': this.options.size,
                        })
                        .append(
                            this.options.drawCell(item).width(this.options.size + 15)
                        )
                        .append(
                            $(document.createElement('span'))
                                .addClass('number')
                                .text(i + 1)
                        )
                        .appendTo(this.wrapper);
                }

                if (items.length > 3) {
                    this.element.als({
                        visible_items: this.options.visibleItems,
                        scrolling_items: 1,
                        orientation: "horizontal",
                        circular: "no",
                        autoscroll: "no",
                        //interval: 6000,
                        speed: this.options.speed,
                        //easing: "linear",
                        //direction: "right",
                        //start_from: 1
                    });

                    $('span', this.element).show();
                }
                else {
                    $('span.als-prev', this.element).hide();
                    $('span.als-next', this.element).hide();
                }
            }
            else {
                $('span', this.element).hide();
            }
        },
    });

}(jQuery));
