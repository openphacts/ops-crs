(function ($) {
    $.widget("compound.chemdoodle_viewer", $.compound.chemdoodle_molecule_base, {

        _onChemDoodleInit: function () {
            this.element
                .addClass('cs-widget cs-widget-chemdoodle-viewer');
        },

        _widget2D: function () {
            if (this.canvas2D == null) {
                this.canvas2D = $(document.createElement('canvas'))
                                        .attr({
                                            id: 'chemdoodle_viewer_' + Math.floor((Math.random() * 10000) + 1)
                                        })
                                        .appendTo(this.element);

                this.widget2D = new ChemDoodle.ViewerCanvas(this.canvas2D.attr('id'), this.options.width, this.options.height);
                $.extend(this.widget2D.specs, {
                    bonds_width_2D: 2,
                    atoms_displayTerminalCarbonLabels_2D: true,
                    atoms_useJMOLColors: true,
                    bonds_useJMOLColors: true,
                    atoms_font_bold_2D: true,
                    //atoms_font_size_2D: 15
                }, this.options.viewerSpecs);
            }

            return this.widget2D;
        },

        _widget3D: function () {
            if (this.canvas3D == null) {
                this.canvas3D = $(document.createElement('canvas'))
                                        .attr({
                                            id: 'chemdoodle_viewer3D_' + Math.floor((Math.random() * 10000) + 1)
                                        })
                                        .appendTo(this.element);

                this.widget3D = new ChemDoodle.ViewerCanvas3D(this.canvas3D.attr('id'), this.options.width, this.options.height);

                this.widget3D.specs.set3DRepresentation(this.options.representation3D);

                $.extend(this.widget3D.specs, {
                }, this.options.viewerSpecs);
            }

            return this.widget3D;
        }
    });
}(jQuery));
