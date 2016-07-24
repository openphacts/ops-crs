(function ($) {
    $.widget("jobs.jobs_gridview", $.toolkit.gridview, {

        options: {
            title: 'Jobs',
            deposition: null,
            columns: [
                { name: 'Id', title: 'ID' },
                { name: 'Created', title: 'Created' },
                {
                    name: 'Parameters', title: 'Command', format: function (parameters) {
                        for (var i = 0; i < parameters.length; i++) {
                            var param = parameters[i];
                            if (param.Name == 'command')
                                return param.Value;
                        }

                        return null;
                    }
                },
                {
                    name: 'Started', title: 'Elapsed (sec)', format: function (value, job) {
                        if (job.Finished != null && job.Started != null)
                        {
                            var difference = new Date(job.Finished) - new Date(job.Started);
                            return (difference / 1000).toFixed(1);
                        }
                        return null;
                    }
                },
                { name: 'Status', title: 'Status' },
            ],
            rid: null
        },

        _create: function () {
            this._super();

            this.element.addClass('cvsp-jobs-gridview');
        },

        _initFromState: function (state) {
            this.options.rid = state.rid;
        },

        _prepareStateToSave: function () {
            return { rid: this.options.rid };
        },

        _prepareContent: function (callback) {
            var oThis = this;

            //  do the search if RID specified...
            if (this.options.rid) {
                this.element.searchProgress();

                $.Search.store.waitForSearchResults(
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
            else {
                //  ... otherwise display all records
                this._super(callback);

                this._trigger("onResultsReady");
            }
        },

        //  transform chunks' IDs to the array of chunks' objects
        _transformItems: function (ids, callback) {
            var oThis = this;

            $.Jobs.store.getJobsByGUID(ids, function (jobs) {
                callback(jobs);
            });
        },

        _loadPage: function (start, count, callback) {
            //  if we are in the search results mode then we have to load data as search results...
            if (this.options.rid) {
                $.Search.store.getSearchResults(
                    {
                        rid: this.options.rid,
                        start: start,
                        count: count
                    },
                    function (results) {
                        callback(results);
                    }
                );
            }
            else {
                this._super(start, count, callback);
            }
        },

        setRid: function (rid) {
            this.options.rid = rid;

            this.load();
        }
    });

}(jQuery));
