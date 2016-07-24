var SearchOptions = Class.extend({
    init: function () {
        this.options = {
            searchResults: {
                limit: 1000
            },
            searchScopes: {
                datasources: [],
                realOnly: false
            }
        };

        this.load();
    },

    load: function () {
        var options = $.localStorage.get('searchOptions');
        if (options != null) {
            this.options = options;
        }
    },

    save: function () {
        $.localStorage.set('searchOptions', this.options);
    },

    getHitsLimit: function () {
        return this.options.searchResults.limit;
    },

    setHitsLimit: function (limit) {
        return this.options.searchResults.limit = limit;
    },

    hasDatasources: function () {
        var datasources = this.getDatasources();
        return datasources.length > 0;
    },

    getDatasources: function () {
        return this.options.searchScopes.datasources;
    },

    getDatasourcesIds: function () {
        var ids = [];

        $(this.options.searchScopes.datasources).each(function (index, ds) {
            ids.push(ds.Guid);
        });

        return ids;
    },

    getDatasourcesNames: function () {
        var names = [];

        $(this.options.searchScopes.datasources).each(function (index, ds) {
            names.push(ds.Name);
        });

        return names;
    },

    setDatasources: function (datasources) {
        return this.options.searchScopes.datasources = datasources;
    }
});
