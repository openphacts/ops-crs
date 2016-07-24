(function ($) {
    $.widget("annotation.annotations_gridview", $.toolkit.gridview, {

        options: {
            id: null,
            title: 'Annotations',
            columns: [
                { name: 'Name', title: 'Name' },
                { name: 'Value', title: 'Value' }
            ],
        },

        _onInit: function () {
            this._super();

            this.element.addClass('cvsp-widget-properties-gridview');
        },

        _prepareContent: function (callback) {
            var oThis = this;

            if (this.options.id != null) {
                this.element.loadProgress();

                $.Records.store.getRecordChemSpiderProperties(this.options.id, function (properties) {
                    oThis.element.hideProgress();

                    oThis.options.items = properties;

                    if (callback != null)
                        callback(oThis.options.items.length);
                });
            }
        },
    });

}(jQuery));
