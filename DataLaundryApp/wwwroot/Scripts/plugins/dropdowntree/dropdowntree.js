var dropDownOptions = {
    title: "Dropdown",
    data: [],
    closedArrow: '<i class="fa fa-caret-right" aria-hidden="true"></i>',
    openedArrow: '<i class="fa fa-caret-down" aria-hidden="true"></i>',
    maxHeight: 300,
    multiSelect: false,
    selectChildren: false,
    addChildren: false,
    clickHandler: function (target) { },
    expandHandler: function (target, expanded) { },
    checkHandler: function (target, checked) { },
    rtl: false,
    IsFilter: false,
    IsClickToOpenChild: false,
    IsClickToOpenAllChild: false
};

var globalTreeIdCounter = 0;

(function ($) {

    //data inits from options
    $.fn.DropDownTree = function (options) {
        //helpers
        function RenderData(data, element) {
            for (var i = 0 ; i < data.length ; i++) {
                globalTreeIdCounter++;
                var dataAttrs = "";
                var IsMatchCss = "";
                if (typeof data[i].dataAttrs != "undefined" && data[i].dataAttrs != null) {
                    for (var d = 0 ; d < data[i].dataAttrs.length ; d++) {
                        dataAttrs += " data-" + data[i].dataAttrs[d].title + "='" + data[i].dataAttrs[d].data + "' ";
                    }
                }
                if (typeof data[i].style != undefined && data[i].style != null) {
                    for (var d = 0 ; d < data[i].style.length ; d++) {
                        IsMatchCss = data[i].style;
                    }
                }
                if (!element.is("li")) {
                    // element.append('<li id="TreeElement'+globalTreeIdCounter+'"'+dataAttrs+'>'+(options.multiSelect?'<i class="fa fa-square-o select-box" aria-hidden="true"></i>':'')+'<a href="'+((typeof data[i].href != "undefined" && data[i].href!=null)?data[i].href:'#')+'">'+data[i].title+'</a></li>');
                    //element.append('<li id="TreeElement' + globalTreeIdCounter + '"' + dataAttrs + '>' + (options.multiSelect ? '<i class="fa fa-square-o select-box" aria-hidden="true"></i>' : '') + '<a href="' + ((typeof data[i].href != "undefined" && data[i].href != null) ? data[i].href : '#') + '">' + data[i].title + '</a></li>');

                    element.append('<li id="TreeElement' + globalTreeIdCounter + '"' + dataAttrs + ' style="' + IsMatchCss + '">' + (options.multiSelect ? '<i class="fa fa-square-o select-box" aria-hidden="true"></i>' : '') + '<a href="javascript:void(0);">' + data[i].title + '</a></li>');

                    if (data[i].data != null && typeof data[i].data != "undefined") {
                        $("#TreeElement" + globalTreeIdCounter).append("<ul style='display:none'></ul>");
                        $("#TreeElement" + globalTreeIdCounter).find("a").first().prepend('<span class="arrow">' + options.closedArrow + '</span>');
                        RenderData(data[i].data, $("#TreeElement" + globalTreeIdCounter).find("ul").first());
                    } else if (options.addChildren) {
                        $("#TreeElement" + globalTreeIdCounter).find("a").first().prepend('<span class="arrow">' + options.closedArrow + '</span>');
                    }
                }
                else {
                    //element.find("ul").append('<li id="TreeElement'+globalTreeIdCounter+'"'+dataAttrs+'>'+(options.multiSelect?'<i class="fa fa-square-o select-box" aria-hidden="true"></i>':'')+'<a href="'+((typeof data[i].href != "undefined" && data[i].href!=null)?data[i].href:'#')+'">'+data[i].title+'</a></li>');
                    element.find("ul").append('<li style="' + IsMatchCss + '" id="TreeElement' + globalTreeIdCounter + '"' + dataAttrs + '>' + (options.multiSelect ? '<i class="fa fa-square-o select-box" aria-hidden="true"></i>' : '') + '<a href="javascript:void(0);">' + data[i].title + '</a></li>');
                    if (data[i].data != null && typeof data[i].data != "undefined") {
                        $("#TreeElement" + globalTreeIdCounter).append("<ul style='display:none'></ul>");
                        $("#TreeElement" + globalTreeIdCounter).find("a").first().prepend('<span class="arrow">' + options.closedArrow + '</span>');
                        RenderData(data[i].data, $("#TreeElement" + globalTreeIdCounter).find("ul").first());
                    } else if (options.addChildren) {
                        $("#TreeElement" + globalTreeIdCounter).find("a").first().prepend('<span class="arrow">' + options.closedArrow + '</span>');
                    }
                }
            }
        }

        options = $.extend({}, dropDownOptions, options, { element: this });


        //protos inits
        $(options.element).init.prototype.clickedElement = null;

        var tree = $(options.element);

        //handlers binders
        //element click handler
        $(options.element).on("click", "li", function (e) {
            tree.init.prototype.clickedElement = $(this);
            options.clickHandler(tree.clickedElement, e);

            if (options.IsClickToOpenChild || options.IsClickToOpenAllChild) {
                /*Check child exists or not*/
                var IsChild = $(this).find("span.arrow");
                if (IsChild.length > 0) 
                {
                    if (!options.IsClickToOpenAllChild)
                        $(this).find("span.arrow:first").click();
                    else 
                        $(this).find("span.arrow").click();                   

                } 
                else 
                {
                    if ($(options.element).hasClass("open"))
                        $(options.element).removeClass("open");
                }
                /*End*/
            }
            e.stopPropagation();
        });

        //arrow click handler close/open
        $(options.element).on("click", ".arrow", function (e) {
            e.stopPropagation();
            $(this).empty();
            var expanded;
            if ($(this).parents("li").first().find("ul").first().is(":visible")) {
                expanded = false;
                $(this).prepend(options.closedArrow);
                $(this).parents("li").first().find("ul").first().hide();
            } else {
                expanded = true;
                $(this).prepend(options.openedArrow);
                $(this).parents("li").first().find("ul").first().show();
            }
            options.expandHandler($(this).parents("li").first(), e, expanded);
        });
        //37 - 39 left arrow and right arrow key dwon
        //$(options.element).on("keydown", "ul > li.jsChildElement", function (e) {
        //    if (e.keyCode == 37 || e.keyCode == 39)
        //    {               
        //        if ($(this).find("a:first").find("span:first").hasClass("arrow"))
        //        {
        //            $(this).find("a:first").find("span:first.arrow").trigger("click");
        //        }
        //    }
        //});
        //$(options.element).on("keydown", "ul > li.jsChildElement > ul > li", function (e)
        //{
        //    console.log("second");
        //    if (e.keyCode == 37 || e.keyCode == 39) {
        //        if ($(this).find("a:first").find("span:first").hasClass("arrow")) {
        //            $(this).find("a:first").find("span:first.arrow").trigger("click");
        //        }
        //    }
        //});

        //select box click handler
        $(options.element).on("click", ".select-box", function (e) {
            e.stopPropagation();
            var checked;
            if ($(this).hasClass("fa-square-o")) {
                //will select
                checked = true;
                $(this).removeClass("fa-square-o");
                $(this).addClass("fa-check-square-o");
                if (options.selectChildren) {
                    $(this).parents("li").first().find(".select-box").removeClass("fa-square-o");
                    $(this).parents("li").first().find(".select-box").addClass("fa-check-square-o");
                }
            } else {
                //will unselect
                checked = false;
                $(this).addClass("fa-square-o");
                $(this).removeClass("fa-check-square-o");
                if (options.selectChildren) {
                    $(this).parents("li").first().find(".select-box").addClass("fa-square-o");
                    $(this).parents("li").first().find(".select-box").removeClass("fa-check-square-o");
                    $(this).parents("li").each(function () {
                        $(this).find(".select-box").first().removeClass("fa-check-square-o");
                        $(this).find(".select-box").first().addClass("fa-square-o");
                    });
                }
            }
            options.checkHandler($(this).parents("li").first(), e, checked);
        });

        if (options.rtl) {
            $(options.element).addClass("rtl-dropdown-tree");
            if (options.closedArrow.indexOf("fa-caret-right") > -1) {
                options.closedArrow = options.closedArrow.replace("fa-caret-right", "fa-caret-left");
            }
        }
        $(options.element).append('<button id="btn-' + (Math.floor((Math.random() * 1000000) + 10)) + '"  style="width: 267px;text-align:left;" class="btn btn-default dropdown-toggle jsSharedButton" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true"><span class="dropdowntree-name">' + options.title + '</span><span style="float:right;margin-top:8px;" class="caret"></span></button>');
        $(options.element).append('<ul style="max-height: ' + options.maxHeight + 'px;" class="dropdown-menu" aria-labelledby="dropdownMenu1"></ul>');

        /*Filter set*/
        if (options.IsFilter) {
            $(options.element).find("ul").first().append('<input oninput="listFilter(this)" type="text" class="form-control" style="width: 100%;" placeholder="Type to filter">');
            $(options.element).find("ul").first().append("<li id='jsCommanMessageli' style='display:none;padding: 10px;'><span id='jsSpanMessage'></span></li>");
        }
        RenderData(options.data, $(options.element).find("ul").first());



        //protos inits
        $(options.element).init.prototype.GetParents = function () {
            var jqueryClickedElement = $(options.element).clickedElement;
            return $(jqueryClickedElement).parents("li");
        };

        $(options.element).init.prototype.SetTitle = function (title) {
            $(this).find(".dropdowntree-name").text(title);
        };

        $(options.element).init.prototype.GetSelected = function (title) {
            var selectedElements = [];
            $(this).find(".fa-check-square-o").each(function () {
                selectedElements.push($(this).parents("li").first());
            });
            return selectedElements;
        };

        $(options.element).init.prototype.AddChildren = function (element, arrOfElements) {
            if (options.addChildren && $(element).find("ul").length == 0)
                $(element).append("<ul></ul>");
            element = $(element).find("ul").first();
            if (element.find("li").length == 0)
                RenderData(arrOfElements, element);
        };

    };
})(jQuery);

