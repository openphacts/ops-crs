(function ($) {
    $.widget("compound.nmrfeatures", {

        options: {
            id: 0,
            showAll: false
        },

        // Set up the widget
        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-nmrfeatures');

            $(document.createElement('table'))
                .addClass('table')
                .append(
                    $(document.createElement('thead'))
                        .append(
                            $(document.createElement('tr'))
                                .append($(document.createElement('th')).text('Count'))
                                .append($(document.createElement('th')).text('Name'))
                                .append($(document.createElement('th')).text('Description'))
                        )
                )
                .append(
                    $(document.createElement('tbody'))
                )
                .appendTo(this.element);

            if (oThis.options.id > 0) {
                oThis.setID(oThis.options.id);
            }
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
            this.molA.remove();
            $('.cs-widget-nmrfeatures', this.element).remove();

            this.element.removeClass("cs-widget-nmrfeatures");
        },

        /*
        Function: setID

        Function that loads data from ChemSpider website and display them in the component.

        Parameters:
        csid - Compound ChemSpider ID that should be loaded

        (start code)
        $('#compoundInfo').compoundinfo("setCSID", 2157);
        (end code)
        */
        setID: function (id) {
            var oThis = this;

            this.options.id = id;

            this.element.loadProgress();

            this._loadData(this.options.id, function (features) {
                oThis._displayFeatures(features);
                oThis.element.hideProgress();
            });
        },

        _loadData: function (id, callback) {
            $.Compounds.store.getNmrFeatures(id, function (features) {
                if (callback != null)
                    callback(features);
            });
        },

        _getFeaturesList: function (callback) {
            var oThis = this;

            if (this.allFeatures == null) {
                $.Compounds.store.getAllNmrFeatures(function (features) {
                    oThis.allFeatures = features;

                    if (callback != null)
                        callback(features);
                });
            }
            else {
                if (callback != null)
                    callback(this.allFeatures);
            }
        },

        _displayFeatures: function (features) {
            var oThis = this;

            var tbody = $('tbody', this.element);

            tbody.empty();

            this._getFeaturesList(function (allFeatures) {
                $(features).each(function (index, compoundFeature) {
                    var feature = $.grep(allFeatures, function (item) {
                        return item.Id == compoundFeature.NMRFeatureId;
                    })[0];

                    var tr = $(document.createElement('tr'))
                        .attr({
                            'data-count': compoundFeature.Count
                        })
                        .append(
                            $(document.createElement('td')).text(compoundFeature.Count)
                        )
                        .append(
                            $(document.createElement('td')).text(feature.Name)
                        )
                        .append(
                            $(document.createElement('td')).text(feature.Description)
                        )

                    if (!oThis.options.showAll && compoundFeature.Count == 0) {
                        tr.hide();
                    }

                    tbody.append(tr);
                });
            });
        },

        showAll: function () {
            $('tr', this.element).show();
        },

        hideZero: function () {
            $('tr[data-count="0"]', this.element).hide();
        }
    });

}(jQuery));
