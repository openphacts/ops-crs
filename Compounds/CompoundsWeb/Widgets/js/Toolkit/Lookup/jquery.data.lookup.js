$(function () {
    $.widget("toolkit.data_lookup", $.toolkit.toolkitbase, {

        options: {
            mode: 'multiple',   //  single|multiple
            columns: [
                { name: 'name', title: 'Name' }
            ],
            list: [
                { id: 1, name: 'first' },
                { id: 2, name: 'second' },
                { id: 3, name: 'third' },
            ],
            selected: [],
            idColumnName: 'id',
            primaryColumnName: 'name',
            displayFormat: null,    //  format function that will be used to modify display of selected item: function(id, name) {}
            title: 'Title',
            readonly: false,
            autoComplete: true,
            autoCompleteMinLength: 3,
            saveState: false,
            searchUrl: null,    //  Search data endpoint. MUST support 'query' parameter
            itemsUrl: null      //  Detailed information endpoint.
        },

        // the constructor
        _create: function () {
            var oThis = this;

            //var id = this.element.attr('id');

            this.randomId = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.random();

            this.element
                .addClass("cs-widget cs-widget-lookup");

            this.list = this.options.list.clone();
            this.selected = this.options.selected.clone();

            this._initModalDialog();

            if (this.isMultipleMode()) {
                this._initMultiple();
            }
            else if (this.isSingleMode()) {
                this._initSingle();
            }

            this._onInit();

            if (this.isStaticMode()) {
                this._initStatic();
            }
            else if (this.isDynamicMode()) {
                this._initDynamic();
            }
        },

        isSingleMode: function () {
            return this.options.mode == 'single';
        },

        isMultipleMode: function () {
            return this.options.mode == 'multiple';
        },

        isStaticMode: function () {
            return this.options.searchUrl == null && this.options.itemsUrl == null;
        },

        isDynamicMode: function () {
            return this.options.searchUrl != null && this.options.itemsUrl != null;
        },

        _initSingle: function () {
            var oThis = this;

            this.singleSelect = $(document.createElement('span'))
                                    .addClass('single-select')
                                    .appendTo(this.element);

            if (!this.options.readonly) {
                $(document.createElement('button'))
                    .attr({
                        type: 'button',
                        'data-toggle': 'modal',
                        'data-target': '#modal_' + this.randomId
                    })
                    .addClass('btn btn-default btn-xs single-select-btn')
                    .append(
                        $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-search')
                        .attr({
                            'aria-hidden': true
                        })
                    )
                    .click(function () {
                        oThis._onModalDialogOpen();
                    })
                    .appendTo(this.element);
            }
        },

        _initMultiple: function() {
            var oThis = this;

            this.progress = $(document.createElement('span'))
                .css({ float: 'left' })
                .addClass('loading-spinner')
                .appendTo(this.element);

            $(document.createElement('ul'))
                .appendTo(this.element);

            if (!this.options.readonly) {
                this.addButton = $(document.createElement('button'))
                    .attr({
                        type: 'button',
                        'data-toggle': 'modal',
                        'data-target': '#modal_' + this.randomId
                    })
                    .addClass('btn btn-default btn-xs add')
                    .append(
                        $(document.createElement('span')).addClass('glyphicon glyphicon-plus')
                    )
                    .click(function () {
                        oThis._onModalDialogOpen();
                    });
            }
        },

        _initStatic: function () {
            var oThis = this;

            this._prepareData(function (data) {
                oThis.list = data;

                oThis._fillModalDialog(oThis.list);

                $.cookie.raw = true;
                if (oThis.options.saveState && $.cookie(oThis._getStateCookieName()) != null) {
                    oThis._loadState($.cookie(oThis._getStateCookieName()));
                }

                if (oThis.selected.length > 0) {
                    oThis._displaySelectedItems();
                }

                if (oThis.isMultipleMode()) {
                    oThis.progress.remove();
                    oThis.addButton.appendTo(oThis.element);
                }
            });
        },

        _initDynamic: function () {
            var oThis = this;

            this.list.clear();

            $.cookie.raw = true;
            if (this.options.saveState && $.cookie(this._getStateCookieName()) != null) {
                this._loadState($.cookie(this._getStateCookieName()));
            }

            this._getItems(this.selected, function (data) {
                oThis.list = data;
                oThis._displaySelectedItems();

                oThis._fillModalDialog(oThis.list);

                if (oThis.isMultipleMode()) {
                    oThis.progress.remove();

                    if(!oThis.options.readonly)
                        oThis.addButton.appendTo(oThis.element);
                }
            });
        },

        _onInit: function () {
            //  ************************************************************
            //  override this method in order to add aditional functionality
            //  ************************************************************
        },

        _loadState: function (value) {
            var oThis = this;

            this.selected.clear();

            var ids = value.split(',');

            $(ids).each(function (index, id) {
                oThis.selected.push(id);
            });
        },

        _saveState: function () {
            if (this.options.saveState) {
                $.cookie(this._getStateCookieName(), this.selected.join(','));
            }
        },

        _initModalDialog: function () {
            var oThis = this;

            if (this.options.readonly)
                return;

            this.modalDilaog = $(document.createElement('div'))
                .addClass('modal fade')
                .attr({
                    id: 'modal_' + this.randomId,
                    role: 'dialog',
                    'aria-labelledby': '',
                    'aria-hidden': true
                })
                .append(
                    $(document.createElement('div'))
                        .addClass('modal-dialog')
                        .append(
                            $(document.createElement('div'))
                                .addClass('modal-content')
                                .append(
                                    this._initToolbar()
                                )
                                .append(
                                    $(document.createElement('div'))
                                        .addClass('modal-body')
                                        .append(
                                            $(document.createElement('table'))
                                                .addClass('table table-striped table-hover table-condensed items')
                                        )
                                )
                                .append(
                                    $(document.createElement('div'))
                                        .addClass('modal-footer')
                                        .append(
                                            $(document.createElement('button'))
                                                .addClass('btn btn-default')
                                                .attr({
                                                    type: 'button',
                                                    'data-dismiss': 'modal'
                                                })
                                                .click(function () {
                                                    oThis._cancelSelectedItems();
                                                })
                                                .text('Cancel')
                                        )
                                        .append(
                                            $(document.createElement('button'))
                                                .addClass('btn btn-primary')
                                                .attr({
                                                    type: 'button',
                                                    'data-dismiss': 'modal'
                                                })
                                                .click(function () {
                                                    oThis._updateSelectedItems();

                                                    oThis._trigger("select", null, {
                                                        selected: oThis.selected
                                                    });
                                                })
                                                .text('Ok')
                                        )
                                )
                        )
                )
                .appendTo(this.element);

            if (!this.isStaticMode()) {
                $('div.modal-body', this.element).prepend(
                    $(document.createElement('div'))
                        .addClass('search-results')
                        .append(
                            $(document.createElement('table'))
                                .addClass('table table-striped table-hover table-condensed search-results-table')
                        )
                );
            }
        },

        _initToolbar: function () {
            var oThis = this;

            this.searchField = $(document.createElement('input'))
                                    .attr({
                                        type: 'text',
                                        placeholder: 'Search for...'
                                    })
                                    .addClass('form-control filter');

            this.searchButton = $(document.createElement('button'))
                                        .addClass('btn btn-default')
                                        .attr({
                                            type: 'button'
                                        })
                                        .append(
                                            $(document.createElement('span'))
                                                .addClass('glyphicon glyphicon-search')
                                                .attr({ 'aria-hidden': true })
                                        )
                                        .click(function () {
                                            var query = oThis.searchField.val();
                                            oThis._search(query);
                                        });

            if (this.isStaticMode()) {
                this.searchField.on('keyup paste', function () {
                    oThis._filter($(this).val());
                });
            }
            else {
                if (this.options.autoComplete) {
                    this.searchField.on('keyup paste', function () {
                        var query = $(this).val();

                        if (query.length >= oThis.options.autoCompleteMinLength)
                            oThis._search(query);
                        else
                            oThis._hideSearchWindow();
                    });
                }
            }

            this.searchResults = $(document.createElement('ul'))
                                    .addClass('search-results');

            this.searchWindow = $(document.createElement('div'))
                                    .addClass('search-window thumbnail')
                                    .append(
                                        $(document.createElement('div'))
                                            .addClass('loading-progress')
                                            .append(
                                                $(document.createElement('span'))
                                                    .addClass('scroll-progress')
                                                    .text('Searching...')
                                            )
                                    )
                                    .append( this.searchResults )
                                    .hide();

            this.toolbar = $(document.createElement('nav'))
                .addClass('navbar navbar-default')
                .attr({ role: 'navigation' })
                .append(
                    $(document.createElement('div'))
                        .addClass('navbar-header')
                        .append(
                            $(document.createElement('span'))
                                .addClass('navbar-brand')
                                .text(this.options.title)
                        )
                )
                .append(
                    $(document.createElement('div'))
                        .addClass('input-group search-group')
                        .append(this.searchField)
                        .append(
                            $(document.createElement('span'))
                                .addClass('input-group-btn')
                                .append(this.searchButton)
                        )
                )
                .append( this.searchWindow )

            return this.toolbar;
        },

        _prepareData: function (readyCallback) {
            //  ******************************************************************
            //  override this method in order to load data dynamically for example
            //  ******************************************************************

            if (readyCallback = null) {
                readyCallback(this.list);
            }
        },

        _fillModalDialog: function (list) {
            var oThis = this;

            var table = $('.modal-body > table', this.element);

            table.empty();

            $(list).each(function (index, item) {
                oThis._addModalDialogItem(item);
            })
        },

        _addItemToList: function (item) {
            var oThis = this;

            var exist = false;

            $(this.list).each(function () {
                if (this[oThis.options.idColumnName] == item[oThis.options.idColumnName])
                    exist = true;
            })

            if (!exist)
                this.list.push(item);
        },

        _addModalDialogItem: function(item) {
            var oThis = this;

            var table = $('.modal-body > table', this.element);

            if ($('tr[data-id="' + item[this.options.idColumnName] + '"]', this.searchWindow).length > 0)
                return;

            table.append(
                $(document.createElement('tr'))
                    .attr({
                        'data-id': item[oThis.options.idColumnName]
                    })
                    .click(function () {
                        if (oThis.isSingleMode()) {
                            $(this).siblings().each(function () {
                                $(this).removeClass('success');
                            });
                        }
                        $(this).toggleClass('success');
                    })
                    .append(
                        $(document.createElement('td'))
                            .text(item[oThis.options.primaryColumnName])
                    )
                    .append(
                        $(document.createElement('td'))
                            .append(
                                $(document.createElement('span'))
                                    .addClass('glyphicon glyphicon-ok')
                                    .attr({ 'aria-hidden': true })
                            )
                    )
            );
        },

        _showSearchResults: function (list) {
            var oThis = this;

            this.searchResults.empty();

            //  hide search progress bar...
            $('div.loading-progress', this.searchWindow).hide();

            if (list.length == 0) {
                oThis.searchResults.append(
                    $(document.createElement('li'))
                        .addClass('no-results')
                        .text('Nothing was found')
                );
            }
            else {
                $(list).each(function () {
                    oThis.searchResults.append(
                        $(document.createElement('li'))
                            .attr({
                                'data-id': this[oThis.options.idColumnName]
                            })
                            .click(function () {
                                var item = {};
                                item[oThis.options.idColumnName] = $(this).data('id');
                                item[oThis.options.primaryColumnName] = $(this).text();

                                oThis._addItemToList(item);

                                oThis._addModalDialogItem(item);

                                oThis._hideSearchWindow();
                            })
                            .text(this[oThis.options.primaryColumnName])
                    );
                });
            }
        },

        _onModalDialogOpen: function () {
            var table = $('.modal-body > table', this.element);

            $('tr.success', table).removeClass('success');

            $(this.selected).each(function () {
                $('tr[data-id=' + this + ']').addClass('success');
            });
        },

        _filter: function (val) {
            var oThis = this;

            val = val.toLowerCase();

            var ids = [];
            $(this.list).each(function (index, item) {
                if (item[oThis.options.primaryColumnName].toLowerCase().indexOf(val) >= 0) {
                    ids.push(item[oThis.options.idColumnName]);
                }
            });

            var table = $('.modal-body > table', this.element);

            $('tr', table).hide();

            $(ids).each(function (index, id) {
                $('tr[data-id=' + id + ']').show();
            });
        },

        _search: function (query) {
            var oThis = this;

            //  show search progress bar...
            $('div.loading-progress', this.searchWindow).show();

            //  clear previous search results...
            this.searchResults.empty();

            var searchGroup = $('div.search-group', this.toolbar);
            var margin = (searchGroup.outerWidth(true) - searchGroup.outerWidth()) / 2;

            this.searchWindow.css({
                top: searchGroup.outerHeight(true) - margin,
                left: searchGroup.position().left + margin,
                right: margin
            })
            .show();

            if (this.options.searchUrl) {
                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.options.searchUrl,
                    data: {
                        query: query
                    }
                })
                .done(function (ids) {
                    oThis._getItems(ids, function (data) {
                        oThis._showSearchResults(data);
                    });
                })
                .fail(function (xhr, error, thrownError) {
                    $.showError('Cannot run search operation on: ' + oThis.options.searchUrl)
                });
            }
        },

        _hideSearchWindow: function () {
            this.searchWindow.hide();
        },

        _getItems: function (ids, callback) {
            var oThis = this;

            if (this.options.itemsUrl) {
                var items = [];

                if (ids.length == 0) {
                    if (callback) {
                        callback(items);
                    }
                }
                else {
                    var size = ids.length;

                    $(ids.chunks(20)).each(function () {
                        var request = $.ajax({
                            type: 'GET',
                            dataType: "jsonp",
                            url: oThis.options.itemsUrl,
                            data: {
                                id: this
                            }
                        })
                        .done(function (data) {
                            items = items.concat(data);

                            if (items.length == size && callback) {
                                callback(items);
                            }
                        })
                        .fail(function (xhr, error, thrownError) {
                            $.showError('Cannot get information from: ' + oThis.options.itemsUrl);
                        });
                    });
                }
            }
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
            this.element.empty();

            this.element.removeClass("cs-widget-lookup");
            this.element.removeClass("cs-widget");

            this._onDestroy();
        },

        _updateSelectedItems: function () {
            var oThis = this;

            var table = $('.modal-body > table', this.element);

            this.selected.clear();

            $('tr.success', table).each(function () {
                oThis.selected.push($(this).attr('data-id'));
            });

            this._saveState();

            this._displaySelectedItems();
        },

        _cancelSelectedItems: function () {
        },

        _displaySelectedItems: function () {
            var oThis = this;

            if (this.isMultipleMode()) {
                var ul = $('ul', this.element);

                ul.empty();

                $(this.selected).each(function (index, value) {
                    var item = oThis.getItem(value);

                    if (!item) return;

                    var li = $(document.createElement('li'))
                        .attr({
                            'data-id': value
                        })
                        .append(
                            $(document.createElement('span'))
                                .append(
                                    oThis.options.displayFormat ? oThis.options.displayFormat(item[oThis.options.idColumnName], item[oThis.options.primaryColumnName]) : $(document.createTextNode(item[oThis.options.primaryColumnName]))
                                )
                        )
                        .appendTo(ul);

                    if (!oThis.options.readonly) {
                        li.append(
                            $(document.createElement('button'))
                                .attr({
                                    type: 'button'
                                })
                                .addClass('btn btn-default btn-xs')
                                .append(
                                    $(document.createElement('span')).addClass('glyphicon glyphicon-remove')
                                )
                                .click(function () {
                                    var li = $(this).closest('li');
                                    var id = li.attr('data-id');

                                    var index = -1;
                                    for (var i = 0; i < oThis.selected.length; i++) {
                                        if (oThis.selected[i] == id) {
                                            index = i;
                                            break;
                                        }
                                    }

                                    if (index > -1) {
                                        oThis.selected.splice(index, 1);
                                        li.remove();
                                    }

                                    oThis._trigger("select", null, {
                                        selected: oThis.selected
                                    });

                                    oThis._saveState();
                                })
                        )
                    }
                });
            }
            else if (this.isSingleMode()) {
                if (this.selected == 0) {
                    this.singleSelect.text('Select...');
                }
                else {
                    var item = oThis.getItem(this.selected[0]);

                    if (!item) {
                    }
                    else {
                        this.singleSelect.text(item[oThis.options.primaryColumnName]);
                    }
                }
            }
        },

        getItem: function(id) {
            var item, i;
            for (i = 0; i < this.list.length; i++) {
                item = this.list[i];

                if (item[this.options.idColumnName] == id)
                    return item;
            }

            return null;
        },

        getSelected: function () {
            if(this.isSingleMode())
                return this.selected.length == 0 ? null : this.selected[0];
            else
                return this.selected;
        },

        setSelected: function (ids) {
            var oThis = this;

            if (this.isSingleMode()) {
                this.selected.clear();
                this.selected.push(ids);
            }
            else {
                this.selected = ids;
            }

            this._saveState();

            this._getItems(this.selected, function (data) {
                oThis.list = oThis.list.concat(data).unique();

                oThis._displaySelectedItems();

                oThis._fillModalDialog(oThis.list);
            });
        },

        getSelectedItems: function () {
            var oThis = this;

            var result = [];

            $(this.selected).each(function (index, id) {
                var item = oThis.getItem(id);

                if(!item) return;

                result.push(item);
            });

            return result;
        },

        clear: function () {
            this.selected.clear();
            this._saveState();
        },

        _onDestroy: function () {
            //  ************************************************************
            //  override this method in order to add aditional functionality
            //  ************************************************************
        },

        // _setOption is called for each individual option that is changing
        _setOption: function (key, value) {
            // In jQuery UI 1.8, you have to manually invoke the _setOption method from the base widget
            $.Widget.prototype._setOption.apply(this, arguments);
            // In jQuery UI 1.9 and above, you use the _super method instead
            this._super("_setOption", key, value);
        }
    });

}(jQuery));