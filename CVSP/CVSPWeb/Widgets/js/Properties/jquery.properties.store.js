$(function () {

    $.Properties = $.Properties || {};

    $.Properties.Store = function (url) {
        return $.extend({

            getUnits: function (callback, error_callback) {
                var oThis = this;

                //  try to get units from cache...
                var units = $.sessionStorage.get('propertyUnits');

                if (units == null) {
                    $.ajax({
                        type: 'GET',
                        dataType: "jsonp",
                        url: this.getDataUrl('api/properties/units')
                    })
                    .done(function (data) {
                        //  cache units list...
                        $.sessionStorage.set('propertyUnits', data);

                        if (callback != null)
                            callback(data);
                    })
                    .fail(function (xhr, error, thrownError) {
                        if (error_callback != null)
                            error_callback("getUnits", xhr, error, thrownError);
                        else if (oThis.onError != null)
                            oThis.onError("getUnits", xhr, error, thrownError);
                    });
                }
                else {
                    if (callback != null)
                        callback(units);
                }
            },

            getPropertyDefinitions: function (callback, error_callback) {
                var oThis = this;

                //  try to get property definitions from cache...
                var units = $.sessionStorage.get('propertyDefinitions');

                if (units == null) {
                    $.ajax({
                        type: 'GET',
                        dataType: "jsonp",
                        url: this.getDataUrl('api/properties/definitions')
                    })
                    .done(function (data) {
                        //  cache units list...
                        $.sessionStorage.set('propertyDefinitions', data);

                        if (callback != null)
                            callback(data);
                    })
                    .fail(function (xhr, error, thrownError) {
                        if (error_callback != null)
                            error_callback("getPropertyDefinitions", xhr, error, thrownError);
                        else if (oThis.onError != null)
                            oThis.onError("getPropertyDefinitions", xhr, error, thrownError);
                    });
                }
                else {
                    if (callback != null)
                        callback(units);
                }
            },

            getSoftwareList: function (callback, error_callback) {
                var oThis = this;

                //  try to get units from cache...
                var software = $.sessionStorage.get('propertySoftware');

                if (software == null) {
                    $.ajax({
                        type: 'GET',
                        dataType: "jsonp",
                        url: this.getDataUrl('api/properties/software')
                    })
                    .done(function (data) {
                        //  cache software list...
                        $.sessionStorage.set('propertySoftware', data);

                        if (callback != null)
                            callback(data);
                    })
                    .fail(function (xhr, error, thrownError) {
                        if (error_callback != null)
                            error_callback("getUnits", xhr, error, thrownError);
                        else if (oThis.onError != null)
                            oThis.onError("getUnits", xhr, error, thrownError);
                    });
                }
                else {
                    if (callback != null)
                        callback(software);
                }
            },

            getProperties: function (ids, callback, error_callback) {
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

            getProperties: function (ids, callback, error_callback) {
                var oThis = this;

                if (ids == null || ids.length == 0) {
                    callback([]);
                    return;
                }

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/properties/list'),
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
                        error_callback("getProperties", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getProperties", xhr, error, thrownError);
                });
            },
        }, $.Toolkit.Store());
    };

}(jQuery));

$.Properties.store = $.Properties.Store();

$.Properties.store.getUnits();
$.Properties.store.getPropertyDefinitions();
$.Properties.store.getSoftwareList();
