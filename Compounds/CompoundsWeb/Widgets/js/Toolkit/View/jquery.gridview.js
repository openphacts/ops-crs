(function ($) {
    $.widget("toolkit.gridview", $.toolkit.pageview, {

        options: {
            tooltipSize: 250,
            showTooltip: true,
            pageSize: 10,
            pageSizes: [5, 10, 15, 30, 50],
            columns: [],
            tooltipFields: [],
            title: 'Results',
            onClick: function (event, id) {
            }
        },

        _onInit: function () {
            this._super();

            this.element.addClass('cs-widget-gridview');

            this.resultsHead = $(document.createElement('thead'));
            this.resultsBody = $(document.createElement('tbody'));

            this.table = $(document.createElement('table'))
                .addClass('table table-striped table-hover table-condensed')
                .append(this.resultsHead)
                .append(this.resultsBody)
                .appendTo(this.contentDiv);

            this._generateTableHead();
        },

        _generateTableHead: function () {
            var tr = $(document.createElement('tr'));

            for (var i = 0; i < this.options.columns.length; i++) {
                var column = this.options.columns[i];
                tr.append(
                    $(document.createElement('th')).attr({'data-name': column.name}).text(column.title)
                );
            }

            tr.appendTo(this.resultsHead);
        },

        _drawItems: function (items) {
            var oThis = this;

            this._transformItems(items, function (transformedItems) {
                oThis.resultsBody.empty();

                for (var i = 0; i < transformedItems.length; i++) {
                    oThis._drawRaw(transformedItems[i]);
                }

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

        _drawRaw: function (item) {
            var oThis = this;

            var tr = $(document.createElement('tr'))
                        .click(function () {
                            oThis._trigger("onClick", null, item);
                        })
                        .appendTo(this.resultsBody);

            for (var i = 0; i < this.options.columns.length; i++) {
                var column = this.options.columns[i];

                tr.append(this._drawCell(column, item));
            }
        },

        _drawCell: function (column, item) {
            //  *************************************************************************
            //  override this method in order to implement custom table's cells rendering
            //  *************************************************************************

            var td = $(document.createElement('td'))

            var value = item[column.name];
            if (value != null) {
                td.html(column.format != null ? column.format(item[column.name], item) : item[column.name]);
            }

            return td;
        },

        _showTooltip: function () {
            var oThis = this;

            $('.cs-gridview-structure', this.element).popover({
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
                                property.format != null ? property.format(value, this) : value
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
            //  override this method if you wish to use compound's tooltip...
        },

        showColumn: function (name) {
            var index = $('th[data-name="' + name + '"]', this.resultsHead).show().index();

            $('tr', this.resultsBody).each(function () {
                $('td', this).get(index).show();
            });
        },

        hideColumn: function (name) {
            var index = $('th[data-name="' + name + '"]', this.resultsHead).hide().index();

            $('tr', this.resultsBody).each(function () {
                $('td', this).get(index).hide();
            });
        }
    });

} (jQuery));
