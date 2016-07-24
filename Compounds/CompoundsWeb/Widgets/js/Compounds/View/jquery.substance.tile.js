(function ($) {
    $.widget("compound.substancetile", $.toolkit.tile, {

        options: {
            id: 0,
            peer_id: null,
            properties: [
                { name: 'Id', title: 'ID' }
            ]
        },

        _initTile: function () {
            this.tile.substance2d({
                width: this.options.width,
                height: this.options.width,
                zoomWidth: this.options.zoomSize,
                zoomHeight: this.options.zoomSize,
                allowSave: false
            })
        },

        _init: function () {
            if (this.options.id > 0) {
                this.setID(this.options.id);
            }
        },

        setID: function (id) {
            var oThis = this;

            this.options.id = id;

            this.element.loadProgress();

            $.Substances.store.getSubstance({ id: this.options.id, peer_id: this.options.peer_id }, function (substance) {
                oThis.drawTile(substance);

                oThis.element.hideProgress();
            });
        },

        drawTile: function (substance) {
            this.tile.substance2d('setID', substance.Id);
        }
    });

}(jQuery));
