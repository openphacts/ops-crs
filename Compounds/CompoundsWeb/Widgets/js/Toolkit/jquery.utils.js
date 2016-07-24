jQuery.expr[':'].regex = function (elem, index, match) {
    var matchParams = match[3].split(','),
        validLabels = /^(data|css):/,
        attr = {
            method: matchParams[0].match(validLabels) ?
                        matchParams[0].split(':')[0] : 'attr',
            property: matchParams.shift().replace(validLabels, '')
        },
        regexFlags = 'ig',
        regex = new RegExp(matchParams.join('').replace(/^\s+|\s+$/g, ''), regexFlags);
    return regex.test(jQuery(elem)[attr.method](attr.property));
}

$.extend($.support, {
    canvas: function () {
        var elem = document.createElement('canvas');
        return !!(elem.getContext && elem.getContext('2d'));
    },

    bootstrap: function () {
        return typeof $.fn.popover == 'function';
    }
});

(function ($) {

    $.fn.outerHTML = function () {
        return $(this)[0].outerHTML;
    };

}(jQuery));
