/*
Title: Database Store

Class: ChemSpider.Database.Store
Base object for interaction with ChemSpider database and extracting data. Based on <JSON.ashx at http://parts.chemspider.com/JSON.ashx> REST-full service that returns responses in JSON format.
*/
$(function () {

    $.ChemSpider = $.ChemSpider || {};

    $.ChemSpider.Web = $.ChemSpider.Web || {};

    $.ChemSpider.Database = $.ChemSpider.Database || {};

    /*
    Constant: $.ChemSpider.Database.BaseUrl
    Specify base URL for getting access to the service and download scripts from.
    */
    $.ChemSpider.Database.BaseUrl = '//parts.chemspider.com';

    $.ChemSpider.Database.DataHandlerUrl = $.ChemSpider.Database.BaseUrl;

    $.ChemSpider.Database.ImageHandlerUrl = $.ChemSpider.Database.BaseUrl;

    /*
    Constant: $.ChemSpider.Web.Url
    Specify base URL to the main ChemSpider website. Default value http://www.chemspider.com
    */
    $.ChemSpider.Web.Url = 'http://www.chemspider.com';

    $.ChemSpider.Database.Store = function (url) {
        return {
            version: "0.2.0",

            scope: null,

            jsonp: true,

            /*
            Function: onError

            Global OnError handler that will handle all errors occured during the intaraction with ChemSpider side by <JSON.ashx at http://parts.chemspider.com/JSON.ashx> service. 
            You can use this handler by default or reimplement it according your needs. Also you can handle all errors inside the function passing error_callback parameter where it is possible

            Parameters:

            proc - Function name where the error occured
            xhr - XmlHttpRequest object with detailed information about error
            */
            onError: function (proc, xhr, error, thrownError) {
                $('body').showMessage(xhr.statusText + ' : ' + xhr.status, 'Error in procedure ' + proc + ' has happened.<br/></br>' + xhr.responseText);
            },

            /*
            Function: getUrl

            Helper function that concatenate base URL <$.ChemSpider.Database.DataHandlerUrl> with the page and list of parameters

            Parameters:

            page - Relative path to the page
            params - List of parameters

            (start code)
            var url = $.ChemSpider.store.getUrl('JSON.asxh', {'op': 'SimpleSearch', 'searchOptions.QueryText': 'Aspirin'});
            (end code)
            */
            getUrl: function (page, params) {
                var url = $.ChemSpider.Database.DataHandlerUrl + '/' + page;

                if (params == null)
                    return url;

                return url + '?' + this.buildUrlParams(params);
            },

            buildUrlParams: function (params, prefix) {
                var urlParams = '';

                for (var key in params) {
                    if (key != null) {
                        var name = prefix != null ? prefix + "." + key : key;

                        if (urlParams != '') urlParams += '&';
                        if ($.isPlainObject(params[key])) {
                            urlParams += this.buildUrlParams(params[key], name);
                        }
                        else {
                            urlParams += name + '=' + params[key];
                        }
                    }
                }

                return urlParams;
            },

            /*
            Function: getJsonUrl

            Helper function that based on <getUrl> and return URL for the specific <JSON.asxh at http://parts.chemspider.com/JSON.ashx> page

            Parameters:

            params - List of parameters

            (start code)
            var url = $.ChemSpider.store.getJsonUrl({'op': 'SimpleSearch', 'searchOptions.QueryText': 'Aspirin'});
            (end code)
            */
            getJsonUrl: function (params) {
                return this.getUrl('JSON.ashx', params);
            },

            getImageUrlBySmiles: function (smiles, width, height) {
                //http://parts.chemspider.com/ImagesHandler.ashx?smiles=CC%28=O%29OC1=CC=CC=C1C%28=O%29O&w=500&h=500
                return this.getUrl('ImagesHandler.ashx', {
                    smiles: smiles,
                    w: width,
                    h: height
                });
            },

            /*
            Function: simpleSearch

            Run a simple search on ChemSpider side which tries to interpret a query string as anything it can search by (Synonym, SMILES, InChI, ChemSpider ID etc.)
            
            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            query - Query string (Synonym, SMILES, InChI, ChemSpider ID etc.)
            callback - Callback function that will be called if function finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.simpleSearch('Aspirin', function(rid) {
            //  Request ID will be returned through 'rid' parameter.
            //  This 'rid' should be used later for further actions...
            });
            (end code)
            */
            simpleSearch: function (query, callback, error_callback) {
                var oThis = this;

                var params = {
                    'op': 'SimpleSearch',
                    'searchOptions.QueryText': query,
                    'resultOptions.Limit': 100,
                    'resultOptions.Start': 0,
                    'resultOptions.Length': -1
                };

                $.extend(params, this.scope);

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                //$.getJSON(this.getJsonUrl(params))
                $.ajax({
                    dataType: this.jsonp ? "json" : "text",
                    url: this.getJsonUrl(params)
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("simpleSearch", xhr, error, thrownError);
                    else if (this.onError != null)
                        this.onError("simpleSearch", xhr, error, thrownError);
                });
            },

            /*
            Function: exactStructureSearch

            Run identical structure search.
            
            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            molecule - Molecule for search as SMILES
            callback - Callback function that will be called if function finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.exactStructureSearch('CC(=O)Oc1ccccc1C(=O)O', function(rid) {
            //  Request ID will be returned through 'rid' parameter.
            //  This 'rid' should be used later for further actions...
            });
            (end code)
            */
            exactStructureSearch: function (molecule, callback, error_callback) {
                var params = {
                    'op': 'ExactStructureSearch',
                    'searchOptions.Molecule': molecule,
                    'resultOptions.Limit': 100,
                    'resultOptions.Start': 0,
                    'resultOptions.Length': -1
                };

                $.extend(params, this.scope);

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                //$.getJSON(this.getJsonUrl(params))
                $.ajax({
                    dataType: this.jsonp ? "json" : "text",
                    url: this.getJsonUrl(params)
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("exactStructureSearch", xhr, error, thrownError);
                    else if (this.onError != null)
                        this.onError("exactStructureSearch", xhr, error, thrownError);
                });
            },

            /*
            Function: substructureSearch

            Run substructure search.
            
            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            molecule - Molecule for search as SMILES
            callback - Callback function that will be called if function finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.substructureSearch('CC(=O)Oc1ccccc1C(=O)O', function(rid) {
            //  Request ID will be returned through 'rid' parameter.
            //  This 'rid' should be used later for further actions...
            });
            (end code)
            */
            substructureSearch: function (molecule, callback, error_callback) {
                this.substructureSearchBase({
                    'searchOptions.Molecule': molecule,
                    'resultOptions.Limit': 100
                },
                    callback,
                    error_callback
                );
            },

            /*
            Function: substructureSMARTSSearch

            Run substructure search by SMARTS.

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            smarts - SMARTS string to search for
            callback - Callback function that will be called if function finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.substructureSMARTSSearch('NC(=N)c(cc1)ccc1C', function(rid) {
            //  Request ID will be returned through 'rid' parameter.
            //  This 'rid' should be used later for further actions...
            });
            (end code)
            */
            substructureSMARTSSearch: function (smarts, callback, error_callback) {
                this.substructureSearchBase({
                    'searchOptions.Molecule': smarts,
                    'searchOptions.MolType': 'SMARTS',
                    'resultOptions.Limit': 100
                },
                    callback,
                    error_callback
                );
            },

            /*
            Function: substructureSearchBase

            Run substructure search.

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            params - list of parameters sent through the request
            callback - Callback function that will be called if function finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.substructureSearch('CC(=O)Oc1ccccc1C(=O)O', function(rid) {
            //  Request ID will be returned through 'rid' parameter.
            //  This 'rid' should be used later for further actions...
            });
            (end code)
            */
            substructureSearchBase: function (params, callback, error_callback) {

                $.extend(params, { 'op': 'SubstructureSearch' });
                $.extend(params, this.scope);

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                $.ajax({
                    dataType: this.jsonp ? "json" : "text",
                    url: this.getJsonUrl(params)
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("substructureSearchBase", xhr, error, thrownError);
                    else if (this.onError != null)
                        this.onError("substructureSearchBase", xhr, error, thrownError);
                });
            },

            /*
            Function: similaritySearch

            Run substructure search.

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            molecule - Molecule for search as SMILES
            threshold - Similarity threshold. Double value from 0 to 1. 99% similarity means 0.99 in this case; 80% should be as 0.8 
            type - Similarity search type. Posible values: {Tanimoto, Tversky, Euclidian}
            callback - Callback function that will be called if function finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.substructureSearch('CC(=O)Oc1ccccc1C(=O)O', 0.99, 'Tanimoto', function(rid) {
            //  Request ID will be returned through 'rid' parameter.
            //  This 'rid' should be used later for further actions...
            });
            (end code)
            */
            similaritySearch: function (molecule, threshold, type, callback, error_callback) {
                this.similaritySearchBase({
                    'searchOptions.Molecule': molecule,
                    'searchOptions.Threshold': threshold,
                    'searchOptions.SimilarityType': type,
                    'resultOptions.Limit': 100,
                    'resultOptions.Start': 0,
                    'resultOptions.Length': -1
                },
                    callback,
                    error_callback
                );
            },

            similaritySearchBase: function (params, callback, error_callback) {

                $.extend(params, { 'op': 'SimilaritySearch' });
                $.extend(params, this.scope);

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                $.ajax({
                    dataType: this.jsonp ? "json" : "text",
                    url: this.getJsonUrl(params)
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("similaritySearch", xhr, error, thrownError);
                    else if (this.onError != null)
                        this.onError("similaritySearch", xhr, error, thrownError);
                });
            },

            advancedSearch: function (params, callback, error_callback) {

                $.extend(params, { 'op': 'AdvancedSearch' });

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                $.ajax({
                    dataType: this.jsonp ? "json" : "text",
                    url: this.getJsonUrl(params)
                })
                .done(function (rid) {
                    if (callback != null)
                        callback(rid);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("advancedSearch", xhr, error, thrownError);
                    else if (this.onError != null)
                        this.onError("advancedSearch", xhr, error, thrownError);
                });
            },

            /*
            Function: getSearchStatus

            Get search status by previously returned request ID. To get more information about status object, please check <here at http://parts.chemspider.com/JSON.ashx#RequestStatus>.

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            rip - Request ID returned from any search functions.
            callback - Callback function that will be called if finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.getSearchStatus(rid, function(status) {
            //  rid - request ID that was returned from any search function
            //  status - current search status
            });
            (end code)
            */
            getSearchStatus: function (rid, callback, error_callback) {
                var oThis = this;

                var params = {
                    'op': 'GetSearchStatus',
                    'rid': rid
                };

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                $.ajax({
                    dataType: "json",
                    url: this.getJsonUrl(params)
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

            /*
            Function: getSearchResults

            Returns search results as list of internal ChemSpider IDs. Be shure that the search already finished and status is ResultReady. See also <waitAndGetSearchResults> function.

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            rip - Request ID returned from any search functions.
            callback - Callback function that will be called if finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.getSearchResults(rid, function(compounds) {
            //  rid - request ID that was returned from any search function
            //  compounds - list of ChemSpider IDs
            });
            (end code)
            */
            getSearchResults: function (rid, callback, error_callback) {
                var oThis = this;

                var params = {
                    'op': 'GetSearchResult',
                    'rid': rid
                };

                $.ajax({
                    type: "GET",
                    dataType: "jsonp",
                    url: this.getJsonUrl(),
                    data: params
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

            /*
            Function: waitAndGetSearchResults

            Wait while the search is finished and return the results as list of internal ChemSpider IDs. 

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            rip - Request ID returned from any search functions.
            callback - Callback function that will be called if finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.waitAndGetSearchResults(rid, function(compounds) {
            //  rid - request ID that was returned from any search function
            //  compounds - list of ChemSpider IDs
            });
            (end code)
            */
            waitAndGetSearchResults: function (rid, callback, error_callback) {
                var oThis = this;

                this.getSearchStatus(rid, function (status) {
                    if (status.Status == 3) {   //  Processing
                        //  ... not ready yet. Check it later again...
                        setTimeout(function () {
                            oThis.waitAndGetSearchResults(rid, callback, error_callback);
                        }, 1000);
                    }
                    else if (status.Status == 6) {   //  Results Ready
                        oThis.getSearchResults(rid, callback, error_callback);
                    }
                    else {
                        $('body').showMessage('Error', status.Message);
                    }
                },
                error_callback);
            },

            /*
            Function: getSearchResultsWithRelevance

            Returns search results as list of internal ChemSpider IDs with one additional column - relevance. Records are sorted by relevance. Be shure that the search already finished and status is ResultReady. See also <waitAndGetSearchResults> function.

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            rip - Request ID returned from any search functions.
            callback - Callback function that will be called if finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.getSearchResultsWithRelevance(rid, function(compounds) {
            //  rid - request ID that was returned from any search function
            //  compounds - list pairs: ChemSpider ID and Relevance
            });
            (end code)
            */
            getSearchResultsWithRelevance: function (rid, callback, error_callback) {
                var oThis = this;

                var params = {
                    'op': 'GetSearchResultWithRelevance',
                    'rid': rid
                };

                $.ajax({
                    type: "GET",
                    dataType: "jsonp",
                    url: this.getJsonUrl(),
                    data: params
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

            /*
            Function: waitAndGetSearchResultsWithRelevance

            Wait while the search is finished and return the results as list of internal ChemSpider IDs with one additional column - relevance. Records are sorted by relevance.

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            rip - Request ID returned from any search functions.
            callback - Callback function that will be called if finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.waitAndGetSearchResultsWithRelevance(rid, function(compounds) {
            //  rid - request ID that was returned from any search function
            //  compounds - list pairs: ChemSpider IDs and Relevance
            });
            (end code)
            */
            waitAndGetSearchResultsWithRelevance: function (rid, callback, error_callback) {
                var oThis = this;

                this.getSearchStatus(rid, function (status) {
                    if (status.Status == 3) {   //  Processing
                        //  ... not ready yet. Check it later again...
                        setTimeout(function () {
                            oThis.waitAndGetSearchResultsWithRelevance(rid, callback, error_callback);
                        }, 1000);
                    }
                    else if (status.Status == 6) {   //  Results Ready
                        oThis.getSearchResultsWithRelevance(rid, callback, error_callback);
                    }
                    else {
                        $('body').showMessage('Error', status.Message);
                    }
                },
                error_callback);
            },

            /*
            Function: getSearchResultAsCompounds

            Returns search results as list of serialized ChemSpider compounds. Be shure that the search already finished and status is ResultReady. See also <waitAndGetSearchResultAsCompounds> function.

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            rip - Request ID returned from any search functions.
            serfilter - Serialization filter. Specify what compound's properties should be included to the response. If this parameter is 'null' then all properties will be serialized.
            callback - Callback function that will be called if finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.getSearchResultAsCompounds(rid, 'Compound[CSID|MF|Name|Mol|Synonyms]', function(compounds) {
            //  rid - request ID that was returned from any search function
            //  Compound[CSID|MF|Name|Mol|Synonyms] - serialize only CSID, Molecular Formula, MOL file and Synonyms list properties
            //  compounds - list of ChemSpider compounds as JSON objects
            });
            (end code)

            Serealization of all compound's properties is pretty slow operation (especially if you have search results with hundreds compounds) and in order to speed up this process 
            we suggest to include to the response only the properties that you really need. You can find the whole list of compound's properties for serialization <here at http://parts.chemspider.com/JSON.ashx#Compound>

            */
            getSearchResultAsCompounds: function (rid, serfilter, callback, error_callback) {
                var oThis = this;

                var params = {
                    'op': 'GetSearchResultAsCompounds',
                    'serfilter': serfilter,
                    'rid': rid
                };

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                $.ajax({
                    dataType: "json",
                    url: this.getJsonUrl(params)
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

            /*
            Function: waitAndGetSearchResultAsCompounds

            Wait while the search is finished and return the results as list of serialized ChemSpider compounds. 

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            rip - Request ID returned from any search functions.
            serfilter - Serialization filter. Specify what compound's properties should be included to the response. If this parameter is 'null' then all properties will be serialized.
            callback - Callback function that will be called if finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.waitAndGetSearchResultAsCompounds(rid, 'Compound[CSID|MF|Name|Mol|Synonyms]', function(compounds) {
            //  rid - request ID that was returned from any search function
            //  Compound[CSID|MF|Name|Mol|Synonyms] - serialize only CSID, Molecular Formula, MOL file and Synonyms list properties
            //  compounds - list of ChemSpider compounds as JSON objects
            });
            (end code)

            Serealization of all compound's properties is pretty slow operation (especially if you have search results with hundreds compounds) and in order to speed up this process 
            we suggest to include to the response only the properties that you really need. You can find the whole list of compound's properties for serialization <here at http://parts.chemspider.com/JSON.ashx#Compound>

            */
            waitAndGetSearchResultAsCompounds: function (rid, serfilter, callback, error_callback) {
                var oThis = this;

                this.getSearchStatus(rid, function (status) {
                    if (status.Status == 3) {   //  Processing
                        //  ... not ready yet. Check it later again...
                        setTimeout(function () {
                            oThis.waitAndGetSearchResultAsCompounds(rid, serfilter, callback, error_callback);
                        }, 1000);
                    }
                    else if (status.Status == 6) {   //  Results Ready
                        oThis.getSearchResultAsCompounds(rid, serfilter, callback, error_callback);
                    }
                    else {
                        $('body').showMessage('Error', status.Message);
                    }
                },
                error_callback);
            },

            /*
            Function: getCompoundInfo

            Returns specifiec ChemSpider compounds properties by compound ID. This function mostly used to get access to the properties assigned to compounds.

            This function doesn't return anything as return parameter, but calls callback function in case of succeess or error_callback in case of any problems. 
            If no error_callback passed to the function, default <onError> handler will be used.

            Parameters:

            csid - ChemSpider ID that was returned from search results for example or already known ID for some reason.
            serfilter - Serialization filter. Specify what compound's properties should be included to the response. If this parameter is 'null' then all properties will be serialized.
            callback - Callback function that will be called if finished successfully
            error_callback - Callback function that will be called if some errors occure

            (start code)
            $.ChemSpider.store.getCompoundInfo(2157, 'Compound[CSID|MF|Name|Mol|Identifiers]', function(compound) {
            //  2157 - ChemSPider ID fro Aspirin compound
            //  Compound[CSID|MF|Name|Mol|Identifiers] - serialize only CSID, Molecular Formula, MOL file and Identifiers list properties
            //  compound - ChemSpider compound as JSON object with requested properties
            });
            (end code)

            Serealization of all compound's properties is pretty slow operation and in order to speed up this process 
            we suggest to include to the response only the properties that you really need. You can find the whole list of compound's properties for serialization <here at http://parts.chemspider.com/JSON.ashx#Compound>
            */
            getCompoundInfo: function (csid, serfilter, callback, error_callback) {
                var oThis = this;

                var params = {
                    'op': 'GetRecordsAsCompounds',
                    'csids[0]': csid,
                    'serfilter': serfilter
                };

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                $.ajax({
                    dataType: "json",
                    url: this.getJsonUrl(params)
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data[0]);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getCompoundInfo", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getCompoundInfo", xhr, error, thrownError);
                });
            },

            getRecordsAsCompounds: function (csids, serfilter, callback, error_callback) {
                var oThis = this;

                var params = {
                    'op': 'GetRecordsAsCompounds',
                    'serfilter': serfilter
                };

                for (var i = 0; i < csids.length; i++) {
                    params['csids[' + i + ']'] = csids[i];
                }

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                $.ajax({
                    dataType: "json",
                    url: this.getJsonUrl(params)
                })
                .done(function (data) {
                    if (callback != null)
                        callback(data);
                })
                .fail(function (xhr, error, thrownError) {
                    if (error_callback != null)
                        error_callback("getRecordsAsCompounds", xhr, error, thrownError);
                    else if (oThis.onError != null)
                        oThis.onError("getRecordsAsCompounds", xhr, error, thrownError);
                });
            },

            //  Direction:  0: Name2Mol
            //              1: Smiles2Mol
            //              2: Name2Smiles
            //              3: Term2Mol
            //              4: InChiKey2CSID
            convertTo: function (direction, text, callback, error_callback) {
                var oThis = this;

                var params = {
                    'op': 'ConvertTo',
                    'convertOptions.Direction': direction,
                    'convertOptions.Text': encodeURIComponent(text)
                };

                if (this.jsonp)
                    $.extend(params, { 'callback': '?' });

                $.ajax({
                    dataType: "json",
                    url: this.getJsonUrl(params)
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

            convertName2Mol: function (text, callback, error_callback) {
                this.convertTo('Name2Mol', text, callback, error_callback);
            },

            convertName2Smiles: function (text, callback, error_callback) {
                this.convertTo('Name2Smiles', text, callback, error_callback);
            },

            convertTerm2Mol: function (text, callback, error_callback) {
                this.convertTo('Term2Mol', text, callback, error_callback);
            },

            convertTerm2Smiles: function (text, callback, error_callback) {
                this.convertTo('Term2SMILES', text, callback, error_callback);
            },

            convertSmiles2Mol: function (text, callback, error_callback) {
                this.convertTo('Smiles2Mol', text, callback, error_callback);
            },

            convertInChiKey2CSID: function (text, callback, error_callback) {
                this.convertTo('InChiKey2CSID', text, callback, error_callback);
            },

            convertInChi2CSID: function (text, callback, error_callback) {
                this.convertTo('InChi2CSID', text, callback, error_callback);
            },

            /*
            Function: getMolImageUrl
            Returns the URL to the 2D image of the molecule with the specific dimentions by ChemSpider ID.

            Parameters:
            csid - ChemSpider compound ID
            width - Image width
            height - Image height

            (start code)
            var url = $.ChemSpider.store.getMolImageUrl(2157, 200, 200);
            (end code)
            */
            getMolImageUrl: function (csid, width, height) {
                return $.ChemSpider.Database.DataHandlerUrl + '/ImagesHandler.ashx?id=' + csid + '&w=' + width + '&h=' + height;
            },

            /*
            Function: getFileUrl
            Returns the URL to the file hosted by ChemSpider.

            Parameters:
            params - parameters that describes file to return. 

            (start code)
            var url = $.ChemSpider.store.getFileUrl({type: 'blob', id: 6182});
            (end code)
            */
            getFileUrl: function (params) {
                return this.getUrl('FilesHandler.ashx', params);
            },

            /*
            Function: getMolImageUrlBySMILES
            Returns the URL to the 2D image of the molecule with the specific dimentions by SMILES.

            Parameters:
            smiles - SMILES of the molecule that should be rendered
            width - Image width
            height - Image height

            (start code)
            var url = $.ChemSpider.store.getMolImageUrlBySMILES('CC(=O)Oc1ccccc1C(=O)O', 200, 200);
            (end code)
            */
            getMolImageUrlBySMILES: function (smiles, width, height) {
                return $.ChemSpider.Database.DataHandlerUrl + '/ImagesHandler.ashx?smiles=' + encodeURIComponent(smiles) + '&w=' + width + '&h=' + height;
            },

            /*
            Function: getMolImageUrlByInchi
            Returns the URL to the 2D image of the molecule with the specific dimentions by SMILES.

            Parameters:
            inchi - InChi of the molecule that should be rendered
            width - Image width
            height - Image height

            (start code)
            var url = $.ChemSpider.store.getMolImageUrlByInchi('InChI=1S/C9H8O4/c1-6(10)13-8-5-3-2-4-7(8)9(11)12/h2-5H,1H3,(H,11,12)', 200, 200);
            (end code)
            */
            getMolImageUrlByInchi: function (inchi, width, height) {
                return $.ChemSpider.Database.DataHandlerUrl + '/ImagesHandler.ashx?inchi=' + encodeURIComponent(inchi) + '&w=' + width + '&h=' + height;
            },

            getMolImageUrlByInchiKey: function (inchi_key, width, height) {
                return $.ChemSpider.Database.DataHandlerUrl + '/ImagesHandler.ashx?inchi_key=' + encodeURIComponent(inchi_key) + '&w=' + width + '&h=' + height;
            },

            /*
            Function: getCompoundUrl
            Returns the URL to the compound page on main ChemSpider website

            Parameters:
            csid - ChemSpider compound ID.

            (start code)
            var url = $.ChemSpider.store.getCompoundUrl(2157);
            (end)
            */
            getCompoundUrl: function (csid) {
                return $.ChemSpider.Web.Url + '/Chemical-Structure.' + csid + '.html';
            },

            getVersion: function () {
                return this.version;
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
        };
    };

}(jQuery));

/*
Class: ChemSpider

Variable: $.ChemSpider.store
Global variable that allows to get access to the ChemSpider.Database.Store object from any place of code without initialisation. 
*/
$.ChemSpider.store = $.ChemSpider.Database.Store();
