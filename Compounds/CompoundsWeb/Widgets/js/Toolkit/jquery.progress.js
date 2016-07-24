$.fn.searchProgress = function () {
    return this.each(function () {
        $(this).block({
            message: '<div class="loading-progress"><span class="scroll-progress">Searching...</span></div>'
        });
    });
};

$.fn.loadProgress = function (msg) {
    return this.each(function () {
        $(this).unblock();

        $(this).block({
            message: '<div class="loading-progress"><span class="scroll-progress">' + (msg != null ? msg : 'Loading...') + '</span></div>'
        });
    });
};

$.fn.hideProgress = function () {
    return this.each(function () {
        $(this).unblock();
    });
};
