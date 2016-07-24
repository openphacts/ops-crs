(function ($) {
    $.widget("record.records_gridview", $.toolkit.gridview, {

        options: {
            dataFilter: 'ExternalId|Ordinal|Original|Standardized|Issues',
            zoomImageSize: 500,
            title: 'Records',
            columns: [
                { name: 'Ordinal', title: 'Ordinal', format: function (value) { return value + 1; } },
                { name: 'Original', title: 'Original' },
                { name: 'Issues', title: 'Issues', format: function (issues) { return issues.length;} },
                { name: 'Standardized', title: 'Standardized' },
            ],
            showTooltip: false,
            rid: null
        },

        _initFromState: function (state) {
            this.options.rid = state.rid;
        },

        _prepareStateToSave: function () {
            return { rid: this.options.rid };
        },

        //  prepare the search results and get the amount of found records
        _prepareContent: function (callback) {
            var oThis = this;

            //  do the search if RID specified...
            if (this.options.rid) {
                this.element.searchProgress();

                $.Records.store.waitForSearchResults(
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

        //  transform records' IDs to the array of depositions' objects
        _transformItems: function (ids, callback) {
            var oThis = this;

            $.Records.store.getRecordsByGUID(ids, this.options.dataFilter, function (depositions) {
                callback(depositions);
            });
        },

        _drawCell: function (column, record) {
            var td = $(document.createElement('td'))

            if (column.name == 'Original') {
                td.append(this._drawStructure(column, record.Original));
            }
            else if (column.name == 'Standardized') {
                td.append(this._drawStructure(column, record.Standardized));
            }
            else {
                var value = record[column.name];
                if (value != null) {
                    td.html(column.format != null ? column.format(record[column.name], record) : record[column.name]);
                }
            }

            return td;
        },

        _drawStructure: function (column, mol) {
            var oThis = this;

            if (mol == null || mol == '')
                return;

            var div = $(document.createElement('div'))
                .addClass('cs-gridview-structure')
                .molecule2d({
                    width: this.options.imageSize,
                    height: this.options.imageSize,
                    zoomWidth: this.options.zoomImageSize,
                    zoomHeight: this.options.zoomImageSize,
                    mol: mol,
                    allowSave: true
                })
                .addClass('thumbnail');

            return div;
        },

        _tooltipUrl: function (id) {
            return null;
        },

        _loadPage: function (start, count, callback) {
            //  if we are in the search results mode then we have to load data as search results...
            if (this.options.rid) {
                $.Records.store.getSearchResults(
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

function DisplayRecordsIssues(issues) {
    var div = $(document.createElement('div')).issues_shortview({
        issues: issues
    });

    return div;
/*
    var ul = $(document.createElement('ul'))

    $(issues).each(function (index, issue) {
        var li = $(document.createElement('li')).appendTo(ul);

        //li.append(IssueSeverityFormat(issue.Severity));

        //li.append(document.createTextNode(issue.Message));

        li.append(document.createTextNode(issue.Code));
    });

    return ul;
*/
}