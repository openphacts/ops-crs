(function ($) {
    $.widget("properties.properties_gridview", $.toolkit.gridview, {

        options: {
            id: null,
            title: 'Properties',
            columns: [
                { name: 'Name', title: 'Name', format: formatPropertyName },
                { name: 'Value', title: 'Value', format: formatPropertyValue }
            ],
        },

        _onInit: function () {
            var oThis = this;

            $.Properties.store.getUnits(function (units) {
                oThis.units = units;

                $.Properties.store.getPropertyDefinitions(function (definitions) {
                    oThis.propertyDefinitons = definitions;

                    oThis._super();
                });
            });

            this.element.addClass('cvsp-widget-properties-gridview');
        },

        _prepareContent: function (callback) {
            var oThis = this;

            if(this.options.id != null) {
                this.element.loadProgress();

                $.Records.store.getRecordProperties(this.options.id, function (guids) {
                    $.Properties.store.getProperties(guids, function (properties) {
                        oThis.element.hideProgress();

                        oThis.options.items = properties;

                        $(oThis.options.items).each(function (index, prop) {
                            prop.Definition = oThis._getPropertyDefinition(prop.Name);

                            if (prop.OriginalUnitId != null)
                                prop.Unit = oThis._getUnit(prop.OriginalUnitId);
                            else if (prop.Definition.DefaultUnitId != null)
                                prop.Unit = oThis._getUnit(prop.Definition.DefaultUnitId);

                            if (prop.Conditions != null && prop.Conditions != null) {
                                $(prop.Conditions).each(function (index, condition) {
                                    condition.Definition = oThis._getPropertyDefinition(condition.Name);

                                    if (condition.OriginalUnitId != null)
                                        condition.Unit = oThis._getUnit(condition.OriginalUnitId);
                                    else if (condition.Definition.DefaultUnitId != null)
                                        condition.Unit = oThis._getUnit(condition.Definition.DefaultUnitId);
                                });
                            }
                        });

                        if (callback != null)
                            callback(oThis.options.items.length);
                    });
                });
            }
        },

        _getUnit: function (id) {
            for (var i = 0; i < this.units.length; i++) {
                if (this.units[i].Id == id)
                    return this.units[i];
            }

            return null;
        },

        _getPropertyDefinition: function (name) {
            for (var i = 0; i < this.propertyDefinitons.length; i++) {
                if (this.propertyDefinitons[i].Name == name)
                    return this.propertyDefinitons[i];
            }

            return null;
        },
    });

}(jQuery));

function formatPropertyName(value, prop)
{
    var result = prop.Definition.DisplayName;

    if (prop.Conditions != null && prop.Conditions.length > 0) {
        result += ' (';

        for (var i = 0; i < prop.Conditions.length; i++) {
            var condition = prop.Conditions[i];

            if (i > 1)
                result += ', ';

            result += condition.Definition.DisplayName + ' ' + condition.Value;
        }

        result += ')';
    }

    return result;
}

function formatPropertyValue(value, prop)
{
    var result = value;

    if (prop.Error != null && prop.Error > 0) {
        result += '&#xB1;' + prop.Error;
    }

    if (prop.Unit != null) {
        result += ' ' + prop.Unit.DisplayName;
    }

    return result;
}
