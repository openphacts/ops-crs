//@ sourceURL=jquery.layout.manager.js
/*
 * $Id: jquery.layout.manager.js 9013 2015-04-29 19:32:21Z alexp $ ::2B6A7A!
 * Copyright (c) 2005 Dr. Vahan Simonyan and Dr. Raja Mazumder.
 * This software is protected by U.S. Copyright Law and International
 * Treaties. Unauthorized use, duplication, reverse engineering, any
 * form of redistribution, use in part or as a whole, other than by
 * prior, express, written and signed agreement is subject to penalties.
 * If you have received this file in error, please notify copyright
 * holder and destroy this and any other copies. All rights reserved.
 */
(function ($) {
    $.widget("layout.layoutmanager", {

        options: {
            type: 'flex',          //  vertical, horizontal, static, flex, border, grid, flow
            saveState: false,
            resizable: false,
            config: null,
            bind: null,
            border: 'all',        //    all, none, between
            overflow: 'hidden',
            allowResize: true,    //    is it allow to resize frames inside layout or not
            roundup: 20
        },

        _create: function () {
            var oThis = this;

            this._onBeforeInit();
            
            this._loadOptions();

            this.element.addClass('layout-manager');

            this.element.parent().css({
                overflow: this.options.overflow
            })

            //   we have to check that 'area' widget is applied to the element and if not - apply it
            if (!this.element.is('.layout-area')) {
                this.element.area({
                    rect: {
                        top: 0,
                        left: 0,
                        right: this.element.parent().outerWidth(),
                        bottom: this.element.parent().outerHeight()
                    },
                    resizable: false    //  we cannot resize top level layout manager...
                });
            }

            this.area = this.element.area('instance');

            this.element.on('area-resize', function (event, params) {
                if ($(event.target).is(this)) {
                    $(this).children('div.layout-area').each(function () {
                        var area = $(this).area('instance');
                        area.setRect(oThis.calculateAreaRect(this));
                        area.sendEvent('area-resize');
                    });
                }

                event.stopPropagation();
            });

            this.element.on('area-resize-stop', function (event, params) {
                if ($(event.target).is(this)) {
                    $(this).children('div.layout-area').each(function () {
                        var area = $(this).area('instance');
                        area.sendEvent('area-resize-stop');
                    });
                }

                event.stopPropagation();
            });
            
            if (this.options.type == 'flex') {
                $(this.element).droppable({
                    accept: '.layout-infobox .nav-tabs > li.ui-draggable',
                    drop: function (event, ui) {
                        //  calculate new infobox's position...
                        var parentPos = $(ui.draggable).closest('div.layout-area').position();
                        var position = $(ui.draggable).position();
                        position.left += parentPos.left;
                        position.top += parentPos.top;

                        var sourceTabs = $(ui.draggable).closest('.layout-area').infoboxtabs('instance');
                        var options = sourceTabs.getOptions();

                        var tabs = oThis.createTabs({
                            left: (position.left * 100.0 / oThis.element.width()).toFixed(2) + '%',
                            top: (position.top * 100.0 / oThis.element.height()).toFixed(2) + '%',
                            right: ((position.left + 300) * 100.0 / oThis.element.width()).toFixed(2) + '%',
                            bottom: ((position.top + 300) * 100.0 / oThis.element.height()).toFixed(2) + '%',
                            allowMaximize: options.allowMaximize,
                            allowClose: options.allowClose,
                            allowSplit: options.allowSplit
                        });

                        oThis.element.append(tabs);

                        $(tabs).on('tabs-create', function (event, params) {
                            $(params.tabs).infoboxtabs('moveTab', ui.draggable);
                        });

                        oThis._initChild(tabs);

                        oThis.findNeighbours();
                    }
                });

                $(this.element).on('split-area', function (event, params) {
                    if (params.direction == 'center') {
                        //  if we were dragging tab...
                        if ($(params.ui.helper).is('.layout-infobox-tabs .nav-tabs > li')) {
                            $(params.area).infoboxtabs('moveTab', params.ui.draggable);
                        }
                    }
                    else {
                        //  calculate new infobox's position...
                        var area = params.area.area('instance');
                        var rect = area.getRect();

                        var newRect = {};
                        var newTabsRect = {};

                        if (params.direction == 'south') {
                            newRect = {
                                top: rect.top,
                                left: rect.left,
                                right: rect.right,
                                bottom: oThis._round(rect.top + (rect.bottom - rect.top) / 2, oThis.element.outerHeight())
                            };

                            newTabsRect = {
                                left: (rect.left * 100.0 / oThis.element.width()).toFixed(2) + '%',
                                top: (newRect.bottom * 100.0 / oThis.element.height()).toFixed(2) + '%',
                                right: (rect.right * 100.0 / oThis.element.width()).toFixed(2) + '%',
                                bottom: (rect.bottom * 100.0 / oThis.element.height()).toFixed(2) + '%'
                            };
                        }
                        else if (params.direction == 'north') {
                            newRect = {
                                top: oThis._round(rect.top + (rect.bottom - rect.top) / 2, oThis.element.outerHeight()),
                                left: rect.left,
                                right: rect.right,
                                bottom: rect.bottom
                            };

                            newTabsRect = {
                                left: (rect.left * 100.0 / oThis.element.width()).toFixed(2) + '%',
                                top: (rect.top * 100.0 / oThis.element.height()).toFixed(2) + '%',
                                right: (rect.right * 100.0 / oThis.element.width()).toFixed(2) + '%',
                                bottom: (newRect.top * 100.0 / oThis.element.height()).toFixed(2) + '%'
                            };
                        }
                        else if (params.direction == 'west') {
                            newRect = {
                                top: rect.top,
                                left: oThis._round(rect.left + (rect.right - rect.left) / 2, oThis.element.outerWidth()),
                                right: rect.right,
                                bottom: rect.bottom
                            };

                            newTabsRect = {
                                left: (rect.left * 100.0 / oThis.element.width()).toFixed(2) + '%',
                                top: (rect.top * 100.0 / oThis.element.height()).toFixed(2) + '%',
                                right: (newRect.left * 100.0 / oThis.element.width()).toFixed(2) + '%',
                                bottom: (rect.bottom * 100.0 / oThis.element.height()).toFixed(2) + '%'
                            };
                        }
                        else if (params.direction == 'east') {
                            newRect = {
                                top: rect.top,
                                left: rect.left,
                                right: oThis._round(rect.left + (rect.right - rect.left) / 2, oThis.element.outerWidth()),
                                bottom: rect.bottom
                            };

                            newTabsRect = {
                                left: (newRect.right * 100.0 / oThis.element.width()).toFixed(2) + '%',
                                top: (rect.top * 100.0 / oThis.element.height()).toFixed(2) + '%',
                                right: (rect.right * 100.0 / oThis.element.width()).toFixed(2) + '%',
                                bottom: (rect.bottom * 100.0 / oThis.element.height()).toFixed(2) + '%'
                            };
                        }

                        area.setRect(newRect);

                        oThis.updateAreaState(params.area);

                        var tabs = oThis.createTabs(newTabsRect);

                        oThis.element.append(tabs);

                        $(tabs).on('tabs-create', function (event, prms) {
                            $(prms.tabs).infoboxtabs('moveTab', params.ui.draggable);
                        });

                        oThis._initChild(tabs);

                        area.sendEvent('area-resize');
                    }

                    oThis.findNeighbours();
                });
            }

            if (this.options.saveState) {
                var id = this.element.attr('id');

                if (!id)
                    console.log('ERROR: layout Id must be specified in order to use save state mode! Please assign unique Id to the layout manager!');

                this.element.on('state-changed', function () {
                    //  save layout config to storage...
                    var config = oThis.save();
                    $.localStorage.set(id + '-layout', config);

                    //console.log(config);
                });
            }

            if (this.options.config) {
                if (this.options.saveState) {
                    //  try to load layout config from storage...
                    var id = this.element.attr('id');
                    var config = $.localStorage.get(id + '-layout');
                    if (config)
                        this.options.config = config;
                }

                if (this.options.bind) {
                    $(this.options.bind).each(function () {
                        JSON.bind(oThis.options.config, this.attrName, this.attrValue, this.value);
                    });
                }

                this.load(this.options.config);
            }
            else {
                this._initChildren();
            }
            
            this._onAfterInit();
        },
        
        _onBeforeInit: function() {
        	/* override this function if you want to do anything 
        	 * before to init layaout manager, generate config for example */
        },
        
        _onAfterInit: function() {
        	/* override this function if you want to do anything 
        	 * after to init layaout manager*/
        },

        _loadOptions: function () {
            //  load options from HTML data attributes...
            if ($(this.element).is('[data-layout-manager]'))
                this.options.type = $(this.element).data('layout-manager');

            if ($(this.element).is('[data-layout-border]'))
                this.options.border = $(this.element).data('layout-border');

            if ($(this.element).is('[data-layout-overflow]'))
                this.options.overflow = $(this.element).data('layout-overflow');

            if ($(this.element).is('[data-layout-allow-resize]'))
                this.options.allowResize = $(this.element).data('layout-allow-resize');

            if ($(this.element).is('[data-layout-save-state]'))
                this.options.saveState = $(this.element).data('layout-save-state');
        },

        _initChildren: function () {
            var oThis = this;

            //    general initialization is finished and we are ready to start init internal infoboxes...
    /*    this.element.children('div[data-layout~="infobox"][data-init!=1], div[data-layout~="manager"]').each(function () {
            oThis._initChild(this);
        });*/
            
            this.element.children('div[data-layout~="infobox"], div[data-layout~="manager"]').each(function () {
                oThis._initChild(this);
            });

            //	send event that we are ready to initiate widgets...
            $.unique($('div[data-need-init-widget=1]', this.element).closest('div.layout-infobox')).each(function() {
            	$(this).trigger('init-widgets');
            });

            this.findNeighbours();
        },

        _initChild: function (area) {
            var oThis = this;

            this.prepareAreaOptions(area);

            var options = parseInfoboxOptions(area);

            options.rect = oThis.calculateAreaRect(area);

            $(area).area(options);

            if (options.resizable) {
                $(area).on('area-resize-start', function (event, params) {
                    oThis.currentAreaResizeAction = params;
                });

                $(area).on('area-resize-stop', function (event, params) {
                    oThis.findNeighbours();

                    oThis.sendEvent('state-changed');

                    event.stopPropagation();
                });

                $(area).on('area-resize-move', function () {
                    oThis._resizeArea(area, oThis.currentAreaResizeAction.border);
                });
            }

            $(area).on('infobox-drag-stop', function (event, params) {
                oThis.findNeighbours();
                oThis.updateAreaState(this);

                oThis.sendEvent('state-changed');
            });

            if ($(area).is('div[data-layout~="panel"]')) {
                $(area).infoboxpanel(options);
            }
            else if ($(area).is('div[data-layout~="tabs"]')) {
                $(area).infoboxtabs(options);
            }
            else if ($(area).is('div[data-layout~="manager"]')) {
                options.type = $(area).data('layout-manager');
                $(area).layoutmanager(options);
            }
            else {
                $(area).infobox(options);
            }

            $(area).attr({'data-init': 1});
        },

        _splitArea: function() {
        },

        _resizeArea: function (area, border, except) {
            var oThis = this;

            var a = $(area).area('instance');

            var newRect = {
                top: oThis._round($(area).position().top, this.element.outerHeight()),
                left: oThis._round($(area).position().left, this.element.outerWidth()),
                right: oThis._round($(area).position().left + $(area).outerWidth(), this.element.outerWidth()),
                bottom: oThis._round($(area).position().top + $(area).outerHeight(), this.element.outerHeight())
            };

            a.setRect(newRect);

            if (border == 'west') {
                $(a.neighbours.west).each(function () {
                    if (this == except) return;

                    var prevRect = $(this).area('getRect');
                    $(this).area('setRect', {
                        top: prevRect.top,
                        left: prevRect.left,
                        bottom: prevRect.bottom,
                        right: newRect.left
                    });

                    oThis._resizeArea(this, 'east', area);

                    oThis.updateAreaState(this);

                    $(this).area('sendEvent', 'area-resize');
                });
            }
            else if (border == 'east') {
                $(a.neighbours.east).each(function () {
                    if (this == except) return;

                    var prevRect = $(this).area('getRect');
                    $(this).area('setRect', {
                        top: prevRect.top,
                        left: newRect.right,
                        bottom: prevRect.bottom,
                        right: prevRect.right
                    });

                    oThis._resizeArea(this, 'west', area);

                    oThis.updateAreaState(this);

                    $(this).area('sendEvent', 'area-resize');
                });
            }
            else if (border == 'north') {
                $(a.neighbours.north).each(function () {
                    if (this == except) return;

                    var prevRect = $(this).area('getRect');
                    $(this).area('setRect', {
                        top: prevRect.top,
                        left: prevRect.left,
                        bottom: newRect.top,
                        right: prevRect.right
                    });

                    oThis._resizeArea(this, 'south', area);

                    oThis.updateAreaState(this);

                    $(this).area('sendEvent', 'area-resize');
                });
            }
            else if (border == 'south') {
                $(a.neighbours.south).each(function () {
                    if (this == except) return;

                    var prevRect = $(this).area('getRect');
                    $(this).area('setRect', {
                        top: newRect.bottom,
                        left: prevRect.left,
                        bottom: prevRect.bottom,
                        right: prevRect.right
                    });

                    oThis._resizeArea(this, 'north', area);

                    oThis.updateAreaState(this);

                    $(this).area('sendEvent', 'area-resize');
                });
            }

            this.updateAreaState(area);

            a.sendEvent('area-resize');
        },

        findNeighbours: function() {
            this.element.children('.layout-area').each(function(index, area) {
                $(area).area('findNeighbours');
            });
        },
        
        _getAbsValue: function (val, range) {
            if (typeof val == 'string' && val.endsWith('%')) {
                var percent = parseFloat(val);
                if (!isNaN(percent))
                    return Math.round(range * percent / 100);
            }
            else
                return val;
        },

        _round: function (val, max) {
            var round = Math.round(val / this.options.roundup) * this.options.roundup;

            //  if rounded value greater than possible MAX value we have to return MAX...
            if (round > max) return max;

            //  ... or if rounded value very close to the end of the interval we have to return MAX again...
            if (max - round < this.options.roundup) return max;

            return round;
        },

        calculateAreaRect: function (area) {
            var oThis = this;

            if (this.options.type == 'horizontal') {
                var size = $(area).data('size');

                //var prevLeft = $(area).prev().length == 1 ? $(area).prev().position().left + $(area).prev().outerWidth() : 0;
                var prevLeft = $(area).prev().length == 1 ? parseInt($(area).prev().css('left')) + $(area).prev().outerWidth() : 0;

                if (size == '*') {
                    var otherAreasSize = 0;

                    //  calculate total size for all other areas...
                    $(area).siblings().each(function () {
                        otherAreasSize += oThis._getAbsValue($(this).data('size'), oThis.element.outerHeight());
                    });

                    return {
                        top: 0,
                        left: prevLeft,
                        right: prevLeft + this.element.outerWidth() - otherAreasSize,
                        bottom: this.element.outerHeight()
                    };
                }
                else {
                    return {
                        top: 0,
                        left: prevLeft,
                        right: this.isAreaFixed(area) ? prevLeft + this._getAbsValue(size, this.element.outerWidth()) : this._round(prevLeft + this._getAbsValue(size, this.element.outerWidth()), this.element.outerWidth()),
                        bottom: this.element.outerHeight()
                    };
                }
            }
            else if (this.options.type == 'vertical') {
                var size = $(area).data('size');

                //var prevBottom = $(area).prev().length == 1 ? $(area).prev().position().top + $(area).prev().outerHeight() : 0;
                var prevBottom = $(area).prev().length == 1 ? parseInt($(area).prev().css('top')) + $(area).prev().outerHeight() : 0;

                if (size == '*') {
                    var otherAreasSize = 0;

                    //  calculate total size for all other areas...
                    $(area).siblings().each(function () {
                        otherAreasSize += oThis._getAbsValue($(this).data('size'), oThis.element.outerHeight());
                    });

                    return {
                        top: prevBottom,
                        left: 0,
                        right: this.element.outerWidth(),
                        bottom: prevBottom + this.element.outerHeight() - otherAreasSize
                    };
                }
                else {
                    return {
                        top: prevBottom,
                        left: 0,
                        right: this.element.outerWidth(),
                        bottom: this.isAreaFixed(area) ? prevBottom + this._getAbsValue(size, this.element.outerHeight()) : this._round(prevBottom + this._getAbsValue(size, this.element.outerHeight()), this.element.outerHeight())
                    };
                }
            }
            else if (this.options.type == 'flex') {
                return {
                    top: this._round(this._getAbsValue($(area).data('top'), this.element.outerHeight()), this.element.outerHeight()),
                    left: this._round(this._getAbsValue($(area).data('left'), this.element.outerWidth()), this.element.outerWidth()),
                    right: this._round(this._getAbsValue($(area).data('right'), this.element.outerWidth()), this.element.outerWidth()),
                    bottom: this._round(this._getAbsValue($(area).data('bottom'), this.element.outerHeight()), this.element.outerHeight()),
                };
            }
        },

        //  returns true if size of area is fixed and canot be changed. E.g. size of area in pixels.
        isAreaFixed: function (area) {
            var size = $(area).data('size');
            if (size)
                return !size.toString().endsWith('%') && size != '*';

            return false;
        },

        prepareAreaOptions: function (area) {
            if ($(area).is('div[data-layout~="manager"]') || this.isAreaFixed(area) || !this.options.allowResize)
                $(area).attr('data-resizable', false);

            if (this.options.border == 'none' || this.options.border == 'between') {
                $('.layout-infobox', area).css({ 'border-color': 'transparent' });
            }

            if (this.options.type == 'horizontal') {
                //  in case of horizontal layout we cannot have draggable areas...
                $(area).attr('data-draggable', false);


                if ($(area).prev().length == 0) {
                    //    first...
                    if ($(area).next().length > 0 && !this.isAreaFixed($(area).next()))
                        $(area).attr('data-resizable-handles', 'e');

                    if (this.options.border == 'between')
                        $('.layout-infobox', area).addClass('east-border');
                }
                else if ($(area).next().length == 0) {
                    //    last...
                    if ($(area).prev().length > 0 && !this.isAreaFixed($(area).prev()))
                        $(area).attr('data-resizable-handles', 'w');

                    if (this.options.border == 'between')
                        $('.layout-infobox', area).addClass('west-border');
                }
                else {
                    //    all others...
                    var handles = '';
                    if (!this.isAreaFixed($(area).prev()))
                        handles = 'w';

                    if (!this.isAreaFixed($(area).next()))
                        handles += handles == '' ? 'e' : ',e';

                    if(handles != '')
                        $(area).attr('data-resizable-handles', handles);
                    else
                        $(area).attr('data-resizable', false);

                    if (this.options.border == 'between')
                        $('.layout-infobox', area).addClass('east-border west-border');
                }
            }
            else if (this.options.type == 'vertical') {
                //  in case of vertical layout we cannot have draggable areas...
                $(area).attr('data-draggable', false);

                if ($(area).prev().length == 0) {
                    //    first...
                    if ($(area).next().length > 0 && !this.isAreaFixed($(area).next()))
                        $(area).attr('data-resizable-handles', 's');

                    if (this.options.border == 'between')
                        $('.layout-infobox', area).addClass('south-border');
                }
                else if ($(area).next().length == 0) {
                    //    last...
                    if ($(area).prev().length > 0 && !this.isAreaFixed($(area).prev()))
                        $(area).attr('data-resizable-handles', 'n');

                    if (this.options.border == 'between')
                        $('.layout-infobox', area).addClass('north-border');
                }
                else {
                    //    all others...
                    var handles = '';
                    if (!this.isAreaFixed($(area).prev()))
                        handles = 'n';

                    if (!this.isAreaFixed($(area).next()))
                        handles += handles == '' ? 's' : ',s';

                    if(handles != '')
                        $(area).attr('data-resizable-handles', handles);
                    else
                        $(area).attr('data-resizable', false);

                    if (this.options.border == 'between')
                        $('.layout-infobox', area).addClass('north-border south-border');
                }
            }
            else if (this.options.type == 'flex') {
            }
        },

        updateAreaState: function(area) {
            if (this.options.type == 'horizontal') {
                $(area).data({
                    size: ($(area).outerWidth() * 100 / this.element.outerWidth()).toFixed(3) + '%'
                });
                $(area).attr({
                    'data-size': $(area).data('size')
                });
            }
            else if (this.options.type == 'vertical') {
                $(area).data({
                    size: ($(area).outerHeight() * 100 / this.element.outerHeight()).toFixed(3) + '%'
                });
                $(area).attr({
                    'data-size': $(area).data('size')
                });
            }
            else if (this.options.type == 'flex') {
                $(area).data({
                    top: ($(area).position().top * 100 / this.element.outerHeight()).toFixed(3) + '%',
                    left: ($(area).position().left * 100 / this.element.outerWidth()).toFixed(3) + '%',
                    right: (($(area).position().left + $(area).width())* 100 / this.element.outerWidth()).toFixed(3) + '%',
                    bottom: (($(area).position().top + $(area).height()) * 100 / this.element.outerHeight()).toFixed(3) + '%'
                });
                $(area).attr({
                    'data-top': $(area).data('top'),
                    'data-left': $(area).data('left'),
                    'data-right': $(area).data('right'),
                    'data-bottom': $(area).data('bottom')
                });
            }
        },

        createArea: function (params) {
            var area = $(document.createElement('div'))
                .attr({
                    'data-id': params.id
                })
                .addClass('layout-area');
            
            if(params.hasOwnProperty('top'))
                area.attr('data-top', params.top);
            if(params.hasOwnProperty('left'))
                area.attr('data-left', params.left);
            if(params.hasOwnProperty('right'))
                area.attr('data-right', params.right);
            if(params.hasOwnProperty('bottom'))
                area.attr('data-bottom', params.bottom);
            if(params.hasOwnProperty('size'))
                area.attr('data-size', params.size);
            if(params.hasOwnProperty('toggler'))
                area.attr('data-toggler', params.toggler);
            if (params.hasOwnProperty('class'))
                area.attr({ 'data-class': params.class })
            
            return area;
        },

        createInfobox: function (params) {
            var oThis = this;

            var area = this.createArea(params);

            area.attr({ 'data-layout': 'infobox' });

            var infobox = $(document.createElement('div'))
                            .addClass("layout-infobox")
                            .appendTo(area);

            if (params.hasOwnProperty('id'))
                infobox.attr({ 'data-id': params.id })

            if (params.hasOwnProperty('allowMaximize'))
                infobox.attr({ 'data-allow-maximize': params.allowMaximize })

            if (params.hasOwnProperty('allowClose'))
                infobox.attr({ 'data-allow-close': params.allowClose })

            if (params.hasOwnProperty('allowSplit'))
                infobox.attr({ 'data-allow-split': params.allowSplit })

            if (params.hasOwnProperty('droppable'))
                infobox.attr({ 'data-droppable': params.droppable })

            return area;
        },

        createLayout: function (params) {
            var oThis = this;

            var area = this.createArea(params);

            area.attr({
                'data-layout': 'manager',
                'data-layout-manager': params.layout.layoutType
            });

            if (params.layout.border !== undefined) {
                area.attr('data-layout-border', params.layout.border);
            }

            if (params.layout.allowResize !== undefined) {
                area.attr('data-layout-allow-resize', params.layout.allowResize);
            }

            if (params.layout.overflow !== undefined) {
                area.attr('data-layout-overflow', params.layout.overflow);
            }

            $(params.layout.items).each(function () {
                if (this.tabs) {
                    var tabs = oThis.createTabs(this);
                    area.append(tabs);
                }
                else if (this.layout) {
                    if (this.title) {
                        var panel = oThis.createPanel(this);
                        area.append(panel);
                    }
                    else {
                        var layout = oThis.createLayout(this);
                        area.append(layout);
                    }
                }
                else {
                    var panel = oThis.createPanel(this);
                    area.append(panel);
                }
            });

            return area;
        },

        createTabs: function (params) {
            var area = this._createTabsArea(params);
            this._addTabs(area, params);
            
            return area;
        },

        //    Just create tabs' area...
        _createTabsArea: function(params) {
            var area = this.createInfobox(params);

            var infobox = $('.layout-infobox', area);

            infobox.addClass("panel panel-default");

            infobox.append(
                    $(document.createElement('div'))
                        .addClass('panel-heading')
                        .css({ position: 'relative' })
                        .append(
                            $(document.createElement('ul'))
                                .addClass('nav nav-tabs')
                                .attr({
                                    role: 'tablist'
                                })
                        )
                )
                .append(
                    $(document.createElement('div'))
                        .addClass('panel-body tab-content')
                );

            $(area).attr('data-layout', $(area).data('layout') + ' tabs');

            if(params.tabs != null && params.tabs.onActivate) {
                $(area).on('tab-shown', function(event, p) {
                	params.tabs.onActivate(p.index, p.tab);
                });
            }

            return area;
        },

        //    add new tabs to the area
        _addTabs: function(area, params) {
            var oThis = this;

            if (params.tabs) {
                $(params.tabs.items).each(function (index, tab) {
                    //    check if tab with this name already exists...
                    if($('a[data-name="' + tab.name + '"]').length > 0) {
                        console.log('ERROR: Tab with name "' + tab.name + '" already exists!');
                        return;
                    }

                    if (tab.active)
                    {
                    	$('ul.nav-tabs', area).children().removeClass("active");
                    	$('.tab-content', area).children().removeClass("active");
                    }

                    var li = $(document.createElement('li'))
	                    .addClass(tab.active ? 'active' : '')
	                    .append(
	                        $(document.createElement('a'))
	                            .addClass(tab.class)
	                            .attr({
	                                href: '#' + tab.name + '-tab',
	                                role: 'tab',
	                                'data-toggle': 'tab',
	                                'data-name': tab.name
	                            })
	                            .text(tab.title)
	                    );

                    if (tab.allowClose) {
                        $('a', li).hoverIntent({
                            sensitivity: 3, // number = sensitivity threshold (must be 1 or higher)
                            interval: 500, // number = milliseconds for onMouseOver polling interval
                            timeout: 500, // number = milliseconds delay before onMouseOut
                            over: function () {
                                $(this).append(
                                    $(document.createElement('span'))
                                        .attr({
                                            title: 'Close'
                                        })
                                        .addClass('close-tab glyphicon glyphicon-remove-circle')
                                        .click(function () {
                                            if ($(this).closest('li').is('.active')) {
                                                if ($(this).closest('li').siblings('li').length > 0)
                                                    $(this).closest('li').siblings('li').first().find('a').click();
                                            }
                                            oThis.remove($(this).closest('a').data('name'));
                                            return false;
                                        })
                                );
                            },
                            out: function () {
                                $('.close-tab', this).remove();
                            }
                        });
                    }

                    $('ul.nav-tabs', area).append( li );

                    var tabPane = $(document.createElement('div'))
                                        .addClass('tab-pane view-area .fade')
                                        .addClass(tab.active ? 'active' : '')
                                        .css({
                                            overflow: tab.overflow == null ? 'hidden' : tab.overflow
                                        })
                                        .attr({
                                            id: tab.name + '-tab'
                                        });

                    $('div.tab-content', area).append(tabPane);

                    if (tab.layout) {
                        var layout = oThis.createLayout(tab);
                        tabPane.append(layout);
                    }

                    if (tab.view) {
                        tabPane.data('jquery-widget', tab.view.name).attr('data-need-init-widget', 1);

                        if (tab.view.options) {
                            tabPane.data('jquery-widget-options', tab.view.options);
                        }
                    }

                    $(area).trigger('new-tab-added', { tab: li, pane: tabPane });
                });
            }
        },
        
        createPanel: function (params) {
            var area = this.createInfobox(params);

            var infobox = $('.layout-infobox', area);

            //    default overflow property set to 'auto'
            if(!params.overflow)
                params.overflow = 'hidden';

            if (params.title) {
                infobox.addClass("panel panel-default")
                    .append(
                        $(document.createElement('div'))
                            .addClass('panel-heading')
                            .addClass(params.class)
                            .css({ position: 'relative' })
                            .append(
                                $(document.createElement('div'))
                                    .addClass('title')
                                    .text(params.title)
                            )
                    )
                    .append(
                        $(document.createElement('div'))
                            .addClass('panel-body view-area')
                            .css({
                                overflow: params.overflow
                            })
                    );
            }
            else {
                infobox.addClass('view-area').css({ overflow: params.overflow });
            }

            $(area).attr('data-layout', $(area).data('layout') + ' panel');

            if (params.name)
                $(infobox).attr({ 'data-name': params.name });
 
            if (params.layout) {
                var layout = this.createLayout({ layout: params.layout });
                $('.view-area', area).append(layout);
            }

            if(params.view) {
            	$('.view-area', area).data('jquery-widget', params.view.name).attr('data-need-init-widget', 1);
                
                if(params.view.options) {
                	$('.view-area', area).data('jquery-widget-options', params.view.options);
                }
            }

            return area;
        },

        //    Save everything. All necessary information in order to reproduse the same layout later
        save: function() {
            var config = {
                layoutType: this.options.type,
                resizable: this.options.resizable,
                border: this.options.border,
                overflow: this.options.overflow,
                items: []
            };

            this.element.children('div[data-layout~="infobox"], div[data-layout~="manager"]').each(function (index, area) {
                if ($(area).is('div.layout-manager')) {
                    config.items.push($(area).layoutmanager('save'));
                }
                else if ($(area).is('div[data-layout~="panel"]')) {
                    config.items.push($(area).infoboxpanel('save'));
                }
                else if ($(area).is('div[data-layout~="tabs"]')) {
                    config.items.push($(area).infoboxtabs('save'));
                }
            });

            var state = this.area.save();
            state.layout = config;

            return state;
        },

        //    Load layout form the config
        load: function (config) {
            this.append(config);
            
            this.sendEvent('manager-loaded')
        },

        //    append new items to the layout
        append: function (config) {
            if (config.layout) {
                var oThis = this;

                $(config.layout.items).each(function () {
                    var area = $();
                    if(this.hasOwnProperty('id')) {
                        //    try to find area if config has identificator
                        area = $('div.layout-area[data-id="' + this.id + '"]');
                    }
                    
                    if (this.tabs) {
                        if(area.length == 1) {
                            oThis._addTabs(area, this);
                        }
                        else if(area.length == 0) {
                            var tabs = oThis.createTabs(this);
                            oThis.element.append(tabs);
                        }
                        else {
                            console.log('ERROR: too many areas found by ID: ' + this.id);
                        }
                    }
                    else if (this.layout) {
                        if (this.title) {
                            var panel = oThis.createPanel(this);
                            oThis.element.append(panel);
                        }
                        else {
                            var layout = oThis.createLayout(this);
                            oThis.element.append(layout);
                        }
                    }
                    else {
                        var panel = oThis.createPanel(this);
                        oThis.element.append(panel);
                    }
                });

                this._initChildren();
            }
        },

        remove: function(id) {
            area = $('div.layout-area[data-id="' + id + '"]');

            if(area.length == 1) {
                //    area was found successfully...
                area.remove();
            }
            else if(area.length == 0) {
                //    nothing was found... try to find among the tabs
                var tab = $('a[data-name="' + id + '"]');
                if(tab.length == 1) {
                    var infobox = tab.closest('div.layout-infobox');
                    
                    var ul = tab.closest('ul');
                    
                    var tabPane = $(tab.attr('href'), infobox);

                    tab.parent().remove();
                    tabPane.remove();

                    if(ul.children('li').length == 0) {
                        ul.closest('div.layout-area').remove();
                    }
                }
            }
            else {
                console.log('ERROR: too many areas found by ID: ' + id);
            }
        },

        clear: function(id) {
            area = $('div.layout-area[data-id="' + id + '"]');

            if(area.length == 1) {
                //    area was found successfully...
                $('.layout-infobox>.panel-heading>ul', area).empty();
                $('.layout-infobox>.panel-body', area).empty();
            }
            else if(area.length == 0) {
                //    nothing was found... try to find among the tabs
                var tab = $('a[data-name="' + id + '"]');
                if(tab.length == 1) {
                    var infobox = tab.closest('div.layout-infobox');
                    
                    var ul = tab.closest('ul');
                    
                    var tabPane = $(tab.attr('href'), infobox);

                    //tab.parent().remove();
                    tabPane.empty();

                    //if(ul.children('li').length == 0) {
                    //    ul.closest('div.layout-area').remove();
                    //}
                }
            }
            else {
                console.log('ERROR: too many areas found by ID: ' + id);
            }
        },
        
        sendEvent: function (name, params) {
            if (params == null)
                params = {};

            if (name == 'manager-loaded') {
                $(this.element).trigger('manager-loaded', params);
            }
            if (name == 'state-changed') {
                $(this.element).trigger('state-changed', params);
            }
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        destroy: function () {
            this.element.empty();
            $.Widget.prototype.destroy.call(this);
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
        },

        // called when created, and later when changing options
        _refresh: function () {
        },

        // Use the _setOption method to respond to changes to options
        _setOption: function (key, value) {
            $.Widget.prototype._setOption.apply(this, arguments);
        },

        _setOptions: function () {
            $.Widget.prototype._setOptions.apply(this, arguments);
            this._refresh();
        }
    });
}(jQuery));

$(document).ready(function () {
    $('div[data-layout~="manager"]:first').each(function (index, manager) {
        var options = {
            type: $(this).data('layout-manager'),
            rect: {
                top: 0,
                left: 0,
                right: $(this).parent().outerWidth(),
                bottom: $(this).parent().outerHeight()
            },
            resizable: false    //  we cannot resize top level layout manager...
        };
        
        $(this).area(options).layoutmanager(options);
    });
});

var delayWindowResize = (function (event) {
    var timer = 0;
    return function (callback, ms) {
        clearTimeout(timer);
        timer = setTimeout(callback, ms);
    };
})();

//  catch event on window's resize action...
$(window).resize(function (event) {
    if (event.target == window) {
        delayWindowResize(function () {
            //  we have to find all top level layout areas...
            $('div.layout-area').each(function () {
                if ($(this).parent().closest('div.layout-area').length == 0) {
                    // ... and notify them about resize...

                    var area = $(this).area('instance')

                    area.setRect({
                        top: 0,
                        left: 0,
                        right: $(this).parent().outerWidth(),
                        bottom: $(this).parent().outerHeight()
                    });

                    area.sendEvent('area-resize');
                    area.sendEvent('area-resize-stop');
                }
            });
        }, 250);
    }
});
