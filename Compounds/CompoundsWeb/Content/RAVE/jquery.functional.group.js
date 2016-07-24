/*
 * jQuery functionalgroup widget 1.1.0
 * Depends:
 *	jquery-ui-1.8.20.min.js
 *	jquery.ui.widget.js
 */

$(function () {
    var data = [
  {
      "groups": [{
          "groups": [{
              "groups": null,
              "isExpandable": true,
              "items": [{
                  "name": "met",
                  "title": "<strong>Total methyl groups (includes methoxyl. N-methyl, S-methyl and acetyl)</strong>"
              }]
          },
                    {
                        "groups": null,
                        "isExpandable": false,
                        "items": [{
                            "name": "mets",
                            "title": "Singlet methyl <br />( R<sub>3</sub>C-CH<sub>3</sub> )"
                        },
                            {
                                "name": "metd",
                                "title": "Doublet methyl <br />( R<sub>2</sub>HC-CH<sub>3</sub> )"
                            },
                            {
                                "name": "mett",
                                "title": " Triplet methyl <br>(RH<sub>2</sub>C-CH<sub>3</sub>)"
                            }
                        ]
                    },
                    {
                        "groups": null,
                        "isExpandable": false,
                        "items": [{
                            "name": "arome",
                            "title": "Aromatic methyl"
                        },
                            {
                                "name": "vnme",
                                "title": "Vinyl methyl<br />( C=CH-CH<sub>3</sub> )"
                            },
                            {
                                "name": "acy",
                                "title": "Acetyl (including acetyl<br />ester and acetyl amide)"
                            }
                        ]
                    },
                    {
                        "groups": null,
                        "isExpandable": false,
                        "items": [{
                            "name": "meo",
                            "title": "Methoxy<br />( -OCH<sub>3</sub> )"
                        },
                            {
                                "name": "men",
                                "title": "N-methyl<br />( -NCH<sub>3</sub> )"
                            },
                            {
                                "name": "mes",
                                "title": "S-methyl<br>(-SCH<sub>3</sub>)"
                            }
                        ]
                    }
          ],
          "isExpandable": false,
          "items": [{
              "name": "",
              "title": "sp3 carbons"
          }]
      },
          {
              "groups": [{
                  "groups": null,
                  "isExpandable": false,
                  "items": [{
                      "name": "mene",
                      "title": "<strong>Total methylene (secondary) carbons</strong>"
                  },
                      {
                          "name": "mine",
                          "title": "<strong>Total methine (tertiary) carbons</strong>"
                      }
                  ]
              }],
              "isExpandable": false,
              "items": null
          },
          {
              "groups": [{
                  "groups": null,
                  "isExpandable": false,
                  "items": [{
                      "name": "sp2h",
                      "title": "<strong>Total sp<sup>2</sup>C–H</strong>"
                  },
                          {
                              "name": "",
                              "title": " "
                          },
                          {
                              "name": "",
                              "title": " "
                          }]
              },
                  {
                      "groups": null,
                      "isExpandable": true,
                      "items": [{
                          "name": "cc",
                          "title": "<strong>Total C=C (excludes benzene and pyridine rings)</strong>"
                      },
                      ]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "vn",
                          "title": "Vinyl group <br />(H<sub>2</sub>C=CH–)"
                      },
                          {
                              "name": "",
                              "title": " "
                          },
                          {
                              "name": "",
                              "title": " "
                          }
                      ]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "cch",
                          "title": "1,1-disubstituted"
                      },
                          {
                              "name": "dic",
                              "title": "1,2-disubstituted"
                          },
                          {
                              "name": "",
                              "title": " "
                          }
                      ]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "cc3",
                          "title": "Trisubstituted"
                      },
                          {
                              "name": "",
                              "title": " "
                          },
                          {
                              "name": "",
                              "title": " "
                          }
                      ]
                  }
              ],
              "isExpandable": false,
              "items": [{
                  "name": "",
                  "title": "sp<sup>2</sup> carbons"
              }]
          },
          {
              "groups": [{
                  "groups": null,
                  "isExpandable": false,
                  "items": [{
                      "name": "tlk",
                      "title": "<strong>Total terminal alkynes (–C≡C–H)</strong>"
                  }]
              }],
              "isExpandable": false,
              "items": [{
                  "name": null,
                  "title": "sp carbons"
              }]
          }
      ],
      "isExpandable": true,
      "items": [{
          "name": "",
          "title": "Carbon skeleton"
      }]
  },
  {
      "groups": [{
          "groups": [{
              "groups": null,
              "isExpandable": true,
              "items": [{
                  "name": "cao",
                  "title": "<strong>Total C-O (excluding acids, esters and lactones)</strong>"
              }
              ]
          },
                      {
                          "groups": null,
                          "isExpandable": false,
                          "items": [{
                              "name": "c2ho",
                              "title": "Primary C-O (-CH<sub>2</sub>-O-)"
                          },
                              {
                                  "name": "c1ho",
                                  "title": "Secondary C-O (>CH-O-)"
                              },
                              {
                                  "name": null,
                                  "title": " "
                              }
                          ]
                      },
                      {
                          "groups": null,
                          "isExpandable": false,
                          "items": [{
                              "name": "ch2o2",
                              "title": "Primary acetal"
                          },
                              {
                                  "name": "cho2",
                                  "title": "Secondary acetal"
                              },
                              {
                                  "name": "co2",
                                  "title": "Tertiary acetal"
                              }
                          ]
                      }
          ],
          "isExpandable": false,
          "items": [{
              "name": null,
              "title": "C-O bonds"
          }]
      },
          {
              "groups": [{
                  "groups": null,
                  "isExpandable": true,
                  "items": [{
                      "name": "co",
                      "title": "<strong>Total C=O</strong>"
                  },
                  {
                      "name": "",
                      "title": ""
                  },
                  {
                      "name": "",
                      "title": ""
                  },
                  ]
              },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "cho",
                          "title": " Aldehyde (including<br /> formyl)"
                      },
                          {
                              "name": "aco",
                              "title": " Acid, ester, lactone"
                          },
                          {
                              "name": "am",
                              "title": " Amide"
                          }
                      ]
                  }
              ],
              "isExpandable": false,
              "items": [{
                  "name": null,
                  "title": "C=O bonds"
              }]
          },
          {
              "groups": [{
                  "groups": null,
                  "isExpandable": false,
                  "items": [{
                      "name": "chn",
                      "title": "Imine"
                  },
                      {
                          "name": "nit",
                          "title": "Nitrile"
                      },
                      {
                          "name": "initr",
                          "title": "Isonitrile"
                      }
                  ]
              }],
              "isExpandable": false,
              "items": [{
                  "name": null,
                  "title": "C=N and C≡N bonds"
              }]
          }
      ],
      "isExpandable": true,
      "items": [{
          "name": null,
          "title": "Functional groups containing heteroatoms"
      }]
  },
  {
      "groups": [{
          "groups": [{
              "groups": null,
              "isExpandable": true,
              "items": [{
                  "name": "ben",
                  "title": "<strong>Total benzenes</strong>"
              },
                  {
                      "name": "",
                      "title": " "
                  },
                  {
                      "name": "",
                      "title": " "
                  }
              ]
          },
                      {
                          "groups": null,
                          "isExpandable": false,
                          "items": [{
                              "name": "b1",
                              "title": "1-substituted"
                          },
                              {
                                  "name": "",
                                  "title": " "
                              },
                              {
                                  "name": "",
                                  "title": " "
                              }
                          ]
                      },
                      {
                          "groups": null,
                          "isExpandable": false,
                          "items": [{
                              "name": "b12",
                              "title": "1,2-substituted"
                          },
                              {
                                  "name": "b13",
                                  "title": "1,3-substituted"
                              },
                              {
                                  "name": "b14",
                                  "title": "1,4-substituted"
                              }
                          ]
                      },
                      {
                          "groups": null,
                          "isExpandable": false,
                          "items": [{
                              "name": "b123",
                              "title": "1,2,3-substituted"
                          },
                              {
                                  "name": "b124",
                                  "title": "1,2,4-substituted"
                              },
                              {
                                  "name": "b135",
                                  "title": "1,3,5-substituted"
                              }
                          ]
                      },
                      {
                          "groups": null,
                          "isExpandable": false,
                          "items": [{
                              "name": "b1234",
                              "title": "1,2,3,4-substituted"
                          },
                              {
                                  "name": "b1235",
                                  "title": " 1,2,3,5-substituted"
                              },
                              {
                                  "name": "b1245",
                                  "title": " 1,2,4,5-substituted"
                              }
                          ]
                      },
                      {
                          "groups": null,
                          "isExpandable": false,
                          "items": [{
                              "name": "b12345",
                              "title": " 1,2,3,4,5-substituted"
                          },
                              {
                                  "name": "b1_6",
                                  "title": "1,2,3,4,5,6-substituted"
                              },
                              {
                                  "name": null,
                                  "title": " "
                              }
                          ]
                      }
          ],
          "isExpandable": false,
          "items": [{
              "name": null,
              "title": "Benzenes"
          }]
      },
          {
              "groups": [
                  {
                      "groups": null,
                      "isExpandable": true,
                      "items": [{
                          "name": "",
                          "title": ""
                      }]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "p2",
                          "title": "2-substituted"
                      },
                          {
                              "name": "p3",
                              "title": " 3-substituted"
                          },
                          {
                              "name": "p4",
                              "title": " 4-substituted"
                          }
                      ]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "p23",
                          "title": "2,3-substituted"
                      },
                          {
                              "name": "p24",
                              "title": "2,4-substituted"
                          },
                          {
                              "name": "p25",
                              "title": "2,5-substituted"
                          }
                      ]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "p26",
                          "title": "2,6-substituted"
                      },
                          {
                              "name": "p34",
                              "title": "3,4-substituted"
                          },
                          {
                              "name": "p35",
                              "title": "3,5-substituted"
                          }
                      ]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "p234",
                          "title": "2,3,4-substituted"
                      },
                          {
                              "name": "p235",
                              "title": "2,3,5-substituted"
                          },
                          {
                              "name": "p236",
                              "title": " 2,3,6-substituted"
                          }
                      ]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "p246",
                          "title": " 2,4,6-substituted"
                      },
                          {
                              "name": "p345",
                              "title": " 3,4,5-substituted"
                          },
                          {
                              "name": null,
                              "title": " "
                          }
                      ]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "p2345",
                          "title": "2,3,4,5-substituted"
                      },
                          {
                              "name": "p2346",
                              "title": "2,3,4,6-substituted"
                          },
                          {
                              "name": "p2356",
                              "title": "2,3,5,6-substituted"
                          }
                      ]
                  },
                  {
                      "groups": null,
                      "isExpandable": false,
                      "items": [{
                          "name": "p2_6",
                          "title": "2,3,4,5,6-substituted"
                      },
                          {
                              "name": null,
                              "title": " "
                          },
                          {
                              "name": null,
                              "title": " "
                          }
                      ]
                  }
              ],
              "isExpandable": false,
              "items": [{
                  "name": null,
                  "title": "Pyridines"
              }]
          }
      ],
      "isExpandable": true,
      "items": [{
          "name": "",
          "title": "Benzene and pyridine rings"
      }]
  }
    ];

    $.widget("fg.functionalGroup", {
        version: "1.1.0",
        // default options
        options: {
            guidelines: null,
            guidelinesText: null,
            width: 800,
            height: 600,
        },
        _create: function () {
            this._createGuideLines(this.element);
            this.element.append(this._builddata());
            this.registerEvents(this);
        },
        _builddata: function () {
            var groups = $(document.createElement('div')).attr("id", 'functional-groups');
            for (var i = 0; i < data.length; i++) {
                var group = data[i];
                var groupHTML = $(document.createElement('div')).addClass('fg-header-block');
                if (group.items != null) {
                    for (var a = 0; a < group.items.length; a++) {
                        groupHTML.append(this._createHeader(group.items[a].title, group.isExpandable))
                    }
                }
                for (var j = 0; j < group.groups.length; j++) {
                    var subgroup = group.groups[j];
                    var subGroupHtml = $(document.createElement('div'));
                    var subItemContent = $(document.createElement('div')).addClass('fg-content-block-branding').append();
                    if (subgroup.items != null) {
                        for (var a = 0; a < subgroup.items.length; a++) {
                            subGroupHtml.append(this._createSubHeader(subgroup.items[a].title))
                        }
                    }
                    for (var k = 0; k < subgroup.groups.length; k++) {
                        var item = subgroup.groups[k];
                        var itemGroupHtml = $(document.createElement('div')).addClass("fg-row fg-full-size fg-margin-b10 fg-margin-t10 ");
                        if (item.isExpandable)
                            itemGroupHtml.append(this._createExpandCollapseBtn("fg-img-expand"));
                        if (item.items != null) {
                            for (var a = 0; a < item.items.length; a++) {
                                var groupItem = item.items[a];
                                if (item.items.length == 3)
                                    var col = this._createDiv('fg-col-3-part fg-pull-left');
                                if (item.items.length == 2)
                                    var col = this._createDiv('fg-col-2-part fg-pull-left');
                                if (item.items.length == 1)
                                    var col = this._createDiv('fg-col-1-part fg-pull-left');
                                this._createDiv('fg-pull-left fg-text-col').html(groupItem.title).appendTo(col);
                                if (groupItem.name != null && groupItem.name.length > 0)
                                    this._createTextbox(groupItem.name, '', '').appendTo(col);

                                itemGroupHtml.append(col);
                            }
                        }
                        subItemContent.append(itemGroupHtml);
                    }
                    subGroupHtml.append(subItemContent);
                    groupHTML.append(subGroupHtml);
                }
                groups.append(groupHTML);
            }
            return groups;
        },
        _createHeader: function (headerText, isExpandable) {
            var header = "";
            if (isExpandable) {
                header = $(document.createElement('div'))
                            .addClass('fg-head fg-bg-gray')
                            .append($(document.createElement('div'))
                            .addClass('fg-col-10 fg-pull-left')
                            .html($(document.createElement('h4'))
                            .addClass('fg-pull-left')
                            .html(headerText)))
                            .append(this._createExpandCollapseBtn("fg-img-collapse"));
            }
            else {
                header = $(document.createElement('div'))
                            .addClass('fg-head fg-bg-gray')
                            .append($(document.createElement('div'))
                            .addClass('fg-col-10 fg-pull-left'))
                            .append($(document.createElement('h4'))
                            .addClass('fg-pull-left').html(headerText));
            }
            return header;
        },
        _createSubHeader: function (subHeaderText) {
            return $(document.createElement('div'))
                    .addClass('fg-sub-header')
                    .html(subHeaderText);
        },
        _createTextbox: function (name, value, cssClass) {
            var fgTextBox = $(document.createElement('input'))
                .addClass('fg-text-box isFGValideRange fg-group-text-box')
                .addClass(cssClass)
                .addClass('isFGValideRange')
                .attr({
                        type: "text",
                        id: name
                    })
                .val(value);
            return fgTextBox;
        },
        _createDiv: function (cssClass) {
            return $(document.createElement('div'))
                .addClass(cssClass);
        },
        _createExpandCollapseBtn: function (css) {
            return $(document.createElement('div'))
                .addClass('fg-pull-right fgEC')
                .addClass(css);
        },
        _createGuideLines: function (element) {
            if (this.options.guidelines != '' || this.options.guidelinesText != '') {
                var guideline = $(document.createElement('div')).addClass('fg-head fg-bg-yellow').appendTo(element);
                $(document.createElement('h4'))
                    .addClass('fg-padding-b10')
                    .html(this.options.guidelines)
                    .appendTo(guideline);
                $(document.createElement('p'))
                    .html(this.options.guidelinesText)
                    .appendTo(guideline);
            }
        },
        registerEvents: function (element) {
            if (this.options.width <= 600) {
                $(".fg-col-3-part ").css("width", "85%").css("padding-bottom", "10px");
                $(".fg-col-2-part").css("width", "90%").css("padding-bottom", "10px");
                $(".fg-line-height13 ").css("line-height", "20px");
            }
            $(".fg-header-block").first().find(".fg-head").find(".fgEC").removeClass('fg-img-collapse').addClass('fg-img-expand');
            $('.fgEC').each(function (e) {
                if ($(this).hasClass('fg-img-collapse')) {
                    $(this).parent('div').siblings('').slideUp(100);
                }
            });
            $('.isFGValideRange').on("focusout", function (event) {
                if (validateRange($(this).val()) == true || validateNumbers($(this).val()) == true || $(this).val() == '') {
                    setCSSForInputBox('valid', $(this));
                    $('#inline_error_msg_box').remove();
                }
                else {
                    displayErrorMessage($(this), "Please enter a valid range or integer value.")
                    $(this).val("");
                    setCSSForInputBox('invalid', $(this));
                    $(this).focus();
                    return false;
                }
            });
            $(".fgEC").on("click", function (e) {
                if ($(this).hasClass('fg-img-expand')) {
                    $(this).addClass('fg-img-collapse').removeClass('fg-img-expand').parent('div').siblings().slideUp(100);
                }
                else if ($(this).hasClass('fg-img-collapse')) {
                    $(this).removeClass('fg-img-collapse').addClass('fg-img-expand').parent('div').siblings('').slideDown(100);
                }
                return false;
            });
            //Method for checking is field is containinfg range or not
            function validateRange(value) {
                var isValid = false;
                var rangeArray = value.split('-');
                if (rangeArray.length == 2) {
                    if (ValidateNumbers(rangeArray[0]) && ValidateNumbers(rangeArray[1])) {
                        if (parseInt(rangeArray[1]) > parseInt(rangeArray[0])) {
                            isValid = true;
                        }
                    }
                }
                return isValid;
            }
            //Method for checking, number is integer or not
            function validateNumbers(value) {
                if (value == "")
                    return true;
                if (value.match(/^[0-9]+$/)) {
                    return true;
                }
                else {
                    return false;
                }
            }
            //
            function setCSSForInputBox(state, thisInst) {
                $('.fg-inline-validation-border-color-invalid')
                    .removeClass('fg-inline-validation-border-color-invalid');
                if (state == "valid") {
                    thisInst.removeClass('fg-inline-validation-border-color-invalid')
                        .addClass('fg-inline-validation-border-color-valid');
                }
                else {
                    thisInst.removeClass('fg-inline-validation-border-color-valid')
                        .addClass('fg-inline-validation-border-color-invalid');
                }
            }
            //Method for generating display error text
            function displayErrorMessage(thisInst, msg) {
                var position = thisInst.position();
                $('#inline_error_msg_box').remove();
                $("<small id='inline_error_msg_box' class='fg-inline-validation-errorMsg multiLine' style='position: absolute;'>" + msg + "</small>")
                    .insertAfter(thisInst);
                $("#inline_error_msg_box")
                    .css("left", position.left)
                    .css("top", position.top);
            }
        },
        //private method for processing functional group data
        _getFunctionalGroups: function (isQuerystringFormat) {
            var searchItems = [];
            var queryString = "";
            $('.fg-group-text-box').each(function () {
                if (this.getAttribute("id") != null && this.getAttribute("id") != "undefined" && this.getAttribute("id") != "") {
                    if (this.value != "undefined" && this.value != "null" && this.value != "") {
                        if (isQuerystringFormat) {
                            queryString = queryString + this.getAttribute("id") + "=" + this.value + "&";
                        }
                        else {
                            var searchItem = { key: this.getAttribute("id"), value: this.value };
                            searchItems.push(searchItem);
                        }
                    }
                }
            });
            if (isQuerystringFormat) {
                return queryString;
            }
            else {
                return searchItems;
            }
        },
        //Public method for getting funtional group data
        getFunctionalGroupSearchParams: function () {
            return this._getFunctionalGroups();
        },
        getFunctionalGroupSearchParamsAsQueryString: function () {
            return this._getFunctionalGroups(true);
        },
    });
}(jQuery));