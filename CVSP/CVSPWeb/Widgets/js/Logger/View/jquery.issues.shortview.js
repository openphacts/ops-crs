(function ($) {
    $.widget("logger.issues_shortview", {

        options: {
            issues: [] //  list of issues
        },

        _create: function () {
            var oThis = this;

            this.element.addClass('cvsp-issues-preview');

            $.Logger.store.getEntryTypes(function (entryTypes) {
                oThis._shorIssues(oThis.options.issues);
            });
        },

        _shorIssues: function (issues) {
            var oThis = this;

            var ul = $(document.createElement('ul')).appendTo(this.element);

            for (var i = 0; i < issues.length; i++)
            {
                var issue = issues[i];

                var li = $(document.createElement('li')).appendTo(ul);

                var entryType = $.Logger.getEntryTypeByCode(issue.Code);

                li.append(oThis._severityFormat(entryType.Severity));

                li.append(document.createTextNode(entryType.Title));

                if (i > 4)
                    li.hide();

                if (i == 4 && issues.length > 5)
                {
                    $(document.createElement('li'))
                        .append(
                            $(document.createElement('a'))
                                .attr({
                                    href: '#'
                                })
                                .text("more...")
                                .click(function () {
                                    $('li:hidden', $(this).parent().parent()).show();

                                    $(this).parent().remove();

                                    return false;
                                })
                        )
                        .appendTo(ul);

                }
            }
        },

        _severityFormat: function (severity) {
            if (severity.toLowerCase() == 'error') {
                return $(document.createElement('span'))
                        .addClass('label label-danger')
                        .append(
                            $(document.createElement('span'))
                                .addClass('glyphicon glyphicon-exclamation-sign')
                        );
            }
            else if (severity.toLowerCase() == 'warning') {
                return $(document.createElement('span'))
                        .addClass('label label-warning')
                        .append(
                            $(document.createElement('span'))
                                .addClass('glyphicon glyphicon-bell')
                        );
            }
            else if (severity.toLowerCase() == 'information') {
                return $(document.createElement('span'))
                        .addClass('label label-info')
                        .append(
                            $(document.createElement('span'))
                                .addClass('glyphicon glyphicon-info-sign')
                        );
            }
        }
    });

}(jQuery));
