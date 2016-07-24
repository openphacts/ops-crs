/*** Handle jQuery plugin naming conflict between jQuery UI and Bootstrap ***/
$.fn.bootstrapButton = $.fn.button.noConflict();
$.fn.bootstrapTooltip = $.fn.tooltip.noConflict();
