//@ sourceURL=jquery.layout.area.js
/*
 * $Id: jquery.layout.area.js 9013 2015-04-29 19:32:21Z alexp $ ::2B6A7A!
 * Copyright (c) 2005 Dr. Vahan Simonyan and Dr. Raja Mazumder.
 * This software is protected by U.S. Copyright Law and International
 * Treaties. Unauthorized use, duplication, reverse engineering, any
 * form of redistribution, use in part or as a whole, other than by
 * prior, express, written and signed agreement is subject to penalties.
 * If you have received this file in error, please notify copyright
 * holder and destroy this and any other copies. All rights reserved.
 */

$(function () {
    $.widget("layout.area", {

        options: {
        	class: null,
            rect: null,
            resizable: {
                minWidth: 50,
                minHeight: 50,
                containment: 'parent',
                handles: "n, e, s, w",
                grid: [20, 20]
            },
            toggler: null	//	east, west, north, south
        },

        _create: function () {
            var oThis = this;

            this.element.addClass("layout-area").addClass(this.options.class);

            this.margin = (this.element.outerWidth(true) - this.element.outerWidth()) / 2;
            this.border = (this.element.outerWidth() - this.element.innerWidth()) / 2;

            if (this.options.resizable) {
                var options = $.extend(this.options.resizable, {
                    start: function (event, ui) {
                        var border = event.toElement ? event.toElement : event.originalEvent.target;
                        if ($(border).hasClass('ui-resizable-e')) {
                            oThis.borderResizeType = 'east';
                        }
                        else if ($(border).hasClass('ui-resizable-n')) {
                        	oThis.borderResizeType = 'north';
                        }
                        else if ($(border).hasClass('ui-resizable-s')) {
                        	oThis.borderResizeType = 'south';
                        }
                        else if ($(border).hasClass('ui-resizable-w')) {
                        	oThis.borderResizeType = 'west';
                        }

                        oThis.sendEvent('area-resize-start', { area: this, border: oThis.borderResizeType });
                    },
                    stop: function (event, ui) {
                    	if(oThis.borderResizeType == 'north') {
                        	$(oThis.neighbours.north).each(function (index, area) {
                                $(area).area('sendEvent', 'area-resize-stop');
                            });
                    	}
                    	else if(oThis.borderResizeType == 'south') {
                        	$(oThis.neighbours.south).each(function (index, area) {
                                $(area).area('sendEvent', 'area-resize-stop');
                            });
                    	}
                    	else if(oThis.borderResizeType == 'west') {
                        	$(oThis.neighbours.west).each(function (index, area) {
                                $(area).area('sendEvent', 'area-resize-stop');
                            });
                    	}
                    	else if(oThis.borderResizeType == 'east') {
                        	$(oThis.neighbours.east).each(function (index, area) {
                                $(area).area('sendEvent', 'area-resize-stop');
                            });
                    	}

                        oThis.sendEvent('area-resize-stop');
                    },
                    resize: function (event, ui) {
                        oThis.sendEvent('area-resize-move');
                    }
                });

                this.element.resizable(options);
            }

            if (this.options.toggler) {
                this.element.append(
                    $(document.createElement('div'))
                        .attr({
                            title: 'Close'
                        })
                        .addClass('toggler')
                        .addClass(this.options.toggler)
                        .click(function () {
                            var area = $(this).closest('.layout-area');

                            oThis.sendEvent('area-resize-start', { area: area, border: oThis.options.toggler });

                            if (oThis.options.toggler == 'east') {
                                if (area.is('.collapsed')) {
                                    area.width(area.data('prev-width'));
                                }
                                else {
                                    area.data('prev-width', area.width()).width(0);
                                }
                            }
                            else if (oThis.options.toggler == 'west') {
                                if (area.is('.collapsed')) {
                                    area.css({ left: area.position().left - area.data('prev-width') });
                                    area.width(area.data('prev-width'));
                                }
                                else {
                                    area.css({ left: area.position().left + area.width() });
                                    area.data('prev-width', area.width()).width(0);
                                }
                            }
                            else if (oThis.options.toggler == 'south') {
                                if (area.is('.collapsed')) {
                                    area.height(area.data('prev-height'));
                                }
                                else {
                                    area.data('prev-height', area.height()).height(0);
                                }
                            }
                            else if (oThis.options.toggler == 'north') {
                                if (area.is('.collapsed')) {
                                    area.css({ top: area.position().top - area.data('prev-height') });
                                    area.height(area.data('prev-height'));
                                }
                                else {
                                    area.css({ top: area.position().top + area.height() });
                                    area.data('prev-height', area.height()).height(0);
                                }
                            }

                            if (area.is('.collapsed')) {
                                area.removeClass('collapsed');
                                $('.layout-infobox', area).show();
                                $(this).attr({ title: 'Close' })
                            }
                            else {
                                area.addClass('collapsed');
                                $('.layout-infobox', area).hide();
                                $(this).attr({ title: 'Open' })
                            }

                            oThis.sendEvent('area-resize-move');

                            oThis.sendEvent('area-resize-stop');
                        })
                );
            }

            if (this.options.rect)
                this.setRect(this.options.rect);

            this.setFocus();

            this.sendEvent('area-create');
        },

        findNeighbours: function() {
            var oThis = this;
            
            var delta = 5;
            
            this.neighbours = {
                north: [],
                east: [],
                south: [],
                west: []
            };

            var rect = this.getRect();

            this.element.siblings('.layout-area').each(function(index, area) {
                var rect2 = $(area).area('getRect');

                if(rect.top > 0) {
                    //    find NORTH neighbours...
                    if (Math.abs(rect.top - rect2.bottom) < delta && rect.left < rect2.right && rect2.left < rect.right) {
                        oThis.neighbours.north.push(area);
                    }
                }

                if(rect.left > 0) {
                    //    find WEST neighbours...
                    if(Math.abs(rect.left - rect2.right) < delta && rect.top < rect2.bottom && rect2.top < rect.bottom) {
                        oThis.neighbours.west.push(area);
                    }
                }

                if(rect.right < oThis.element.parent().width()) {
                    //    find EAST neighbours...
                    if(Math.abs(rect.right - rect2.left) < delta && rect.top < rect2.bottom && rect2.top < rect.bottom) {
                        oThis.neighbours.east.push(area);
                    }
                }

                if(rect.bottom < oThis.element.parent().height()) {
                    //    find SOUTH neighbours...
                    if(Math.abs(rect.bottom - rect2.top) < delta && rect.left < rect2.right && rect2.left < rect.right) {
                        oThis.neighbours.south.push(area);
                    }
                }
            });
        },

        _refresh: function () {
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
        },

        height: function () {
            return this.element.height();
        },

        width: function () {
            return this.element.width();
        },

        sendEvent: function (name, params) {
            if (params == null)
                params = {};

            $.extend(params, {
                oThis: this,
                infobox: this.element,
                rect: this.getRect()
            });

            if (name == 'area-resize-start') {
                $(this.element).trigger('area-resize-start', params);
            }
            else if (name == 'area-resize-stop') {
                $(this.element).trigger('area-resize-stop', params);
            }
            else if (name == 'area-resize-move') {
                $(this.element).trigger('area-resize-move', params);
            }
            else if (name == 'area-resize') {
                $(this.element).trigger({
                    type: 'area-resize',
                    target: this.element
                }, params);
            }
            else if (name == 'area-create') {
                $(this.element.parent()).trigger('area-create', params);
            }
        },

        setRect: function (rect) {
            this.element.css({
                top: (rect.top + this.margin) + 'px',
                left: (rect.left + this.margin) + 'px',
                width: (rect.right - rect.left - 2 * this.margin) + 'px',
                height: (rect.bottom - rect.top - 2 * this.margin) + 'px'
            });
        },

        getRect: function () {
            //var position = this.element.position();
        	var position = {
	            top: parseInt(this.element.css('top')),
	            left: parseInt(this.element.css('left')),
        	};


            return {
                top: position.top,
                left: position.left,
                right: position.left + this.element.outerWidth(true),
                bottom: position.top + this.element.outerHeight(true)
            }
        },

        setFocus: function () {
            $('div.layout-area.focus').removeClass('focus');
            $(this.element).addClass('focus');
        },

        save: function () {
            var state = {
            };

            if (this.element.is('[id]'))
                state.id = this.element.attr('id');
            if (this.element.is('[data-top]'))
                state.top = this.element.data('top');
            if (this.element.is('[data-left]'))
                state.left = this.element.data('left');
            if (this.element.is('[data-right]'))
                state.right = this.element.data('right');
            if (this.element.is('[data-bottom]'))
                state.bottom = this.element.data('bottom');
            if (this.element.is('[data-size]'))
                state.size = this.element.data('size');

            return state;
        },

        getOptions: function () {
            return {
                rect: this.getRect()
            }
        }
    });

}(jQuery));

function parseAreaOptions(element) {
    var options = {
        resizable: {}
    };

    if ($(element).is('[data-roundup]')) {
        options.roundup = parseInt($(element).data('roundup'));
        options.resizable.grid = [options.roundup, options.roundup];
        options.draggable.grid = [options.roundup, options.roundup];
    }

    if ($(element).is('[data-class]'))
        options.class = $(element).data('class');
    
    if ($(element).is('[data-resizable]'))
        options.resizable = $(element).data('resizable');

    if ($(element).is('[data-resizable-min-width]'))
        options.resizable.minWidth = $(element).data('min-width');

    if ($(element).is('[data-resizable-max-width]'))
        options.resizable.maxWidth = $(element).data('max-width');

    if ($(element).is('[data-resizable-min-height]'))
        options.resizable.minHeight = $(element).data('min-height');

    if ($(element).is('[data-resizable-max-height]'))
        options.resizable.maxHeight = $(element).data('max-height');

    if ($(element).is('[data-resizable-handles]'))
        options.resizable.handles = $(element).data('resizable-handles');

    if ($(element).is('[data-toggler]'))
        options.toggler = $(element).data('toggler');
    
    return options;
}
