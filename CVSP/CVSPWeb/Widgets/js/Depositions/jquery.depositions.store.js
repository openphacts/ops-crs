$(function () {

    $.Depositions = $.Depositions || {};

    $.Depositions.Store = function (url) {
        return $.extend({

            getDeposition: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/' + guid)
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDeposition", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDeposition", xhr, error, thrownError);
                });
            },

            getDepositionFiles: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/' + guid + '/files')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionFiles", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionFiles", xhr, error, thrownError);
                });
            },

            getDepositionRecords: function (guid, start, count, callback, error_callback) {
            	var oThis = this;

            	var params = {
            		start: start,
            		count: count
            	};

            	$.ajax({
            		type: 'GET',
            		dataType: "jsonp",
            		url: this.getDataUrl('api/depositions/' + guid + '/records'),
					data: params
            	})
                .done(function (data) {
                	if (callback != null)
                		callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                	if (error_callback != null)
                		error_callback("getDepositionRecords", xhr, error, thrownError);
                	else if (oThis.onError != null)
                		oThis.onError("getDepositionRecords", xhr, error, thrownError);
                });
            },

            getDepositionFields: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/' + guid + '/fields')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionFields", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionFields", xhr, error, thrownError);
                });
            },

            getDepositionAnnotations: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/' + guid + '/annotations')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionAnnotations", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionAnnotations", xhr, error, thrownError);
                });
            },


            getDepositionStatus: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/' + guid + '/status')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionStatus", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionStatus", xhr, error, thrownError);
                });
            },

            getDepositionStatistic: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/' + guid + '/stats')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionStatistic", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionStatistic", xhr, error, thrownError);
                });
            },

            getDepositionChunksStatistic: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/' + guid + '/chunks/stats')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionChunksStatistic", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionChunksStatistic", xhr, error, thrownError);
                });
            },

            getJobsStatistic: function (guid, callback, error_callback) {
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
                        error_callback("getJobsStatistic", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getJobsStatistic", xhr, error, thrownError);
                });
            },

            newDeposition: function (data, callback, error_callback) {
                var oThis = this;

                if (!data.Files || data.Files.length == 0) {
                    return false;
                }

                var formData = new FormData();

                for (var key in data) {
                    if (key == "Files") {
                        for (var i = 0; i < data.Files.length; i++)
                            formData.append('deposition-file[]', data.Files[i]);
                    }
                    else {
                        formData.append(key, data[key]);
                    }
                }

                $.ajax({
                    type: "POST",
                    url: this.getUrl('/api/depositions'),
                    contentType: false,
                    processData: false,
                    data: formData
                })
                .done(function (res) {
                    if (callback != null)
                        callback(res);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("uploadLogo", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("uploadLogo", xhr, error, thrownError);
                });
            },

            deleteDeposition: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'DELETE',
                    url: this.getDataUrl('api/depositions/' + guid)
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("deleteDeposition", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("deleteDeposition", xhr, error, thrownError);
                });
            },

            deleteDepositionFromGCN: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'DELETE',
                    url: this.getDataUrl('api/depositions/' + guid + '/gcn')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("deleteDepositionFromGCN", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("deleteDepositionFromGCN", xhr, error, thrownError);
                });
            },

            depositToGCN: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'PUT',
                    url: this.getDataUrl('api/depositions/' + guid + '/deposit2gcn')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("depositToGCN", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("depositToGCN", xhr, error, thrownError);
                });
            },

            getDepositionsGUIDsByURL: function (url, start, count, callback, error_callback) {
                var oThis = this;

                var params = {
                    start: start,
                    count: count
                };

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: url,
                    data: params
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionsGUIDsByURL", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionsGUIDsByURL", xhr, error, thrownError);
                });
            },

            getDepositionsGUIDs: function (start, count, callback, error_callback) {
                this.getDepositionsGUIDsByURL(this.getDataUrl('api/depositions/guids'), start, count, callback, error_callback);
            },

            getDepositionsByGUID: function (ids, callback, error_callback) {
                var oThis = this;

                var params = {
                    id: ids
                };

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/depositions/list'),
                    data: params
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDepositionsByGUID", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDepositionsByGUID", xhr, error, thrownError);
                });
            },

        }, $.Toolkit.Store());
    };

}(jQuery));

$.Depositions.store = $.Depositions.Store();
