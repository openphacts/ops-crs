(function ($) {
    $.widget("depositions.deposition_progress", {

        options: {
            guid: null,
            delay: 2000, // ms,
            progress: 0,
            onChange: function(event, guid, progress) {
                $(document).trigger('onProgressChange', guid, progress);
            },
            onComplete: function(event, guid) {
                $(document).trigger('onProgressComplete', guid);
            }
        },

        _create: function () {
            this.element.addClass('cvsp-deposition-progress')
                    .addClass('progress')
                    .append(
                        $(document.createElement('div'))
                            .addClass('progress-bar')
                            .attr({
                                role: 'progressbar',
                                'aria-valuenow': 0,
                                'aria-valuemin': 0,
                                'aria-valuemax': 100
                            })
                    );

            this._showProgress(this.options.progress);

            this._refreshProgress();
        },

        _showProgress: function (progress) {
            $('.progress-bar', this.element).attr('aria-valuenow', progress).css({ width: progress + '%' }).text(progress + '% Complete');
        },

        _refreshProgress: function () {
            var oThis = this;

            $.Chunks.store.getDepositionChunks(this.options.guid, 'Original', 0, -1, function (original) {
                $.Chunks.store.getDepositionChunks(oThis.options.guid, 'Processed', 0, -1, function (guids) {
                    $.Chunks.store.getChunksByGUID(guids, function (chunks) {
                        var processed = 0;

                        $(chunks).each(function (index, chunk) {
                            if (chunk.Status.toLowerCase() == 'processed')
                                processed++;
                        });

                        var progress = chunks.length == 0 ? 0 : processed * 100 / original.length;

                        if (oThis.options.progress != progress) {
                            oThis.options.progress = progress;
                            oThis._trigger("onChange", null, oThis.options.guid, progress);
                        }

                        oThis._showProgress(progress);

                        if (progress < 100) {
                            setTimeout(function () {
                                oThis._refreshProgress();
                            }, oThis.options.delay);
                        }
                        else {
                            oThis._trigger("onComplete", null, oThis.options.guid);
                        }
                    });
                })
            });
        }
    });

}(jQuery));
