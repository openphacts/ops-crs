(function ($) {
    $.widget("record.record_info", {

        options: {
            id: null,
            imageSize: 300,
            zoomImageSize: 500
        },

        _create: function () {
            var oThis = this;

            this.element.addClass('cvsp-records-info row');

            if (this.options.id != null) {
                this._loadRecord(this.options.id, function (record) {
                    oThis._showRecord(record);
                });
            }
        },

        _loadRecord: function (guid, callback) {
            var oThis = this;

            this.element.loadProgress();

            $.Records.store.getRecord(guid, function (record) {
                oThis.element.hideProgress();

                if (callback != null)
                    callback(record);
            });
        },

        _showRecord: function (record) {
            this.element.empty();

            this.originalMolDiv = $(document.createElement('div'));

            $(document.createElement('div'))
                .addClass('col-md-6')
                .append(
                    $(document.createElement('div'))
                        .addClass('form-group')
                        .append(
                            $(document.createElement('label'))
                                .text('Original')
                        )
                        .append(
                            this.originalMolDiv
                                .addClass('form-control')
                                .molecule2d({
                                    width: this.options.imageSize,
                                    height: this.options.imageSize,
                                    zoomWidth: this.options.zoomImageSize,
                                    zoomHeight: this.options.zoomImageSize,
                                    mol: record.Original,
                                    allowSave: true
                                })
                        )
                )
                .appendTo(this.element);

            if (record.Standardized != null) {
                this.standardizedMolDiv = $(document.createElement('div'));

                $(document.createElement('div'))
                    .addClass('col-md-6')
                    .append(
                        $(document.createElement('div'))
                            .addClass('form-group')
                            .append(
                                $(document.createElement('label'))
                                    .text('Standardized')
                            )
                            .append(
                                this.standardizedMolDiv
                                    .addClass('form-control')
                                .addClass('form-control')
                                .molecule2d({
                                    width: this.options.imageSize,
                                    height: this.options.imageSize,
                                    zoomWidth: this.options.zoomImageSize,
                                    zoomHeight: this.options.zoomImageSize,
                                    mol: record.Standardized,
                                    allowSave: true
                                })
                            )
                    )
                    .appendTo(this.element);
            }
        }
    });

}(jQuery));
