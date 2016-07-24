//@ sourceURL=jquery.layout.tabs.js
/*
 * $Id: jquery.layout.tabs.js 8873 2015-04-15 18:44:55Z alexp $ ::2B6A7A!
 * Copyright (c) 2005 Dr. Vahan Simonyan and Dr. Raja Mazumder.
 * This software is protected by U.S. Copyright Law and International
 * Treaties. Unauthorized use, duplication, reverse engineering, any
 * form of redistribution, use in part or as a whole, other than by
 * prior, express, written and signed agreement is subject to penalties.
 * If you have received this file in error, please notify copyright
 * holder and destroy this and any other copies. All rights reserved.
 */

$(function () {
    $.widget("layout.infoboxtabs", $.layout.infobox, {

        options: {
            droppable: {
                accept: function (ui) {
                    return $(ui.context).is('.layout-area:has(div.layout-infobox-tabs), .layout-infobox-tabs .nav-tabs > li');
                }
            }
        },

        onInfoboxInit: function () {
            var oThis = this;

            this.infobox.addClass("layout-infobox-tabs");

            this.adjustHeight();

            if (this.options.droppable) {
                this._initTabsDraggable();

                $('.tab-pane', this.infobox).each(function (index, tabPane) {
                    if ($('div[data-layout~="manager"]', this).length > 0) {
                        $('div[data-layout~="manager"]:first', this)
                            .area({
                                rect: {
                                    top: 0,
                                    left: 0,
                                    right: $(this).outerWidth(),
                                    bottom: $(this).outerHeight()
                                },
                                resizable: false
                            })
                            .layoutmanager();
                    }

                    $(tabPane).on('area-resize-move', function (event) {
                        //    if tab pane contains layout manager inside and if the areas inside 
                        //    are resized we must stop event propagation here
                        event.stopPropagation();
                    });
                });
            }

            //	in case if any layout manager placed inside the tab(s) we have to notify it about area resize...
            oThis.element.on('area-resize', function (event, params) {
            	$('.tab-pane', oThis.infobox).each(function (index, tabPane) {
            		if($(tabPane).children('div.layout-manager').length > 0) {
            			var area = $(tabPane).children('div.layout-manager').first().area('instance')

                        area.setRect({
                            top: 0,
                            left: 0,
                            right: $(tabPane).outerWidth(),
                            bottom: $(tabPane).outerHeight()
                        });

                        area.sendEvent('area-resize');
            		}
            	});
            });

            oThis.element.on('area-resize-stop', function (event, params) {
            	$('.tab-pane', oThis.infobox).each(function (index, tabPane) {
            		if($(tabPane).children('div.layout-manager').length > 0) {
            			var area = $(tabPane).children('div.layout-manager').first().area('instance')

                        area.sendEvent('area-resize-stop');
            		}
            	});
            });
            
            //    tabs infobox is initialized and we have to check if it has another layout manager inside...
            if (!this.element.is('div[data-layout~="manager"]')) {
                $('div[data-layout~="manager"]', this.element).each(function (index, manager) {
                    var options = parseAreaOptions(this);

                    options.type = $(this).data('layout-manager');
                    options.resizable = false;

                    $(this).layoutmanager(options);
                });
            }

            $(this.element).on('shown.bs.tab', 'a[data-toggle="tab"]', function (e) {
                oThis.sendEvent('state-changed');
                oThis.sendEvent('tab-shown', { index: $(e.target).parent().index(), tab: $(e.target).parent() });
            });

            this.element.on('init-widgets', function () {
            	oThis.adjustHeight();
            	oThis._initWidgets();
            });

            this.sendEvent('tabs-create');
            
            this.element.on('new-tab-added', function(event, tab) {
            	oThis._initTabDraggable(tab.tab);
            });
        },

        _initTabsDraggable: function() {
            var oThis = this;

            if (this.element.draggable) {
            	$('.nav-tabs > li', this.element).each(function() {
            		oThis._initTabDraggable(this);
            	});
            }
        },

        _initTabDraggable: function(li) {
        	var oThis = this;

            if (this.element.draggable) {
	            $(li).draggable({
	                revert: 'invalid',
	                distance: 10,
	                start: function (event, ui) {
	                    oThis.area.setFocus();
	                },
	                stop: function (event, ui) {
	                }
	            });
            }
        },
        
        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
        },

        sendEvent: function (name, params) {
            if (params == null)
                params = {};

            $.extend(params, {
                tabs: this.element
            });

            if (name == 'tabs-create') {
                $(this.element).trigger('tabs-create', params);
            }
            if (name == 'tab-shown') {
                $(this.element).trigger('tab-shown', params);
            }
            else {
                // Invoke the parent widget's sendEvent().
                this._super(name, params);
            }
        },

        _dropHandler: function (direction, area, ui) {
            if ($.contains(this.element.get(0), ui.draggable.get(0))) {
                ui.draggable.draggable('option', 'revert', true);
                return;
            }

            this.sendEvent('split-area', {
                area: this.element,
                dropArea: area,
                direction: direction,
                ui: ui
            });

            this._super(name, area, ui);
        },

        moveTab: function (li) {
            var fromInfobox = $(li).closest('.layout-infobox-tabs');

            var panelId = $('a', li).attr('href');

            var panel = $(panelId, fromInfobox).detach();

            var tab = $(li).detach().css({ top: '', left: '' });

            $('ul.nav-tabs', this.element).append(tab);
            $('div.panel-body', this.element).append(panel);

            if ($(tab).hasClass('active')) {
                //  need to deactivate previouse active tab...
                $('.nav-tabs > li', this.element).removeClass('active');
                $(tab).addClass('active');

                $('.panel-body > div.tab-pane.active', this.element).removeClass('active');
                $(panel).addClass('active');
            }

            if ($('.nav-tabs > li.active', fromInfobox).length == 0) {
                //  source infobox doesn't have any active tab anymore...
                //  we have to activate one... let's say first one
                $('.nav-tabs > li:first', fromInfobox).addClass('active');
                $('.panel-body > div.tab-pane:first', fromInfobox).addClass('active');
            }

            //  if infobox does't have any tabs we probably have to close it...
            if ($('ul.nav-tabs > li', fromInfobox).length == 0) {
                if (fromInfobox.parent().is('.layout-area'))
                    fromInfobox.parent().remove();
            }

            if ($('ul.nav-tabs > li', this.element).length == 1) {
                $('.nav-tabs > li', this.element).addClass('active');
                $('.panel-body > div.tab-pane', this.element).addClass('active');
            }

            this.adjustHeight();
        },

        moveInfobox: function (fromInfobox, toInfobox) {
            $('ul.nav-tabs', toInfobox).append(
                $('ul.nav-tabs > li', fromInfobox).removeClass('active').detach()
            );

            $('div.panel-body', toInfobox).append(
                $('.panel-body > div.tab-pane', fromInfobox).removeClass('active').detach()
            );

            fromInfobox.remove();

            this.adjustHeight();
        },
        //    Save config information in order to reproduse the same Tabs infobos later on the fly
        save: function () {
            var oThis = this;

            var config = this._super();

            $.extend(config, {
                type: 'tabs',
                tabs: {
                    items: []
                }
            });

            $('ul.nav-tabs', this.element).children('li').each(function (index, li) {
                var a = $('a', li);
                var pane = $(a.attr('href'), oThis.element);

                var tabConfig = {
                    active: $(li).is('.active'),
                    title: a.text(),
                    name: a.data('name'),
                    class: a.attr('class'),
                    overflow: pane.css('overflow')
                };

                if (pane.children().length == 1 && pane.children().first().is('div.layout-manager')) {
                    //  layout manager inside panel...
                    $.extend(tabConfig, pane.children().first().layoutmanager('save'));
                }
                else {
                    if (pane.data('jquery-widget')) {
                        tabConfig.view = {
                            name: pane.data('jquery-widget')
                        };
                    }
                    if (pane.data('jquery-widget-options')) {
                        $.extend(tabConfig.view, {
                            options: pane.data('jquery-widget-options')
                        });
                    }
                }

                config.tabs.items.push(tabConfig);
            });

            return config;
        },

        adjustHeight: function () {
            this._super();

            $('.tab-pane', this.infobox)
                .height(this.height() - $('.panel-heading', this.infobox).outerHeight(true))
                .width(this.width());
        }
    });

}(jQuery));
