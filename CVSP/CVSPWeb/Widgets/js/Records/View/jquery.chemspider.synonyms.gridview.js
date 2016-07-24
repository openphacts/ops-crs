(function ($) {
    $.widget("record.chemspider_synonyms_gridview", $.toolkit.gridview, {

        options: {
            id: null,
            title: 'ChemSpider Synonyms',
            columns: [
                { name: 'Name', title: 'Name' },
                { name: 'LanguageId', title: 'Language' }
            ],
        },

        _onInit: function () {
            this._super();

            this.element.addClass('cvsp-widget-properties-gridview');
        },

        _prepareContent: function (callback) {
            var oThis = this;

            if(this.options.id != null) {
                this.element.loadProgress();

                $.Records.store.getRecordChemSpiderSynonyms(this.options.id, function (synonyms) {
                    oThis.element.hideProgress();

                    oThis.options.items = synonyms;

                    if (callback != null)
                        callback(oThis.options.items.length);
                });
            }
        },
    });

}(jQuery));
