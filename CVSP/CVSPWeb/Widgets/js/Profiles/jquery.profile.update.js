(function ($) {
    $.widget("cvsp.profile_update", $.toolkit.generalinfo_update, {

        options: {
            showImage: false,
            properties: [
                { name: 'Id', title: 'Profile\'s GUID', type: 'string', readonly: true },
                { name: 'SendEmail', title: 'Send Emails', type: 'bool' },
                { name: 'Datasource', title: 'Datasource', type: 'datasource_lookup', options: { mode: 'single' } }
            ],
            onCancel: function () { },  //  callback on cancel
            onSave: function () { }     //  callback on save
        },

        _loadData: function (id, callback) {
            $.Profiles.store.getProfile(id, function (profile) {
                if (callback)
                    callback(profile);
            });
        },

        _setImage: function (profile) {
        },

        _save: function () {
            var oThis = this;

            var profile = this.form.form('getData');

            $.Profiles.store.updateProfile(this.options.id, profile, function (result) {
                if (result) {
                    oThis._trigger("onSave", null, profile);
                }
            });
        }
    });

}(jQuery));
