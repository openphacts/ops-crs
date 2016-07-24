(function ($) {
    $.widget("compound.substance2d", $.compound.molecule2d, {

        _loadMolFileById: function (id, callback) {
            $.Substances.store.getSubstance({ id: this.options.id }, function (substance) {
                if (callback)
                    callback(substance.MOL);
            });
        },

        _setUrlById: function (id) {
            this._setUrls(
                $.Substances.store.getImageUrl(id, this.options.width, this.options.height),
                $.Substances.store.getImageUrl(id, this.options.zoomWidth, this.options.zoomHeight)
            );
        },
    });

}(jQuery));

$(document).ready(function () {
    $('div[data-type="substance2d"]').livequery(function () {
        var options = {
        };

        if ($(this).is('[data-id]'))
            options.id = $(this).data('id');

        if ($(this).is('[data-inchi]'))
            options.inchi = $(this).data('inchi');

        if ($(this).is('[data-smiles]'))
            options.smiles = $(this).data('smiles');

        if ($(this).is('[data-mol-url]'))
            options.molUrl = $(this).data('mol-url');

        if ($('input.mol-file', this).length == 1)
            options.mol = $('input.mol-file', this).val();

        if ($(this).is('[data-width]'))
            options.width = $(this).data('width');

        if ($(this).is('[data-height]'))
            options.height = $(this).data('height');

        if ($(this).is('[data-zoom-width]'))
            options.zoomWidth = $(this).data('zoom-width');

        if ($(this).is('[data-zoom-height]'))
            options.zoomHeight = $(this).data('zoom-height');

        if ($(this).is('[data-allow-save]'))
            options.allowSave = $(this).data('allowSave');

        $(this).substance2d(options);
    });
});
