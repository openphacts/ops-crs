$(function () {

    $.Profiles = $.Profiles || {};

    $.Profiles.Store = function (url) {
        return $.extend({

            isAuthenticated: function (callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/profiles/authenticated')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("isAuthenticated", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("isAuthenticated", xhr, error, thrownError);
                });
            },

            isAdmin: function (callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/profiles/isadmin')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("isAdmin", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("isAdmin", xhr, error, thrownError);
                });
            },

            getProfile: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/profiles/' + guid)
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getProfile", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getProfile", xhr, error, thrownError);
                });
            },

            updateProfile: function (guid, profile, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "PUT",
                    url: this.getDataUrl('api/profiles/' + guid),
                    data: JSON.stringify(profile),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (result) {
                    if (callback != null)
                        callback(result);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("updateProfile", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("updateProfile", xhr, error, thrownError);
                });
            },

        }, $.Toolkit.Store());
    };

}(jQuery));

$.Profiles.store = $.Profiles.Store();
