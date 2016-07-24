(function ($) {
    $.widget("compound.compounds_tileview", $.toolkit.tileview, {

        options: {
            imageSize: 100,
            count: 0,
            tooltipFields: [
                { name: 'Id', title: 'ID' },
                //{ name: 'MolecularFormula', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } },
                //{ name: 'Indigo_MonoisotopicMass', title: 'Monoisotopic Mass', format: function (value) { return parseFloat(value).toFixed(6) + ' Da'; } },
                //{ name: 'Indigo_MolecularWeight', title: 'Molecular Weight', format: function (value) { return parseFloat(value).toFixed(3) + ' Da'; } }
            ]
        },

        //  transform compounds' IDs to the array of compounds' objects
        _transformItems: function (ids, callback) {
            var oThis = this;

            $.Compounds.store.getCompoundsByID(ids, function (compounds) {
                callback(compounds);
            });
        },

        _drawCell: function (compound) {
            var oThis = this;
            var cell = $(document.createElement('div'))
                .molecule2d({
                    width: this.options.imageSize,
                    height: this.options.imageSize,
                    id: compound.Id,
                    click: function (cmp) {
                        oThis._trigger("onClick", null, cmp.id);
                    }
                })
                .data('ID', compound['Id'])
                .width(this.options.imageSize + 'px')
                .height(this.options.imageSize + 'px')
                .addClass('cs-tileview-cell thumbnail')
                .appendTo(this.contentDiv);

            for (var i = 0; i < this.options.tooltipFields.length; i++) {
                var field = this.options.tooltipFields[i];

                cell.data(field.name, compound[field.name]);
            }
        },

        _tooltipUrl: function (id) {
            return $.Compounds.store.getImageUrl(id, this.options.tooltipSize, this.options.tooltipSize)
        }
    });
}(jQuery));
