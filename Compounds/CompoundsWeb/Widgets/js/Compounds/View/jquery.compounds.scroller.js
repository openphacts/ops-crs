(function ($) {
    $.widget("compound.compounds_scroller", $.toolkit.scroller, {

        options: {
            drawCell: function (item) {
                return $(document.createElement('div'))
                            .compoundtile({
                                item: item,
                                width: 200,
                                properties: [
                                    { name: 'COMPOUND_ID', title: 'ID', format: function (value) { return $(document.createElement('a')).attr({ href: '/Compounds/' + value }).text(value) } },
                                    { name: 'IsVirtual', title: 'Virtual', format: function (virtual) { return virtual ? 'Yes' : 'No'; } },
                                    { name: 'MolecularFormula', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } },
                                    { name: 'CompoundTypes', title: 'Type', format: buildCompoundTypesProperty }
                                ]
                            });
            }
        }
    });

}(jQuery));
