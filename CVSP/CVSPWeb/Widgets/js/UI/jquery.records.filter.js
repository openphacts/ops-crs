(function ($) {
    $.widget("record.records_filter", $.toolkit.filter_dialog, {

        options: {
            deposition: null,   //  deposition's guid
        },

        _create: function () {
            var oThis = this;

            this._super();

            this.element.addClass('cvsp-records-filter');

            $.Logger.store.getEntryTypes(function (entryTypes) {
                $.Depositions.store.getDepositionStatistic(oThis.options.deposition, function (stats) {
                    oThis._initFiltersOptions(stats);
                });
            });
        },

        _initFilter: function () {
            this.filter = {
                severities: [],
                codes: [],
                ordinals: [],
                regids: []
            };
        },

        _initFiltersOptions: function (stats) {
            var oThis = this;

            this.stats = stats;

            this.severities = $(document.createElement('div')).addClass('severities thumbnail');
            this.issueTypes = $(document.createElement('div')).addClass('issue-types thumbnail');
            this.externalIds = $(document.createElement('input'))
                                        .addClass('form-control')
                                        .attr({
                                            type: 'text',
                                            readonly: true,
                                            placeholder: 'Example: regid1, regid2'
                                        });
            this.ordinals = $(document.createElement('input'))
                                        .addClass('form-control')
                                        .attr({
                                            type: 'text',
                                            placeholder: 'Example: 1, 9, 23'
                                        });

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
                                        .text('By REGIDs')
                                )
                                .append(this.externalIds)
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
                                        .text('By Ordinals')
                                )
                                .append(this.ordinals)
                        )
                )
                .appendTo(this.filtersBody);

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
                                        .text('Severities')
                                )
                                .append(this.severities)
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
                                        .text('Issue Types')
                                )
                                .append(this.issueTypes)
                        )
                )
                .appendTo(this.filtersBody);

            if (stats.ErrorsNumber > 0) {
                this.severities.append(this._addSeverityCheckbox('Error', stats.ErrorsNumber))
            }
            if (stats.WarningsNumber > 0) {
                this.severities.append(this._addSeverityCheckbox('Warning', stats.WarningsNumber))
            }
            if (stats.InfosNumber > 0) {
                this.severities.append(this._addSeverityCheckbox('Information', stats.InfosNumber))
            }

            $(stats.Issues).each(function (index, issue) {
                oThis.issueTypes.append(oThis._addIssueTypeCheckbox(issue));
            });

            //  load state and initiate selected options...
            $(this.filter.severities).each(function (index, severity) {
                $('input[severity="' + severity + '"]', oThis.severities).prop('checked', true).change();
            });
            $(this.filter.codes).each(function (index, code) {
                $('input[code="' + code + '"]', oThis.issueTypes).prop('checked', true);
            });
            this.ordinals.val(this.filter.ordinals.join());
        },

        _addSeverityCheckbox: function (severity, count) {
            var oThis = this;

            return $(document.createElement('div'))
                        .addClass('checkbox')
                        .append(
                            $(document.createElement('label'))
                                .append(
                                    $(document.createElement('input'))
                                        .attr({ type: 'checkbox', severity: severity })
                                        .change(function () {
                                            oThis._filterIssueTypes($(this).attr('severity'));
                                        })
                                )
                                .append(
                                    $(document.createElement('span'))
                                        .addClass(severity.toLowerCase() == 'error' ? 'glyphicon glyphicon-exclamation-sign' : severity.toLowerCase() == 'warning' ? 'glyphicon glyphicon-bell' : 'glyphicon glyphicon-info-sign')
                                )
                                .append(
                                    $(document.createTextNode(severity + ' (' + count + ')'))
                                )
                        );
        },

        _addIssueTypeCheckbox: function (issue) {
            var entryType = $.Logger.getEntryTypeByCode(issue.Code);

            //  skip entry type if we can't find it...
            if (entryType == null)
                return;

            return $(document.createElement('div'))
                        .attr({
                            'data-severity': entryType.Severity
                        })
                        .addClass('checkbox')
                        .append(
                            $(document.createElement('label'))
                                .append(
                                    $(document.createElement('input'))
                                        .attr({ type: 'checkbox', code: entryType.Code, severity: entryType.Severity })
                                )
                                .append(
                                    $(document.createTextNode(entryType.Code + ' - ' + entryType.Title + ' (' + issue.Count + ')'))
                                )
                        );
        },

        _filterIssueTypes: function (severity) {
            var oThis = this;

            //  show all first... 
            $('div.checkbox', this.issueTypes).show();

            //  ... and if we have any selected severities, hide all not related issue types...
            if ($('input[type="checkbox"]:checked', this.severities).length > 0) {
                $('input[type="checkbox"]:not(:checked)', this.severities).each(function () {
                    var sev = $(this).attr('severity');
                    $('input[type="checkbox"][severity="' + sev + '"]', oThis.issueTypes).prop('checked', false).closest('div.checkbox').hide();
                });
            }

            if ($('input[type="checkbox"][severity="' + severity + '"]', this.severities).is(':checked')) {
                $('input[type="checkbox"][severity="' + severity + '"]', this.issueTypes).prop('checked', true);
            }
            else {
                $('input[type="checkbox"][severity="' + severity + '"]', this.issueTypes).prop('checked', false);
            }
        },

        _applyFilter: function () {
            var oThis = this;

            this._clearFilter();

            $('input[type="checkbox"]', this.severities).each(function (index, checkbox) {
                if ($(checkbox).prop('checked')) {
                    oThis.filter.severities.push($(checkbox).attr('severity'));
                }
            });

            $('input[type="checkbox"]', this.issueTypes).each(function (index, checkbox) {
                if ($(checkbox).prop('checked')) {
                    oThis.filter.codes.push($(checkbox).attr('code'));
                }
            });

            this.filter.ordinals = this.ordinals.val().trim() == '' ? [] : this.ordinals.val().split(',');
            this.filter.regids = this.externalIds.val().trim() == '' ? [] : this.externalIds.val().split(',');

            for (var i = 0; i < this.filter.ordinals.length; i++) { this.filter.ordinals[i] = this.filter.ordinals[i].trim(); }
            for (var i = 0; i < this.filter.regids.length; i++) { this.filter.regids[i] = this.filter.regids[i].trim(); }

            this._saveState();

            this._trigger("onFilter", null, this.filter);
        },

        _clearFilter: function () {
            this.filter = {
                severities: [],
                codes: [],
                ordinals: [],
                regids: []
            };
        },

        _resetFilter: function () {
            this._clearFilter();

            $('input[type="checkbox"]', this.severities).prop('checked', false);
            $('input[type="checkbox"]', this.issueTypes).prop('checked', false);
            $('div.checkbox', this.issueTypes).show();
            this.externalIds.val('');
            this.ordinals.val('');
        },

        filterDescription: function () {
            var filteredBy = [];
            if (this.filter.severities.length > 0) {
                if (this.filter.severities.length == 1)
                    filteredBy.push('Severity (' + this.filter.severities[0] + ')');
                else 
                    filteredBy.push('Severities (' + this.filter.severities.join(', ') + ')');
            }
            if (this.filter.codes.length > 0) {
                if (this.filter.codes.length == 1)
                    filteredBy.push('Issue Type (' + this.filter.codes[0] + ')');
                else if (this.filter.codes.length <= 3)
                    filteredBy.push('Issue Types (' + this.filter.codes.join(', ') + ')');
                else
                    filteredBy.push('Issue Types (' + this.filter.codes.slice(0, 3).join(', ') + ', ...)');
            }
            if (this.filter.ordinals.length > 0) {
                if (this.filter.ordinals.length == 1)
                    filteredBy.push('Ordinal (' + this.filter.ordinals[0] + ')');
                else if(this.filter.ordinals.length <= 3)
                    filteredBy.push('Ordinals (' + this.filter.ordinals.join(', ') + ')');
                else
                    filteredBy.push('Ordinals (' + this.filter.ordinals.slice(0, 3).join(', ') + ', ...)');
            }
            if (this.filter.regids.length > 0) {
                if (this.filter.regids.length == 1)
                    filteredBy.push('REGID (' + this.filter.regids[0] + ')');
                else if (this.filter.regids.length <= 3)
                    filteredBy.push('REGIDs (' + this.filter.regids.join(', ') + ')');
                else
                    filteredBy.push('REGIDs (' + this.filter.regids.slice(0, 3).join(', ') + ', ...)');
            }

            if(filteredBy.length == 0)
                return '';

            var description = '';

            description = filteredBy.slice(0, filteredBy.length > 1 ? filteredBy.length - 1 : 1).join(', ');

            if (filteredBy.length > 1)
                description += ' and ' + filteredBy[filteredBy.length - 1];

            return description;
        }
    });

}(jQuery));
