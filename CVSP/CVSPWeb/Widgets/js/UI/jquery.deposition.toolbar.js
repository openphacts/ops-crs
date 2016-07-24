(function ($) {
    $.widget("depositions.deposition_toolbar", {

        options: {
            id: null,   //  deposition guid
        },

        _create: function () {
            var oThis = this;

            this.element.addClass('cvsp-deposition.toolbar');

            this._generateToolbar();
        },

        refresh: function () {
            this._generateToolbar();
        },

        _generateToolbar: function () {
            var oThis = this;

            this.element.empty();

            $.Profiles.store.isAuthenticated(function (authenticated) {
                if (authenticated) {
                    oThis._createAnnotationsButton();
                    oThis._createJobsButton();
                    oThis._createChunksButton();

                    oThis._createDeleteButton();

                    $.Depositions.store.getDeposition(oThis.options.id, function (deposition) {
                        if (deposition.Status.toLowerCase() == 'processed') {
                            oThis._createUpload2GCNButton();
                        }

                        if (deposition.Status.toLowerCase() == 'deposited2gcn') {
                            oThis._createDeleteFromGCNButton();
                        }
                    });
                }
            });
        },

        _createAnnotationsButton: function () {
            var oThis = this;

            $(document.createElement('a'))
                .addClass('btn btn-default btn-xs')
                .attr({ href: '#', role: 'button' })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-edit')
                        .attr({ 'aria-hidden': true })
                )
                .append($(document.createTextNode(' Annotations')))
                .click(function () {
                    window.location = $.Depositions.store.getUrl('/depositions/' + oThis.options.id + '/annotations');
                    return false;
                })
                .appendTo(this.element);
        },

        _createJobsButton: function () {
            var oThis = this;

            $(document.createElement('a'))
                .addClass('btn btn-default btn-xs')
                .attr({ href: '#', role: 'button' })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-tasks')
                        .attr({ 'aria-hidden': true })
                )
                .append($(document.createTextNode(' Jobs')))
                .click(function () {
                    window.location = $.Depositions.store.getUrl('/depositions/' + oThis.options.id + '/jobs');
                    return false;
                })
                .appendTo(this.element);
        },

        _createChunksButton: function () {
            var oThis = this;

            $(document.createElement('a'))
                .addClass('btn btn-default btn-xs')
                .attr({ href: '#', role: 'button' })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-folder-open')
                        .attr({ 'aria-hidden': true })
                )
                .append($(document.createTextNode(' Chunks')))
                .click(function () {
                    window.location = $.Depositions.store.getUrl('/depositions/' + oThis.options.id + '/chunks');
                    return false;
                })
                .appendTo(this.element);
        },

        _createDeleteButton: function () {
            var oThis = this;

            $(document.createElement('a'))
                .addClass('btn btn-default btn-xs')
                .attr({ href: '#', role: 'button' })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-trash')
                        .attr({ 'aria-hidden': true })
                )
                .append( $(document.createTextNode(' Delete')) )
                .click(function () {
                    $.Depositions.store.deleteDeposition(oThis.options.id, function (result) {
                        window.location = $.Depositions.store.getUrl('/depositions/');
                    });
                    return false;
                })
                .appendTo(this.element);
        },

        _createDeleteFromGCNButton: function () {
            var oThis = this;

            $(document.createElement('a'))
                .addClass('btn btn-default btn-xs')
                .attr({ href: '#', role: 'button' })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-trash')
                        .attr({ 'aria-hidden': true })
                )
                .append($(document.createTextNode(' Delete From GCN')))
                .click(function () {
                    $.Depositions.store.deleteDepositionFromGCN(oThis.options.id, function (result) {
                        $(document).trigger('onDepositionStatusChange', { guid: oThis.options.id, status: 'deletingfromgcn' });
                    });
                    return false;
                })
                .appendTo(this.element);
        },

        _createUpload2GCNButton: function () {
            var oThis = this;

            $(document.createElement('a'))
                .addClass('btn btn-default btn-xs')
                .attr({ href: '#', role: 'button' })
                .append(
                    $(document.createElement('span'))
                        .addClass('glyphicon glyphicon-upload')
                        .attr({ 'aria-hidden': true })
                )
                .append($(document.createTextNode(' Upload to GCN')))
                .click(function () {
                    $.Depositions.store.depositToGCN(oThis.options.id, function () {
                        $(document).trigger('onDepositionStatusChange', { guid: oThis.options.id, status: 'depositing2gcn' });
                    }, function (procedure, xhr, error, thrownError) {
                        var json = JSON.parse(xhr.responseText);

                        $('body').showMessage('Error', json.ExceptionMessage);
                    });
                    return false;
                })
                .appendTo(this.element);
        }
    });

}(jQuery));
