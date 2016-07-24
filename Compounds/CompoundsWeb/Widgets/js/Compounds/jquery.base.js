$(function () {
    $.widget("compound.compoundbase", {
        getVersion: function () {
            return $.Compounds.store.getVersion();
        },
        _getStateCookieName: function () {
            return this.element.attr('id') + '_SESSION_STATE';
        }
    });
}(jQuery));
