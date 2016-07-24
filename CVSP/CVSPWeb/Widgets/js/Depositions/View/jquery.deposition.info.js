(function ($) {
	$.widget("depositions.deposition_info", $.toolkit.generalinfo, {

	    options: {
            showImage: false,
			properties: [
                { name: 'Id', title: 'Guid' },
		    	{ name: 'IsPublic', title: 'Public', format: function (value) { return value ? "Yes" : "No"; } },
                { name: 'DatasourceId', title: 'Datasource', format: DatasourceFormat },
                { name: 'Status', title: 'Status', format: DepositionStatusFormat },
                { name: 'Id', title: 'Files', format: DepositionFilesFormat },
                { name: 'UserGuid', title: 'Depositor', format: DepositorDisplayNameFormat },
			    { name: 'DateSubmitted', title: 'Submitted', format: function (value) { return (new Date(value)).yyyymmdd(); } },
		    	{ name: 'Id', title: 'Records', format: DepositionRecordsFormat },
                { name: 'Parameters', title: 'Processing Parameters', format: ProcessingParametersFormat },
			]
		},

	    _create: function () {
	        this._super();

	        this.element.addClass('cvsp-deposition-info');
	    },

		_initImage: function () {
		},

		_loadData: function (id, callback) {
		    $.Depositions.store.getDeposition(id, function (deposition) {
		        if (callback)
		            callback(deposition);
			});
		},

		_setImage: function (data) {
		},

		_setTitle: function (data) {
			//  we currently do not have any title... 
		}
	});

}(jQuery));

function DepositorDisplayNameFormat(guid) {
    var span = $(document.createElement('span'))
        .append(
            $(document.createElement('span')).addClass('loading-spinner')
        );

    $.Users.store.getUser(guid, function (user) {
        if (user)
            span.empty().text(user.DisplayName);
        else {
            span.empty().html('<i>user not found!</i>');
            console.log('ERROR: user with guid ' + guid + ' not found!');
        }
    })

    return span;
};

function DepositionStatusFormat(status, record) {
    return $(document.createElement('span')).deposition_status({ status: status, guid: record.Id });
};

function DepositionRecordsFormat(guid) {
    return $(document.createElement('span')).deposition_stats({ guid: guid });
};

function DepositionProgressFormat(guid) {
    return $(document.createElement('div')).deposition_progress({ guid: guid });
};

function ProcessingParametersFormat(parameters) {
    var ul = $(document.createElement('ul'))

    for (var i = 0; i < parameters.length; i++) {
        var param = parameters[i];

        var li = $(document.createElement('li'))
                    .append(
                        $(document.createElement('label'))
                            .addClass('title')
                            .text(param.Name)
                    )
                    .append(
                        $(document.createElement('span'))
                            .addClass('value')
                            .text(param.Value)
                    )
                    .appendTo(ul);
    }

    return ul;
};

function DatasourceFormat(guid) {
    var span = $(document.createElement('span'))
        .append(
            $(document.createElement('span')).addClass('loading-spinner')
        );

    $.Datasources.store.getDatasource(guid, function (datasource) {
        if (datasource) {
            span.empty().append(
                $(document.createElement('a'))
                    .attr({
                        href: $.Datasources.store.getUrl('datasources/' + datasource.Guid),
                    })
                    .text(datasource.Name)
                );
        }
        else {
            span.parent().prev().remove();
            span.parent().remove();
        }
    })

    return span;
}

function DepositionFilesFormat(guid) {
    var span = $(document.createElement('span'))
        .append(
            $(document.createElement('span')).addClass('loading-spinner')
        );

    $.Depositions.store.getDepositionFiles(guid, function (files) {
        var parent = span.parent();
        parent.empty();
        $(files).each(function () {
            parent.append($(document.createElement('span')).text(this.Name));
        });
    });

    return span;
};
