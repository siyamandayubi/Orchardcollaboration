window.crm = window.crm || {};

(function () {

    // Fix the issue of overlapping Bootstrap and JQueryUI
    $.widget("ui.dialog", $.ui.dialog, {
        open: function () {
            $(this.uiDialogTitlebarClose).html("<span class='ui-button-icon-primary ui-icon ui-icon-closethick'></span><span class='ui-button-text'>&nbsp;</span>");
            // Invoke the parent widget's open().
            return this._super();
        }
    });

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
                } else {
                    for (var name in objValue) {
                        if (objValue.hasOwnProperty(name)) {
                            convertObject(objName + "." + name, objValue[name]);
                        }
                    }
                }
            } else {
                $('<input />', {
                    type: 'hidden',
                    name: objName,
                    value: objValue
                }).appendTo(form);
            }
        };

        if (typeof input !== 'undefined') {
            $.each(input, convertObject);
        }

        form.appendTo('body').submit();
    };

    // base class
    // TODO: The logic to retrive VerificationToken is repreated across Orchard.CRM.Project/Scripts/ProjectWidgets/EditBaseWidget, Orchard.CRM.Core/Scripts/CRMWidgets/EditBaseWidget 
    // and Orchard.SuiteCRM.Connector/Scripts/SyncCRM/Helper. It must move to a common js library
    crm.EditBaseWidget = function (widget) {
        var _self = this;

        this.options = {
            antiForgeryTokenId: "__RequestVerificationToken"
        };

        this.changeLinkToBehaveAsPostRequest = function (event) {
            var href = this.href;

            event.preventDefault();
            var verificationToken = _self.getRequestVerificationToken();

            jqueryPost(href, "POST", verificationToken);
        };

        this.getRequestVerificationToken = function () {
            var antiForgeryToken = $("input[name='" + _self.options.antiForgeryTokenId + "']").val();

            return { __RequestVerificationToken: antiForgeryToken };
        };
    };

    crm.FileUpload = function (widget) {
        var _self = this;

        var op = {
            guid: "",
            url: "",
            element: "",
            allowedExtensions: null,
            sizeLimit: 10 * 1024 * 1024
        };

        crm.EditBaseWidget.apply(this, arguments);
        $.extend(_self.options, op);

        var token = _self.getRequestVerificationToken().__RequestVerificationToken;

        this.initialize = function () {

            var uploader = new qq.FileUploader({
                params: _self.getRequestVerificationToken(),
                element: _self.options.element,
                action: _self.options.url,
                allowedExtensions: _self.options.allowedExtensions,
                sizeLimit: _self.options.sizeLimit,
                debug: true,
                fileTemplate: '<li>' + '<span class="qq-upload-file"></span>' + '<span class="qq-upload-spinner"></span>' + '<span class="qq-upload-size"></span>' + '<a class="qq-upload-cancel" href="#">Cancel</a>' + '<span class="qq-upload-failed-text">Failed</span>' + '</li>',
                onComplete: function (id, fileName, response) {
                    if (!response.success) {
                        $('<ul><li class=\'error\'>' + response.message + '</li></ul>').insertBefore($('#fileuploader_@(Model.ContentId)'));
                    } else {
                        widget._trigger("fileUploadComplete", null, response);
                        $(".qq-upload-list").find("li").each(function () {
                            if (this.qqFileId == id) {
                                $(this).remove();
                            }
                        });
                    }
                },
                onSubmit: function (id, fileName) { }
            });
        };
    };

    crm.TicketsWidget = function (widget) {
        var _self = this;

        var op = {
            createButtonId: "createbutton",
            dueDateId: "DueDate",
            checkAllId: "checkAll",
            itemsTableId: "items",
            toolbarId: "toolbar",
            advanceSearchIconId: "advancedSearchOpen",
            advanceSearchId: "advanceSearchBar",
            advanceSearchCloseButtonId: "advanceSearchCloseId",
            deleteButtonId: "deleteButton",
            closeButtonId: "closeButton",
            reopenButtonId: "reopenButton",
            assignButtonId: "assignButton",
            searchFormId: "searchForm",
            deleteConfirmDialogId: "deleteConfirmDialog",
            deleteConfirmDialogYesButton: "deleteConfirmDialogYesButton",
            deleteConfirmDialogNoButton: "deleteConfirmDialogNoButton",
            shareDialogShareButton: "shareDialogShareButton",
            shareDialogCancelButton: "shareDialogCancelButton",
            shareDialogId: "shareDialog",
            viewSharePageLink: "viewSharePageLink",
            removePreviousPermissionClass: "removePreviousPermissionDiv"
        };

        crm.EditBaseWidget.apply(this, arguments);
        $.extend(_self.options, op);

        this.initialize = function () {
            $("#" + _self.options.checkAllId).click(checkAllClickHandler);
            $("#" + _self.options.dueDateId).datepicker({ dateFormat: 'yy/mm/dd' });
            var itemsTable = $("#" + _self.options.itemsTableId);

            itemsTable.find("tr").find("td:first input[type='checkbox']").click(checkBoxClickHandler);

            itemsTable.find("thead").find(".col a").click(columnHeaderClickHandler);
            $("#" + _self.options.closeButtonId).click(changeStatusOfSelectedItems);
            $("#" + _self.options.reopenButtonId).click(changeStatusOfSelectedItems);
            $("#" + _self.options.assignButtonId).click(assignButtonClickHandler);
            $("#" + _self.options.deleteButtonId).click(deleteClickHandler);
            $("#" + _self.options.deleteConfirmDialogNoButton).click(function () {
                $("#" + _self.options.deleteConfirmDialogId).dialog("close");
            });
            $("#" + _self.options.deleteConfirmDialogYesButton).click(deleteConfirmDialogYesButtonClickHandler);

            var shareDialog = $("#" + _self.options.shareDialogId);

            $("#" + _self.options.shareDialogCancelButton).click(function () {
                shareDialog.dialog("close");
            });
            $("#" + _self.options.shareDialogShareButton).click(shareDialogYesButtonClickHandler);

            shareDialog.dialog({
                resizable: false,
                maxWidth: 400,
                height: 'auto',
                width: 400,
                minWidth: 400,
                autoOpen: false,
                open: function (event, ui) {
                    shareDialog.css('overflow', 'hidden'); //this line does the actual hiding
                }
            });

            shareDialog.find("input[name='AccessType']").click(accessTypeClickHandler);
            accessTypeClickHandler();

            var groupsOptions = shareDialog.find(".groups select option");
            var usersOptions = shareDialog.find(".users select option");

            //$("#" + _self.options.createButtonId).TicketDialog({
            //    Users: usersOptions,
            //    BusinesUnits: groupsOptions,
            //    ticketCreated: ticketCreatedHandler
            //});

            shareDialog.find(".groups select").addClass("chosen-select").chosen({ width: "95%", max_selected_options: 3 });

            shareDialog.find(".users select").addClass("chosen-select").chosen({ width: "95%", max_selected_options: 3 });

            var advanceSearchIcon = $("#" + _self.options.advanceSearchIconId);
            advanceSearchIcon.click(advanceSearchIconClick);

            $("#" + _self.options.advanceSearchCloseButtonId).click(advanceSearchCloseClickHandler);

            var advanceSearchBar = $("#" + _self.options.advanceSearchId);
            var users = advanceSearchBar.find(".agents-container").find("select");
            users.addClass("chosen-select");
            users.chosen({ width: "95%", max_selected_options: 3 });

            var businessUnits = advanceSearchBar.find(".groups-container").find("select");
            businessUnits.addClass("chosen-select");
            businessUnits.chosen({ width: "95%", max_selected_options: 5 });
        };

        var ticketCreatedHandler = function (data) {
            var url = "/OrchardLocal/OrchardCollaboration/Ticket/Display/{id}?displayType=TableRow";
            url = url.replace("{id}", data.Id);
            $.ajax({
                method: "GET",
                url: url,
                success: function (response) {
                    alert("jejr");
                }
            });
        };

        var accessTypeClickHandler = function () {
            var shareDialog = $("#" + _self.options.shareDialogId);
            var checkedAccessType = shareDialog.find("input[name='AccessType']:checked");

            var removePreviousPermission = shareDialog.find("." + _self.options.removePreviousPermissionClass);

            if (checkedAccessType.length > 0 && checkedAccessType.val() == "1") {
                removePreviousPermission.children().show();
            } else {
                removePreviousPermission.children().hide();
            }
        };

        var advanceSearchCloseClickHandler = function (e) {
            var advanceSearchBar = $("#" + _self.options.advanceSearchId);
            advanceSearchBar.addClass("hidden");
            e.preventDefault();
        };

        var advanceSearchIconClick = function (event) {
            var advanceSearchBar = $("#" + _self.options.advanceSearchId);
            advanceSearchBar.removeClass("hidden");
        };

        var deleteConfirmDialogYesButtonClickHandler = function (event) {
            $("#" + _self.options.deleteConfirmDialogId).dialog("close");

            var checkedCheckBoxes = $("#" + _self.options.itemsTableId).find("tr").find("td:first").find("input[type='checkbox']:checked");

            if (checkedCheckBoxes.length == 0) {
                return;
            }

            var data = {
                returnUrl: location.href,
                ids: []
            };

            for (var i = 0; i < checkedCheckBoxes.length; i++) {
                data.ids.push(checkedCheckBoxes[i].value);
            }

            var verificationToken = _self.getRequestVerificationToken();

            $.extend(data, verificationToken);
            var url = $(this).data("url");

            jqueryPost(url, "POST", data);
        };

        var deleteClickHandler = function (event) {
            var checkedCheckBoxes = $("#" + _self.options.itemsTableId).find("tr").find("td:first").find("input[type='checkbox']:checked");

            if (checkedCheckBoxes.length == 0) {
                return;
            }

            $("#" + _self.options.deleteConfirmDialogId).dialog({ minHeight: 80, resizable: false });
        };

        var columnHeaderClickHandler = function (event) {
            var sortFieldInput = document.getElementById("PagerParameters.SortField");
            var descendingInput = document.getElementById("PagerParameters.Descending");

            var $this = $(this);
            sortFieldInput.value = $this.data("sortfield");
            descendingInput.value = $this.data("descending");

            $("#" + _self.options.searchFormId).submit();
        };

        var setViewSharePageLink = function (checkedCheckBoxes) {
            if (checkedCheckBoxes.length == 0) {
                return;
            }

            var data = {
                returnUrl: location.href
            };

            var ids = "";
            for (var i = 0; i < checkedCheckBoxes.length; i++) {
                ids += "ids=" + checkedCheckBoxes[i].value + "&";
            }

            ids = ids.substr(0, ids.length - 1);

            var verificationToken = _self.getRequestVerificationToken();

            $.extend(data, verificationToken);

            var linkTag = $("#" + _self.options.viewSharePageLink);
            var url = linkTag.data("url");
            linkTag.attr("href", url + "?" + $.param(data) + "&" + ids);
        };

        var assignButtonClickHandler = function (event) {
            var checkedCheckBoxes = $("#" + _self.options.itemsTableId).find("tr").find("td:first").find("input[type='checkbox']:checked");

            if (checkedCheckBoxes.length == 0) {
                return;
            }

            setViewSharePageLink(checkedCheckBoxes);

            var dialogBox = $("#" + _self.options.shareDialogId);
            dialogBox.find(".error").addClass("hidden").text("");
            dialogBox.dialog("open");
        };

        var shareDialogYesButtonClickHandler = function (event) {
            var dialogBox = $("#" + _self.options.shareDialogId);
            var checkedCheckBoxes = $("#" + _self.options.itemsTableId).find("tr").find("td:first").find("input[type='checkbox']:checked");

            if (checkedCheckBoxes.length == 0) {
                return;
            }

            var verificationToken = _self.getRequestVerificationToken();

            var data = {
                RemoveOldPermission: dialogBox.find("input[name=RemoveOldPermission]").is(':checked'),
                AccessType: parseInt(dialogBox.find('input[name=AccessType]:checked').val()),
                returnUrl: location.href,
                ContentIds: [],
                BusinessUnits: [],
                Users: []
            };

            // add contentIds
            for (var i = 0; i < checkedCheckBoxes.length; i++) {
                data.ContentIds.push(checkedCheckBoxes[i].value);
            }

            // add groups
            var checkedGroups = dialogBox.find(".groups").find("option:selected");
            checkedGroups.each(function () {
                data.BusinessUnits.push(this.value);
            });

            var users = dialogBox.find(".users").find("option:selected");
            users.each(function () {
                data.Users.push(this.value);
            });

            // check selecting at list one target
            if (data.Users.length == 0 && data.BusinessUnits.length == 0) {
                var errorTag = dialogBox.find(".error");
                errorTag.removeClass("hidden").text(errorTag.data("validationselecttarget"));
                return;
            }

            // check selecting only one owner
            var targetsLength = data.BusinessUnits.length + data.Users.length;
            if (data.AccessType == 1 /*Owner*/ && targetsLength > 1) {
                var errorTag = dialogBox.find(".error");
                errorTag.removeClass("hidden").text(errorTag.data("validationselectoneowner"));

                return;
            }

            $.extend(data, verificationToken);
            var url = $(this).data("url");
            jqueryPost(url, "POST", data);
            dialogBox.dialog("close");
        };

        var changeStatusOfSelectedItems = function (event) {
            var checkedCheckBoxes = $("#" + _self.options.itemsTableId).find("tr").find("td:first").find("input[type='checkbox']:checked");

            if (checkedCheckBoxes.length == 0) {
                return;
            }

            var data = {
                returnUrl: location.href,
                UpdateStatusId: true,
                StatusId: $(this).data("status"),
                Ids: []
            };

            for (var i = 0; i < checkedCheckBoxes.length; i++) {
                data.Ids.push(checkedCheckBoxes[i].value);
            }

            var verificationToken = _self.getRequestVerificationToken();

            $.extend(data, verificationToken);
            var url = $(this).data("url");

            jqueryPost(url, "POST", data);
        };

        var checkBoxClickHandler = function (event) {
            var checkedCheckBoxes = $("#" + _self.options.itemsTableId).find("tr").find("td:first").find("input[type='checkbox']:checked");

            if (checkedCheckBoxes.length > 0) {
                enableButtons();
            } else {
                disableButtons();
            }
        };

        var enableButtons = function () {
            var toolbar = $("#" + _self.options.toolbarId);
            toolbar.addClass("active-toolbar");
            var buttons = toolbar.find(".btn");
            buttons.removeClass("disable").addClass("active-buttons");
            buttons.each(function () {
                var button = $(this);
                var specificActiveClass = button.data("activeclass");
                var specificInActiveClass = button.data("inactiveclass");
                button.addClass(specificActiveClass).removeClass(specificInActiveClass);
            });
        };

        var disableButtons = function () {
            var toolbar = $("#" + _self.options.toolbarId);
            toolbar.removeClass("active-toolbar");
            var buttons = toolbar.find(".btn");
            buttons.removeClass("active-buttons").addClass("disable");
            for (var i = 0; i < buttons.length; i++) {
                var button = $(buttons[i]);
                var specificActiveClass = button.data("activeclass");
                var specificInActiveClass = button.data("inactiveclass");
                button.removeClass(specificActiveClass).addClass(specificInActiveClass);
            };
        };

        var checkAllClickHandler = function (event) {
            var checkBoxes = $("#" + _self.options.itemsTableId).find("tr").find("td:first input[type='checkbox']");

            checkBoxes.prop("checked", this.checked);

            if (this.checked) {
                enableButtons();
            } else {
                disableButtons();
            }
        };
    };

    crm.EditPermissionsWidget = function (widget) {
        var _self = this;

        var op = {
            deleteButtonClass: "delete-button",
            deleteConfirmDialogId: "deleteConfirmDialog",
            deleteConfirmDialogYesButton: "deleteConfirmDialogYesButton",
            deleteConfirmDialogNoButton: "deleteConfirmDialogNoButton",
            currentPpermissionOwnershipClass: "current-permission-ownership",
            removePreviousPermissionClass: "removePreviousPermissionDiv",
            editOwnershipBar: "edit-ownership-bar"
        };

        crm.EditBaseWidget.apply(this, arguments);
        $.extend(_self.options, op);

        this.initialize = function () {
            $("." + op.currentPpermissionOwnershipClass).find("." + op.deleteButtonClass + " a").click(deleteClickHandler);
            $("#" + _self.options.deleteConfirmDialogNoButton).click(function () {
                $("#" + _self.options.deleteConfirmDialogId).dialog("close");
            });
            $("#" + _self.options.deleteConfirmDialogYesButton).click(deleteConfirmDialogYesButtonClickHandler);

            var editOwnershipBar = $("." + _self.options.editOwnershipBar);
            editOwnershipBar.find("select[name='AccessType']").change(accessTypeClickHandler);

            editOwnershipBar.find("select").chosen();
            accessTypeClickHandler();
        };

        var accessTypeClickHandler = function () {
            var editOwnershipBar = $("." + _self.options.editOwnershipBar);
            var checkedAccessType = editOwnershipBar.find("select[name='AccessType']");

            var removePreviousPermission = editOwnershipBar.find("." + _self.options.removePreviousPermissionClass);

            if (checkedAccessType.length > 0 && checkedAccessType.val() == "1") {
                removePreviousPermission.show();
            } else {
                removePreviousPermission.hide();
            }
        };

        var deleteConfirmDialogYesButtonClickHandler = function (event) {
            var deleteDialog = $("#" + _self.options.deleteConfirmDialogId);
            var url = deleteDialog.data("url");
            var id = deleteDialog.data("id");

            var li = $("." + op.currentPpermissionOwnershipClass).find("li[data-id= '" + id + "']");

            var verificationToken = _self.getRequestVerificationToken();

            $.post(url, verificationToken, function (data) {

                if (data.IsDone) {
                    deleteDialog.dialog("close");
                    li.remove();
                } else {
                    alert("Error happens in sending data to server");
                }
            });

            event.preventDefault();
        };

        var deleteClickHandler = function (event) {
            var deleteDialog = $("#" + _self.options.deleteConfirmDialogId);
            deleteDialog.data("url", this.href);
            deleteDialog.data("id", $(this).data("id"));
            deleteDialog.dialog({ minHeight: 80, resizable: false });
            event.preventDefault();
        };
    };

    crm.EditTicketWidget = function (widget) {

        var _self = this;

        var op = {
            dueDateId: "",
            userId: "",
            groupId: "",
            publishLink: "publishLink",
            unPublishLinkId: "unPublishLink"
        };

        crm.EditBaseWidget.apply(this, arguments);
        $.extend(this.options, op);

        this.initialize = function () {

            $("#" + _self.options.publishLink).removeClass("hidden").click(_self.changeLinkToBehaveAsPostRequest);
            $("#" + _self.options.unPublishLinkId).removeClass("hidden").click(_self.changeLinkToBehaveAsPostRequest);

            $("#" + _self.options.dueDateId).datepicker();
            //$("#" + _self.options.dueDateId).calendarsPicker({
            //    showAnim: "",
            //    renderer: $.extend({}, $.calendarsPicker.themeRollerRenderer, {
            //        picker: "<div {popup:start} id='ui-datepicker-div'{popup:end} class='ui-datepicker ui-widget ui-widget-content ui-helper-clearfix ui-corner-all{inline:start} ui-datepicker-inline{inline:end}'><div class='ui-datepicker-header ui-widget-header ui-helper-clearfix ui-corner-all'>{link:prev}{link:today}{link:next}</div>{months}{popup:start}{popup:end}<div class='ui-helper-clearfix'></div></div>",
            //        month: "<div class='ui-datepicker-group'><div class='ui-datepicker-month ui-helper-clearfix'>{monthHeader:MM yyyy}</div><table class='ui-datepicker-calendar'><thead>{weekHeader}</thead><tbody>{weeks}</tbody></table></div>"
            //    })
            //});
            $("#" + _self.options.userId).chosen().change(_self.userChanged);
            $("#" + _self.options.groupId).chosen().change(_self.groupChanged);
        };

        this.userChanged = function (event) {
            var user = $("#" + _self.options.userId);

            var userVal = user.val();
            if (userVal && userVal != "") {
                var group = $("#" + _self.options.groupId);
                group.get(0).selectedIndex = -1;
                group.trigger('chosen:updated');
            }
        };

        this.groupChanged = function (event) {
            var group = $("#" + _self.options.groupId);

            var groupVal = group.val();
            if (groupVal && groupVal != "") {
                var user = $("#" + _self.options.userId);

                if (user.length > 0) {
                    user.get(0).selectedIndex = -1;
                    user.trigger('chosen:updated');
                }
            }
        };
    };

    crm.FileListtWidget = function (widget) {

        var _self = this;

        var op = {
            deleteButtonClass: "file-delete",
            deleteConfirmDialogId: "deleteFilesConfirmDialog",
            deleteConfirmDialogYesButton: "deleteFileConfirmDialogYesButton",
            deleteConfirmDialogNoButton: "deleteFileConfirmDialogNoButton",
            fileListClass: "files",
            fileListId: "file-list",
            noFileUploadedClass: "noFile"
        };

        crm.EditBaseWidget.apply(this, arguments);
        $.extend(_self.options, op);

        this.initialize = function () {
            $("." + _self.options.fileListClass).find("." + _self.options.deleteButtonClass).click(deleteClickHandler);
            $("#" + _self.options.deleteConfirmDialogNoButton).click(function () {
                $("#" + _self.options.deleteConfirmDialogId).dialog("close");
            });
            $("#" + _self.options.deleteConfirmDialogYesButton).click(deleteConfirmDialogYesButtonClickHandler);
        };

        this.addNewFile = function (data) {
            if (data.success) {
                var filesTag = $("." + _self.options.fileListClass);
                filesTag.append(data.FileLinkHtml);

                var latestLi = filesTag.find("li:last");
                latestLi.find("." + _self.options.deleteButtonClass).click(deleteClickHandler);
                latestLi.find(".file-link").linktype();

                filesTag.find("." + _self.options.noFileUploadedClass).remove();
            }
        };

        var deleteClickHandler = function (event) {
            var deleteDialog = $("#" + _self.options.deleteConfirmDialogId);
            deleteDialog.data("url", this.href);
            deleteDialog.data("id", $(this).data("id"));
            deleteDialog.dialog({ minHeight: 80, resizable: false });
            event.preventDefault();
        };

        var deleteConfirmDialogYesButtonClickHandler = function (event) {
            var deleteDialog = $("#" + _self.options.deleteConfirmDialogId);
            var url = deleteDialog.data("url");
            var id = deleteDialog.data("id");

            var li = $("." + op.fileListClass).find("li[data-id= '" + id + "']");

            var verificationToken = _self.getRequestVerificationToken();

            $.post(url, verificationToken, function (data) {

                if (data.IsDone) {
                    deleteDialog.dialog("close");
                    li.remove();

                    var files = $("#" + _self.options.fileListId);
                    if (files.find("li").length == 0) {
                        // TODO: replace with a template
                        files.find("ul").append("<li class='well " + _self.options.noFileUploadedClass + "'>" + files.data("nofilestext") + "</li>");
                    }
                } else {
                    alert("Error happens in sending data to server");
                }
            });

            event.preventDefault();
        };
    };

    crm.DisplayTicketWidget = function (widget) {
        var _self = this;

        var op = {
            dueDateId: "",
            publishLink: "publishLink",
            unPublishLinkId: "unPublishLink"
        };

        crm.EditBaseWidget.apply(this, arguments);

        $.extend(this.options, op);

        this.initialize = function () {
            $.fn.jAlert.defaults.size = 'lg';
            $.fn.jAlert.defaults.theme = 'blue';
            $.fn.jAlert.defaults.showAnimation = 'fadeInUp';
            $.fn.jAlert.defaults.hideAnimation = 'fadeOutDown';
            $.fn.jAlert.defaults.animationTimeout = 500;

            $("#" + _self.options.dueDateId).datepicker();

            $("#" + _self.options.publishLink).removeClass("hidden").click(_self.changeLinkToBehaveAsPostRequest);

            $("#" + _self.options.unPublishLinkId).removeClass("hidden").click(_self.changeLinkToBehaveAsPostRequest);

            var group = "group-owner-value";
            var user = "user-owner-value";
            addPopupMenu("ticket-priority-value", 'change-priorities-options');
            addPopupMenu("ticket-service-value", 'change-services-options');
            addPopupMenu("ticket-type-value", 'change-type-options');
            addPopupMenu("ticket-status-value", 'change-status-options');
            addPopupMenu("ticket-duedate-value", 'change-duedate-options', function (data, a) {
                var target = $(".ticket-duedate-value").find("span");
                target.text(a.data("text"));
            }, true);
            addPopupMenu(group, 'change-group-options', function (response) {
                if (!response.Data.ChangeOwnershipIsPossible) {
                    disableGroupAndUserMenu();
                }

                $("." + user).find("span").text("-");
            });
            addPopupMenu(user, 'change-user-options', function (response) {

                if (!response.Data.ChangeOwnershipIsPossible) {
                    disableGroupAndUserMenu();
                }

                $("." + group).find("span").text("-");
            });
        };

        var disableGroupAndUserMenu = function () {
            var groupTag = $(".group-owner-value");
            groupTag.data("noPopup", true);
            groupTag.find("ul").remove();
            var userTag = $(".user-owner-value");
            userTag.data("noPopup", true);
            userTag.find("ul").remove();
        };

        var addPopupMenu = function (targetClass, menuClass, callback, doNotSetText) {
            var target = $("." + targetClass);
            var menu = target.find("." + menuClass);
            var menuItems = menu.find("li");

            // we don't do any thing regarding empty menus
            if (menuItems.length == 0) {
                return;
            };

            target.find("span").click(function (e) {
                if (!target.data("noPopup")) {
                    target.addClass("menu-selected");
                    menu.show();
                }
            });

            target.mouseleave(function (e) {
                menu.hide();
                target.removeClass("menu-selected");
            });

            target.find("a").click(function (e) {
                var a = this;
                $.get(a.href, null, function (data) {
                    var $a = $(a);
                    if (data.IsDone) {
                        var span = target.find("span");

                        if (!doNotSetText) {
                            span.text($a.text());
                        }
                        span.attr("title", $(a).attr("title"));
                        target.removeClass("menu-selected");

                        if (callback) {
                            callback(data, $(a));
                        }
                    } else {
                        var error = data.Errors && data.Errors.length > 0 ? data.Errors[0] : "an error occured in server";
                        errorAlert('Error in server', error.Value);
                    }
                });
                menu.hide();
                e.preventDefault();
            });
        };
    };

    crm.TicketsForContentItemsWidget = function (widget) {
        var _self = this;

        this.options = {
            flipFlopId: "ticketsForContentItemFlipFlop",
            ticketsForContentItemTagsClass: "tickets-for-content-item"
        };

        this.initialize = function () {
            $("#" + _self.options.flipFlopId).click(flipFlopClickHandler);
        };

        var flipFlopClickHandler = function (event) {
            var flipFlop = $("#" + _self.options.flipFlopId);

            if (flipFlop.data("state") == "active-buttons") {
                flipFlop.data("state", "inactive").attr("title", flipFlop.data("inactivetitle")).css("background-color", "red");
                $("." + _self.options.ticketsForContentItemTagsClass).hide();
            } else {
                flipFlop.data("state", "active-buttons").attr("title", flipFlop.data("activetitle")).css("background-color", "green");
                $("." + _self.options.ticketsForContentItemTagsClass).show();
            }
        };
    };

    $.widget("CRM.TicketsForContentItemsWidget", {
        options: {},
        _create: function () {
            this.ticketsForContentItemsWidget =
                crm.TicketsForContentItemsWidget(this);
            $.extend(this.ticketsForContentItemsWidget.options, this.options);
            this.ticketsForContentItemsWidget.initialize();
        }
    });

    $.widget("CRM.FileList", {
        options: {},

        _create: function () {
            this.fileListWidget = new crm.FileListtWidget(this);
            $.extend(this.fileListWidget.options, this.options);
            this.fileListWidget.initialize();
        },
        addNewFile: function (data) {
            this.fileListWidget.addNewFile(data);
        }
    });

    $.widget("CRM.Tickets", {
        options: {},

        _create: function () {
            this.ticketsWidget = new crm.TicketsWidget(this);
            $.extend(this.ticketsWidget.options, this.options);
            this.ticketsWidget.initialize();
        }
    });

    $.widget("CRM.EditPermissions", {
        options: {},

        _create: function () {
            this.editPermissions = new crm.EditPermissionsWidget(this);
            $.extend(this.editPermissions.options, this.options);
            this.editPermissions.initialize();
        }
    });

    $.widget("CRM.EditTicket", {
        options: {
            dueDateId: "TicketPart_DueDate",
            userId: "ContentItemPermissionPart_UserId",
            groupId: "ContentItemPermissionPart_GroupId"
        },

        _create: function () {
            this.editTicketWidget = new crm.EditTicketWidget(this);
            $.extend(this.editTicketWidget.options, this.options);
            this.editTicketWidget.initialize();
        }
    });

    $.widget("CRM.DisplayTicket", {
        options: {
            dueDateId: "TicketPart_DueDate"
        },

        _create: function () {
            this.displayTicketWidget = new crm.DisplayTicketWidget(this);
            $.extend(this.displayTicketWidget.options, this.options);
            this.displayTicketWidget.initialize();
        }
    });

    $.widget("CRM.EditRequest", {
        options: {
            DueDateId: "RequestPart_DueDate"
        },

        _create: function () {
            this.editTicketWidget = new crm.EditTicketWidget(this);
            $.extend(this.editTicketWidget.options, this.options);
            this.editTicketWidget.initialize();
        }
    });

    $.widget("CRM.FileUpload", {
        options: {},

        _create: function () {
            this.fileUpload = new crm.FileUpload(this);
            $.extend(this.fileUpload.options, this.options);
            this.fileUpload.initialize();
        }
    });
    $.widget("CRM.TicketDialog", {
        options: {
            fullTicketCreateButton: "fullTicket",
            spin: "spinId",
            createUrl: "/OrchardLocal/OrchardCollaboration/Ticket/CreatePOST",
            dialogId: "ticketDialog",
            saveButtonLabel: "Save",
            cancelButtonLabel: "Cancel",
            errorCommentClass: "error",
            titleName: "TicketPart.Title",
            description: "TicketPart.Text",
            UserFieldName: "ContentItemPermissionPart.UserId",
            BusinessunitFieldName: "ContentItemPermissionPart.GroupId",
            Users: [],
            BusinesUnits: [],
            ticketCreated: function (data) { }
        },
        _create: function () {
            var _self = this;
            var dialog = $("#" + this.options.dialogId);
            //_self.options.createUrl = _self.element.attr("href");
            var buttons = {};

            var BusinessunitSelect = dialog.find("select[name='" + _self.options.BusinessunitFieldName + "']");

            var UserSelect = dialog.find("select[name='" + _self.options.UserFieldName + "']");
            UserSelect.on("change", function () {
                BusinessunitSelect.val("");
            });

            BusinessunitSelect.on("change", function () {
                UserSelect.val("");
            });

            $.each(_self.options.BusinesUnits, function (i, item) {
                BusinessunitSelect.append($('<option>', {
                    value: item.value,
                    text: item.text
                }));
            });
            $.each(_self.options.Users, function (i, item) {
                UserSelect.append($('<option>', {
                    value: item.value,
                    text: item.text
                }));
            });
            var errorComment = function (text) {
                var dialog = $("#" + _self.options.dialogId);
                dialog.find("." + errorCommentClass).text(text);
            };


            var checkLength = function (min, max, t, n) {
                if (t.val().length > max || t.val().length < min) {
                    errorComment("please write the length between" + min + "and" + max);
                    return false;
                }
                else {
                    return true;
                }
            };


            var save = function () {
                var valid = true;
                var description = dialog.find("textarea[name='" + _self.options.description + "']");
                var title = dialog.find("input[name='" + _self.options.titleName + "']");
                var users = dialog.find("select[name='" + _self.options.UserFieldName + "']");
                var businessunit = dialog.find("select[name='" + _self.options.BusinessunitFieldName + "']");
                valid = valid && checkLength(1, 100, title, "Title");
                if (valid) {
                    var editBaseWidget = new crm.EditBaseWidget();
                    var token = editBaseWidget.getRequestVerificationToken();
                    var data = {};
                    data[_self.options.description] = description.text();
                    data[_self.options.titleName] = title.val();
                    data[_self.options.UserFieldName] = users.val();
                    data[_self.options.BusinessunitFieldName] = 'BusinessUnit:' + businessunit.val();
                    data['id'] = 'Ticket';
                    $.extend(data, token);

                    var target = document.getElementById(_self.options.spin)
                    var spinner = new Spinner().spin(target)
                    $.ajax({
                        type: "post",
                        url: _self.options.createUrl,
                        data: data,
                        success: function (response) {
                            //why it is closed,when it becomes answer?it must close ,when we click an save button
                            dialog.dialog("close");
                            spinner.stop();
                            _self.options.ticketCreated(response);
                        },
                        error: function () { }
                    });
                }

            };

            buttons[this.options.saveButtonLabel] = save;
            buttons[this.options.cancelButtonLabel] = function () {
                dialog.dialog("close");
            };

            buttons[this.options.fullTicketCreateButton] = function () {
                window.location.assign(_self.element.attr("href"));
            };

            dialog.dialog({
                modal: true,
                autoOpen: false,
                fluid: true,
                width: $(window).width() > 800 ? 500 : 250,
                height: $(window).height() > 800 ? 1000 : 400,
                maxWidth: 600,
                buttons: buttons,
                open: function () {
                    dialog.find("input[name='" + _self.options.titleName + "']").val("");
                    dialog.find("textarea[name='" + _self.options.description + "']").text("");
                    dialog.find("select[name='" + _self.options.Businessunit + "']").val("");
                    dialog.find("select[name='" + _self.options.User + "']").val("");
                }

            });

            $(window).resize(function () {
                var dialog = $("#" + _self.options.dialogId);
                dialog.dialog("option", "position", "center");
                dialog.dialog({ width: $(window).width() > 800 ? 500 : 250, });
                dialog.dialog({ height: $(window).height() > 800 ? 1000 : 400 });
            });

            this.element.click(function (e) {
                var dialog = $("#" + _self.options.dialogId);
                e.preventDefault();
                dialog.dialog("open");
            });

        },
        open: function () {
            $("#" + this.options.dialogId).dialog("open");
        }


    });
    $.widget("CRM.CrmCommentForm", {
        options: {
            displayUrl: "/OrchardLocal/OrchardCollaboration/Item/Display/{id}",
        },
        _create: function () {
            var _self = this;
            _self.element.find("input[type='submit']").click(function (e) {
                e.preventDefault();
                var data = {};
                var inputValue = _self.element.find("input");
                var textarea = _self.element.find("textarea");
                $.each(inputValue, function (i, l) {
                    data[l.name] = l.value;
                });
                data[textarea.attr("name")] = textarea.text();
                $.ajax({
                    url: _self.element.attr("action"),
                    data: data,
                    type: "post",
                    success: function (response) {
                        var responseData = JSON.parse(response.Data);
                        var url = _self.options.displayUrl.replace("{id}", responseData.Id);
                        $.get(url, null, function (htmlReponse) {

                        });
                    }
                });
            });
        }
    });
    $.widget("CRM.TicketDetailTablControl", {
        options: {
            tabControlClass: "detail",
            tabControlHeaderClass: "detail-list-header",
            tabControlHeaderItemClass: "header",
            tabControlDetailPaneClass: "detail-list",
            selectedClass: "selected-tab"
        },

        _create: function () {
            var _self = this;
            var tabControl = $("." + _self.options.tabControlClass);
            var tabHeaderControl = tabControl.find("." + _self.options.tabControlHeaderClass);
            var detailList = tabControl.find("." + _self.options.tabControlDetailPaneClass);
            var headers = tabHeaderControl.find("." + _self.options.tabControlHeaderItemClass);
            headers.click(function () {
                var index = $(this).parent().children().index(this);
                detailList.children().addClass("hidden");
                $(detailList.children().get(index)).removeClass("hidden").addClass(_self.options.selectedClass);
                headers.removeClass(_self.options.selectedClass);
                $(this).addClass(_self.options.selectedClass);
            });
        }
    });
})();