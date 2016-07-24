/*
Class: compoundslookup widget that adds a tabbed view of simple search or structure search and if you search with them shows the results from the search underneath them 
*/
(function ($) {
    $.widget("compound.compoundslookup", {
        version: "0.1",

        store: $.Compounds.store,

        /*
        Constructor: compoundslookup
        compoundslookup widget that adds a tabbed view of simple search or structure search and if you search with them shows the results from the search underneath them 
        
        Parameters:
        options - Initialization <Options>

        (start code)
        $('#compoundslookupcontainer').compoundslookup({
            buttontitle: "RSC compounds lookup",
			successcallback: function () { alert("Selected compound: " + $('#compoundslookupcontainer').compoundslookup("getCompoundID")); },
            failcallback: function () { alert('Nothing was found' ); }
        });
        (end code)

        Type: Options

        Properties:
        buttontitle - text to be displayed in lookup button
 		successcallback - callback function when a compound returned from the search is clicked on
        failcallback - callback function when no matching compounds are found
		*/
        options: {
            buttontitle: "RSC compounds lookup",
            successcallback: null,
            failcallback: null
        },

        /*
        Function: getCompoundID

        Get ChemSpider ID of selected search result from the widget.  
        note that it will return null until a compound in the search results has been clicked on

        (start code)
        var compoundid = $('#compoundslookupcontainer').compoundslookup("getCompoundID");

        (end code)
        */
        getCompoundID: function () {
            return this.compoundID;
        },

        /*
        Function: getDrawnMol

        Get Mol of molecule drawn in structure editor to search on.  
        note that it will return null unless a compound has been drawn

        (start code)
        var drawnmol = $('#compoundslookupcontainer').compoundslookup("getDrawnMol");

        (end code)
        */
        getDrawnMol: function () {
            return this.drawwnmol;
        },

        // Set up the widget
        _create: function () {
            var oThis = this;
            this.compoundID = null;
            this.drawwnmol = null;
            oThis.element.addClass("cs-widget-compoundslookup")
            var compoundslookupbutton = $(document.createElement('input'))
                                    .attr({ 'id': 'compundsLookupBtn' })
                                    .attr({ 'type': 'submit' })
                                    .addClass("button")
                                    .attr({ 'value': this.options.buttontitle })
            oThis.compoundslookupbuttondiv = compoundslookupbutton.appendTo(oThis.element);

           var lookupdialog = $(document.createElement('div'))
                                .attr({ 'id': 'compoundlookupdialog' })
                                .attr({ 'title': 'Search by compound' })
                                .attr({ 'style': 'display: none' })
                                .attr({ 'width': 650 })
                                .attr({ 'resizable': true })
                                .addClass("cs-widget-compoundslookup")

            var searchtabs = $(document.createElement('div'))
                .attr({ 'id': 'simpleandstructuretabs' })
                .width(600)
                //.addClass("cs-widget-compoundslookup")
                .append(
                    $(document.createElement('ul'))
                        .append(
                            $(document.createElement('li'))
                                 .append(
                                        $(document.createElement('a'))
                                            .attr({ 'href': '#simpleSearch' })
                                            .text('Simple search')
                                 )
                        )
                        .append(
                            $(document.createElement('li'))
                                 .append(
                                        $(document.createElement('a'))
                                            .attr({ 'href': '#structureSearch' })
                                            .text('Structure search')
                                 )
                        )
                 )
                .append(
                    $(document.createElement('div'))
                        .attr({ 'id': 'simpleSearch' })
                        .addClass(".cs-widget-compoundslookup tabsdiv")
                        .append(
                                $(document.createElement('input'))
                                    .attr({ 'id': 'searchField' })
                                    .attr({ 'type': 'text' })
                                    .addClass(".cs-widget-compoundslookup textbox")
                                    .attr({ 'style': 'width: 500px!important' })
                        )
                         .append(
                                $(document.createElement('input'))
                                    .attr({ 'id': 'simpleSearchBtn' })
                                    .attr({ 'type': 'submit' })
                                    .addClass("button")
                                    .attr({ 'value': 'Search' })
                        )

                )
                .append(
                    $(document.createElement('div'))
                        .attr({ 'id': 'structureSearch' })
                        .addClass(".cs-widget-compoundslookup tabsdiv")
                        .append(
                                $(document.createElement('div'))
                                    .attr({ 'id': 'structureEditor' })
                                    .attr({ 'width': '250px' })
                                    .attr({ 'height': '250px' })
                                    .attr({ 'style': 'float: left; margin-right: 10px' })
                                )
                        .append(
                                $(document.createElement('div'))
                                    .attr({ 'id': 'structureOptions' })
                                    .attr({ 'width': '340px' })
                                    .attr({ 'style': 'float: left' })
                                )
                         .append(
                                $(document.createElement('br'))
                                    .attr({ 'style': 'clear: both;' })
                                )
                         .append(
                                $(document.createElement('input'))
                                    .attr({ 'id': 'structureSearchBtn' })
                                    .attr({ 'type': 'submit' })
                                    .addClass("button")
                                    .attr({ 'value': 'Search' })
                        )
                    )
            oThis.searchtabsdiv = searchtabs.appendTo(lookupdialog);
            var searchResults = $(document.createElement('div'))
                .attr({ 'id': 'searchResultsContainer' })
                .attr({ 'style': 'display: none;' })
                .append(
                     $(document.createElement('p')).text("Click on compound image below to confirm it as search result.")
                )

            oThis.searchResultsdiv = searchResults.appendTo(lookupdialog);

            
            var noResults = $(document.createElement('div'))
                .attr({ 'id': 'noResultsDiv' })
                .attr({ 'style': 'display: none;' })
                .append(
                        $(document.createElement('p')).text("No matches found.")
                            .attr({ 'id': 'noResultsMsg' })
                            .attr({ 'style': 'display: none;' })
                    )
                

            oThis.noResultsdiv = noResults.appendTo(lookupdialog);

            
            oThis.lookupdialogsdiv = lookupdialog.appendTo(oThis.element);

            $(document).ready(function () {

                $('#compundsLookupBtn').click(function () {
                    $("#compoundlookupdialog").dialog({
                        width: 650,
                        height: 520,
                        buttons: [{
                            text: "Cancel",
                            id: 'cancelBtn',
                            click: function () {
                                this.compoundID = null;
                                this.drawwnmol = null;
                                $('#publishBtn').hide();
                                $(this).dialog("close");
                            }
                        },
                        {
                            text: "Close",
                            id: 'closeBtn',
                            style: 'display: none;',
                            click: function () {
                                $(this).dialog("close");
                            }
                        },
                        {
                            text: "Search Again",
                            id : 'searchAgainBtn',
                            style: 'display: none;',
                            click: function () {
                                this.compoundID = null;
                                this.drawwnmol = null;
                                $("#simpleandstructuretabs").show();
                                $('#searchResultsContainer').hide();
                                $('#noResultsDiv').hide();
                                $('#searchAgainBtn').hide();
                                $('#publishBtn').hide();
                            }
                        },
                        {
                            text: "Publish",
                            id: 'publishBtn',
                            style: 'display: none;',
                            click: function () {
                                $('body').showMessage("Info", "We are very sorry this functionality is yet to be implemented. This is the mol that you have drawn: " + this.drawwnmol);
                            }
                        }]
                    });
                    $("#searchAgainBtn").click();
                });

                $('#simpleandstructuretabs').tabs({
                    active: 0,                    activate: function (event, ui) {
                        if (ui.newPanel.attr('id') === 'structureSearch') {
                            $("#structureOptions").structureoptions({
                            });
                        }
                    }
                });


                $('#simpleSearchBtn').click(function () {
                    oThis.drawwnmol = null;                    if ($('#searchField').val() != '') {                        //$('body').searchProgress();                        $(document).trigger("SimpleSearch", { value: $('#searchField').val() });
                    }
                });

                var structEditor = $("#structureEditor").structureeditor({
                    jsdraw: true,                    jchempaint: false,                    sketcher: false,                    width: 250,                    height: 250,                    saveState: true
                });

                $('#structureSearchBtn').click(function () {
                    var smiles = structEditor.structureeditor("smiles");                    if (smiles == '') {
                        $('body').showMessage("Info", "Please, enter some structure first!");                        return;
                    }                    oThis.drawwnmol =$('#structureEditor').structureeditor("molecule");                    //$('body').loadProgress("Searching...");                    var activeIndex = $("#structureOptions").structureoptions("activeIndex");                    //  Exact structure search...                    if (activeIndex == 0) {
                        $(document).trigger("ExactStructureSearch", { smiles: smiles });
                    }                        //  Sub-structure search...                    else if (activeIndex == 1) {
                        $(document).trigger("SubStructureSearch", { smiles: smiles });
                    }                        //  Similarity structure search...                    else if (activeIndex == 2) {
                        var threshold = $("#structureOptions").structureoptions("similarityThreshold")                        var type = $("#structureOptions").structureoptions("similarityType");                        $(document).trigger("SimilarityStructureSearch", { smiles: smiles, threshold: threshold, type: type });
                    }
                });

                

                $(document)                    .on('SimpleSearch', function (e, params) {                        $.Compounds.store.simpleSearch(params.value, function (rid) {                            oThis._showsearchresults(rid);
                        });
                    })                    .on('ExactStructureSearch', function (e, params) {
                        $.Compounds.store.exactStructureSearch(params.smiles, function (rid) {
                            oThis._showsearchresults(rid);                        });
                    })                    .on('SubStructureSearch', function (e, params) {
                        $.Compounds.store.substructureSearch(params.smiles, true, function (rid) {
                            oThis._showsearchresults(rid);                        });
                    })                    .on('SimilarityStructureSearch', function (e, params) {
                        $.Compounds.store.similarityStructureSearch(params.smiles, params.threshold, params.type, function (rid) {
                            oThis._showsearchresults(rid);                        });
                    });
                
            });

        },
		

		// called when created, and later when changing options
        _refresh: function () {
            var oThis = this;
			
        },

        // events bound via _bind are removed automatically
        // revert other modifications here
        _destroy: function () {
        },

        _showsearchresults: function (rid) {
            var oThis = this;
            $("#simpleandstructuretabs").hide();
            $("#searchResults").remove();
            var searchResults = $(document.createElement('div'))
                .attr({ 'id': 'searchResults' })
            $("#searchResultsContainer").append(searchResults);
            //oThis.searchResultsdiv = searchResults.appendTo(oThis.Element);
            $("#searchResults")		            .compounds_gridresults({
		                rid: rid,                        imgSize: 170,		                tooltipSize: 250,		                columns: [		                    { name: 'COMPOUND_ID', title: '#', format: function (id) { return $(document.createElement('a')).attr({ href: '/Compounds/v1/Compounds/' + id }).text(id); } },		                    { name: 'Structure', title: 'Structure' },		                    { name: 'MolecularFormula', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } },		                    { name: 'Indigo_MolecularWeight', title: 'Molecular Weight', format: function (value) { return parseFloat(value).toFixed(3); } },		                    { name: 'Indigo_MonoisotopicMass', title: 'Monoisotopic Mass', format: function (value) { return parseFloat(value).toFixed(6); } }		                ],		                tooltipFields: [		                    { name: 'COMPOUND_ID', title: 'ID' },		                    { name: 'IsVirtual', title: 'Virtual', format: function (virtual) { return virtual ? 'Yes' : 'No'; } },		                    { name: 'MolecularFormula', title: 'Molecular Formula', format: function (value) { return value.molecularFormula(); } },		                    { name: 'CompoundTypes', title: 'Type', format: buildCompoundTypesProperty }		                ],		                onResultsReady: function (e, status) {
		                    if (status.Count == 0) {
		                        $('#searchResultsContainer').hide();		                        
		                        if (oThis.drawwnmol == null) {
		                            $('#noResultsMsg').text("No matches found.");
		                        } else {
		                            $('#noResultsMsg').text("No matches found. Please consider depositing this mol to ChemSpider");
		                            $('#publishBtn').show();
		                        }
		                        $('#noResultsMsg').show();
		                        if (oThis.options.failcallback != null) {
		                            oThis.options.failcallback();
		                        }
		                    }
		                },
		                onClick: function (event, id) {
		                    oThis.compoundID = id;
		                    if (oThis.options.successcallback != null) {
		                        oThis.options.successcallback(id);
		                    } else {
		                        $('body').showMessage('Info', "Selected compound:" + id, null);
		                    }
		                    $("#closeBtn").click();
		                }
		            });            $('#searchResultsContainer').show();
            $('#noResultsDiv').show();
            $('#searchAgainBtn').show();
        }


    });
} (jQuery));
