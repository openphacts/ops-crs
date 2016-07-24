$(function () {

    $.Records = $.Records || {};

    $.Records.Store = function (url) {
        return $.extend({

            getRecord: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/records/' + guid)
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getRecords", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getRecords", xhr, error, thrownError);
                });
            },

            getRecordIssues: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/records/' + guid + '/issues')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getRecordIssues", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getRecordIssues", xhr, error, thrownError);
                });
            },

            getRecordFields: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/records/' + guid + '/fields')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getRecordFields", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getRecordFields", xhr, error, thrownError);
                });
            },

            getRecordProperties: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/records/' + guid + '/properties')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getRecordProperties", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getRecordProperties", xhr, error, thrownError);
                });
            },

            getRecordChemSpiderProperties: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/records/' + guid + '/chemspiderproperties')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getRecordChemSpiderProperties", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getRecordChemSpiderProperties", xhr, error, thrownError);
                });
            },

            getRecordChemSpiderSynonyms: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/records/' + guid + '/chemspidersynonyms')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getRecordChemSpiderSynonyms", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getRecordChemSpiderSynonyms", xhr, error, thrownError);
                });
            },

            getRecordsByGUID: function (ids, filter, callback, error_callback) {
                var oThis = this;

                if (ids == null || ids.length == 0) {
                    callback([]);
                    return;
                }

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/records/list?filter=' + filter),
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
                        error_callback("getRecordsByGUID", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getRecordsByGUID", xhr, error, thrownError);
                });
            },

            recordsSearch: function (options, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/searches/records'),
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
                        error_callback("recordsSearch", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("recordsSearch", xhr, error, thrownError);
                });
            },

            getSearchStatus: function (rid, callback, error_callback) {
                $.Search.store.getSearchStatus(rid, callback, error_callback);
            },

            getSearchResults: function (params, callback, error_callback) {
                $.Search.store.getSearchResults(params, callback, error_callback);
            },

            waitForSearchResults: function (rid, callback, error_callback) {
                $.Search.store.waitForSearchResults(rid, callback, error_callback);
            },

            waitAndGetSearchResults: function (rid, callback, error_callback) {
                $.Search.store.waitAndGetSearchResults(rid, callback, error_callback);
            },

        }, $.Toolkit.Store());
    };

}(jQuery));

$.Records.store = $.Records.Store();
