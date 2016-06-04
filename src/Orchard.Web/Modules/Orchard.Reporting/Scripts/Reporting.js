/*
Project: OrcharCRM
Developer: Siyamand Ayubi
2015
*/
(function () {
    var crm = crm || {};

    function jqueryPost(action, method, input) {
        "use strict";
        var form;
        form = $('<form />', {
            action: action,
            method: method,
            style: 'display: none;'
        });

        var convertObject = function (objName, objValue) {
            if (typeof objValue === 'object') {

                if (objValue instanceof Array) {
                    var counter = 0;
                    $.each(objValue, function (name, value) {
                        convertObject(objName + '[' + counter + ']', value);
                        counter++;
                    });
                }
                else {
                    for (var name in objValue) {
                        if (objValue.hasOwnProperty(name)) {
                            convertObject(objName + "." + name, objValue[name]);
                        }
                    }
                }
            }
            else {
                $('<input />', {
                    type: 'hidden',
                    name: objName,
                    value: objValue
                }).appendTo(form);
            }
        }

        if (typeof input !== 'undefined') {
            $.each(input, convertObject);
        }

        form.appendTo('body').submit();
    };

    crm.Reporting = function (widget) {

        var _self = this;
        var mainPlot;
        var subPlots = [];

        this.options = {
            groupHiddenFieldId: "reportData",
            firstLevelChartContainerId: "firstLevelChartContainer",
            firstLevelChartId: "firstLevelChart",
            secondLevelChartsContainerId: "secondLevelCharts",
            agentsSelectBoxId: "Users",
            businessUnitsSelectBoxId: "BusinessUnits"
        };

        this.initialize = function () {

            renderReports();
        };

        var renderReports = function () {
            var reportDataJson = widget.element.find("input[name='reportData']").val();
            var groupData = jQuery.parseJSON(reportDataJson);

            if (groupData.length > 0) {
                var chart = widget.element.find(".chart");
                chart.removeClass("hidden");

                mainPlot = createPlot(groupData, chart.attr("id"));
            }
        };

        var createPlot = function (groupData, id) {
            var data = [];
            for (var i = 0; i < groupData.length; i++) {
                data.push([groupData[i].Label, groupData[i].Value]);
            }

            return jQuery.jqplot(id, [data], {
                seriesDefaults: {
                    // Make this a pie chart.
                    renderer: jQuery.jqplot.PieRenderer,
                    rendererOptions: {
                        // Put data labels on the pie slices.
                        // By default, labels show the percentage of the slice.
                        showDataLabels: true
                    }
                },
                legend: { show: true, location: 'e' }
            }
             );
        };
    };

    $.widget("CRM.Reporting", {
        options: {},

        _create: function () {
            this.reportingWidget = new crm.Reporting(this);
            $.extend(this.reportingWidget.options, this.options);
            this.reportingWidget.initialize();
        }
    });
})();