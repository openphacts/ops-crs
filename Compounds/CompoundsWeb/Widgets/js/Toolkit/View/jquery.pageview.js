(function ($) {
    $.widget("toolkit.pageview", $.toolkit.toolkitbase, {

        options: {
            pageSize: 15,
            pageSizes: [10, 15, 30, 50],
            saveState: true,
            count: 0,
            countProviderUrl: null, //  endpoint MUST return integer as total amount of records that should be displayed
            items: null,            //  array of items that should be displayed
            itemsProviderUrl: null, //  endpoint returns list items that should be desplayed. MUST support JSONP as well as 'start' and 'count' request parameters
            title: 'Results'
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-pageview panel panel-default');

            this.pageSize = this.options.pageSize;
            this.pageIndex = 1;

            this._createHeader();

            this._createContent();

            this._createFooter();

            this._loadState();

            this._onInit();

            this.options.onPageRendered = function () {
                oThis.contentDiv.hideProgress();
            }

            this.load();
        },

        _onInit: function() {
        },

        reload: function () {
            this.load();
        },

        load: function () {
            var oThis = this;

            //  prepare page content and display the first page...
            this._prepareContent(function (count) {
                oThis.count = count;
                oThis.totalPages = Math.floor(oThis.count / oThis.pageSize);
                if (oThis.totalPages * oThis.pageSize < oThis.count)
                    oThis.totalPages += 1;

                oThis.countBadge.text(oThis.count);

                //if (oThis.count > 0) {
                    //  draw first page...
                    oThis._drawPage(oThis.pageIndex);
                //}
            });
        },

        //  _prepareContent - function that needed to calculate the total number of records that should be displayed.
        //  Sometimes count will be specified throught the widget's options, sometime it should be loaded from some endpoint
        //  and sometime it will be precalculated based on search results and in this case we should wait while the search 
        //  procedure is finished and results are ready...
        _prepareContent: function (callback) {
            if (!callback) {
                $.showError('toolkit.pageview._prepareContent: callback cannot be <i>null</i>!');
                return;
            }

            if (this.options.countProviderUrl) {
                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.options.countProviderUrl
                })
                .done(function (count) {
                    callback(count);
                })
                .fail(function (xhr, error, thrownError) {
                    $.showError('Error in procedure <i>toolkit.pageview._prepareContent</i> has happened.<br/></br>' + xhr.responseText);
                });
            }
            else if (this.options.items) {
                callback(this.options.items.length);
            }
            else {
                callback(this.options.count);
            }
        },

        _loadState: function () {
            if (this.options.saveState) {
                var state = $.sessionStorage.get(this._getStateCookieName());
                if (state != null) {
                    this.pageSize = state.size,
                    this.pageIndex = state.index

                    $('select', this.footerDiv).val(this.pageSize);

                    this._initFromState(state);
                }
            }
        },

        _saveState: function () {
            if (this.options.saveState) {
                var state = this._prepareStateToSave()

                $.sessionStorage.set(this._getStateCookieName(), $.extend(state, {
                    index: this.pageIndex,
                    size: this.pageSize
                }));
            }
        },

        _initFromState: function (state) {
            //  override this method if you want to save/load any additional parameters
        },

        _prepareStateToSave: function () {
            //  override this method if you want to save/load any additional parameters
            return {};
        },

        _createHeader: function () {
            this.titleDiv = $(document.createElement('div'))
                .addClass('cs-widget-pageview-header panel-heading')
                .appendTo(this.element);

            this.countBadge = $(document.createElement('span')).addClass('badge');

            this.titleDiv
                .append($(document.createTextNode(this.options.title + ' ')))
                .append(this.countBadge);

            //if (this.options.allowSearch) {
            //    this.titleDiv.append(this._createSearch());
            //}
        },

        _createSearch: function () {
            this.searchBtn = $(document.createElement('button'))
                .attr({
                    type: 'button',
                    title: 'Search'
                })
                .addClass('btn btn-default btn-sm')
                .click(function () {
                    alert('Start search!')
                })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-search')
                );

            this.searchDiv = $(document.createElement('div'))
                .addClass('search')
                .append(this.searchBtn)
                .append(
                    $(document.createElement('input'))
                        .addClass('form-control input-sm')
                        .attr({
                            type: 'text'
                        })
                );

            return this.searchDiv;
            //<input type="email" class="form-control" id="exampleInputEmail1" placeholder="Enter email">
        },

        _createContent: function () {
            this.contentDiv = $(document.createElement('div'))
                .addClass('cs-widget-pageview-content')
                .appendTo(this.element);
        },

        _createFooter: function () {
            var oThis = this;

            this.footerDiv = $(document.createElement('div'))
                .addClass('cs-widget-pageview-footer panel-footer')
                .appendTo(this.element);

            var pageSizesList = $(document.createElement('select'))
                                    .addClass('form-control input-sm')
                                    //.width(50)
                                    .change(function () {
                                        oThis.pageSize = Number($(this).val());
                                        oThis.totalPages = Math.floor(oThis.count / oThis.pageSize);
                                        if (oThis.totalPages * oThis.pageSize < oThis.count)
                                            oThis.totalPages += 1;
                                        oThis._drawPage(1);
                                    });

            for (var i = 0; i < this.options.pageSizes.length; i++) {
                var size = this.options.pageSizes[i];
                var option = $(document.createElement('option'))
                                .val(size)
                                .text(size)
                                .appendTo(pageSizesList);
                if (size == this.options.pageSize) {
                    option.attr({ selected: 'selected' })
                }
            }

            $('option[value=' + this.options.pageSize + ']', this.list).attr('selected', 'selected');

            this.firstBtn = $(document.createElement('button'))
                .attr({
                    type: 'button',
                    title: 'First Page'
                })
                .addClass('btn btn-default btn-sm')
                .click(function () {
                    oThis._drawPage(1);
                })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-fast-backward')
                );

            this.prevBtn = $(document.createElement('button'))
                .attr({
                    type: 'button',
                    title: 'Previous Page'
                })
                .addClass('btn btn-default btn-sm')
                .click(function () {
                    oThis._drawPage(oThis.pageIndex - 1);
                })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-backward')
                );

            this.nextBtn = $(document.createElement('button'))
                .attr({
                    type: 'button',
                    title: 'Next Page'
                })
                .addClass('btn btn-default btn-sm')
                .click(function () {
                    oThis._drawPage(oThis.pageIndex + 1);
                })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-forward')
                );

            this.lastBtn = $(document.createElement('button'))
                .attr({
                    type: 'button',
                    title: 'Last Page'
                })
                .addClass('btn btn-default btn-sm')
                .click(function () {
                    oThis._drawPage(oThis.totalPages);
                })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-fast-forward')
                );

            $(document.createElement('div'))
                .addClass('navbar-left')
                .append(pageSizesList)
                .appendTo(this.footerDiv);

            $(document.createElement('div'))
                .addClass('navbar-left')
                .append(this.firstBtn)
                .append(this.prevBtn)
                .append(this.nextBtn)
                .append(this.lastBtn)
                .appendTo(this.footerDiv);

            this.statusBar = $(document.createElement('div'))
                .addClass('navbar-right')
                .appendTo(this.footerDiv);

            $(document.createElement('br'))
                        .css('clear', 'both')
                        .appendTo(this.footerDiv);
        },

        // called when created, and later when changing options
        _refresh: function () {
        },

        // Use the _setOption method to respond to changes to options
        _setOption: function (key, value) {
            if (key == 'items') {
                this.setItems(value);
            }

            this._super(key, value);
        },

        _setOptions: function (options) {
            var oThis = this;

            $.each(options, function (key, value) {
                oThis._setOption(key, value);
            });
        },

        setItems: function (items) {
            var oThis = this;

            this.options.items = items;

            //  prepare page content and display the first page...
            this._prepareContent(function (count) {
                oThis.count = count;
                oThis.totalPages = Math.floor(oThis.count / oThis.pageSize);
                if (oThis.totalPages * oThis.pageSize < oThis.count)
                    oThis.totalPages += 1;

                oThis.countBadge.text(oThis.count);

                //if (oThis.count > 0) {
                    //  draw first page...
                    oThis._drawPage(oThis.pageIndex);
                //}
            });
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
        },

        _drawPage: function (index) {
            var oThis = this;

            if (index > this.totalPages)
                index = this.totalPages == 0 ? 1 : this.totalPages;

            this.pageIndex = index;

            this.contentDiv.loadProgress('Loading...');

            this._loadPage((this.pageIndex - 1) * this.pageSize, this.pageSize, function (items) {
                oThis._drawItems(items);
            });

            this._setStatusBar(index);

            if (index == 1) {
                this.firstBtn.prop('disabled', true);
                this.prevBtn.prop('disabled', true);
            }
            else {
                this.firstBtn.prop('disabled', false);
                this.prevBtn.prop('disabled', false);
            }

            if (index == this.totalPages) {
                this.nextBtn.prop('disabled', true);
                this.lastBtn.prop('disabled', true);
            }
            else {
                this.nextBtn.prop('disabled', false);
                this.lastBtn.prop('disabled', false);
            }

            this._saveState();
        },

        _loadPage: function (start, count, callback) {
            if (!callback) {
                $.showError('toolkit.pageview._loadPage: callback cannot be <i>null</i>!');
                return;
            }

            if (this.options.items != null) {
                //  we have the list of items that we should display...

                var items = this.options.items.slice(start, start + count);
                callback(items);
            }
            else if (this.options.itemsProviderUrl) {
                //  ... we have URL where we should get items from...

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.options.itemsProviderUrl,
                    data: {
                        start: start,
                        count: count
                    }
                })
                .done(function (items) {
                    callback(items);
                })
                .fail(function (xhr, error, thrownError) {
                    $.showError('Error in procedure <i>toolkit.pageview._loadPage</i> has happened.<br/></br>' + xhr.responseText);
                });
            }
            else {
                $.showError('toolkit.pageview._loadPage: None of items provider is specified!');
            }
        },

        _drawItems: function (items) {
            //  override this function in order to implement custom items rendering...
            $.showError('toolkit.pageview._drawItems: Drawing function is not implemented!');
        },

        _setStatusBar: function (index) {
            this.statusBar.text('Displaying ' + ((this.pageIndex - 1) * this.pageSize + 1) + ' to ' + Math.min(this.pageIndex * this.pageSize, this.count) + ' of ' + this.count + ' items');
        },
    });
}(jQuery));
