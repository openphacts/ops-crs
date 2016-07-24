(function ($) {
    $.widget("compound.substanceinfo", $.toolkit.generalinfo, {

        options: {
            properties: [
                { name: 'SUBSTANCE_ID', title: 'ID' },
                { name: 'SUBSTANCE_VERSION', title: 'Version' },
                { name: 'COMPOUND_ID', title: 'Compound ID', format: function (value) { return $(document.createElement('a')).attr({ href: '/Compounds/' + value }).text(value) } },
                { name: 'DEPOSITOR_DATA_SOURCE_NAME', title: 'Data Source' },
                { name: 'DEPOSITOR_SUBSTANCE_REGID', title: 'Registry ID' }
            ]
        },

        _initImage: function () {
            this.image.substance2d({
                width: this.options.imgSize,
                height: this.options.imgSize,
                zoomWidth: this.options.zoomSize,
                zoomHeight: this.options.zoomSize,
                allowSave: true
            });
        },

        _loadData: function (id, callback) {
            $.Substances.store.getSubstance({ id: id }, function (substance) {
                if (callback)
                    callback(substance);
            });
        },

        _setImage: function (data) {
            this.image.substance2d('setID', data.Id);
        },

        _setTitle: function (data) {
            //  we currently do not have any title... 
        }
    });

}(jQuery));
