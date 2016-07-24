(function ($) {
    $.widget("depositions.deposition_status", {

        options: {
            guid: null,
            status: null,
            delay: 3000, // ms,
            onStatusChange: function(event, guid, status) {
                $(document).trigger('onDepositionStatusChange', { guid: guid, status: status });
            },
            onProgressChange: function (event, guid, status, progress) {
                $(document).trigger('onDepositionProgressChange', { guid: guid, status: status, progress: progress });
            }
        },

        _create: function () {
            this.element.addClass('cvsp-deposition-status');

            this._showStatus(this.options.status);
        },

        _showStatus: function (status) {
            status = status.toLowerCase();

            var oThis = this;

            if (status == 'submitted') {
                this.element.text(this._statusTitle(status));

                setTimeout(function () {
                    oThis._refreshStatus(oThis.options.guid);
                }, this.options.delay);
            }
            else if (status == 'processing') {
                if (this.spinner == null) {
                    this.spinner = $(document.createElement('span'))
                                        .addClass('loading-spinner');
                }

                if (this.processedProgress == null) {
                    this.processedProgress = $(document.createElement('span')).addClass('deposition-progress')
                }
                if (this.uploadedProgress == null) {
                    this.uploadedProgress = $(document.createElement('span')).addClass('deposition-progress')
                }

                this.element
                    .empty()
                    .append(this.spinner)
                    .append(document.createTextNode(this._statusTitle(status) + ' : Processed - '))
                    .append(this.processedProgress)
                    .append(document.createTextNode(' Uploaded - '))
                    .append(this.uploadedProgress);

                oThis._calculateProgress(status);

                setTimeout(function () {
                    oThis._refreshStatus(oThis.options.guid);
                }, this.options.delay);
            }
            else if (status == 'depositing2gcn') {
                if (this.spinner == null) {
                    this.spinner = $(document.createElement('span'))
                                        .addClass('loading-spinner');
                }

                this.element
                    .empty()
                    .append(this.spinner)
                    .append(document.createTextNode(this._statusTitle(status)))

                setTimeout(function () {
                    oThis._refreshStatus(oThis.options.guid);
                }, this.options.delay);
            }
            else if (status == 'deletingfromgcn') {
                if (this.spinner == null) {
                    this.spinner = $(document.createElement('span'))
                                        .addClass('loading-spinner');
                }

                this.element
                    .empty()
                    .append(this.spinner)
                    .append(document.createTextNode(this._statusTitle(status)))

                setTimeout(function () {
                    oThis._refreshStatus(oThis.options.guid);
                }, this.options.delay);
            }
            else {
                this.element.text(this._statusTitle(status));
            }
        },

        _refreshStatus: function (guid) {
            var oThis = this;

            $.Depositions.store.getDepositionStatus(guid, function (status) {
                oThis._showStatus(status);

                if (oThis.options.status != status) {
                    oThis.options.status = status;
                    oThis._trigger("onStatusChange", null, oThis.options.guid, status);
                }
            });
        },

        _calculateProgress: function (status) {
            var oThis = this;

            var preparedChunkType = '';
            var processedChunkType = '';

            if (status == 'processing') {
                preparedChunkType = 'Original';
                processedChunkType = 'Processed';
            }
            else if (status == 'depositing2gcn') {
                preparedChunkType = 'Prepared4GCN';
                processedChunkType = 'Processed4GCN';
            }

            $.Depositions.store.getDepositionChunksStatistic(this.options.guid, function (stats) {
                var processedProgress = stats.Original.Total == 0 ? 0 : stats.Original.Processed * 100 / stats.Original.Total;

                oThis.processedProgress.text(processedProgress.toFixed(0) + '%');

                var uploadedProgress = stats.Processed.Total == 0 ? 0 : stats.Processed.Processed * 100 / stats.Processed.Total;

                oThis.uploadedProgress.text(uploadedProgress.toFixed(0) + '%');

                if (oThis.progressValue == null) {
                    oThis.progressValue = { processed: processedProgress, uploaded: uploadedProgress };
                }
                else if (oThis.progressValue.processed != processedProgress || oThis.progressValue.uploaded != uploadedProgress) {
                    oThis.progressValue = { processed: processedProgress, uploaded: uploadedProgress };
                    oThis._trigger("onProgressChange", null, oThis.options.guid, status, oThis.progressValue);
                }
            });
        },

        _statusTitle: function (status) {
            if (status == 'depositing2gcn') return 'Depositing to GCN';
            else if (status == 'deposited2gcn') return 'Deposited to GCN';
            else if (status == 'deletingfromgcn') return 'Deleting from GCN';
            else return status.capitalize();
        }
    });

}(jQuery));
