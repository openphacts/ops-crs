$(function () {
    $.widget("toolkit.toolkitbase", {
        getVersion: function () {
            return '0.1.0';
        },
        _getStateCookieName: function () {
            return this.element.attr('id') + '_STATE_' + window.location.pathname.replace("/", "_");
        }
    });
}(jQuery));
