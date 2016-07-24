(function ($) {
    $.widget("compound.substances_scroller", $.toolkit.scroller, {

        options: {
            drawCell: function (item) {
                return $(document.createElement('div'))
                            .substancetile({
                                item: item,
                                width: 200,
                                properties: [
                                    { name: 'SUBSTANCE_ID', title: 'ID', format: function (value) { return $(document.createElement('a')).attr({ href: '/Substances/' + value }).text(value) } },
                                    { name: 'DEPOSITOR_DATA_SOURCE_NAME', title: 'Datasource' },
                                    { name: 'DEPOSITOR_SUBSTANCE_REGID', title: 'Reg. ID' }
                                ]
                            });
            },
        }
    });

}(jQuery));
