(function ($) {
    $.widget("compound.compounds_gridview", $.toolkit.gridview, {

        options: {
            imageSize: 100,
            count: 0,
            columns: [
                { name: 'Id', title: 'ID' },
                { name: 'Structure', title: 'Structure' },
                { name: 'MolecularFormula', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } },
                { name: 'Indigo_MonoisotopicMass', title: 'Monoisotopic Mass', format: function (value) { return parseFloat(value).toFixed(6) + ' Da'; } },
                { name: 'Indigo_MolecularWeight', title: 'Molecular Weight', format: function (value) { return parseFloat(value).toFixed(3) + ' Da'; } }
            ],
            tooltipFields: [
                { name: 'ID', title: 'ID' },
                { name: 'MF', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } }
            ]
        },

        //  transform compounds' IDs to the array of compounds' objects
        _transformItems: function (ids, callback) {
            var oThis = this;

            $.Compounds.store.getCompoundsByID(ids, function (compounds) {
                callback(compounds);
            });
        },

        _drawCell: function (column, compound) {
            var td = $(document.createElement('td'))

            if (column.name == 'Structure') {
                td.append(this._drawStructure(column, compound));
            }
            else {
                var value = compound[column.name];
                if (value != null) {
                    td.html(column.format != null ? column.format(compound[column.name]) : compound[column.name]);
                }
            }

            return td;
        },

        _drawStructure: function (column, compound) {
            var oThis = this;

            var div = $(document.createElement('div'))
                .addClass('cs-gridview-structure')
                .molecule2d({
                    width: this.options.imageSize,
                    height: this.options.imageSize,
                    id: compound.Id,
                    allowZoom: false,
                    //click: function (cmp) {
                    //    oThis._trigger("onClick", null, cmp.id);
                    //}
                })
                .data('ID', compound.Id)
                .addClass('thumbnail');

            for (var j = 0; j < this.options.tooltipFields.length; j++) {
                var field = this.options.tooltipFields[j];

                div.data(field.name, compound[field.name]);
            }

            return div;
        },

        _tooltipUrl: function (id) {
            return $.Compounds.store.getImageUrl(id, this.options.tooltipSize, this.options.tooltipSize)
        }
    });

}(jQuery));
