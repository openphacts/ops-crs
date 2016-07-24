$(function () {
    $.widget("toolkit.editor_base", $.toolkit.toolkitbase, {

        options: {
            width: 300,
            height: 300,
            showBtnIcon: true,
            showBtnText: false,
            saveState: false,
            emptyText: 'Empty',
            editDialogWidth: 820,
            editDialogHeight: 640
        },

        // the constructor
        _create: function () {
            var oThis = this;

            var id = this.element.attr('id');

            this.element
                .addClass("cs-widget cs-widget-editor")
                .width(this.options.width)
                .height(this.options.height);

            this.emptySpan = $(document.createElement('span'))
                .height(this.options.height)
                .width(this.options.width)
                .css({
                    'line-height': this.options.height + 'px'
                })
                .addClass('empty-text')
                .text(this.options.emptyText)
                .appendTo(this.element);

            this.molImg = $(document.createElement('img'))
                .addClass('cs-widget-edit-image')
                .attr({
                    src: $.Compounds.store.getUrl('/images/empty.png'),
                    width: this.options.width,
                    height: this.options.height
                }).appendTo(this.element);

            this.topToolbar = $(document.createElement('div'))
                                    .addClass('top-toolbar')
                                    .appendTo(this.element);

            this._buildTopToolbar();

            this._buildEditDialog();

            if (this.options.saveState) {
                var state = $.localStorage.get(this._getStateCookieName());
                if (state != null) {
                    this._loadState(state);
                }
            }

            this._onInit();
        },

        _onInit: function () {
            //  ************************************************************
            //  override this method in order to add aditional functionality
            //  ************************************************************
        },

        _loadState: function(value) {
            //  ****************************************************
            //  override this method in order to load widget's state
            //  ****************************************************
        },

        _saveState: function (value) {
            if (this.options.saveState) {
                $.localStorage.set(this._getStateCookieName(), value);
                //$.cookie(this._getStateCookieName(), value);
            }
        },

        _editButton: function() {
            var oThis = this;

            return $(document.createElement('button'))
                .addClass('cs-edit-button')
                .text('Edit')
                .button({
                    icons: {
                        primary: this.options.showBtnIcon ? "ui-icon-pencil" : ""
                    },
                    text: this.options.showBtnText
                })
                .click(function (event) {
                    oThis.editDlg.dialog("open");

                    oThis._onEditDialogOpen();

                    event.stopPropagation();
                    return false;
                });
        },

        _buildTopToolbar: function () {
            var editButton = this._editButton();

            this.topToolbar.append(editButton);
        },

        _buildEditDialog: function () {
            var oThis = this;

            this.editDlg = $(document.createElement('div'))
                    .attr({ title: 'Edit Structure' })
                    .css({ display: 'none' })
                    .appendTo(this.element);

            this.editDlg.append(
                $(document.createElement('ul'))
            );

            this.editDlg.dialog({
                width: this.options.editDialogWidth,
                height: this.options.editDialogHeight,
                resizable: false,
                autoOpen: false,
                buttons: {
                    Ok: function () {
                        oThis._onEditDialogOk();

                        $(this).dialog("close");
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                },
                open: function () {
                    oThis._buildTabs();
                }
            });
        },

        _onEditDialogOpen: function() {
            //  ************************************************************************
            //  override this method if you need to add any functionality on open dialog
            //  ************************************************************************
        },

        _onEditDialogOk: function () {
            //  **********************************************************************
            //  override this method if you need to add any functionality on Ok action
            //  **********************************************************************
        },

        _buildEditors: function () {
            //  ******************************************************************
            //  override this method in order to implement desired list of editors
            //  ******************************************************************
        },

        _buildTabs: function () {
            if (this.editDlgBuild == true)
                return;

            this._buildEditors();

            //  apply custom styles...
            $('div.ui-tabs-panel', this.editDlg).css('padding', 0);
            this.editDlg.css('padding', '0.2em');
            $('.ui-dialog-buttonpane', this.editDlg.parent()).css('padding', 0);

            this.editDlgBuild = true;
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
            this.element.empty();

            this.element.removeClass("cs-widget-editor");
            this.element.removeClass("cs-widget");

            this._onDestroy();
        },

        _onDestroy: function() {
        },

        // _setOption is called for each individual option that is changing
        _setOption: function (key, value) {
            // In jQuery UI 1.8, you have to manually invoke the _setOption method from the base widget
            $.Widget.prototype._setOption.apply(this, arguments);
            // In jQuery UI 1.9 and above, you use the _super method instead
            this._super("_setOption", key, value);
        }
    });

} (jQuery));