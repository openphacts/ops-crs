(function ($) {
    $.widget("chunks.chunks_gridview", $.toolkit.gridview, {

        options: {
            title: 'Chunks',
            deposition: null,
            type: 'All',
            columns: [
                { name: 'Id', title: 'ID' },
                { name: 'Deposition', title: 'Deposition' },
                { name: 'NumberOfRecords', title: '# of Records' },
                { name: 'Type', title: 'Type' },
                { name: 'Status', title: 'Status' },
            ]
        },

        _create: function () {
            this._super();

            this.element.addClass('cvsp-chunks-gridview');
        },

        _prepareContent: function (callback) {
            var oThis = this;

            $.Chunks.store.getDepositionChunks(this.options.deposition, this.options.type, 0, -1, function (chunks) {
                oThis.options.items = chunks;

                callback(chunks.length);
            });
        },

        //  transform chunks' IDs to the array of chunks' objects
        _transformItems: function (ids, callback) {
            var oThis = this;

            $.Chunks.store.getChunksByGUID(ids, function (chunks) {
                callback(chunks);
            });
        }
    });

}(jQuery));
