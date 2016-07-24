(function ($) {
    $.widget("toolkit.tileview", $.toolkit.pageview, {

        options: {
            tooltipSize: 150,
            showTooltip: true,
            tooltipFields: [],
            title: 'Results'
        },

        _onInit: function () {
            this._super();

            this.element.addClass('cs-widget-tileview');
        },

        _drawItems: function (items) {
            var oThis = this;

            this._transformItems(items, function (transformedItems) {
                oThis.contentDiv.empty();

                for (var i = 0; i < transformedItems.length; i++) {
                    oThis._drawCell(transformedItems[i]);
                }

                $(document.createElement('br'))
                            .css('clear', 'both')
                            .appendTo(oThis.contentDiv);

                if (oThis.options.showTooltip)
                    oThis._showTooltip();

                oThis._trigger("onPageRendered");
            });
        },

        //  very often widget will operate with items' IDs and this IDs will be used later to extract the information for the representation.
        //  so this function is responsible for tranforming some items to another... by default it doesn't do anythin abd just pass the same items further. 
        _transformItems: function (items, callback) {
            if (callback)
                callback(items);
        },

        _drawCell: function (item) {
            //  **********************************************************
            //  override this method in order to implement cells rendering
            //  **********************************************************

            $('body').showMessage('Error', '_drawCell is not implemented!');
        },

        _showTooltip: function () {
            var oThis = this;

            $('.cs-tileview-cell', this.element).popover({
                trigger: 'hover',
                html: true,
                placement: 'right',
                delay: {
                    show: 500,
                    hide: 100
                },
                content: function () {
                    var properties = $(document.createElement('ul'))
                                        .addClass('properties');

                    for (var i = 0; i < oThis.options.tooltipFields.length; i++) {
                        var property = oThis.options.tooltipFields[i];

                        var value = $(this).data(property.name);

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
                            .appendTo(properties);
                        }
                    }

                    var image = $(document.createElement('img'))
                                    .attr({
                                        src: oThis._tooltipUrl($(this).data('ID'))
                                    })
                                    .width(oThis.options.tooltipSize)
                                    .height(oThis.options.tooltipSize);

                    return image.outerHTML() + properties.outerHTML();
                }
            });
        },

        _tooltipUrl: function (id) {
            //  override this method if you wish to use tooltip...
        }
    });

}(jQuery));
