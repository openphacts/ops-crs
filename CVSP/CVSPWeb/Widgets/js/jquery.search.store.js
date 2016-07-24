$(function () {

    $.Search = $.Search || {};

    $.Search.Store = function (url) {
        return $.extend({

            getSearchStatus: function (rid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/searches/' + rid + '/status')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSearchStatus", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getSearchStatus", xhr, error, thrownError);
                });
            },

            getSearchResults: function (params, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/searches/' + params.rid + '/result'),
                    data: {
                        start: params.start,
                        count: params.count
                    }
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSearchResults", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getSearchResults", xhr, error, thrownError);
                });
            },

            waitForSearchResults: function (rid, callback, error_callback) {
                var oThis = this;

                this.getSearchStatus(rid, function (status) {
                    if (status == null) {
                        $('body').showMessage('Error', 'Cannot get search status for RID: ' + rid);
                        return;
                    }

                    if (status.Status == 'Processing') {   //  Processing
                        //  ... not ready yet. Check it later again...
                        setTimeout(function () {
                            oThis.waitForSearchResults(rid, callback, error_callback);
                        }, 1000);
                    }
                    else if (status.Status == 'ResultReady') {   //  Results Ready
                        if (callback != null)
                            callback(status);
                    }
                    else if (status.Status == 'TooManyRecords') {   //  Too many records found
                        if (callback != null)
                            callback(status);
                    }
                    else {
                        $('body').showMessage('Error', status.Message);
                    }
                },
                error_callback);
            },

            waitAndGetSearchResults: function (rid, callback, error_callback) {
                var oThis = this;

                this.getSearchStatus(rid, function (status) {
                    if (status.Status == 'Processing') {   //  Processing
                        //  ... not ready yet. Check it later again...
                        setTimeout(function () {
                            oThis.waitAndGetSearchResults(rid, callback, error_callback);
                        }, 1000);
                    }
                    else if (status.Status == 'ResultReady') {   //  Results Ready
                        oThis.getSearchResults({ rid: rid }, callback, error_callback);
                    }
                    else {
                        $('body').showMessage('Error', status.Message);
                    }
                },
                error_callback);
            },

        }, $.Toolkit.Store());
    };

}(jQuery));

$.Search.store = $.Search.Store();
