$(function () {

    $.Chunks = $.Chunks || {};

    $.Chunks.Store = function (url) {
        return $.extend({

            getChunk: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/chunks/' + guid)
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getChunk", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getChunk", xhr, error, thrownError);
                });
            },

            getDepositionChunks: function (guid, type, start, count, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/' + guid + '/chunks'),
                    data: {
                        type: type,
                        start: start,
                        count: count
                    }
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionChunks", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionChunks", xhr, error, thrownError);
                });
            },

            getChunksByGUID: function (ids, callback, error_callback) {
                var oThis = this;

                var params = {
                    id: ids
                };

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/chunks/list'),
                    data: JSON.stringify(ids),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getChunksByGUID", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getChunksByGUID", xhr, error, thrownError);
                });
            },

        }, $.Toolkit.Store());
    };

}(jQuery));

$.Chunks.store = $.Chunks.Store();
