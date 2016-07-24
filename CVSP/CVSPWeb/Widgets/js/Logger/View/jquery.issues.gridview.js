(function ($) {
    $.widget("logger.issues_gridview", $.toolkit.gridview, {

        options: {
            id: null,
            title: 'Issues',
            columns: [
                { name: 'Severity', title: '', format: IssueSeverityFormat },
                { name: 'Code', title: 'Code' },
                { name: 'Title', title: 'Title' },
                { name: 'Message', title: 'Message' },
            ],
        },

        _onInit: function () {
            var oThis = this;

            $.Logger.store.getEntryTypes(function (entryTypes) {
                oThis._super();
            });

            this.element.addClass('cvsp-widget-issues-gridview');
        },

        _prepareContent: function (callback) {
            var oThis = this;

            if(this.options.id != null) {
                this.element.loadProgress();

                $.Records.store.getRecordIssues(this.options.id, function (issues) {
                    var guids = [];

                    $(issues).each(function () { guids.push(this.Id) });

                    $.Logger.store.getEntries(guids, function (entries) {
                        oThis.element.hideProgress();

                        $(entries).each(function () {
                            var entryType = $.Logger.getEntryTypeById(this.TypeId);

                            this.Severity = entryType.Severity;
                            this.Code = entryType.Code;
                            this.Title = entryType.Title;
                        });

                        oThis.options.items = entries;

                        if (callback != null)
                            callback(oThis.options.items.length);
                    })
                });
            }
        }
    });

}(jQuery));

function IssueSeverityFormat(severity) {
    if (severity.toLowerCase() == 'error') {
        return $(document.createElement('span'))
                .addClass('label label-danger')
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-exclamation-sign')
                );
    }
    else if (severity.toLowerCase() == 'warning') {
        return $(document.createElement('span'))
                .addClass('label label-warning')
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-bell')
                );
    }
    else if (severity.toLowerCase() == 'information') {
        return $(document.createElement('span'))
                .addClass('label label-info')
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-info-sign')
                );
    }
}
