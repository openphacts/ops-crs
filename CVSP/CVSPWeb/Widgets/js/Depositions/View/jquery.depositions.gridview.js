(function ($) {
    $.widget("depositions.depositions_gridview", $.toolkit.gridview, {

        options: {
            title: 'Depositions',
            columns: [
                { name: 'Id', title: 'ID' },
                { name: 'FileName', title: 'File Name' },
            ]
        },

        _create: function () {
            this._super();

            this.element.addClass('cvsp-depositions-gridview');
        },

        _prepareContent: function (callback) {
            var oThis = this;

            $.Depositions.store.getDepositionsGUIDs(0, -1, function (depositions) {
                oThis.options.items = depositions;

                callback(depositions.length);
            });
        },

        //  transform depositions' IDs to the array of depositions' objects
        _transformItems: function (ids, callback) {
            var oThis = this;

            $.Depositions.store.getDepositionsByGUID(ids, function (depositions) {
                callback(depositions);
            });
        },
    });

}(jQuery));
