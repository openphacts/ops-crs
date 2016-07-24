(function ($) {
    $.widget("toolkit.xsltransform", $.toolkit.toolkitbase, {

        options: {
            xslurl: null,
            xmlurl: null
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-xsltransform');

            this.element.loadProgress("Loading...");

            $.ajax({
                type: 'GET',
                dataType: 'text',
                url: this.options.xmlurl
            })
            .done(function (xml) {
                $.ajax({
                    type: 'GET',
                    dataType: 'text',
                    url: oThis.options.xslurl
                })
                .done(function (xsl) {
                    $.ajax({
                        type: "POST",
                        url: $.Toolkit.store.getDataUrl('/api/transform/xsl'),
                        data: JSON.stringify({
                            Xml: xml,
                            Xsl: xsl
                        }),
                        contentType: 'application/json'
                    })
                    .done(function (html) {
                        oThis.element.hideProgress();
                        oThis.element.html(html);
                    })
                    .fail(function (xhr, error, thrownError) {
                        $.Toolkit.store.onError("toolkit.xsltransform", xhr, error, thrownError);
                    });
                })
                .fail(function (xhr, error, thrownError) {
                    $.Toolkit.store.onError("toolkit.xsltransform", xhr, error, thrownError);
                });
            })
            .fail(function (xhr, error, thrownError) {
                $.Toolkit.store.onError("toolkit.xsltransform", xhr, error, thrownError);
            });
        }
    });

}(jQuery));
