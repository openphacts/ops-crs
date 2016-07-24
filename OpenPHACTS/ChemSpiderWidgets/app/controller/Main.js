Ext.define('ChemSpider.controller.Main', {
    extend: 'Ext.app.Controller',

    init: function () {
        this.control({
            'portlet': {
                close: this.onPortletClose
            }
        });
    },

    onPortletClose: function (portlet) {
        this.showMsg('"' + portlet.title + '" was removed');
    },

    showMsg: function (msg) {
        var el = Ext.get('app-msg'),
            msgId = Ext.id();

        this.msgId = msgId;
        el.update(msg).show();

        Ext.defer(this.clearMsg, 3000, this, [msgId]);
    },

    clearMsg: function (msgId) {
        if (msgId === this.msgId) {
            Ext.get('app-msg').hide();
        }
    }
});
