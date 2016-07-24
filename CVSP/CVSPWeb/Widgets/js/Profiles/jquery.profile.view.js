(function ($) {
    $.widget("cvsp.profile_info", $.toolkit.generalinfo, {

        options: {
            id: null,   //  CVSP user profile GUID
            showImage: false,
            properties: [
                { name: 'Id', title: 'Profile\'s GUID' },
                { name: 'SendEmail', title: 'Send Emails', format: SendEmailsFormat },
                { name: 'Datasource', title: 'Datasource', format: DatasourceFormat },
            ]
        },

        // Set up the widget
        _create: function () {
            this._super();

            var oThis = this;

            this.element.addClass('cvsp-user-profile-info');
        },

        _initImage: function () {
        },

        _loadData: function (id, callback) {
            $.Profiles.store.getProfile(id, function (profile) {
                if (callback)
                    callback(profile);
            });
        },

        _setImage: function (data) {
        },

        _setTitle: function (data) {
            //  we currently do not have any title... 
        }
    });

}(jQuery));

function SendEmailsFormat(val) {
    return val ? "True" : "False";
}

function DatasourceFormat(val) {
    var span = $(document.createElement('span'))
                    .datasource_lookup({
                        readonly: true,
                        mode: 'single',
                        selected: [val]
                    });

    return span;

}