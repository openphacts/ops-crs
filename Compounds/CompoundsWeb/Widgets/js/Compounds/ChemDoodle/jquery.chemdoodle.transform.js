(function ($) {
    $.widget("compound.chemdoodle_transform", $.compound.chemdoodle_molecule_base, {

        _onChemDoodleInit: function () {
            this.element
                .addClass('cs-widget cs-widget-chemdoodle-transform');
        },

        _widget2D: function () {
            if (this.canvas2D == null) {
                this.canvas2D = $(document.createElement('canvas'))
                                            .attr({
                                                id: 'chemdoodle_transform_' + Math.floor((Math.random() * 10000) + 1)
                                            })
                                            .appendTo(this.element);

                this.widget2D = new ChemDoodle.TransformCanvas(this.canvas2D.attr('id'), this.options.width, this.options.height, true);
                $.extend(this.widget2D.specs, {
                    bonds_width_2D: 2,
                    atoms_displayTerminalCarbonLabels_2D: true,
                    atoms_useJMOLColors: true,
                    bonds_useJMOLColors: true,
                    atoms_font_bold_2D: true,
                    atoms_font_size_2D: 15,
                    atoms_display: false,
                    bonds_clearOverlaps_2D: true
                }, this.options.viewerSpecs);
            }

            return this.widget2D;
        },

        _widget3D: function () {
            if (this.canvas3D == null) {
                this.canvas3D = $(document.createElement('canvas'))
                                    .attr({
                                        id: 'chemdoodle_transform3D_' + Math.floor((Math.random() * 10000) + 1)
                                    })
                                    .appendTo(this.element);

                this.widget3D = new ChemDoodle.TransformCanvas3D(this.canvas3D.attr('id'), this.options.width, this.options.height, true);

                this.widget3D.specs.set3DRepresentation(this.options.representation3D);

                $.extend(this.widget3D.specs, {
                    bonds_useJMOLColors: true,
                    bonds_useJMOLColors: true,
                    //atoms_useVDWDiameters_3D: true,
                    //atoms_displayLabels_3D: true,
                    //bonds_resolution_3D: 30,
                }, this.options.viewerSpecs);
            }

            return this.widget3D;
        }
    });
}(jQuery));
