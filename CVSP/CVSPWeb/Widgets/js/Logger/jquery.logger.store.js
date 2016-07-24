$(function () {

    $.Logger = $.Logger || {};

    $.Logger.EntryTypes = [];

    $.Logger.Store = function (url) {
        return $.extend({

            getEntryTypes: function (callback, error_callback) {
                var oThis = this;

                //  try to get log entry types from cache...
                $.Logger.EntryTypes = $.sessionStorage.get('logEntryTypes');

                if ($.Logger.EntryTypes == null) {
                    $.ajax({
                        type: 'GET',
                        dataType: "jsonp",
                        url: this.getDataUrl('api/logger/entrytypes')
                    })
                    .done(function (data) {
                        //  cache log entry types...
                        $.sessionStorage.set('logEntryTypes', data);

                        $.Logger.EntryTypes = data;

                        if (callback != null)
                            callback(data);
                    })
                    .fail(function (xhr, error, thrownError) {
                        if (error_callback != null)
                            error_callback("getEntryTypes", xhr, error, thrownError);
                        else if (oThis.onError != null)
                            oThis.onError("getEntryTypes", xhr, error, thrownError);
                    });
                }
                else {
                    if (callback != null)
                        callback($.Logger.EntryTypes);
                }
            },

            getEntries: function (ids, callback, error_callback) {
                var oThis = this;

                if (ids == null || ids.length == 0) {
                    callback([]);
                    return;
                }

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/logger/entries'),
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
                        error_callback("getEntries", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getEntries", xhr, error, thrownError);
                });
            },
        }, $.Toolkit.Store());
    };

    $.Logger.getEntryTypeByCode = function (code) {
        if($.Logger.EntryTypes == null)
            return null;

        for (var i = 0; i < $.Logger.EntryTypes.length; i++) {
            if ($.Logger.EntryTypes[i].Code == code)
                return $.Logger.EntryTypes[i];
        }

        return null;
    };

    $.Logger.getEntryTypeById = function (id) {
        if ($.Logger.EntryTypes == null)
            return null;

        for (var i = 0; i < $.Logger.EntryTypes.length; i++) {
            if ($.Logger.EntryTypes[i].Id == id)
                return $.Logger.EntryTypes[i];
        }

        return null;
    };

}(jQuery));

$.Logger.store = $.Logger.Store();

$.Logger.store.getEntryTypes();
