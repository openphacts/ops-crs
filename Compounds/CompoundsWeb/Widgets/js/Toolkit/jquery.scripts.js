(function ($) {
    $.fn.loadScript = function (url, callback) {

        if (location.protocol == 'https:') {
            url = url.replace('http://', 'https://');
        }

        var scripts = $('script[src="' + url + '"]');

        if (scripts.length > 0) {
            if ($(scripts).attr('loading')) {
                $(scripts).on('load', function () {
                    callback();

                    $(this).removeAttr('loading')
                });
            }
            else {
                //  script already loaded
                if (callback != null) {
                    callback();

                    $(this).removeAttr('loading')
                }
            }

            return this;
        }
        else {
            var script = document.createElement("script")
            script.type = "text/javascript";
            $(script).attr({loading: true});

            if (script.readyState) {  //IE
                script.onreadystatechange = function () {
                    if (script.readyState == "loaded" || script.readyState == "complete") {
                        script.onreadystatechange = null;
                        callback();

                        $(this).removeAttr('loading')
                    }
                };
            }
            else {  //Others
                script.onload = function () {
                    callback();

                    $(this).removeAttr('loading')
                };
            }

            script.src = url;
            this.get(0).appendChild(script);

            return this;
        }
    };

}(jQuery));
