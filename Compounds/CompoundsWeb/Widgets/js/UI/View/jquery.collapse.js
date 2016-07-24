$(function () {
    $.widget("ui.collapse", {

        options: {
            collapseAll: true,
            saveState: true
        },

        // the constructor
        _create: function () {
            var oThis = this;

            this.element
                .addClass("cs-widget cs-widget-collapse");

            $('h3', this.element)
                .addClass('header ui-state-active')
                .prepend(
                    $(document.createElement('span'))
                        .addClass('ui-accordion-header-icon ui-icon ui-icon-triangle-1-s')
                )
                .click(function () {
                    if ($(this).is('.collapsed')) {
                        oThis.expand($(this).parent().children('h3').index(this));
                    }
                    else {
                        oThis.collapse($(this).parent().children('h3').index(this));
                    }

                    oThis._saveState();
                })
                .next().addClass('body');

            if (this.options.collapseAll) {
                $('h3', this.element).addClass('collapsed').next().hide();
            }

            this._loadState();
        },

        _saveState: function () {
            var oThis = this;

            if (this.options.saveState) {
                var storageId = this.element.attr('id') + '_STATE'

                var state = {
                    items: []
                };

                this.element.children('h3').each(function (index, header) {
                    state.items[index] = $(header).is('.collapsed') ? 0 : 1;
                });

                $.localStorage.set(storageId, state);
            }
        },

        _loadState: function () {
            var oThis = this;

            if (this.options.saveState) {
                var storageId = this.element.attr('id') + '_STATE'

                var state = $.localStorage.get(storageId);

                if (state != null) {
                    if (state.items) {
                        $(state.items).each(function (index, item) {
                            if (item == 0)
                                oThis.collapse(index);
                            else
                                oThis.expand(index);
                        });
                    }
                }
            }
        },

        collapse: function (index) {
            var header = this.element.children('h3').eq(index);

            header.addClass('collapsed');
            header.next().hide();

            header.removeClass('ui-state-active').addClass('ui-state-default');
            $('.ui-icon', header).removeClass('ui-icon-triangle-1-s').addClass('ui-icon-triangle-1-e');
        },

        expand: function (index) {
            var header = this.element.children('h3').eq(index);

            header.removeClass('collapsed');
            header.next().show();

            $(header).removeClass('ui-state-default').addClass('ui-state-active');
            $('.ui-icon', header).removeClass('ui-icon-triangle-1-e').addClass('ui-icon-triangle-1-s');
        },

        isCollapsed: function (index) {
            return this.element.children('h3').eq(index).is('collapsed');
        },

        isExpanded: function (index) {
            return !this.isCollapsed();
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
            this.element
                .removeClass("cs-widget cs-widget-collapse");
        },

        // _setOption is called for each individual option that is changing
        _setOption: function (key, value) {
            this._super("_setOption", key, value);
        }
    });

}(jQuery));
