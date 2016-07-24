$.fn.showMessage = function (title, message, okCallback) {
    return this.each(function () {
        var oThis = this;
        $(this).block({
            theme: true,
            title: title,
            message: '<p>' + message + '</p><p><button style="float: right;" id="messageOkBtn" type="button" class="ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only" role="button" aria-disabled="false"><span class="ui-button-text">Ok</span></button></p>',
        });

        $('.blockUI.blockMsg', this).center();

        $('#messageOkBtn').click(function () {
            $(oThis).unblock()

            if (okCallback != null)
                okCallback();
        });
        $('.blockOverlay').click(function () {
            $(oThis).unblock()
        });
    });
}

$.fn.showConflict = function (message, okCallback) {
    return this.each(function () {
        $(this).showMessage('Conflict', message, okCallback);
    });
}

$.showError = function (message) {
    $('body').showMessage('Error', message);
}

$.fn.center = function () {
    this.css("position", "absolute");
    this.css("top", ($(window).height() - this.height()) / 2 + $(window).scrollTop() + "px");
    this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
    return this;
}
