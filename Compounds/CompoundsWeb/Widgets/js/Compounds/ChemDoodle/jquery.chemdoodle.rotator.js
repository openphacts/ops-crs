(function ($) {
    $.widget("compound.chemdoodle_rotator", $.compound.chemdoodle_molecule_base, {

        _onChemDoodleInit: function () {
            this.element
                .addClass('cs-widget cs-widget-chemdoodle-rotator');
        },

        _widget2D: function () {
            if (this.canvas2D == null) {
                this.canvas2D = $(document.createElement('canvas'))
                                        .attr({
                                            id: 'chemdoodle_rotator_' + Math.floor((Math.random() * 10000) + 1)
                                        })
                                        .appendTo(this.element);

                this.widget2D = new ChemDoodle.RotatorCanvas(this.canvas2D.attr('id'), this.options.width, this.options.height, true);
                $.extend(this.widget2D.specs, {
                    atoms_useJMOLColors: true,
                    bonds_useJMOLColors: true,
                    atoms_font_bold_2D: true,
                    atoms_circles_2D: true,
                    bonds_symmetrical_2D: true
                }, this.options.viewerSpecs);

                this.widget2D.startAnimation();
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

                this.widget3D = new ChemDoodle.RotatorCanvas3D(this.canvas3D.attr('id'), this.options.width, this.options.height);

                this.widget3D.specs.set3DRepresentation(this.options.representation3D);

                $.extend(this.widget3D.specs, {
                }, this.options.viewerSpecs);

                this.widget3D.startAnimation();
            }

            return this.widget3D;
        },

        startAnimation: function () {
            if (this.is3DInitialized())
                this.widget3D.startAnimation();

            if (this.is2DInitialized())
                this.widget2D.startAnimation();
        },

        stopAnimation: function () {
            if (this.is3DInitialized())
                this.widget3D.stopAnimation();
            if (this.is2DInitialized())
                this.widget2D.stopAnimation();
        },

        _setOption: function (key, value) {
            if (key === 'animation') {
                if (value === 'start')
                    this.startAnimation();
                else (value === 'stop')
                    this.stopAnimation();
            }

            //$.compounds.chemdoodle_molecule_base.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);
        }
    });
}(jQuery));
