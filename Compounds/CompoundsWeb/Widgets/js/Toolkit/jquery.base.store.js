
$.Toolkit = $.Toolkit || {};

$.Toolkit.GetBaseWidgetsURL = function () {
    var scripts = document.getElementsByTagName("script");

    var src = scripts[scripts.length - 1].src;

    var base = src.substring(0, src.toLowerCase().indexOf('/widgets/'))

    return base;
}

$.Toolkit.Store = function () {
    return {

        version: '0.2.0',

        baseUrl: $.Toolkit.GetBaseWidgetsURL(),

        baseDataUrl: $.Toolkit.GetBaseWidgetsURL(),

        getVersion: function () {
            return this.version;
        },

        onError: function (proc, xhr, error, thrownError) {
            $('body').showMessage(xhr.statusText + ' : ' + xhr.status, 'Error in procedure ' + proc + ' has happened.<br/></br>' + xhr.responseText);
        },

        _buildUrl: function (base, page, params) {
            var url = base.rtrim('/') + '/' + page.ltrim('/');

            if (params == null)
                return url;

            var urlParams = '';

            for (var key in params) {
                if (key != null) {
                    if (urlParams != '') urlParams += '&';
                    urlParams += key + '=' + encodeURIComponent(params[key]);
                }
            }

            return url + '?' + urlParams;
        },

        getUrl: function (page, params) {
            return this._buildUrl(this.baseUrl, page, params);
        },

        getLibraryUrl: function (page) {
            return this.getUrl('Widgets/' + page);
        },

        getDataUrl: function (page, params) {
            return this._buildUrl(this.baseDataUrl, page, params);
        }
    };
};

$.Toolkit.store = $.Toolkit.Store();
