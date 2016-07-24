Ext.define('ChemSpider.controller.Search', {
    extend: 'Ext.app.Controller',

    requires: ['CS.store.Compound'],

    init: function () {
        this.compoundDetailsWnd = Ext.create('CS.view.CompoundWindow', {});

        this.control({
            '#csSearchResults': {
                itemclick: function (view, record, item, index, e, opts) {
                    if (!this.isDblClick)
                        this.showCompound(record.data.CSID);
                },
                itemdblclick: function (view, record, item, index, e, opts) {
                    //  clear selection after double ckicl action
                    if (document.selection && document.selection.empty)
                        document.selection.empty();
                    else if (window.getSelection)
                        window.getSelection().removeAllRanges();

                    this.compoundDetailsWnd.showCompound(record.data.CSID);
                }
            },
            'viewport': {
                afterrender: function () {
                    //  Viagra
                    this.showCompound(56586);
                    //  Aspirin
                    //this.showCompound(2157);
                }
            }
        });
    },

    showCompound: function (csid) {
        var oThis = this;

        var store = this.getStore('CS.store.Compound');
        store.load({
            params: { 'csids[0]': csid, serfilter: 'Compound[CSID|Name|MF|Mol|MM|Synonyms|References|Blobs|Identifiers]' },
            callback: function (records, operation, success) {
                var compound = store.first();

                var cmp = Ext.getCmp('csCompound');
                if (cmp != null) cmp.loadData(compound);

                var syn = Ext.getCmp('csSynonyms');
                if (syn != null) syn.loadData(compound);

                var ds = Ext.getCmp('csDataSources');
                if (ds != null) ds.loadData(compound);

                var spectra = Ext.getCmp('csSpectra');
                if (spectra != null) spectra.loadData(compound);

                var mainController = oThis.getController("Main");
                mainController.showMsg("Compound " + compound.data.Name + " loaded");
            }
        });
    }

});