function listFilter(input) {
    jQuery.expr[':'].contains = function (a, i, m) {
        return jQuery(a).text().toUpperCase().indexOf(m[3].toUpperCase()) >= 0;
    };
    //console.log("Start time " + Date());
    var filter = $(input).val();
    if (filter) {
        $($(input).parent()).find("li:not(:Contains(" + filter + "))").hide();
        $($(input).parent()).find("li:Contains(" + filter + ")").show();
        var oFindLegth = $($(input).parent()).find("li:Contains(" + filter + ")").length;
        //console.log("oFindLegth " + oFindLegth);
        if (oFindLegth == 0 || oFindLegth == 1) {
            $(input).parent().find("li#jsCommanMessageli > span").html("No found '" + filter + "'");
            $(input).parent().find("li#jsCommanMessageli").show();
        }
        else {
            $(input).parent().find("li#jsCommanMessageli").hide()
        }
    }
    else {
        $($(input).parent()).find("li").show();
        $(input).parent().find("li#jsCommanMessageli").hide();
    }
    $($(input).parent()).removeHighlight().highlight(filter);
    //console.log("End time " + Date());
}
//highlight
jQuery.fn.highlight = function (pat) {
    function innerHighlight(node, pat) {
        var skip = 0;
        //console.log(node);
        if (node.nodeType == 3) {
            var pos = node.data.toUpperCase().indexOf(pat);
            pos -= (node.data.substr(0, pos).toUpperCase().length - node.data.substr(0, pos).length);
            if (pos >= 0) {
                var spannode = document.createElement('span');
                spannode.className = 'highlight';
                var middlebit = node.splitText(pos);
                var endbit = middlebit.splitText(pat.length);
                var middleclone = middlebit.cloneNode(true);
                spannode.appendChild(middleclone);
                middlebit.parentNode.replaceChild(spannode, middlebit);
                skip = 1;
            }
        }
        else if (node.nodeType == 1 && node.childNodes && !/(script|style)/i.test(node.tagName)) {
            for (var i = 0; i < node.childNodes.length; ++i) {
                i += innerHighlight(node.childNodes[i], pat);
            }
        }
        return skip;
    }
    return this.length && pat && pat.length ? this.each(function () {
        innerHighlight(this, pat.toUpperCase());
    }) : this;
};

jQuery.fn.removeHighlight = function () {
    return this.find("span.highlight").each(function () {
        this.parentNode.firstChild.nodeName;
        with (this.parentNode) {
            replaceChild(this.firstChild, this);
            normalize();
        }
    }).end();
};