(function ($) {
    $.widget("job.jobs_filter", $.toolkit.filter_dialog, {

        options: {
            deposition: null,   //  deposition's guid
        },

        _create: function () {
            var oThis = this;

            this._super();

            this.element.addClass('cvsp-jobs-filter');

            $.Depositions.store.getJobsStatistic(this.options.deposition, function (stats) {
                oThis._initFiltersOptions(stats);
            })
        },

        _initFilter: function () {
            this.filter = {
                status: '',
                command: ''
            };
        },

        _initFiltersOptions: function (stats) {
            var oThis = this;

            this.stats = stats;

            this.statuses = $(document.createElement('div')).addClass('status thumbnail');
            this.commands = $(document.createElement('div')).addClass('command thumbnail');

            $(document.createElement('div'))
                .addClass('row')
                .append(
                    $(document.createElement('div'))
                        .addClass('col-md-6')
                        .append(
                            $(document.createElement('div'))
                                .addClass('form-group')
                                .append(
                                    $(document.createElement('label'))
                                        .text('Status')
                                )
                                .append(this.statuses)
                        )
                )
                .append(
                    $(document.createElement('div'))
                        .addClass('col-md-6')
                        .append(
                            $(document.createElement('div'))
                                .addClass('form-group')
                                .append(
                                    $(document.createElement('label'))
                                        .text('Command')
                                )
                                .append(this.commands)
                        )
                )
                .appendTo(this.filtersBody);

            this.statuses.append(this._addStatusRadio('All', this.stats.Total));
            this.statuses.append(this._addStatusRadio('New', this.stats.New));
            this.statuses.append(this._addStatusRadio('Processing', this.stats.Processing));
            this.statuses.append(this._addStatusRadio('Processed', this.stats.Processed));
            this.statuses.append(this._addStatusRadio('Failed', this.stats.Failed));

            this.commands.append(this._addCommandRadio('All'));
            for (var i = 0; i < this.stats.Commands.length; i++) {
                var command = this.stats.Commands[i];
                this.commands.append(this._addCommandRadio(command));
            }

            //  load state and initiate selected options...
            $('input[type="radio"][value="' + this.filter.status + '"]', this.statuses).prop('checked', true);
            $('input[type="radio"][value="' + this.filter.command + '"]', this.commands).prop('checked', true);
        },

        _addStatusRadio: function (status, count) {
            return $(document.createElement('div'))
                        .addClass('radio')
                        .append(
                            $(document.createElement('label'))
                                .append(
                                    $(document.createElement('input'))
                                        .attr({
                                            type: 'radio',
                                            name: this.id + 'Status',
                                            value: status == 'All' ? '' : status
                                        })
                                )
                                .append(
                                    $(document.createTextNode(status + ' (' + count + ')'))
                                )
                        );
        },

        _addCommandRadio: function (command) {
            return $(document.createElement('div'))
                        .addClass('radio')
                        .append(
                            $(document.createElement('label'))
                                .append(
                                    $(document.createElement('input'))
                                        .attr({
                                            type: 'radio',
                                            name: this.id + 'Command',
                                            value: command == 'All' ? '' : command
                                        })
                                )
                                .append(
                                    $(document.createTextNode(command))
                                )
                        );
        },

        _applyFilter: function () {
            var oThis = this;

            this._clearFilter();

            this.filter.status = $('input[type="radio"]:checked', this.statuses).val();
            this.filter.command = $('input[type="radio"]:checked', this.commands).val();

            this._saveState();

            this._trigger("onFilter", null, this.filter);
        },

        _clearFilter: function () {
            this.filter = {
                status: '',
                command: ''
            };
        },

        _resetFilter: function () {
            this._clearFilter();

            $('input[type="radio"][value=""]', this.statuses).prop('checked', true);
            $('input[type="radio"][value=""]', this.commands).prop('checked', true);
        },

        filterDescription: function () {
            var filteredBy = [];
            if (this.filter.status) {
                filteredBy.push('Status (' + this.filter.status + ')');
            }
            if (this.filter.command) {
                filteredBy.push('Command (' + this.filter.command + ')');
            }

            if (filteredBy.length == 0)
                return '';

            var description = filteredBy.slice(0, filteredBy.length > 1 ? filteredBy.length - 1 : 1).join(', ');

            if (filteredBy.length > 1)
                description += ' and ' + filteredBy[filteredBy.length - 1];

            return description;
        }
    });

}(jQuery));
