(function ($) {
    $.widget("annotation.annotation_add", {

        options: {
            guid: null
        },

        _create: function () {
            var oThis = this;

            this.id = 'addAnnotation';

            this.element.addClass('cs-widget cs-widget-new-annotation');

            this._loadData(function () {
                oThis._showAnnotations();
                oThis._addAnnotationButton();
                oThis._createModal();
            });
        },

        _loadData: function(callback) {
            var oThis = this;

            this._showProgress();

            $.Annotations.store.getAllAnnotations(function (annotations) {
                oThis.allAnnotations = annotations;

                $.Depositions.store.getDepositionFields(oThis.options.guid, function (fields) {
                    oThis.allFields = fields;

                    oThis._hideProgress();

                    if (callback != null)
                        callback();
                });
            });
        },

        _showAnnotations: function () {
            var oThis = this;

            this._showProgress();

            if (this.annotationsTable == null) {
                this.annotationsTable = $(document.createElement('table'))
                    .addClass('table table-condensed table-hover')
                    .append(
                        $(document.createElement('thead'))
                            .append(
                                $(document.createElement('tr'))
                                    .append(
                                        $(document.createElement('th')).text('Field')
                                    )
                                    .append(
                                        $(document.createElement('th')).text('Annotation')
                                    )
                                    .append(
                                        $(document.createElement('th')).css({width: '50px'})
                                    )
                            )
                    )
                    .append(
                        $(document.createElement('tbody'))
                    )
                    .appendTo(this.element);
            }

            var body = this.annotationsTable.find('tbody');
            body.empty();

            $.Depositions.store.getDepositionAnnotations(oThis.options.guid, function (depositionAnnotations) {
                oThis.depositionAnnotations = depositionAnnotations;

                $(oThis.depositionAnnotations).each(function () {
                    $(document.createElement('tr'))
                        .data({
                            annotation: this.Annotaition.Name,
                            field: this.Name
                        })
                        .append(
                            $(document.createElement('td')).text(this.Name)
                        )
                        .append(
                            $(document.createElement('td')).text(this.Annotaition.Title)
                        )
                        .append(
                            $(document.createElement('td'))
                                .append(
                                    $(document.createElement('button'))
                                        .addClass('btn btn-default btn-xs delete-btn')
                                        .attr({ type: 'button' })
                                        .append(
                                            $(document.createElement('span'))
                                                .addClass('glyphicon glyphicon-trash')
                                                .attr({ 'aria-hidden': true })
                                        )
                                        .click(function () {
                                            oThis._deleteAnnotation($(this.closest('tr')).data('annotation'));
                                        })
                                        .hide()
                                )
                        )
                        .hover(function () {
                            $('.delete-btn', this).show();
                        }, function () {
                            $('.delete-btn', this).hide();
                        })
                        .appendTo(body);
                });

                oThis._hideProgress();
            });
        },

        _addAnnotationButton: function () {
            this.addAnnotationButton = $(document.createElement('button'))
                .addClass('btn btn-default btn-xs')
                .attr({ type: 'button', 'data-toggle': 'modal', 'data-target': '#' + this.id + 'Modal' })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-plus')
                        .attr({ 'aria-hidden': true })
                )
                .append(document.createTextNode(' Add Annotation'))
                .appendTo(this.element);
        },

        _createModal: function () {
            var oThis = this;

            this.modalBody = $(document.createElement('div'))
                .append(
                    $(document.createElement('div'))
                        .addClass('row')
                        .append(
                            $(document.createElement('div'))
                                .addClass('col-md-5')
                                .append(this._allAnnotationsSelect())
                        )
                        .append(
                            $(document.createElement('div'))
                                .addClass('col-md-2')
                                .css({ 'text-align' : 'center' })
                                .append(
                                    $(document.createElement('span'))
                                        .addClass('glyphicon glyphicon-link')
                                        .attr({ 'aria-hidden': true })
                                )
                        )
                        .append(
                            $(document.createElement('div'))
                                .addClass('col-md-5')
                                .append(this._allFieldsSelect())
                        )
                )
                .addClass('modal-body');

            this.modal = $(document.createElement('div'))
                .addClass('modal fade')
                .attr({
                    id: this.id + 'Modal',
                    tabindex: -1,
                    role: 'dialog',
                    'aria-labelledby': this.id + 'ModalLabel'
                })
                .append(
                    $(document.createElement('div'))
                        .addClass('modal-dialog')
                        .attr({ role: 'document' })
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
                                                .attr({
                                                    id: this.id + 'ModalLabel'
                                                })
                                                .text('Add Annotation')
                                        )
                                )
                                .append(this.modalBody)
                                .append(
                                    $(document.createElement('div'))
                                        .addClass('modal-footer')
                                        .append(
                                            $(document.createElement('button'))
                                                .addClass('btn btn-default')
                                                .attr({ type: 'button', 'data-dismiss': 'modal' })
                                                .text('Cancel')
                                        )
                                        .append(
                                            $(document.createElement('button'))
                                                .addClass('btn btn-primary')
                                                .attr({ type: 'button', 'data-dismiss': 'modal' })
                                                .text('Add')
                                                .click(function () {
                                                    oThis._addAnnotation();
                                                })
                                        )
                                )
                        )
                )
                .appendTo(this.element);
        },

        _allAnnotationsSelect: function () {
            var oThis = this;

            this.allAnnotationsSelect = $(document.createElement('select')).addClass('form-control');

            $(this.allAnnotations).each(function () {
                $(document.createElement('option'))
                    .attr({ value: this.Name })
                    .text(this.Title)
                    .appendTo(oThis.allAnnotationsSelect)
            });

            return this.allAnnotationsSelect;
        },

        _allFieldsSelect: function () {
            var oThis = this;

            this.allFieldsSelect = $(document.createElement('select')).addClass('form-control');

            $(this.allFields).each(function () {
                $(document.createElement('option'))
                    .attr({ value: this.Name })
                    .text(this.Name)
                    .appendTo(oThis.allFieldsSelect)
            });

            return this.allFieldsSelect;
        },

        _addAnnotation: function () {
            var oThis = this;

            var annotation = this.allAnnotationsSelect.val();
            var field = this.allFieldsSelect.val();

            $.Annotations.store.annotateDeposition(this.options.guid, field, annotation, function () {
                oThis._showAnnotations();
            });
        },

        _deleteAnnotation: function (annotation) {
            var oThis = this;

            $.Annotations.store.deleteDepositionAnnotation(this.options.guid, annotation, function () {
                oThis._showAnnotations();
            });
        },

        _showProgress: function () {
            this.element.loadProgress();
        },

        _hideProgress: function () {
            this.element.hideProgress();
        }
    });

}(jQuery));
