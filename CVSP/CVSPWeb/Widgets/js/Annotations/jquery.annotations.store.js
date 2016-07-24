$(function () {

    $.Annotations = $.Annotations || {};

    $.Annotations.Store = function (url) {
        return $.extend({

            getAllAnnotations: function (callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/annotations/all')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getAllAnnotations", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getAllAnnotations", xhr, error, thrownError);
                });
            },

            annotateDeposition: function (guid, field, annotation, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "PUT",
                    url: this.getDataUrl('api/depositions/' + guid + '/annotate'),
                    data: JSON.stringify({ field: field, annotation: annotation }),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (result) {
                    if (callback != null)
                        callback(result);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("annotateDeposition", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("annotateDeposition", xhr, error, thrownError);
                });
            },

            deleteDepositionAnnotation: function (guid, annotation, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "DELETE",
                    url: this.getDataUrl('api/depositions/' + guid + '/annotation/' + annotation)
                })
                .done(function (result) {
                    if (callback != null)
                        callback(result);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("deleteDepositionAnnotation", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("deleteDepositionAnnotation", xhr, error, thrownError);
                });
            },

        }, $.Toolkit.Store());
    };

}(jQuery));

$.Annotations.store = $.Annotations.Store();
