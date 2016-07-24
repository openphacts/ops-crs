(function ($) {
    $.widget("record.fields_gridview", $.toolkit.gridview, {

        options: {
            id: null,
            title: 'Fields',
            columns: [
                { name: 'Name', title: 'Name' },
                {
                    name: 'Value', title: 'Value', format: function (value) {
                        var expression = /[-a-zA-Z0-9@:%_\+.~#?&//=]{2,256}\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]*)?/gi;
                        var regex = new RegExp(expression);
                        if (value.match(regex)) {
                            return $(document.createElement('a'))
                                .attr({
                                    href: value,
                                    target: '_blank'
                                })
                                .text(value);
                        }
                        else {
                            return value;
                        }
                    }
                }
            ],
        },

        _onInit: function () {
            this._super();

            this.element.addClass('cvsp-widget-fields-gridview');
        },

        _prepareContent: function (callback) {
            var oThis = this;

            if (this.options.id != null) {
                this.element.loadProgress();

                $.Records.store.getRecordFields(this.options.id, function (fields) {
                    oThis.element.hideProgress();

                    oThis.options.items = fields;

                    if (callback != null)
                        callback(oThis.options.items.length);
                });
            }
        },
    });

}(jQuery));
