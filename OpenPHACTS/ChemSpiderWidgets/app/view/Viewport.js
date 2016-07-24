Ext.define('ChemSpider.view.Viewport', {
    extend: 'Ext.container.Viewport',

    uses:
    [
        'CS.view.search.Simple',
        'CS.view.search.Structure',
        'CS.view.search.Results',
        'CS.view.Synonyms',
        'CS.view.DataSources',
        'CS.view.Compound',
        'CS.view.Spectra',

        'CS.layout.portlet.Panel',
        'CS.layout.portlet.Column',
        'CS.layout.portlet.Portlet',
        'CS.layout.portlet.DropZone'
    ],

    initComponent: function () {

        Ext.apply(this, {
            id: 'app-viewport',
            layout: {
                type: 'border',
                padding: '0 5 5 5' // pad the layout from the window edges
            },
            items: [{
                id: 'app-header',
                xtype: 'container',
                region: 'north',
                layout: 'hbox',
                height: 40,
                items:
                    [
                        {
                            xtype: 'simplesearch',
                            id: 'csSearch',
                            results: 'csSearchResults',
                            width: 400
                        },
                        {
                            xtype: 'structuresearch',
                            id: 'csSubStructureSearch',
                            results: 'csSearchResults'
                        },
//                        {
//                            xtype: 'button',
//                            text: 'Compound Info',
//                            id: 'csShowCompoundInfo',
//                            handler: function (btn, evn) {
//                                Ext.create('CS.view.CompoundWindow').showCompound(2157);
//                            }
//                        }
                    ]
            }, {
                xtype: 'container',
                region: 'center',
                layout: 'border',
                items: [{
                    id: 'app-options',
                    title: 'Search results',
                    region: 'west',
                    animCollapse: true,
                    width: 300,
                    minWidth: 150,
                    maxWidth: 400,
                    split: true,
                    collapsible: true,
                    layout: 'fit',
                    items: [{
                        id: 'csSearchResults',
                        xtype: 'cs.searchresults'
                    }]
                }, {
                    id: 'app-portal',
                    xtype: 'portalpanel',
                    region: 'center',
                    items: [{
                        id: 'col-1',
                        items: [{
                            id: 'basePropsPortlet',
                            title: 'Base Properties',
                            items: {
                                xtype: 'cs.compound',
                                id: 'csCompound'
                            }
                        }, {
                            id: 'synonymsPortlet',
                            title: 'Synonyms',
                            items: {
                                xtype: 'cs.synonyms',
                                id: 'csSynonyms'
                            }
                        }]
                    }, {
                        id: 'col-2',
                        items: [{
                            id: 'dataSourcesPortlet',
                            title: 'Data Sources',
                            items: {
                                xtype: 'cs.datasources',
                                id: 'csDataSources',
                                height: 200
                            }
                        }, {
                            id: 'spectraPortlet',
                            title: 'Spectra',
                            items: {
                                xtype: 'cs.spectra',
                                id: 'csSpectra'
                            }
                        }]
                    }]
                }]
            }]
        });
        this.callParent(arguments);
    }
});
