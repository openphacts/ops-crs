(function ($) {
	$.widget("jobs.deposition_jobs_info", $.toolkit.generalinfo, {

	    options: {
            showImage: false,
			properties: [
                { name: 'Total', title: 'Total' },
                { name: 'New', title: 'New' },
                { name: 'Processing', title: 'Processing' },
                { name: 'Processed', title: 'Processed' },
                { name: 'Failed', title: 'Failed' },
                { name: 'Delayed', title: 'Delayed' },
                { name: 'Commands', title: 'Commands', format: JobCommandsFormat },
                { name: 'ProcessingTime', title: 'Processing Time' },
                { name: 'Stopwatches', title: 'Stopwatches', format: JobsWatchesFormat }
			]
		},

	    _create: function () {
	        this._super();

	        this.element.addClass('cvsp-deposition-info');
	    },

		_initImage: function () {
		},

		_loadData: function (id, callback) {
		    $.Jobs.store.getDepositionJobStats(id, function (stats) {
		        if (callback)
		            callback(stats);
			});
		},

		_setImage: function (data) {
		},

		_setTitle: function (data) {
			//  we currently do not have any title... 
		}
	});

}(jQuery));

function JobCommandsFormat(commands) {
    return commands.join(', ');
};

function JobsWatchesFormat(watches) {
    var tbody = $(document.createElement('tbody'))
    var table = $(document.createElement('table'))
                    .addClass('table table-hover')
                    .append(tbody);

    $.each(watches, function (name, value) {
        tbody.append(
            $(document.createElement('tr'))
                .append(
                    $(document.createElement('td'))
                        .text(name)
                )
                .append(
                    $(document.createElement('td'))
                        .text((new TimeSpan(value)).toString())
                )
        );
    });

    return table;
};
