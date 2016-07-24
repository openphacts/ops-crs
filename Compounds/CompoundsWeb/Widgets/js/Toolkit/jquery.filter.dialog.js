(function ($) {
    $.widget("toolkit.filter_dialog", $.toolkit.toolkitbase, {

        options: {
            saveState: true,
            title: 'Filter',
            onFilter: function(event, filter) {  //  callback on filter apply
            }
        },

        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget-filter');

            this.id = "abcdefghijklmnopqrstuvwxyz".random();

            this._initFilter();

            this._loadState();

            this._showFilterButton();

            this._initFilterDialog();
        },

        _initFilter: function() {
            this.filter = {
            };
        },

        _loadState: function () {
            if (this.options.saveState) {
                var state = $.sessionStorage.get(this._getStateCookieName());
                if (state != null) {
                    this.filter = state.filter
                }
            }
        },

        _saveState: function () {
            if (this.options.saveState) {
                $.sessionStorage.set(this._getStateCookieName(), {
                    filter: this.filter,
                });
            }
        },

        _showFilterButton: function () {
            this.filterBtn = $(document.createElement('button'))
                .addClass('btn btn-sm')
                .attr({
                    type: 'button',
                    'data-toggle': 'modal',
                    'data-target': '#' + this.id + '_FilterDialog',
                    'data-placement': 'left',
                    title: this.options.title
                })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-filter')
                        .attr({ 'aria-hidden': true })
                )
                .appendTo(this.element);

            this.filterBtn.bootstrapTooltip();
        },

        _initFilterDialog: function () {
            var oThis = this;

            this.filtersBody = $(document.createElement('div'));

            $(document.createElement('div'))
                .addClass('modal fade filter-dialog')
                .attr({
                    id: this.id + '_FilterDialog',
                    tabindex: -1,
                    role: 'dialog',
                    'aria-labelledby': this.id + '_ModalLabel',
                    'aria-hidden': true
                })
                .append(
                    $(document.createElement('div'))
                        .addClass('modal-dialog modal-lg')
                        .append(
                            $(document.createElement('div'))
                                .addClass('modal-content')
                                .append(
                                    $(document.createElement('div'))
                                        .addClass('modal-header')
                                        .append(
                                            $(document.createElement('button'))
                                                .addClass('close')
                                                .attr({
                                                    type: 'button',
                                                    'data-dismiss': 'modal',
                                                    'aria-label': 'Close'
                                                })
                                                .append(
                                                    $(document.createElement('span'))
                                                        .attr({ 'aria-hidden': true })
                                                        .html('&times;')
                                                )
                                        )
                                        .append(
                                            $(document.createElement('h4'))
                                                .addClass('modal-title')
                                                .attr({ id: this.id + '_ModalLabel' })
                                                .text(this.options.title)
                                        )
                                )
                                .append(
                                    $(document.createElement('div'))
                                        .addClass('modal-body')
                                        .append(this.filtersBody)
                                )
                                .append(
                                    $(document.createElement('div'))
                                        .addClass('modal-footer')
                                        .append(
                                            $(document.createElement('button'))
                                                .addClass('btn btn-default')
                                                .attr({ type: 'button', 'data-dismiss': 'modal' })
                                                .text('Close')
                                        )
                                        .append(
                                            $(document.createElement('button'))
                                                .addClass('btn btn-default')
                                                .attr({ type: 'button' })
                                                .text('Reset')
                                                .click(function () {
                                                    oThis._resetFilter();
                                                })
                                        )
                                        .append(
                                            $(document.createElement('button'))
                                                .addClass('btn btn-primary')
                                                .attr({ type: 'button' })
                                                .text('Apply')
                                                .click(function () {
                                                    oThis._applyFilter();
                                                    $('#' + oThis.id + '_FilterDialog').modal('hide');
                                                })
                                        )
                                )
                        )
                )
                .appendTo(this.element);
        },


        _applyFilter: function () {
            this._saveState();

            this._trigger("onFilter", null, this.filter);
        },

        _clearFilter: function () {
            this.filter = {};
        },

        _resetFilter: function () {
            this._clearFilter();
        },

        filterDescription: function () {
        }
    });

}(jQuery));
