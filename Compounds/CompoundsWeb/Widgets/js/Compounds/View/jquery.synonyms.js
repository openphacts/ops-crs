(function ($) {
    $.widget("compound.synonyms", $.toolkit.gridview, {

        options: {
            id: null,
            title: 'Synonyms',
            columns: [
                { name: 'Name', title: 'Name' },
                { name: 'LanguageId', title: 'Language' }
            ],
        },

        _onInit: function () {
            this._super();

            this.element.addClass('cmp-widget-synonyms-gridview');
        },

        _prepareContent: function (callback) {
            var oThis = this;

            if (this.options.id != null) {
                this.element.loadProgress();

                $.Compounds.store.getCompoundSynonyms(this.options.id, function (synonyms) {
                    oThis.element.hideProgress();

                    oThis.options.items = synonyms;

                    if (callback != null)
                        callback(oThis.options.items.length);
                });
            }
        },
    });

}(jQuery));
