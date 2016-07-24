(function ($) {
    $.widget("compound.compoundtile", $.toolkit.tile, {

        options: {
            id: 0,
            peer_id: null,
            properties: [
                { name: 'Id', title: 'ID' },
                //{ name: 'MolecularFormula', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } },
            ]
        },

        _initTile: function() {
            this.tile.molecule2d({
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

            $.Compounds.store.getCompound({ id: this.options.id, peer_id: this.options.peer_id }, function (compound) {
                oThis.drawTile(compound);

                oThis.element.hideProgress();
            });
        },

        drawTile: function (compound) {
            this.tile.molecule2d('setID', compound.Id);
        }
    });

}(jQuery));
