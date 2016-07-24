(function ($) {
    $.widget("toolkit.form", $.toolkit.toolkitbase, {

        options: {
            labelClass: 'col-sm-3',
            fieldClass: 'col-sm-9',
            properties: [],
            data: null
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-form');

            this.form = $(document.createElement('form'))
                            .addClass('form-horizontal')
                            .appendTo(this.element);

            $('body').loadScript($.Toolkit.store.getUrl('Widgets/3rd/bootstrap.filestyle.js'), function () {
                //oThis.uploadFile.filestyle({ buttonText: "Find file" });

                oThis._addFields();

                if (oThis.options.data) {
                    oThis.setData(oThis.options.data);
                }
            });
        },

        // called when created, and later when changing options
        _refresh: function () {
        },

        // Use the _setOption method to respond to changes to options
        _setOption: function (key, value) {
            $.Widget.prototype._setOption.apply(this, arguments);
        },

        _setOptions: function () {
            $.Widget.prototype._setOptions.apply(this, arguments);
            this._refresh();
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
        },

        _addFields: function() {
            this.form.empty();

            for (var i = 0; i < this.options.properties.length; i++) {
                var property = this.options.properties[i];

                var label = $(document.createElement('label'))
                                .addClass(this.options.labelClass)
                                .addClass('control-label')
                                .text(property.title);

                var field = null;

                if (property.type == 'string' ||
                    property.type == 'phone' ||
                    property.type == 'fax' ||
                    property.type == 'url' ||
                    !property.type) {
                    field = $(document.createElement('input'))
                                .addClass('form-control')
                                .attr({
                                    type: 'text',
                                    name: property.name
                                });

                    if (property.readonly) {
                        field.attr({ readonly: true });
                    }
                }
                else if (property.type == 'email') {
                    field = $(document.createElement('input'))
                                .addClass('form-control')
                                .attr({
                                    type: 'email',
                                    name: property.name
                                });

                    if (property.readonly) {
                        field.attr({ readonly: true });
                    }
                }
                else if (property.type == 'file') {
                    field = $(document.createElement('input'))
                                .addClass('filestyle')
                                .attr({
                                    type: 'file',
                                    name: property.name,
                                    accept: property.accept
                                });

                    if (property.multiple)
                        field.attr({ multiple: true });
                }
                else if (property.type == 'bool') {
                    field = $(document.createElement('input'))
                                .addClass('form-control')
                                .attr({
                                    type: 'checkbox',
                                    name: property.name
                                });

                    if (property.readonly) {
                        field.attr({ disabled: 'disabled' });
                    }
                }
                else if (property.type == 'html' || property.type == 'text') {
                    field = $(document.createElement('textarea'))
                                .addClass('form-control')
                                .attr({
                                    rows: 3,
                                    name: property.name
                                });

                    if (property.readonly) {
                        field.attr({ readonly: true });
                    }
                }
                else if (property.type == 'user_lookup') {
                    field = $(document.createElement('div'))
                                .addClass('form-control')
                                .attr({
                                    name: property.name
                                })
                                .user_lookup(property.options);
                }
                else if (property.type == 'datasource_lookup') {
                    field = $(document.createElement('div'))
                                .addClass('form-control')
                                .attr({
                                    name: property.name
                                })
                                .datasource_lookup(property.options);
                }

                $(document.createElement('div'))
                    .addClass('form-group')
                    .append(label)
                    .append(
                        $(document.createElement('div'))
                            .addClass(this.options.fieldClass)
                            .append(field)
                    )
                    .appendTo(this.form);
            }

            $('input.filestyle', this.form).filestyle({ buttonText: "Find file" });
        },

        setData: function (data) {
            for (var i = 0; i < this.options.properties.length; i++) {
                var property = this.options.properties[i];

                var value = data[property.name];

                if (property.type == 'string' ||
                    property.type == 'phone' ||
                    property.type == 'fax' ||
                    property.type == 'url' ||
                    property.type == 'email' ||
                    !property.type) {
                    $('input[name="' + property.name + '"]', this.element).val(value);
                }
                else if (property.type == 'text' ||
                    property.type == 'html') {
                    $('textarea[name="' + property.name + '"]', this.element).val(value);
                }
                else if (property.type == 'bool' ) {
                    $('input[name="' + property.name + '"]', this.element).attr('checked', value);
                }
                else if (property.type == 'user_lookup') {
                    $('div[name="' + property.name + '"]', this.element).user_lookup('setSelected', value);
                }
                else if (property.type == 'datasource_lookup') {
                    $('div[name="' + property.name + '"]', this.element).datasource_lookup('setSelected', value);
                }
            }
        },

        getData: function () {
            var oThis = this;

            var data = {}

            $(this.options.properties).each(function (index, prop) {
                data[prop.name] = oThis.getValue(prop.name);
            });

            return data;
        },

        getValue: function (name) {
            var property = null;

            $(this.options.properties).each(function (index, prop) {
                if (prop.name == name)
                    property = prop;
            });

            if (property) {
                if (property.type == 'string' ||
                    property.type == 'phone' ||
                    property.type == 'fax' ||
                    property.type == 'url' ||
                    property.type == 'email' ||
                    !property.type) {
                    return $('input[name="' + property.name + '"]', this.element).val();
                }
                else if (property.type == 'text' ||
                    property.type == 'html') {
                    return $('textarea[name="' + property.name + '"]', this.element).val();
                }
                else if (property.type == 'file') {
                    return $('input[name="' + property.name + '"]', this.element)[0].files;
                }
                else if (property.type == 'bool') {
                    return $('input[name="' + property.name + '"]', this.element).prop('checked');
                }
                else if (property.type == 'user_lookup') {
                    return $('div[name="' + property.name + '"]', this.element).user_lookup('getSelected');
                }
                else if (property.type == 'datasource_lookup') {
                    return $('div[name="' + property.name + '"]', this.element).datasource_lookup('getSelected');
                }
            }

            return null;
        }
    });

}(jQuery));
