(function ($) {
    $.widget("chunks.chunk_info", $.toolkit.generalinfo, {

        options: {
            showImage: false,
            properties: [
                { name: 'Id', title: 'Id' },
                { name: 'Status', title: 'Status' },
                { name: 'NumberOfRecords', title: '# of Records', format: NumberOfRecordsFormat },
                { name: 'Parameters', title: 'Parameters', format: ChunkParametersFormat },
            ]
        },

        _create: function () {
            this._super();

            this.element.addClass('cvsp-chunk-info');
        },

        _initImage: function () {
        },

        _loadData: function (id, callback) {
            $.Chunks.store.getChunk(id, function (chunk) {
                if (callback)
                    callback(chunk);
            });
        },

        _setImage: function (data) {
        },

        _setTitle: function (data) {
            //  we currently do not have any title... 
        }
    });

}(jQuery));

function NumberOfRecordsFormat(number, chunk) {
    return $(document.createElement('a'))
                .attr({
                    href: $.Chunks.store.getUrl('api/chunks/' + chunk.Id + '/download')
                })
                .text(number);
}

function ChunkParametersFormat(parameters) {
    var ul = $(document.createElement('ul'))

    for (var i = 0; i < parameters.length; i++) {
        var param = parameters[i];

        var li = $(document.createElement('li'))
                    .append(
                        $(document.createElement('label'))
                            .addClass('title')
                            .text(param.Name)
                    )
                    .appendTo(ul)

        if (param.Name == 'deposition') {
            li.append(
                $(document.createElement('a'))
                    .addClass('value')
                    .attr({
                        href: $.Chunks.store.getUrl('depositions/' + param.Value)
                    })
                    .text(param.Value)
            );
        }
        else {
            li.append(
                $(document.createElement('span'))
                    .addClass('value')
                    .text(param.Value)
            );
        }
    }

    return ul;
};
