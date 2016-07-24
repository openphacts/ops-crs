(function ($) {
    $.widget("depositions.deposition_new", {

        options: {
            accept: '.mol, .cdx, .sdf, .zip, .gz',
            showButtons: true,
            onSave: null,
            properties: {
                Datasource: null,
                Validate: true,
                Standardize: true,
                PropertiesCalculation: true,
                ParentsGeneration: true,
                Public: true
            }
        },

        _create: function () {
            var oThis = this;

            this.element.addClass('cs-widget cs-widget-new-deposition');

            this.form = $(document.createElement('div'))
                            .form({
                                labelClass: 'col-sm-4',
                                fieldClass: 'col-sm-8',
                                properties: [
                                    { name: 'Datasource', title: 'Datasource', type: 'datasource_lookup', options: { mode: 'single' } },
                                    { name: 'Files', title: 'File', type: 'file', accept: this.options.accept, multiple: true },
                                    { name: 'Validate', title: 'Validate', type: 'bool', readonly: true },
                                    { name: 'Standardize', title: 'Standardize', type: 'bool', readonly: true },
                                    { name: 'PropertiesCalculation', title: 'Calculate Properties', type: 'bool' },
                                    { name: 'ParentsGeneration', title: 'Parents Generation', type: 'bool' },
                                    { name: 'Public', title: 'Public', type: 'bool' },
                                ],
                                data: this.options.properties
                            })
                            .appendTo(this.element);

            if (this.options.showButtons) {
                this._showFormButtons();
            }
        },

        _showFormButtons: function () {
            var oThis = this;

            this.cancelButton = $(document.createElement('button'))
                                .addClass('btn btn-default pull-right cancel-btn')
                                .attr({
                                    type: 'button'
                                })
                                .append(
                                    $(document.createElement('span'))
                                        .addClass('glyphicon glyphicon-remove')
                                        .attr({ 'aria-hidden': true })
                                )
                                .append(
                                    $(document.createTextNode(' Cancel'))
                                )
                                .click(function () {
                                    oThis.cancel();
                                });

            this.submitButton = $(document.createElement('button'))
                                .addClass('btn btn-default pull-right save-btn')
                                .attr({
                                    type: 'button'
                                })
                                .append(
                                    $(document.createElement('span'))
                                        .addClass('glyphicon glyphicon-ok')
                                        .attr({ 'aria-hidden': true })
                                )
                                .append(
                                    $(document.createTextNode(' Submit'))
                                )
                                .click(function () {
                                    oThis.save();
                                });

            $(document.createElement('div'))
                //.append(this.cancelButton)
                .append(this.submitButton)
                .appendTo(this.element);
        },

        save: function () {
            var oThis = this;

            var data = this.form.form('getData');

            if (data.Files.length == 0) {
                this.element.showMessage('Warning', 'Please select some file first!')
                return;
            }

            this.element.loadProgress();

            $.Depositions.store.newDeposition(data, function (guid) {
                oThis.element.hideProgress();
                oThis._trigger("onSave", null, guid);
            }, function (procName, xhr, error, thrownError) {
                if (xhr.status == 409) {
                    oThis.element.showConflict(xhr.responseText)
                }
                else {
                    $.Datasources.store.onError(procName, xhr, error, thrownError);
                }
            });

            //$.Depositions.store.newDeposition({
            //        Validate: this.form.form('getValue', 'Validate'),
            //        Standardize: this.form.form('getValue', 'Standardize'),
            //    },
            //    files,
            //    function (guid) {
            //        oThis.element.hideProgress();

            //        oThis._trigger("onSave", null, guid);
            //    },
            //    function (procName, xhr, error, thrownError) {
            //        if (xhr.status == 409) {
            //            oThis.element.showConflict(xhr.responseText)
            //        }
            //        else {
            //            $.Datasources.store.onError(procName, xhr, error, thrownError);
            //        }
            //    });
        },

        cancel: function () {
        }
    });

}(jQuery));
