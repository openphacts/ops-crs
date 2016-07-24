$(function () {

    $.Compounds = $.Compounds || {};

    $.Compounds.Store = function (url) {
        return $.extend({

            getImageUrl: function (id, width, height) {
                return this.getDataUrl('api/image/compound/' + id, {
                    width: width,
                    height: height
                });
            },

            getImageUrlBySmiles: function (smiles, width, height) {
                return this.getUrl('api/image/compound/smiles', {
                    smiles: smiles,
                    width: width,
                    height: height
                });
            },

            getImageUrlByMolUrl: function (url, width, height) {
                return this.getUrl('api/image/compound/url', {
                    url: url,
                    width: width,
                    height: height
                });
            },

            simpleSearchWithOptions: function (options, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/searches/simple'),
                    data: JSON.stringify(options),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("simpleSearchWithOptions", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("simpleSearchWithOptions", xhr, error, thrownError);
                });
            },

            simpleSearch: function (query, callback, error_callback) {
                this.simpleSearchWithOptions({
                    searchOptions: {
                        QueryText: query
                    },
                    resultOptions: {
                        Limit: 100
                    }
                }, callback, error_callback);
            },

            exactStructureSearchWithOptions: function (options, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/searches/exact'),
                    data: JSON.stringify(options),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("exactStructureSearchWithOptions", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("exactStructureSearchWithOptions", xhr, error, thrownError);
                });
            },

            exactStructureSearch: function (molecule, callback, error_callback) {
                this.exactStructureSearchWithOptions({
                    searchOptions: {
                        Molecule: molecule
                    },
                    resultOptions: {
                        Limit: 100
                    }
                }, callback, error_callback);
            },

            substructureSearchWithOptions: function (options, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/searches/substructure'),
                    data: JSON.stringify(options),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("substructureSearchWithOptions", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("substructureSearchWithOptions", xhr, error, thrownError);
                });
            },

            substructureSearch: function (molecule, matchTautomers, callback, error_callback) {
                this.substructureSearchWithOptions({
                    searchOptions: {
                        Molecule: molecule,
                        MatchTautomers: matchTautomers
                    },
                    resultOptions: {
                        Limit: 100
                    }
                }, callback, error_callback);
            },

            similarityStructureSearchWithOptions: function (options, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/searches/similarity'),
                    data: JSON.stringify(options),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("similarityStructureSearchWithOptions", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("similarityStructureSearchWithOptions", xhr, error, thrownError);
                });
            },


            similarityStructureSearch: function (molecule, threshold, type, callback, error_callback) {
                this.similarityStructureSearchWithOptions({
                    searchOptions: {
                        Molecule: molecule,
                        Threshold: threshold,
                        SimilarityType: type
                    },
                    resultOptions: {
                        Limit: 100
                    }
                }, callback, error_callback)
            },

            advancedSearch: function (options, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/searches/advanced'),
                    data: JSON.stringify(options),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("advancedSearch", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("advancedSearch", xhr, error, thrownError);
                });
            },

            getSearchStatus: function (rid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/searches/status/' + rid)
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSearchStatus", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getSearchStatus", xhr, error, thrownError);
                });
            },

            getSearchResults: function (params, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/searches/result/' + params.rid),
                    data: {
                        start: params.start,
                        count: params.count
                    }
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSearchResults", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getSearchResults", xhr, error, thrownError);
                });
            },

            getSearchResultsWithInfo: function (params, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/searches/result-with-info/' + params.rid),
                    data: {
                        start: params.start,
                        count: params.count
                    }
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSearchResultsWithInfo", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getSearchResultsWithInfo", xhr, error, thrownError);
                });
            },

            waitForSearchResults: function (rid, callback, error_callback) {
                var oThis = this;

                this.getSearchStatus(rid, function (status) {
                    if (status.Status == 'Processing') {   //  Processing
                        //  ... not ready yet. Check it later again...
                        setTimeout(function () {
                            oThis.waitForSearchResults(rid, callback, error_callback);
                        }, 1000);
                    }
                    else if (status.Status == 'ResultReady') {   //  Results Ready
                        if (callback != null)
                            callback(status);
                    }
                    else if (status.Status == 'TooManyRecords') {   //  Too many records found
                        if (callback != null)
                            callback(status);
                    }
                    else {
                        $('body').showMessage('Error', status.Message);
                    }
                },
                error_callback);
            },

            waitAndGetSearchResults: function (rid, callback, error_callback) {
                var oThis = this;

                this.getSearchStatus(rid, function (status) {
                    if (status.Status == 'Processing') {   //  Processing
                        //  ... not ready yet. Check it later again...
                        setTimeout(function () {
                            oThis.waitAndGetSearchResults(rid, callback, error_callback);
                        }, 1000);
                    }
                    else if (status.Status == 'ResultReady') {   //  Results Ready
                        oThis.getSearchResults({ rid: rid }, callback, error_callback);
                    }
                    else {
                        $('body').showMessage('Error', status.Message);
                    }
                },
                error_callback);
            },

            getSearchResultsWithRelevance: function (params, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/searches/resultwithrelevance/' + params.rid),
                    data: {
                        start: params.start,
                        count: params.count
                    }
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSearchResultsWithRelevance", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getSearchResultsWithRelevance", xhr, error, thrownError);
                });
            },

            waitAndGetSearchResultsWithRelevance: function (rid, callback, error_callback) {
                var oThis = this;

                this.getSearchStatus(rid, function (status) {
                    if (status.Status == 'Processing') {   //  Processing
                        //  ... not ready yet. Check it later again...
                        setTimeout(function () {
                            oThis.waitAndGetSearchResultsWithRelevance(rid, callback, error_callback);
                        }, 1000);
                    }
                    else if (status.Status == 'ResultReady') {   //  Results Ready
                        oThis.getSearchResultsWithRelevance({ rid: rid }, callback, error_callback);
                    }
                    else {
                        $('body').showMessage('Error', status.Message);
                    }
                },
                error_callback);
            },
