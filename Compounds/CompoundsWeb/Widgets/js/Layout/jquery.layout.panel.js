//@ sourceURL=jquery.layout.panel.js
/*
 * $Id: jquery.layout.panel.js 9013 2015-04-29 19:32:21Z alexp $ ::2B6A7A!
 * Copyright (c) 2005 Dr. Vahan Simonyan and Dr. Raja Mazumder.
 * This software is protected by U.S. Copyright Law and International
 * Treaties. Unauthorized use, duplication, reverse engineering, any
 * form of redistribution, use in part or as a whole, other than by
 * prior, express, written and signed agreement is subject to penalties.
 * If you have received this file in error, please notify copyright
 * holder and destroy this and any other copies. All rights reserved.
 */

$(function () {
    $.widget("layout.infoboxpanel", $.layout.infobox, {

        options: {
        },

        onInfoboxInit: function () {
            var oThis = this;

            this.infobox.addClass("layout-infobox-panel");

            this.sendEvent('panel-create');

            this.adjustHeight();

            if ($('div[data-layout~="manager"]', this.infobox).length > 0) {
                $('div[data-layout~="manager"]:first', this.infobox)
                    .area({
                        rect: {
                            top: 0,
                            left: 0,
                            right: $('.panel-body', this.infobox).outerWidth(),
                            bottom: $('.panel-body', this.infobox).outerHeight()
                        },
                        resizable: false
                    })
                    .layoutmanager();
            }

            //	in case if layout manager placed inside the panel we have to notify it about area resize...
            this.element.on('area-resize', function (event, params) {
            	if (oThis.infobox.is('div.layout-manager') || $('div.layout-manager', oThis.infobox).length > 0) {
	                var area = null; 

	                if(oThis.infobox.is('div.layout-area'))
	                	area = oThis.infobox.area('instance');
	                else
	                	area = $('.view-area', oThis.infobox).children().first().area('instance');

	                area.setRect({
	                    top: 0,
	                    left: 0,
	                    right: $('.view-area', oThis.infobox).outerWidth(),
	                    bottom: $('.view-area', oThis.infobox).outerHeight()
	                });
	
	                area.sendEvent('area-resize');
            	}
            });

            this.element.on('area-resize-stop', function (event, params) {
            	if (oThis.infobox.is('div.layout-manager') || $('div.layout-manager', oThis.infobox).length > 0) {
	                var area = null; 

	                if(oThis.infobox.is('div.layout-area'))
	                	area = oThis.infobox.area('instance');
	                else
	                	area = $('.view-area', oThis.infobox).children().first().area('instance');

	                area.sendEvent('area-resize-stop');
            	}
            });
            
            var viewArea = $('.view-area:first', this.element);

            viewArea.on('area-resize-move', function (event) {
                //    if panael contains layout manager inside and if the areas inside 
                //    are resized we must stop event propagation here
                event.stopPropagation();
            });

            this.element.on('init-widgets', function () {
            	oThis._initWidgets();
            });
        },

        sendEvent: function (name, params) {
            if (params == null)
                params = {};

            $.extend(params, {
                panel: this.element
            });

            if (name == 'panel-create') {
                $(this.element).trigger('panel-create', params);
            }
            else {
                // Invoke the parent widget's sendEvent().
                this._super(name, params);
            }
        },

        //    Save config information in order to reproduse the same Tabs infobos later on the fly
        save: function () {
            var oThis = this;

            var config = this._super();

            if ($('div.title', this.element).length)
                config.title = $('div.title', this.element).text();

            if ($(this.infobox).is('[data-name]'))
                config.name = $(this.infobox).data('name');

            var viewArea = $('div.view-area', this.element).first();

            if (viewArea.children().length == 1 && viewArea.children().first().is('div.layout-manager')) {
                //  layout manager inside panel...
                $.extend(config, viewArea.children().first().layoutmanager('save'));
            }
            else {
                //  regular view inside panel...
                if (viewArea.is('[data-jquery-widget]')) {
                    config.view = {
                        name: viewArea.data('jquery-widget')
                    };
                }
                if (viewArea.is('[data-jquery-widget-options]')) {
                    $.extend(config.view, {
                        options: viewArea.data('jquery-widget-options')
                    });
                }
            }

            return config;
        },

        adjustHeight: function () {
            this._super();

            $('.panel-body', this.infobox).height(this.height() - $('.panel-heading', this.infobox).outerHeight(true));
        }

    });

}(jQuery));
