(function ($) {
    $.widget("jobs.job_info", $.toolkit.generalinfo, {

        options: {
            showImage: false,
            properties: [
                { name: 'Id', title: 'Id' },
                { name: 'Status', title: 'Status' },
                { name: 'Created', title: 'Created' },
                { name: 'Started', title: 'Started' },
                { name: 'Finished', title: 'Finished' },
                {
                    name: 'Finished', title: 'Elapsed (sec)', format: function (value, job) {
                        if (job.Finished != null && job.Started != null) {
                            var difference = new Date(job.Finished) - new Date(job.Started);
                            return (difference / 1000).toFixed(1);
                        }
                        return null;
                    }
                },
                { name: 'Parameters', title: 'Parameters', format: JobParametersFormat },
                { name: 'Watches', title: 'Stopwatches', format: JobWatchesFormat },
                { name: 'Error', title: 'Error' },
            ]
        },

        _create: function () {
            this._super();

            this.element.addClass('cvsp-job-info');
        },

        _initImage: function () {
        },

        _loadData: function (id, callback) {
            $.Jobs.store.getJob(id, function (job) {
                if (callback)
                    callback(job);
            });
        },

        _setImage: function (data) {
        },

        _setTitle: function (data) {
            //  we currently do not have any title... 
        }
    });

}(jQuery));

function JobParametersFormat(parameters) {
    var tbody = $(document.createElement('tbody'))
    var table = $(document.createElement('table'))
                    .addClass('table table-hover')
                    .append(tbody);

    $(parameters).each(function (index, param) {
        var tr = $(document.createElement('tr'))
            .append(
                $(document.createElement('td'))
                    .text(param.Name)
            );

        if (param.Name == 'chunk') {
            tr.append(
                $(document.createElement('td'))
                    .append(
                        $(document.createElement('a'))
                            .addClass('value')
                            .attr({
                                href: $.Chunks.store.getUrl('chunks/' + param.Value)
                            })
                            .text(param.Value)
                    )
            );
        }
        else if (param.Name == 'deposition') {
            tr.append(
                $(document.createElement('td'))
                    .append(
                        $(document.createElement('a'))
                            .addClass('value')
                            .attr({
                                href: $.Chunks.store.getUrl('depositions/' + param.Value)
                            })
                            .text(param.Value)
                    )
            );
        }
        else {
            tr.append(
                $(document.createElement('td'))
                    .text(param.Value)
            );
        }

        tbody.append(tr);
    });

    return table;
};

function JobWatchesFormat(watches) {
    var tbody = $(document.createElement('tbody'))
    var table = $(document.createElement('table'))
                    .addClass('table table-hover')
                    .append(tbody);

    $(watches).each(function (index, watch) {
        var diff = new TimeSpan(new Date(watch.End) - new Date(watch.Begin));

        tbody.append(
            $(document.createElement('tr'))
                .append(
                    $(document.createElement('td'))
                        .text(watch.Name)
                )
                .append(
                    $(document.createElement('td'))
                        .text(diff.toString())
                )
        );
    });

    return table;
};

