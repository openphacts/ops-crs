$(function () {
    $.widget("compound.structureoptions", $.compound.compoundbase, {

        // default options
        options: {
            active: 0,
            saveState: true,
            exact: true,
            exact_strict: true,
            exact_all_tautomers: true,
            exact_allow_isotopes_and_stereoisomers: true,
            exact_field_name: null,
            substructure: true,
            substructure_match_tautomers: true,
            substructure_match_tautomers_field_name: null,
            similarity: true,
            similarity_type_field_name: null,
            similarity_threshold_field_name: null
        },

        // the constructor
        _create: function () {
            var oThis = this;

            this.element.addClass("cs-widget cs-widget-structure-options")

            var id = this.element.attr('id');

            if (this.options.exact_field_name == null) this.options.exact_field_name = id + '_exact';
            if (this.options.substructure_match_tautomers_field_name == null) this.options.substructure_match_tautomers_field_name = id + '_match_tautomers';
            if (this.options.similarity_type_field_name == null) this.options.similarity_type_field_name = id + '_similarity_type';
            if (this.options.similarity_threshold_field_name == null) this.options.similarity_threshold_field_name = id + '_similarity_threshold';

            this.accordion = $(document.createElement('div'));

            if (this.options.exact) {
                var ul = $(document.createElement('ul'));

                if (this.options.exact_strict) {
                    ul.append(
                        $(document.createElement('li'))
                            .append(
                                $(document.createElement('input'))
                                    .css('width', '20px')
                                    .attr({
                                        type: 'radio',
                                        value: 'strict',
                                        name: this.options.exact_field_name,
                                        checked: 'checked'
                                    })
                            )
                            .append(
                                $(document.createElement('label'))
                                    .text('Strict')
                            )
                    );
                }

                if (this.options.exact_all_tautomers) {
                    ul.append(
                        $(document.createElement('li'))
                            .append(
                                $(document.createElement('input'))
                                    .css('width', '20px')
                                    .attr({
                                        type: 'radio',
                                        value: 'all-tautomers',
                                        name: this.options.exact_field_name
                                    })
                            )
                            .append(
                                $(document.createElement('label'))
                                    .text('All Tautomers')
                            )
                    );
                }

                if (this.options.exact_allow_isotopes_and_stereoisomers) {
                    ul.append(
                        $(document.createElement('li'))
                        .append(
                            $(document.createElement('input'))
                                .css('width', '20px')
                                .attr({
                                    type: 'radio',
                                    value: 'allow-isotopes-and-stereoisomers',
                                    name: this.options.exact_field_name
                                })
                        )
                        .append(
                            $(document.createElement('label'))
                                .text('Allow Isotopes and Stereoisomers')
                        )
                    );
                }

                this.accordion.append(
                    $(document.createElement('h1'))
                        .append(document.createElement('a'))
                            .attr({ href: '#' })
                            .text('Exact Search')
                )
                .append($(document.createElement('div')).css('overflow', 'hidden').append(ul));
            }

            if (this.options.substructure) {
                this.accordion.append(
                    $(document.createElement('h3'))
                        .append(document.createElement('a'))
                            .attr({ href: '#' })
                            .text('Substructure Search')
                );

                var div = $(document.createElement('div'));

                if (this.options.substructure_match_tautomers) {
                    var ul = $(document.createElement('ul'));

                    ul.append(
                        $(document.createElement('li'))
                            .append(
                                $(document.createElement('input'))
                                    .css('width', '20px')
                                    .attr({
                                        type: 'radio',
                                        checked: 'checked',
                                        value: 'match-specified-tautomer',
                                        name: this.options.substructure_match_tautomers_field_name
                                    })
                            )
                            .append(
                                $(document.createElement('label'))
                                    .text('Match specified tautomer')
                            )
                    );

                    ul.append(
                        $(document.createElement('li'))
                            .append(
                                $(document.createElement('input'))
                                    .css('width', '20px')
                                    .attr({
                                        type: 'radio',
                                        value: 'match-all-tautomers',
                                        name: this.options.substructure_match_tautomers_field_name
                                    })
                            )
                            .append(
                                $(document.createElement('label'))
                                    .text('Match all tautomers')
                            )
                    );

                    div.append(ul);
                }

                div.append(this._addGGA());

                this.accordion.append(div);
            }

            if (this.options.similarity) {
                this.accordion.append(
                    $(document.createElement('h3'))
                        .append(document.createElement('a'))
                            .attr({ href: '#' })
                            .text('Similarity Search')
                )
                .append(
                    $(document.createElement('div'))
                        .append(
                            $(document.createElement('select'))
                                .attr({
                                    name: this.options.similarity_type_field_name
                                })
                                .change(function () {
                                    oThis._saveState();
                                })
                                .append(
                                    $(document.createElement('option'))
                                        .attr({ value: 'Tanimoto', selected: 'selected' })
                                        .text('Tanimoto')
                                )
                                .append(
                                    $(document.createElement('option'))
                                        .attr({ value: 'Tversky' })
                                        .text('Tversky')
                                )
                                .append(
                                    $(document.createElement('option'))
                                        .attr({ value: 'Euclidian' })
                                        .text('Euclidian')
                                )
                        )
                        .append(
                            $(document.createElement('select'))
                                .attr({
                                    name: this.options.similarity_threshold_field_name
                                })
                                .change(function () {
                                    oThis._saveState();
                                })
                                .append(
                                    $(document.createElement('option'))
                                        .attr({ value: '0.99', selected: 'selected' })
                                        .text('>=99%')
                                )
                                .append(
                                    $(document.createElement('option'))
                                        .attr({ value: '0.95' })
                                        .text('>=95%')
                                )
                                .append(
                                    $(document.createElement('option'))
                                        .attr({ value: '0.90' })
                                        .text('>=90%')
                                )
                                .append(
                                    $(document.createElement('option'))
                                        .attr({ value: '0.80' })
                                        .text('>=80%')
                                )
                                .append(
                                    $(document.createElement('option'))
                                        .attr({ value: '0.70' })
                                        .text('>=70%')
                                )
                        )
                        .append(this._addGGA())
                );
            }

            this.accordion.appendTo(this.element);

            this.accordion.accordion({
                active: this.options.active,
                activate: function (event, ui) {
                    oThis._saveState();
                    oThis._trigger("onChange", null, ui.newHeader.text());
                }
            });

            //  connect labels with input 
            $('label', this.element).each(function () {
                if (!$(this).prev().attr('id'))
                    $(this).prev().attr('id', 'id' + Math.floor((Math.random() * 1000000) + 1));

                $(this).attr('for', $(this).prev().attr('id'));
            });

            this.conditionsOfUseDialog = $(document.createElement('div'))
                .css('display', 'none')
                .attr({ 'title': 'Conditions of Use...' })
                .append(
                    $(document.createElement('iframe'))
                        .attr({
                            'width': '100%',
                            'height': '100%',
                            //'src': $.Compounds.Web.Url + '/GGA_Conditions_of_Use.htm'
                        })
                )

            this.element.append(this.conditionsOfUseDialog);

            this.conditionsOfUseDialog.dialog({ autoOpen: false,
                buttons: { "Close": function () { oThis.conditionsOfUseDialog.dialog("close"); } },
                modal: true,
                overlay: { opacity: 0.5, background: "black" },
                resizable: true,
                height: 520,
                width: 750
            });

            //  load state from cookies...
            this._loadState();
        },

        _saveState: function () {
            if (this.options.saveState) {
                var active = this.activeIndex();

                $.localStorage.set(this._getStateCookieName(), {
                    active: this.activeIndex(),
                    similarityThreshold: this.similarityThreshold(),
                    similarityType: this.similarityType()
                });
            }
        },

        _loadState: function () {
            //$.cookie.json = true;
            //$.cookie.raw = true;

            if (this.options.saveState) {
                var state = $.localStorage.get(this._getStateCookieName());
                if (state != null) {
                    this.activeIndex(state.active);
                    this.similarityThreshold(state.similarityThreshold);
                    this.similarityType(state.similarityType);
                }
            }
        },

        // called when created, and later when changing options
        _refresh: function () {
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
        },

        // _setOptions is called with a hash of all options that are changing
        // always refresh when changing options
        _setOptions: function () {
            // in 1.9 would use _superApply
            $.Widget.prototype._setOptions.apply(this, arguments);
            this._refresh();
        },

        // _setOption is called for each individual option that is changing
        _setOption: function (key, value) {
            $.Widget.prototype._setOption.apply(this, arguments);
        },

        activeIndex: function (index) {
            if (index)
                this.accordion.accordion("option", "active", index);
            else
                return this.accordion.accordion("option", "active");
        },

        exactSearchType: function (type) {
            if (type)
                $("input[name='" + this.options.exact_field_name + "']:checked").val(type);
            else
                return $("input[name='" + this.options.exact_field_name + "']:checked").val();
        },

        similarityThreshold: function (threshold) {
            if (threshold)
                $("select[name='" + this.options.similarity_threshold_field_name + "']").val(threshold);
            else
                return $("select[name='" + this.options.similarity_threshold_field_name + "']").val();
        },

        similarityType: function (type) {
            if (type)
                $("select[name='" + this.options.similarity_type_field_name + "']").val(type);
            else
                return $("select[name='" + this.options.similarity_type_field_name + "']").val();
        },

        _addGGA: function (elem) {
            var oThis = this;

            return $(document.createElement('div'))
                .addClass('gga')
                .append(
                    $(document.createElement('a'))
                    .addClass('logo')
                    .attr({
                        //href: $.Compounds.Web.Url + '/sponsor/22',
                        target: 'gga_sponsor'
                    })
                    .append(
                        $(document.createElement('img'))
                        .attr({
                            alt: 'GGA Software Services LLC',
                            src: $.Compounds.store.getUrl('Widgets/css/images/powered-by-gga.png')
                        })
                    )
                )
                .append(
                    $(document.createElement('a'))
                        .addClass('conditions')
                        .attr({
                            href: '#'
                        })
                        .click(function () {
                            oThis.conditionsOfUseDialog.dialog('open');
                            return false;
                        })
                        .html('Conditions for using Bingo:</br>GGA\'s Molecular Search Engine')
                );
        }
    });

}(jQuery));
