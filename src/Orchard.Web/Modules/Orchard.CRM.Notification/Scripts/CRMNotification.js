window.crm = window.crm || {};

(function () {

    crm.notification = crm.notification || {};

    // Fix the issue of overlapping Bootstrap and JQueryUI
    $.widget("ui.dialog", $.ui.dialog, {
        open: function () {
            $(this.uiDialogTitlebarClose).html("<span class='ui-button-icon-primary ui-icon ui-icon-closethick'></span><span class='ui-button-text'>&nbsp;</span>");
            // Invoke the parent widget's open().
            return this._super();
        }
    });

    // helper class
    // TODO: This logic is repreated across Orchard.CRM.Notification, Orchard.CRM.TimeTracking, Orchard.CRM.Project/Scripts/ProjectWidgets/EditBaseWidget, Orchard.CRM.Core/Scripts/CRMWidgets/EditBaseWidget 
    // and Orchard.SuiteCRM.Connector/Scripts/SyncCRM/Helper. It must move to a common js library
    crm.notification.Helper = function (widget) {
        var _self = this;

        this.options = {
            antiForgeryTokenId: "__RequestVerificationToken"
        };

        this.getRequestVerificationToken = function () {
            var antiForgeryToken = $("input[name='" + _self.options.antiForgeryTokenId + "']").val();

            return { __RequestVerificationToken: antiForgeryToken };
        };
    };

    crm.notification.notificationWidget = function (widget) {
        var _self = this;
        var latestCount = 0;
        this.options = {
            intervalPeriond: 10000,
            loadingPageId: "loadingPage",
            countContainerClass: "count-container",
            notificationContainerId: "notificationContainer",
            dialogId: "notificationDialog",
            activeNotificationClass: "active-notification",
            dialogContentClass: "notification-content",
            dialogContainerClass: "notification-dialog"
        };

        var intervalHandler = null;
        this.initialize = function () {

            intervalHandler = setInterval(getCount, _self.options.intervalPeriond);
            var countContainer = widget.element.find("." + _self.options.countContainerClass);
            widget.element.click(countContainerClickHandler);

            latestCount = getNotificationCountFromLocalStorage();

            showUnreadMessages(countContainer, latestCount);

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
        };

        var showUnreadMessages = function (countContainer, latestCount) {
            if (latestCount > 99) {
                countContainer.text("+99");
            } else if (latestCount == "0") {
                countContainer.text("");
            }
            else {
                countContainer.text(latestCount);
            }

            if (latestCount > 0) {
                widget.element.addClass(_self.options.activeNotificationClass);
            }
            else {
                widget.element.removeClass(_self.options.activeNotificationClass);
            }
        }

        var storeLastNotificationCountInLocalStorage = function (count) {
            if (typeof (Storage) === "undefined") {
                return "0";
            }

            var userId = widget.element.data("userid");
            if (!userId) {
                return null;
            }

            var key = "user" + userId;
            localStorage.setItem(key, count);
        };

        var getNotificationCountFromLocalStorage = function () {

            if (typeof (Storage) === "undefined") {
                return 0;
            }

            var userId = widget.element.data("userid");
            if (!userId) {
                return 0;
            }

            var key = "user" + userId;
            if (localStorage.getItem(key) === null) {
                return 0;
            }

            return localStorage[key];
        };

        var pagerClick = function (event) {
            var url = this.href;

            $.ajax({
                type: "GET",
                url: url,
                success: function (response) {
                    if (response.IsDone) {
                        var dialogContainer = $("#" + _self.options.dialogId);
                        dialogContainer.find("." + _self.options.dialogContentClass).html(response.Html);
                        dialogContainer.find(".pager a").click(pagerClick);
                    }
                }
            });

            event.preventDefault();
        };

        var countContainerClickHandler = function (event) {
            var url = widget.element.data("listurl");

            if (latestCount == 0) {
                return;
            }

            var loadingPage = $("#" + _self.options.loadingPageId);
            loadingPage.show();
            $.ajax({
                type: "GET",
                url: url,
                success: function (response) {
                    loadingPage.hide();
                    if (response.IsDone) {
                        try {
                            var dialogContainer = $("#" + _self.options.dialogId);
                            dialogContainer.find("." + _self.options.dialogContentClass).html(response.Html);
                            dialogContainer.dialog("open");
                            dialogContainer.find(".pager a").click(pagerClick);
                            updateCount();
                        }
                        catch (e) {
                            console.log(e.message);
                        }
                    }
                }
            }).always(function () {
                loadingPage.hide();
            })
        };

        var updateCount = function () {
            var url = widget.element.data("updateurl");

            var dialogContainer = $("#" + _self.options.dialogId);
            var ids = dialogContainer.find(".activity-stream-item");

            var max = 0;
            ids.each(function () {
                var id = $(this).data("activitystreamid");
                if (id > max) {
                    max = id;
                }
            });

            var toSubmitData = {
                activityStreamId: max
            };

            var helper = new crm.notification.Helper();
            var verificationToken = helper.getRequestVerificationToken();

            $.extend(toSubmitData, verificationToken);


            url = url + "?activityStreamId=" + max;
            $.ajax({
                type: "POST",
                data: toSubmitData,
                url: url,
                success: function (response) {
                    if (response.IsDone) {
                        var countContainer = widget.element.find("." + _self.options.countContainerClass);
                        storeLastNotificationCountInLocalStorage(0);
                        showUnreadMessages(countContainer, 0);
                    }
                }
            })
        }

        var getCount = function () {
            var url = widget.element.data("counturl");
            $.ajax({
                type: "GET",
                url: url,
                success: function (response) {
                    if (response.IsDone) {
                        var countContainer = widget.element.find("." + _self.options.countContainerClass);

                        latestCount = response.Data.Count;

                        showUnreadMessages(countContainer, latestCount);

                        storeLastNotificationCountInLocalStorage(latestCount);
                    }
                }
            })
        };
    };

    $.widget("CRM.CRMNotification", {
        options: {},
        _create: function () {
            this.widgetImplementation = new crm.notification.notificationWidget(this);
            $.extend(this.widgetImplementation.options, this.options);
            this.widgetImplementation.initialize();

        }
    });
})();