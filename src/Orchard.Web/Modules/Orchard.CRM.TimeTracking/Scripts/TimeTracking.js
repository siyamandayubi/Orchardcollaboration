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

    crm.timeTracking.timeTrackingWidget = function (widget) {
        var _self = this;
        var latestCount = 0;
        this.options = {
            loadingPageId: "loadingPage",
            addTimeTrackingButtonId: "addLogWork",
            dialogId: "timeTrackingDialog",
            dialogContentClass: "timetracking-content",
            dialogContainerClass: "timetracking-dialog",
            saveButtonClass: "time-tracking-save",
            cancelButtonClass: "time-tracking-cancel"
        };

        var intervalHandler = null;
        this.initialize = function () {

            var windowWidth = $(window).width(); //retrieve current window width
            var windowHeight = $(window).height(); //retrieve current window height
            var dialogWidth = windowWidth > 500 ? 400 : windowWidth > 350 ? 300 : 250;
            var dialogHeight = windowHeight > 600 ? 500 : windowHeight > 450 ? 400 : 300;
            var dialogContainer = $("#" + _self.options.dialogId);

            var dialogOptions = {
                resizable: false,
                maxWidth: dialogWidth,
                height: dialogHeight,
                width: dialogWidth,
                minWidth: dialogWidth,
                autoOpen: false,
                open: function (event, ui) {
                    dialogContainer
                        .parent()
                        .addClass(_self.options.dialogContainerClass); //this line does the actual hiding
                }
            };

            // set top position in mobile devices
            if (windowWidth < 600) {
                dialogOptions.position = {
                    my: "left+20 top+60",
                    at: "left top",
                    of: window
                };
            }

            dialogContainer.dialog(dialogOptions);

            $("#" + _self.options.addTimeTrackingButtonId).click(addTimeTrackingEventHandler);
            dialogContainer.find("." + _self.options.saveButtonClass).click(cancelTimeTrackingClickHadnler);
            dialogContainer.find("." + _self.options.cancelButtonClass).click()
        };

        var addTimeTrackingEventHandler = function (e) {
            var dialogContainer = $("#" + _self.options.dialogId);
            dialogContainer.dialog("open");
        }

        var saveTimeTrackingClickHadnler = function (e) {
            var toSubmitData = {
                activityStreamId: max
            };

            var helper = new crm.timeTracking.Helper();
            var verificationToken = helper.getRequestVerificationToken();

            $.extend(toSubmitData, verificationToken);

        };

        var cancelTimeTrackingClickHadnler = function (e) {
            var dialogContainer = $("#" + _self.options.dialogId);
            dialogContainer.dialog("close");
        };
    };

    $.widget("CRM.TimeTracking", {
        options: {},
        _create: function () {
            this.editFolder = new crm.timeTracking.timeTrackingWidget(this);
            $.extend(this.editFolder.options, this.options);
            this.editFolder.initialize();
        }
    });
})();