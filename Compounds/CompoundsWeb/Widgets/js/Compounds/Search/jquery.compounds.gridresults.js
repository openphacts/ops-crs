(function ($) {
    $.widget("compound.compounds_gridresults", $.compound.compounds_gridview, {

        options: {
            rid: null
        },

        //   we do not have to transform search results as they already came as compounds...
        _transformItems: function (compounds, callback) {
            callback(compounds);
        },

        //  prepare the search results and get the amount of found records
        _prepareContent: function (callback) {
            var oThis = this;

            if (this.options.rid) {
                this.element.searchProgress();

                $.Compounds.store.waitForSearchResults(
                    this.options.rid,
                    function (status) {
                        oThis.element.hideProgress();

                        if (status.Status == 'ResultReady') {
                            if (callback)
                                callback(status.Count);

                            oThis._trigger("onResultsReady", null, status);
                        }
                        else if (status.Status == 'TooManyRecords') {
                            $.showError('Too many records were found. Please try to narrow your request and try again.');
                        }
                    }
                );
            }
        },

        _loadPage: function (start, count, callback) {
            var oThis = this;

            $.Compounds.store.getSearchResultsWithInfo({ rid: this.options.rid, start: start, count: count }, function (results) {
                var compoundIds = [];

                //  we should treat Similarity as a very specific column...
                var hasSimilarity = false;

                $(results).each(function () {
                    compoundIds.push(this.Id);
                    hasSimilarity = hasSimilarity || this.hasOwnProperty('Similarity');
                });

                if (!hasSimilarity)
                    oThis.hideColumn('Similarity');

                $.Compounds.store.getCompoundsByID(compoundIds, function (compounds) {
                    $(compounds).each(function (index, compound) {
                        $.extend(compound, $.grep(results, function (res) { return res.Id === compound.Id })[0]);
                    });

                    callback(compounds.sort(sortBySimilarity));
                });
            });
        }
    });
} (jQuery));

function sortBySimilarity(a, b) {
    if (!a.hasOwnProperty('Similarity'))
        return 1;

    return a.Similarity > b.Similarity ? -1 : a.Similarity < b.Similarity ? 1 : 0;
}
