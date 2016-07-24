$.fn.zeroClipboard = function () {
    var oThis = this;

    $('body').loadScript("//cdnjs.cloudflare.com/ajax/libs/zeroclipboard/2.1.6/ZeroClipboard.js", function () {
        var client = new ZeroClipboard($(oThis).get(0), {
            moviePath: '//ajax.cdnjs.com/ajax/libs/zeroclipboard/2.1.6/ZeroClipboard.swf'
        });

        client.on("mouseover", function (client, args) {
            $(oThis).mouseover();
        });

        client.on("mouseout", function (client, args) {
            $(oThis).mouseout();
        });
    });

    return this;
};
