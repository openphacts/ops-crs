$(function () {

    $.Jobs = $.Jobs || {};

    $.Jobs.Store = function (url) {
        return $.extend({

            getJob: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/jobs/' + guid)
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getJob", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getJob", xhr, error, thrownError);
                });
            },

            restartJob: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'PUT',
                    url: this.getDataUrl('api/jobs/' + guid + '/restart')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("restartJob", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("restartJob", xhr, error, thrownError);
                });
            },

            getDepositionJobStats: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/' + guid + '/jobstats')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionJobStats", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionJobStats", xhr, error, thrownError);
                });
            },

            getJobsByGUID: function (ids, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/jobs/list'),
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
                        error_callback("getJobsByGUID", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getJobsByGUID", xhr, error, thrownError);
                });
            },

            jobsSearch: function (options, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/searches/jobs'),
                    data: JSON.stringify(options),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("jobsSearch", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("jobsSearch", xhr, error, thrownError);
                });
            },

        }, $.Toolkit.Store());
    };

}(jQuery));

$.Jobs.store = $.Jobs.Store();
