(function ($) {
    $.widget("compound.similarities", $.toolkit.gridview, {

        options: {
            id: 0,
            threshold: 0.75,
            columns: [
                { name: 'COMPOUND_ID', title: 'ID' },
                { name: 'Structure', title: 'Structure' },
                { name: 'MolecularFormula', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } },
                { name: 'Indigo_MolecularWeight', title: 'Molecular Weight', format: function (value) { return parseFloat(value).toFixed(3); } },
                { name: 'COMPOUND_ID', title: 'Score', format: function (id, widget) { return widget.getScore(id); } },
            ],
            tooltipFields: [
                { name: 'COMPOUND_ID', title: 'ID' },
                { name: 'MolecularFormula', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } },
            ]
        },

        //  prepare list of similar compounds and calculate the total records count
        _prepareContent: function (callback) {
            var oThis = this;

            if (this.options.id > 0) {
                this.element.loadProgress();

                $.Compounds.store.getSimilarities(this.options.id, this.options.threshold, function (similarities) {
                    oThis.similarities = similarities;

                    oThis.element.hideProgress();

                    if (callback)
                        callback(similarities.length);

                    oThis._trigger("onResultsReady", null, oThis);
                });
            }
        },

        _loadPage: function (start, count, callback) {
            var ids = [];
            for (var i = start; i < start + count && i < this.similarities.length; i++) {
                ids.push(this.similarities[i].ID);
            }

            $.Compounds.store.getCompoundsByID(ids, function (compounds) {
                callback(compounds);
            });
        },

        _drawCell: function (column, compound) {
            var oThis = this;

            var td = $(document.createElement('td'))

            if (column.name == 'Structure') {
                var div = $(document.createElement('div'))
                            .addClass('cs-gridview-structure')
                            .molecule2d({
                                width: this.options.imgSize,
                                height: this.options.imgSize,
                                id: compound.COMPOUND_ID,
                                allowZoom: false,
                                click: function (cmp) {
                                    oThis._trigger("onClick", null, cmp.id);
                                }
                            })
                            .data('ID', compound.COMPOUND_ID)
                            .addClass('thumbnail')
                            .appendTo(td);

                for (var j = 0; j < this.options.tooltipFields.length; j++) {
                    var field = this.options.tooltipFields[j];

                    div.data(field.name, compound[field.name]);
                }
            }
            else {
                var value = compound[column.name];
                if (value != null) {
                    td.html(column.format != null ? column.format(compound[column.name], this) : compound[column.name]);
                }
            }

            return td;
        },

        getScore: function (id) {
            for (var i = 0; i < this.similarities.length; i++) {
                if (this.similarities[i].ID == id)
                    return this.similarities[i].Score;
            }

            return '';
        }
    });

}(jQuery));
