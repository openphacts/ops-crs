(function ($) {
    $.widget("compound.substances_tileview", $.toolkit.tileview, {

        options: {
            count: 0,
            tooltipFields: [
                { name: 'SUBSTANCE_ID', title: 'ID' },
                { name: 'SUBSTANCE_VERSION', title: 'Version' },
                { name: 'DEPOSITOR_DATA_SOURCE_NAME', title: 'Data Source' },
                { name: 'DEPOSITOR_SUBSTANCE_REGID', title: 'Registry ID' }
            ]
        },

        //  transform substances' IDs to the array of substances' objects
        _transformItems: function (ids, callback) {
            var oThis = this;

            $.Substances.store.getSubstancesByID(ids, function (substances) {
                callback(substances);
            });
        },

        _drawCell: function (substance) {
            var oThis = this;
            var cell = $(document.createElement('div'))
                .substance2d({
                    width: this.options.imageSize,
                    height: this.options.imageSize,
                    id: substance.SUBSTANCE_ID,
                    click: function (cmp) {
                        oThis._trigger("onClick", null, cmp.id);
                    }
                })
                .data('ID', substance['SUBSTANCE_ID'])
                .width(this.options.cellSize)
                .height(this.options.cellSize)
                .addClass('cs-tileview-cell thumbnail')
                .appendTo(this.contentDiv);

            for (var i = 0; i < this.options.tooltipFields.length; i++) {
                var field = this.options.tooltipFields[i];

                cell.data(field.name, substance[field.name]);
            }
        },

        _tooltipUrl: function (id) {
            return $.Substances.store.getImageUrl(id, this.options.tooltipSize, this.options.tooltipSize)
        }
    });
}(jQuery));
