(function ($) {
    $.widget("compound.compounds_tileresults", $.compound.compounds_tileview, {

        options: {
            rid: null
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
            $.Compounds.store.getSearchResultAsRecords({ rid: this.options.rid, start: start, count: count }, function (compounds) {
                callback(compounds);
            });
        }
    });
}(jQuery));
