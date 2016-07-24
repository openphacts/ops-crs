$(function () {

    $.Substances = $.Substances || {};

    $.Substances.Store = function () {
        return $.extend({

            getImageUrl: function (id, width, height) {
                return this.getDataUrl('api/image/substance/' + id, {
                    width: width,
                    height: height
                });
            },

            getSubstance: function (params, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/Substances/' + params.id),
                    data: params
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSubstance", xhr, error, thrownError);
                    else
                        this.onError("getSubstance", xhr, error, thrownError);
                });
            },

            getSubstancesByID: function (ids, callback, error_callback) {
                var oThis = this;

                var params = {
                    id: ids
                };

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/Substances/List'),
                    data: params
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSubstancesByID", xhr, error, thrownError);
                    else
                        this.onError("getSubstancesByID", xhr, error, thrownError);
                });
            },

            getSubstances: function (start, count, callback, error_callback) {
                var oThis = this;

                var params = {
                    start: start,
                    count: count
                };

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/Substances'),
                    data: params
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSubstances", xhr, error, thrownError);
                    else
                        this.onError("getSubstances", xhr, error, thrownError);
                });
            },
        }, $.Toolkit.Store());
    };

}(jQuery));

$.Substances.store = $.Substances.Store();