/*
            getSearchResultAsRecords: function (params, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/searches/resultasrecords/' + params.rid),
                    data: {
                        start: params.start,
                        count: params.count
                    }
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSearchResultAsRecords", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getSearchResultAsRecords", xhr, error, thrownError);
                });
            },

            waitAndGetSearchResultAsRecords: function (rid, callback, error_callback) {
                var oThis = this;

                this.getSearchStatus(rid, function (status) {
                    if (status.Status == 'Processing') {   //  Processing
                        //  ... not ready yet. Check it later again...
                        setTimeout(function () {
                            oThis.waitAndGetSearchResultAsRecords(rid, callback, error_callback);
                        }, 1000);
                    }
                    else if (status.Status == 'ResultReady') {   //  Results Ready
                        oThis.getSearchResultAsRecords({ rid: rid }, callback, error_callback);
                    }
                    else {
                        $('body').showMessage('Error', status.Message);
                    }
                },
                error_callback);
            },
*/
            getCompound: function (params, callback, error_callback) {
                var oThis = this;
                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/Compounds/' + params.id),
                    data: params
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getCompound", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getCompound", xhr, error, thrownError);
                });
            },

            getCompoundsByID: function (ids, callback, error_callback) {
                var oThis = this;

                //var params = {
                //    id: ids
                //};

                //$.ajax({
                //    type: 'GET',
                //    dataType: "jsonp",
                //    url: this.getDataUrl('api/compounds/list'),
                //    data: params
                //})
                $.ajax({
                    type: "POST",
                    url: this.getDataUrl('api/compounds/list'),
                    data: JSON.stringify(ids),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getCompoundsByID", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getCompoundsByID", xhr, error, thrownError);
                });
            },

            getCompoundsIDsByURL: function (url, start, count, callback, error_callback) {
                var oThis = this;

                var params = {
                    start: start,
                    count: count
                };

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: url,
                    data: params
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getCompoundsIDsByURL", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getCompoundsIDsByURL", xhr, error, thrownError);
                });
            },

            getCompoundsID: function (start, count, callback, error_callback) {
                this.getCompoundsIDsByURL(this.getDataUrl('api/compoundsid'), start, count, callback, error_callback);
            },

            getCompounds: function (start, count, callback, error_callback) {
                var oThis = this;

                var params = {
                    start: start,
                    count: count
                };

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/compounds'),
                    data: params
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getCompounds", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getCompounds", xhr, error, thrownError);
                });
            },

            getParents: function (id, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/compounds/' + id + '/parents'),
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getParents", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getParents", xhr, error, thrownError);
                });
            },

            getChildren: function (id, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/compounds/' + id + '/children'),
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getChildren", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getChildren", xhr, error, thrownError);
                });
            },

            getSimilarities: function (id, threshold, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/compounds/' + id + '/similarities'),
                    data: {
                        threshold: threshold
                    }
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getSimilarities", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getSimilarities", xhr, error, thrownError);
                });
            },

            getCompoundSubstances: function (id, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/compounds/' + id + '/substances'),
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getCompoundSubstances", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getCompoundSubstances", xhr, error, thrownError);
                });
            },

            getCompoundSynonyms: function (id, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/compounds/' + id + '/synonyms'),
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getCompoundSynonyms", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getCompoundSynonyms", xhr, error, thrownError);
                });
            },

            getMOL: function (url, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    dataType: 'jsonp',
                    url: url
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getMOL", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getMOL", xhr, error, thrownError);
                });
            },

            molToBase64Image: function (mol, width, height, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getUrl('api/image/compound/mol'),
                    data: JSON.stringify({ Mol: mol, Width: width, Height: height }),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("molToBase64Image", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("molToBase64Image", xhr, error, thrownError);
                });
            },

            convertTo: function (direction, text, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: "POST",
                    url: this.getUrl('api/convert'),
                    data: JSON.stringify({ Direction: direction, Text: text }),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("convertTo", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("convertTo", xhr, error, thrownError);
                });
            },

            convertMol2Smiles: function (text, callback, error_callback) {
                this.convertTo('Mol2SMILES', text, callback, error_callback);
            },

            convertSmiles2Mol: function (text, callback, error_callback) {
                this.convertTo('Smiles2Mol', text, callback, error_callback);
            },

            convertInChI2Mol: function (text, callback, error_callback) {
                this.convertTo('InChI2Mol', text, callback, error_callback);
            },

            getNmrFeatures: function (id, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/compounds/' + id + '/nmrfeatures')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getNmrFeatures", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getNmrFeatures", xhr, error, thrownError);
                });
            },

            getAllNmrFeatures: function (callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/nmrfeatures')
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getAllNmrFeatures", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getAllNmrFeatures", xhr, error, thrownError);
                });
            },

            nmrFeaturesSearch: function(features, callback, error_callback) {
                this.nmrFeaturesSearchWithOptions({
                    searchOptions: {
                        Features: features
                    },
                    resultOptions: {
                        Limit: 100
                    }
                }, callback, error_callback)
            },

            nmrFeaturesSearchWithOptions: function (options, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/searches/nmrfeatures'),
                    data: options
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("similarityStructureSearchWithOptions", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("similarityStructureSearchWithOptions", xhr, error, thrownError);
                });
            },

            getDatasourceCompoundsCount: function (guid, callback, error_callback) {
                var oThis = this;

                $.ajax({
                    type: 'GET',
                    dataType: "jsonp",
                    url: this.getDataUrl('api/datasources/' + guid + '/compounds/count')
                })
                .done(function (count) {
                    if (callback != null)
                        callback(count);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getDatasourceCompoundsCount", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getDatasourceCompoundsCount", xhr, error, thrownError);
                });
            }

        }, $.Toolkit.Store());
    };

}(jQuery));

$.Compounds.store = $.Compounds.Store();
