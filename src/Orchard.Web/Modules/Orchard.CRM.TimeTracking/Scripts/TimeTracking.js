window.crm = window.crm || {};

(function () {
    crm.timeTracking = crm.timeTracking || {};

    // helper class
    // TODO: This logic is repreated across Orchard.CRM.Notification, Orchard.CRM.TimeTracking, Orchard.CRM.Project/Scripts/ProjectWidgets/EditBaseWidget, Orchard.CRM.Core/Scripts/CRMWidgets/EditBaseWidget 
    // and Orchard.SuiteCRM.Connector/Scripts/SyncCRM/Helper. It must move to a common js library
    crm.timeTracking.Helper = function (widget) {
        var _self = this;

        this.options = {
            antiForgeryTokenId: "__RequestVerificationToken"
        };

        this.getRequestVerificationToken = function () {
            var antiForgeryToken = $("input[name='" + _self.options.antiForgeryTokenId + "']").val();

            return { __RequestVerificationToken: antiForgeryToken };
        };
    };

    crm.timeTracking.timeTrackingController = function (contentContainer, dataContainer) {
        var _self = this;

        var data = JSON.parse($("#" + dataContainer).html());

        this.translate = function (data, key, text) {
            if (!data.TranslateTable) {
                return typeof text !== "undefined" ? text : key;
            }

            if (typeof data.TranslateTable[key] !== "undefined") {
                return data.TranslateTable[key];
            }

            console.log(key + "       " + text);
            return typeof text !== "undefined" ? text : key;
        };

        var T = function (key, text) {

            return _self.translate(data, key, text);
        };

        var timeTrackingMainComponent = React.createClass({
            render: function () {
                var model = {
                    data: data,
                    root: {
                        T: T,
                        Routes: data.Routes,
                        Controller: _self,
                        actions: {}
                    }
                };

                return React.createElement(
                   "div",
                   null,
                   React.createElement(orchardcollaboration.react.allComponents.InfoPage, model),
                   React.createElement(orchardcollaboration.react.allComponents.TimeTrackingList, model));

            }
        });

        var element = React.createElement(timeTrackingMainComponent);
        var _reactComponent = ReactDOM.render(element, document.getElementById(contentContainer));
    };

})();