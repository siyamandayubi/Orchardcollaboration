/*
Project: OrchardCollaboration
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

        var chartTypes = {
            piechart: 1,
            barChart: 3,
            dateAxis: 4
        };

        var _self = this;
        var mainPlot;
        var subPlots = [];

        this.options = {
            chartType: null,
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

            if (groupData.Data.length > 0) {
                var chart = widget.element.find(".chart");
                chart.removeClass("hidden");

                mainPlot = createPlot(groupData, chart.attr("id"));
            }
        };

        var createPlot = function (groupData, id) {
            var twoDimensionalArray = [];
            var labels = [];
            var values = [];
            for (var i = 0; i < groupData.Data.length; i++) {
                labels.push(groupData.Data[i].Label);
                values.push(groupData.Data[i].Value);
                twoDimensionalArray.push([groupData.Data[i].Label, groupData.Data[i].Value]);
            }

            var maxValue = Math.max.apply(this, values);
            maxValue = maxValue ? Math.ceil(maxValue * 3 / 2) : 0;
            var minValue = Math.min.apply(this, values);
            minValue = minValue < 0 ? minValue : 0;

            var chartType = _self.options.chartType || chartTypes.piechart;

            switch (chartType) {
                case chartTypes.piechart:
                    return jQuery.jqplot(id, [twoDimensionalArray], {
                        title: groupData.Title,
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
                    });
                case chartTypes.barChart:
                    return jQuery.jqplot(id, [values], {
                        animate: !$.jqplot.use_excanvas,
                        title: groupData.Title,
                        seriesDefaults: {
                            // Make this a bar chart.
                            renderer: jQuery.jqplot.BarRenderer,
                            pointLabels: { show: true },
                            rendererOptions: {
                                barDirection: 'horizontal',
                                barWidth: 20,
                            }
                        },
                        axes: {
                            yaxis: {
                                renderer: $.jqplot.CategoryAxisRenderer,
                                ticks: labels
                            },
                            xaxis: {
                                numberTicks: 1,
                                min: minValue,
                                max: maxValue
                            }
                        },
                        highlighter: { show: false }
                    });
                case chartTypes.dateAxis:
                    return jQuery.jqplot(id, [twoDimensionalArray], {
                        title: groupData.Title,
                        seriesDefaults: {
                            // Make this a dateAxis renderer.
                            //renderer: jQuery.jqplot.DateAxisRenderer,
                            rendererOptions: {
                                // Put data labels on the bars
                                // By default, labels show the percentage of the slice.
                                showDataLabels: true
                            }
                        },
                        series: [{ label: groupData.SeriesName }],
                        axes: {
                            xaxis: {
                                renderer: $.jqplot.DateAxisRenderer,
                                min: groupData.MinDate,
                                max: groupData.MaxDate
                            },
                            yaxis: {
                                numberTicks: 1,
                                min: minValue,
                                max: maxValue
                            }
                        },
                        legend: { show: true, location: 'e' }
                    });
            }
        };
    };

    $.widget("CRM.Reporting", {
        options: {
            chartType: null
        },

        _create: function () {
            this.reportingWidget = new crm.Reporting(this);
            $.extend(this.reportingWidget.options, this.options);
            this.reportingWidget.initialize();
        }
    });
})();