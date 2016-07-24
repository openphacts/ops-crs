(function ($) {
    $.widget("compound.compoundinfo", $.toolkit.generalinfo, {

        options: {
            properties: [
                { name: 'Id', title: 'ID' },
                //{ name: 'MolecularFormula', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } },
                //{ name: 'Indigo_MonoisotopicMass', title: 'Monoisotopic Mass', format: function (value) { if (value) return value.toFixed(6) + ' Da'; else return ''; } },
                //{ name: 'Indigo_MolecularWeight', title: 'Molecular Weight', format: function (value) { if (value) return value.toFixed(6) + ' Da'; else return ''; } },
                { name: 'Smiles', title: 'SMILES', format: buildSMILESProperty },
                { name: 'StandardInChI', title: 'Std. InChI', format: buildInChIProperty },
                { name: 'StandardInChI', title: 'Std. InChIKey', format: buildInChIKeyProperty },
                { name: 'StandardInChI', title: 'ChemSpider ID', format: buildChemSpiderIDProperty }
            ]
        },

        _loadData: function (id, callback) {
            $.Compounds.store.getCompound({ id: id }, function (compound) {
                if (callback)
                    callback(compound);
            });
        },

        _setImage: function (data) {
            this.image.molecule2d('setID', data.Id);
        },

        _setTitle: function (data) {
            //  we currently do not have any title... 
        }
    });

}(jQuery));

function buildSMILESProperty(smiles) {
    return $(document.createElement('span'))
                .append(
                    smiles.IndigoSmiles.softHyphen(10)
                )
                .append(
                    $(document.createElement('button'))
                        .attr({
                            type: 'button',
                            'data-toggle': 'tooltip',
                            'data-placement': 'right',
                            title: 'Copy SMILES',
                            'data-clipboard-text': smiles.IndigoSmiles
                        })
                        .addClass('btn btn-default btn-xs copy-button')
                        .append(
                            $(document.createElement('span'))
                                .addClass('glyphicon glyphicon-share')
                        )
                        .zeroClipboard()
                );
}

function buildInChIProperty(inchi) {
    return $(document.createElement('span'))
                .append(
                    $(document.createElement('a'))
                        .attr({
                            target: 'GoogleSearch',
                            href: 'http://www.google.com/search?q=' + inchi.Inchi,
                            'data-toggle': 'tooltip',
                            'data-placement': 'top',
                            title: 'Search on Google'
                        })
                        .html(inchi.Inchi.softHyphen(10))
                )
                .append(
                    $(document.createElement('button'))
                        .attr({
                            type: 'button',
                            'data-toggle': 'tooltip',
                            'data-placement': 'right',
                            title: 'Copy InChI',
                            'data-clipboard-text': inchi.Inchi
                        })
                        .addClass('btn btn-default btn-xs copy-button')
                        .append(
                            $(document.createElement('span'))
                                .addClass('glyphicon glyphicon-share')
                        )
                        .zeroClipboard()
                )
}

function buildInChIKeyProperty(inchi) {
    var index = inchi.InChIKey.indexOf('-');
    if (index > 1) {
        var sceleton = inchi.InChIKey.substring(0, index);

        return $(document.createElement('span'))
                        .append(
                        $(document.createElement('a'))
                            .attr({
                                target: 'GoogleSearch',
                                href: 'http://www.google.com/search?q=' + sceleton,
                                'data-toggle': 'tooltip',
                                'data-placement': 'top',
                                title: 'Search similar structures on Google'
                            })
                            .text(sceleton)
                        )
                        .append(
                            document.createTextNode('-')
                        )
                        .append(
                            $(document.createElement('a'))
                                .attr({
                                    target: 'GoogleSearch',
                                    href: 'http://www.google.com/search?q=' + inchi.InChIKey,
                                    'data-toggle': 'tooltip',
                                    'data-placement': 'top',
                                    title: 'Search exact structure on Google'
                                })
                                .text(inchi.InChIKey.substring(index + 1))
                        )
                        .append(
                            $(document.createElement('button'))
                                .attr({
                                    type: 'button',
                                    'data-toggle': 'tooltip',
                                    'data-placement': 'right',
                                    title: 'Copy InChIKey',
                                    'data-clipboard-text': inchi.InChIKey
                                })
                                .addClass('btn btn-default btn-xs copy-button')
                                .append(
                                    $(document.createElement('span'))
                                        .addClass('glyphicon glyphicon-share')
                                )
                                .zeroClipboard()
                        );
    }
    else {
        return value;
    }
}

function buildChemSpiderIDProperty(inchi) {
    var span = $(document.createElement('span'))
        .append(
            $(document.createElement('span')).addClass('loading-spinner')
        );

    $.ChemSpider.store.convertInChiKey2CSID(inchi.InChIKey, function (res) {
        if (res.confidence == 100) {
            span.empty().append(
                $(document.createElement('a'))
                    .attr({
                        href: 'http://www.chemspider.com/' + res.mol,
                        target: '_blank'
                    })
                    .text(res.mol)
                );
        }
        else {
            span.parent().prev().fadeOut(500);
            span.parent().fadeOut(500);
        }
    });

    return span;
}

function buildCompoundTypesProperty(types) {
    var value = '';

    for (var i = 0; i < types.length; i++) {
        if (value)
            value += ', ';

        value += types[i].match(/[A-Z][a-z]+/g).join(' ');
    }

    return value;
}
