(function ($) {
    $.widget("depositions.deposition_stats", {

        options: {
            guid: null,
        },

        _create: function () {
            var oThis = this;

            this.element.addClass('cvsp-deposition-records-stats')
                .append(
                    $(document.createElement('span')).addClass('loading-spinner')
                );

            this._loadRecordsStats();

            $(document).on('onDepositionProgressChange', function (event, params) {
                oThis._loadRecordsStats();
            })
            .on('onDepositionStatusChange', function (event, params) {
                oThis._loadRecordsStats();
            });
        },

        _loadRecordsStats: function () {
            var oThis = this;

            $.Depositions.store.getDepositionStatistic(this.options.guid, function (stat) {
                oThis.element
                    .empty()
                    .append(
                        $(document.createElement('span'))
                            .addClass('badge')
                            .text(stat.RecordsNumber)
                    )

                if (stat.ErrorsNumber > 0) {
                    oThis.element.append(
                        $(document.createElement('span'))
                            .addClass('counter errors')
                            .append($(document.createElement('i')).text('Errors - '))
                            .append($(document.createTextNode(stat.ErrorsNumber)))
                    );
                }

                if (stat.WarningsNumber > 0) {
                    oThis.element.append(
                        $(document.createElement('span'))
                            .addClass('counter warnings')
                            .append($(document.createElement('i')).text('Warnings - '))
                            .append($(document.createTextNode(stat.WarningsNumber)))
                    );
                }

                if (stat.InfosNumber > 0) {
                    oThis.element.append(
                        $(document.createElement('span'))
                            .addClass('counter information')
                            .append($(document.createElement('i')).text('Information - '))
                            .append($(document.createTextNode(stat.InfosNumber)))
                    );
                }
            });
        }
    });

}(jQuery));
