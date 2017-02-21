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
        data.showModal = false;
        data.asyncState = "normal";

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

        var closeModal = function () {
            data.showModal = false;
            _reactComponent.setState(data);
        };

        var saveItem = function (item) {
            data.showModal = false;
            _reactComponent.setState(data);

            var toPostData = {
                ContentItemId: data.ContentItem.Id,
                Comment: item.comment,
                TrackedTimeInString: item.timeSpend,
                TrackingDate: item.trackingDate,
                TrackingItemId: item.trackingItemId,
                UserId: item.userId
            };

            var isInAddMode = toPostData.TrackingItemId ? false : true;
            var url = isInAddMode ? data.Routes.AddLogUrl : data.Routes.EditLogUrl;

            var helper = new crm.timeTracking.Helper();
            var verificationToken = helper.getRequestVerificationToken();
            $.extend(toPostData, verificationToken);

            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                type: "POSt",
                url: url,
                data: toPostData,
                error: function (e) {
                    data.asyncState = "error";
                    _reactComponent.setState(data);
                }
            }).done(function (response) {
                if (response.Errors.length > 0) {
                    data.asyncState = "error";
                    _reactComponent.setState(data);
                    return;
                }

                var savedItem = typeof response.Data === "String" ? JSON.parse(response.Data) : response.Data;
                if (isInAddMode) {
                    data.Model.Items.push(savedItem);
                }
                else {
                    for (var i = 0; i < data.Model.Items.length; i++) {
                        if (data.Model.Items[i].TrackingItemId == savedItem.TrackingItemId) {
                            data.Model.Items[i] = savedItem;
                        }
                    }
                }

                data.asyncState = "normal";
                _reactComponent.setState(data);
            });
        };

        var addItem = function () {
            data.selectedItem = { title: T("Work Log", "Work Log") };
            data.showModal = true;
            _reactComponent.setState(data);
        };

        var editItem = function (item) {
            data.selectedItem = item;
            data.selectedItem.title = T("Work Log", "Work Log");
            data.showModal = true;
            _reactComponent.setState(data);
        };

        var render = function () {
            var model = {
                data: data,
                root: {
                    T: T,
                    Routes: data.Routes,
                    Controller: _self,
                    actions: {
                        closeModal: closeModal,
                        saveItem: saveItem,
                        addItem: addItem,
                        editItem: editItem
                    }
                }
            };

            return React.createElement(
               "div",
               null,
               React.createElement(orchardcollaboration.react.allComponents.InfoPage, model),
               React.createElement(orchardcollaboration.react.allComponents.TimeTrackingList, model),
               React.createElement(orchardcollaboration.react.allComponents.EditLogWorkModal, model));

        };

        var getInitialState = function () {
            data.asyncState = "normal";
            return data;
        };

        var timeTrackingMainComponent = React.createClass({
            render: render,
            getInitialState: getInitialState
        });

        var element = React.createElement(timeTrackingMainComponent);
        var _reactComponent = ReactDOM.render(element, document.getElementById(contentContainer));
    };

})();