/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

window.crm = window.crm || {};

(function () {
    crm.project = crm.project || {};

    function effectiveDeviceWidth() {
        var deviceWidth = window.orientation == 0 ? window.screen.height : window.screen.width;
        // iOS returns available pixels, Android returns pixels / pixel ratio
        // http://www.quirksmode.org/blog/archives/2012/07/more_about_devi.html
        if (navigator.userAgent.indexOf('Android') >= 0 && window.devicePixelRatio) {
            deviceWidth = deviceWidth / window.devicePixelRatio;
        }
        return deviceWidth;
    };

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

    // base class
    // TODO: The logic to retrive VerificationToken is repreated across Orchard.CRM.Project/Scripts/ProjectWidgets/EditBaseWidget, Orchard.CRM.Core/Scripts/CRMWidgets/EditBaseWidget 
    // and Orchard.SuiteCRM.Connector/Scripts/SyncCRM/Helper. It must move to a common js library
    crm.project.EditBaseWidget = function (widget) {
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

    crm.project.FolderListWidget = function (widget) {

        var _self = this;

        this.options = {
            folderListClass: "folder-list",
            folderListContainerClass: "folder-list-container",
            folderListHeaderClass: "folder-list-header",
            folderListOuterContainerClass: "folder-list-outer-container",
            folderListOuterContainerCollapsedClass: "folder-list-outer-container-collapsed",
            besideContentListContainerClass: "beside-content-list-container",
            besideContentListContainerCollapsedClass: "beside-content-list-collapsed",
            folderListCollapseExpandButton: "collapse-expand-button",
            collapseClass: "collapse-icon",
            expandButtonCcontainerClass: "expand-button-container",
            expandClass: "expand-icon",
            expandButtonId: "folderListExpandButton",
            besideContentListMobile: "beside-content-list-mobile",
            besideContentListMobileCollapsed: "beside-content-list-mobile-collapsed",
            besideContentListMobileExpanded: "beside-content-list-mobile-expanded"
        };

        this.initialize = function () {
            var besideContentListContainer = $("." + _self.options.besideContentListContainerClass);
            var folderList = besideContentListContainer.find("." + _self.options.folderListClass);

            folderList.on("loaded.jstree", function (event, data) {

                /** 
                * Open nodes on load (until x'th level) 
                */
                var depth = 5;
                data.instance.get_container().find('li').each(function (i) {
                    var selectedAttribute = $(this).data("selected");
                    if (selectedAttribute && selectedAttribute.toString() == "true") {
                        folderList.jstree(true).select_node(this);

                        if (data.instance.get_path($(this)).length <= depth) {
                            data.instance.open_node($(this));
                        }
                    }
                });
            }).jstree({
                "default": {
                    draggable: false
                }
            }).on("click", "li.jstree-node a", function () {
                document.location.href = this;
            });

            // scroll the folder container to right
            setTimeout(function () {
                var folderListContainer = $("." + _self.options.folderListContainerClass);

                if (folderListContainer.length == 0) {
                    throw "There is no folder container tag";
                };

                var node = folderListContainer.get(0);
                node.scrollLeft = node.scrollLeft + 10000;
            }, 500);

            // add expandButtonCcontainerClass class to the panel plus the expand button
            var panel = besideContentListContainer.closest(".panel")
                .addClass(_self.options.expandButtonCcontainerClass);
            panel.find(".panel-body").append("<div id='folderListExpandButton' class='hidden'></div>");

            var expandButton = $("#" + _self.options.expandButtonId);
            var collapsedButton = besideContentListContainer.find("." + _self.options.folderListCollapseExpandButton);
            var deviceWidth = effectiveDeviceWidth();

            //if (deviceWidth >= 500) {
            expandButton.on("click touchstart", function () {
                    besideContentListContainer
                        .removeClass(_self.options.besideContentListContainerCollapsedClass)
                        .addClass(_self.options.besideContentListContainerClass);

                    $("#" + _self.options.expandButtonId).addClass("hidden");
                });

            collapsedButton.on("click touchstart", function () {
                    besideContentListContainer
                        .removeClass(_self.options.besideContentListContainerClass)
                        .addClass(_self.options.besideContentListContainerCollapsedClass);

                    $("#" + _self.options.expandButtonId).removeClass("hidden");
                });
            //}
            //else {
            //    // mobile devices
            //    expandButton.removeClass("hidden");
            //    besideContentListContainer
            //        .removeClass(_self.options.besideContentListContainerClass)
            //        .addClass(_self.options.besideContentListMobile)
            //        .addClass(_self.options.besideContentListMobileCollapsed);

            //    expandButton.on("click", function () {
            //        expandButton.addClass("hidden");
            //        besideContentListContainer
            //            .removeClass(_self.options.besideContentListMobileCollapsed)
            //            .addClass(_self.options.besideContentListMobileExpanded);
            //    });

            //    collapsedButton.on("click", function () {
            //        expandButton.removeClass("hidden");
            //        besideContentListContainer
            //            .removeClass(_self.options.besideContentListMobileExpanded)
            //            .addClass(_self.options.besideContentListMobileCollapsed);
            //    });
            //}
        };
    };

    crm.project.PeopleInitializationWidget = function (widget) {
        var _self = this;

        this.options = {
            groupsAndAgentsRowCssClass: "groups-and-agents",
            visibleAllCheckboxName: "ProjectItemPermissionsPart.VisibleToAll"
        };

        var selectBoxesInitialized = false;
        this.initialize = function () {
            var checkBox = widget.element.find("input[name='" + _self.options.visibleAllCheckboxName + "']");
            var groupsAndAgentsRow = widget.element.find("." + _self.options.groupsAndAgentsRowCssClass);

            if (checkBox.length == 0) {
                console.log("There is no input with the name:" + _self.options.visibleAllCheckboxName);
                return;
            }

            if (groupsAndAgentsRow.length == 0) {
                console.log("There is no tag with the css:" + _self.options.groupsAndAgentsRowCssClass);
                return;
            }

            if (checkBox.get(0).checked) {
                groupsAndAgentsRow.hide();
            }
            else {
                if (!selectBoxesInitialized) {
                    groupsAndAgentsRow.find("select").chosen();
                    selectBoxesInitialized = true;
                }
            }

            checkBox.on("change", function (event) {
                groupsAndAgentsRow.find("select").chosen();
                if (checkBox.get(0).checked) {
                    groupsAndAgentsRow.hide();
                }
                else {
                    groupsAndAgentsRow.show();
                    if (!selectBoxesInitialized) {
                        groupsAndAgentsRow.find("select").chosen();
                        selectBoxesInitialized = true;
                    }
                }
            });
        };
    };

    crm.project.EditFolderWidget = function (widget) {
        var _self = this;

        this.options = {
            mainFormId: "folderForm",
            folderPickerCssClass: "folder-picker"
        };

        this.initialize = function () {
            var mainForm = $("#" + _self.options.mainFormId);
            var folderPicker = mainForm.find("." + _self.options.folderPickerCssClass);

            var selectedNode = folderPicker.find("li[data-selected='true']");
            // hide radio-buttons
            folderPicker.find("input[type='radio']").hide();

            folderPicker.on("loaded.jstree", function (event, data) {

                //if (selectedNode.length > 0) {
                //    folderPicker.jstree("select_node", selectedNode).trigger("select_node.jstree");
                //}

            }).jstree({
                "ui": {
                    "select_limit": 1  //only allow one node to be selected at a time
                },
                "core": {
                    multiple: false
                }
            }).bind("select_node.jstree", function (event, data) {
                var seletedFolder = data.node.data.id;
                folderPicker.find("input[type='radio'][value='" + seletedFolder + "']").prop('checked', true);
            });
        };
    };

    crm.project.EditFolderItemWidget = function (widget) {
        var _self = this;

        this.options = {
            mainFormId: "folderForm",
            folderPickerCssClass: "folder-picker"
        };

        this.initialize = function () {
            var model = new _self.ModelClass();

            var folderPicker = model.getFolderPicker();

            // hide radio-buttons
            var folderRadioButtons = folderPicker.find("input[type='radio']");
            folderRadioButtons.hide();
            var selectedFolder = folderRadioButtons.filter(":checked");

            folderPicker.on("loaded.jstree", function (event, data) {

                // it is a hack, but there is no easier way around it, the problem is, the tree remove the check attribute of the selected node
                // 
                if (selectedFolder.length > 0) {
                    setTimeout(function () {
                        folderPicker.find("input[type='radio'][value='" + selectedFolder.val() + "']").prop('checked', true)
                    }, 500);
                }
            }).jstree({
                "ui": {
                    "select_limit": 1  //only allow one node to be selected at a time
                },
                "core": {
                    multiple: false
                }
            }).bind("select_node.jstree", function (event, data) {
                var selectedFolder = data.node.data.id;

                selectedFolder = selectedFolder || "";
                var inputs = folderPicker.find("input[type='radio']");
                inputs.prop("checked", false);
                inputs.filter("[value='" + selectedFolder + "']").prop('checked', true);
            });
        };

        this.ModelClass = function () {
            var mainForm = $("#" + _self.options.mainFormId);
            var folderPicker = mainForm.find("." + _self.options.folderPickerCssClass);

            this.getFolderPicker = function () {
                return folderPicker;
            };

            this.getSelectedFolderElement = function () {
                return folderPicker.find("li[data-selected='true']");
            };
        };
    };

    crm.project.deleteDialogModel = function (widget) {
        var _self = this;

        this.options = {
            toolbarId: "discussionMenu",
            removeCssClass: "remove-discussion",
            deleteConfirmDialogId: "deleteConfirmDialog",
            deleteConfirmDialogYesButton: "deleteConfirmDialogYesButton",
            deleteConfirmDialogNoButton: "deleteConfirmDialogNoButton",
        };

        $.extend(_self.options, this.options);

        this.getDeleteDialog = function () {
            return $("#" + _self.options.deleteConfirmDialogId);
        };

        this.getDeleteLink = function () {
            var toolbar = $("#" + _self.options.toolbarId);
            return toolbar.find("." + _self.options.removeCssClass);
        };

        this.getDeleteDialogYesButton = function () {
            return $("#" + _self.options.deleteConfirmDialogYesButton);
        };

        this.getDeleteDialogNoButton = function () {
            return $("#" + _self.options.deleteConfirmDialogNoButton);
        };
    };

    crm.project.deleteController = function (widget, model) {
        var _self = this;

        crm.project.EditBaseWidget.apply(this, arguments);

        this.initialize = function () {
            var removeLink = model.getDeleteLink();
            removeLink.click(removeLinkClick);

            model.getDeleteDialogNoButton().click(function () {
                model.getDeleteDialog().dialog("close");
            });

            model.getDeleteDialogYesButton().click(deleteConfirmDialogYesButtonClickHandler);
        };

        var deleteConfirmDialogYesButtonClickHandler = function (event) {
            var deleteDialog = model.getDeleteDialog();
            var url = deleteDialog.data("url");

            var verificationToken = _self.getRequestVerificationToken();

            jqueryPost(url, "POST", verificationToken);

            event.preventDefault();
        };

        var removeLinkClick = function (event) {
            var deleteDialog = model.getDeleteDialog();
            deleteDialog.data("url", this.href);
            deleteDialog.data("id", $(this).data("id"));
            deleteDialog.dialog({ minHeight: 80, resizable: false });
            event.preventDefault();
        }
    };

    crm.project.wikiTabControlModel = function (widget) {
        var _self = this;

        this.options = {
            wikiContentFileSwitcherId: "wiki-content-file-switcher",
            contentSwitcherClass: "contentSwitcher",
            filesSwitcherClass: "fileSwitcher",
            besideContentClass: "beside-content-2",
            contentClass: "wiki-content"
        };

        var container = $("#" + _self.options.wikiContentFileSwitcherId);

        this.getContentTabHeader = function () {
            return container.find("." + _self.options.contentSwitcherClass);
        };

        this.getFilesTabHeader = function () {
            return container.find("." + _self.options.filesSwitcherClass);
        };

        this.getFilesTab = function () {
            return $("." + _self.options.besideContentClass);
        };

        this.getContentTab = function () {
            return $("." + _self.options.contentClass);
        };
    };

    crm.project.wikiTabsController = function (widget, model) {
        var _self = this;

        crm.project.EditBaseWidget.apply(this, arguments);

        this.initialize = function () {
            var contentSwitcherClass = model.getContentTabHeader();
            var filesSwitcherClass = model.getFilesTabHeader();
            var content = model.getContentTab();
            var besideContent = model.getFilesTab();

            contentSwitcherClass.click(function (e) {
                filesSwitcherClass.removeClass("selected");
                content.removeClass("hidden");
                besideContent.addClass("hidden");
                contentSwitcherClass.addClass("selected");
            });

            filesSwitcherClass.click(function (e) {
                contentSwitcherClass.removeClass("selected");
                besideContent.removeClass("hidden");
                content.addClass("hidden");
                filesSwitcherClass.addClass("selected");
            });
        };
    };

    crm.project.activityStreamDetailViewerController = function (widget, model) {
        var _self = this;

        this.initialize = function () {
            var icons = model.getDetailViewers();
            icons.click(function (event) {
                var container = model.getDetailContainerOfGivenIcon($(this));

                if (container.hasClass("hidden")) {
                    container.removeClass("hidden");
                }
                else {
                    container.addClass("hidden");
                }
            });
        };
    };

    crm.project.activityStreamDetailViewerModel = function (widget) {
        var _self = this;

        this.options = {
            detailViewerClass: "activity-detail-view",
            detailContainerClass: "detail"
        };

        this.getDetailViewers = function () {
            return widget.element.find("." + _self.options.detailViewerClass);
        }

        this.getDetailContainerOfGivenIcon = function (item) {
            return $("#" + item.data("detail-id"));
        };
    };

    crm.project.followerLinkController = function (widget, model) {
        var _self = this;

        this.initialize = function () {
            model.getFollowerLink().click(function (event) {

                var link = this;
                var $link = $(link);
                $.get(this.href, null, function (data) {
                    if (data.IsDone) {
                        var isFollowed = $link.data("follow").toLowerCase();

                        if (isFollowed == "true") {
                            link.href = $link.data("followlink");
                            $link.text($link.data("followtitle"));
                            $link.data("follow", "false");
                        }
                        else {
                            $link.data("follow", "true");
                            link.href = $link.data("unfollowlink");
                            $link.text($link.data("unfollowtitle"));
                        }
                    }
                });

                event.preventDefault();
            });
        };
    };

    crm.project.followerLinkModel = function (widget) {
        var _self = this;

        this.options = {
            followerLinkClass: "follow-link"
        };

        this.getFollowerLink = function () {
            return widget.element.parent().find("." + _self.options.followerLinkClass);
        }
    }

    crm.project.skypeTooltipController = function (widget, model) {

        var _self = this;

        var latestOpenContextMenu = null;

        this.initialize = function () {
            var links = model.getUserLinks()
            links.click(userLinkClickHandler);
            links.each(function () {
                var link = this;
                model.getContextMenuClosebutton(this).click(function (event) {
                    contextMenuCloseButton(event, link);
                });
            });
        };

        var contextMenuCloseButton = function (event, userLink) {
            var contextMenu = model.getContextMenu(userLink);

            contextMenu.hide();
            latestOpenContextMenu = null;
            event.preventDefault();
        }

        var userLinkClickHandler = function (event) {

            var contextMenu = model.getContextMenu(this);

            if (latestOpenContextMenu) {
                latestOpenContextMenu.hide();
            }

            contextMenu.show();
            latestOpenContextMenu = contextMenu;
            event.preventDefault();
        }
    };

    crm.project.skypeTooltipModel = function (widget) {
        var _self = this;

        this.options = {
            userLinkCssClass: "user-link",
            mainContainerClass: "activity-stream",
            subContainerClass: "activity-stream-item",
            contextMenuCssClass: "user-context-menu",
            contextMenuCloseButtonClass: "user-context-menu-close"
        };

        this.getContextMenu = function (userLink) {
            return $(userLink)
                .closest("." + _self.options.subContainerClass)
                .find("." + _self.options.contextMenuCssClass);
        };

        this.getContextMenuClosebutton = function (userLink) {
            return $(userLink)
                .closest("." + _self.options.subContainerClass)
                .find("." + _self.options.contextMenuCloseButtonClass);
        };

        this.getUserLinks = function () {
            return $("." + _self.options.mainContainerClass).find("." + _self.options.userLinkCssClass);
        };
    };

    crm.project.editProjectDashboadPortletsController = function (widget, model) {
        var _self = this;

        this.initialize = function () {
            var portletList = model.getPortletList();
            portletList.sortable({
                update: _self.updateOrderFields
            });
        };

        this.updateOrderFields = function () {
            var portletList = model.getPortletList();

            var order = 0;
            portletList.find("li").each(function () {
                var inputOrder = $(this).find("input[name*='Order']");
                inputOrder.val(order);
                order++;
            });
        };
    };

    crm.project.editProjectDashboadPortletsModel = function (widget) {
        var _self = this;

        this.options = {
            portletListClass: "portlet-list"
        };

        this.getPortletList = function () {
            return widget.element.find("." + _self.options.portletListClass);
        }
    }

    crm.project.editMilestoneController = function (widget, model) {
        var _self = this;

        this.initialize = function () {
            model.getStartTimeElement().datepicker();
            model.getEndtTimeElement().datepicker();
        };
    };

    crm.project.editMilestoneModel = function (widget) {
        var _self = this;

        this.options = {
            startTimeClass: "stat-date",
            endTimeClass: "end-date"
        };

        this.getStartTimeElement = function () {
            return widget.element.find("." + _self.options.startTimeClass);
        };

        this.getEndtTimeElement = function () {
            return widget.element.find("." + _self.options.endTimeClass);
        };
    };

    crm.project.SyncLinkControllerBase = function (model, widget) {
        var _self = this;

        crm.project.EditBaseWidget.apply(this, arguments);

        this.formatDate = function formatDate(date) {
            var hours = date.getHours();
            var minutes = date.getMinutes();
            var ampm = hours >= 12 ? 'PM' : 'AM';
            hours = hours % 12;
            hours = hours ? hours : 12; // the hour '0' should be '12'
            hours = hours < 10 ? '0' + hours : hours;
            minutes = minutes < 10 ? '0' + minutes : minutes;
            var strTime = hours + ':' + minutes + ' ' + ampm;
            var months = date.getMonth() + 1;
            months = months < 10 ? '0' + months : months;
            var days = date.getDate();
            days = days < 10 ? '0' + days : days;
            return days + "/" + months + "/" + date.getFullYear() + "  " + strTime;
        };
    }

    /*
    This controller and corresponding model and widget are related to the SuiteCRMProjectDriver in Orchard.SuiteCRM.Connector.
    But since the content generated by the driver only has been represented with Projects, I added these jasvascript code here in order
    to prevent adding another js file in Project detail page
    */
    crm.project.syncProjectLinkController = function (model, widget) {
        var _self = this;

        crm.project.SyncLinkControllerBase.apply(this, arguments);

        this.initialize = function () {
            var syncLink = model.getSyncLink();
            syncLink.click(syncClick);
            var copyFROMSuiteCRM = model.getCopyFromSuiteCRMButton();

            if (!syncLink.data("externalid")) {
                copyFROMSuiteCRM.attr("disabled", true);
                copyFROMSuiteCRM.addClass("disabled");
            }

            copyFROMSuiteCRM.click(copyFromSuiteCRMClickHandler);
            model.getCopyToSuiteCRMButton().click(copyToSuiteCRMClickHandler);
            var infoModal = model.getInfoModal();
            infoModal.find("a").click(function () { infoModal.hide(); });
            model.getCancelButton().click(cancelButtonClickHandler)
        };

        var cancelButtonClickHandler = function (e) {
            model.getDialog().dialog('close');
        };

        var copyFromSuiteCRMClickHandler = function (e) {
            var syncLink = model.getSyncLink();
            sync(e, syncLink.data("copyfrom"))
        };

        var sync = function (e, url) {
            var syncLink = model.getSyncLink();
            var dialogElement = model.getDialog();
            var syncTasksCheckbox = model.getSyncTicketsCheckbox(dialogElement);
            var syncSubTasksCheckbox = model.getSyncSubTasksCheckbox(dialogElement);
            var doNotOverrideNewerValuesCheckbox = model.getDoNotOverrideNewerValuesCheckbox(dialogElement);

            var toSubmitData = {
                Projects: [{
                    SyncTasks: syncTasksCheckbox.is(":checked"),
                    SyncSubTasks: syncSubTasksCheckbox.is(":checked"),
                    DoNotOverrideNewerValues: doNotOverrideNewerValuesCheckbox.is(":checked"),
                    OrchardCollaborationProjectId: syncLink.data("projectid"),
                    SuiteCRMId: syncLink.data("externalid")
                }]
            };

            var verificationToken = _self.getRequestVerificationToken();
            $.extend(toSubmitData, verificationToken);

            var infoModal = model.getInfoModal();
            infoModal.show();

            $.ajax({
                url: url,
                type: "POST",
                data: toSubmitData,
                error: function (e) {
                    infoModal.find(".loading").hide();
                    infoModal.find(".error").show();
                    infoModal.find("a").show();
                }
            }).done(function (response) {
                if (response.Errors.length > 0) {
                    infoModal.find(".loading").hide();
                    infoModal.find(".error").show();
                    infoModal.find("a").show();
                    return;
                }

                if (response.IsDone) {
                    var syncTime = model.getSyncTimeElement();
                    var date = new Date(response.Data.Projects[0].LastSyncTime);
                    syncTime.text(_self.formatDate(date));
                    syncLink.data("externalid", response.Data.Projects[0].SuiteCRMProject.Id);
                    syncLink.data("projectid", response.Data.Projects[0].OrchardCollaborationProject.Id);
                    model.getCopyFromSuiteCRMButton().attr("disabled", false).removeClass("disabled");
                    infoModal.hide();
                }
            });

            dialogElement.dialog('close');
        };

        var copyToSuiteCRMClickHandler = function (e) {
            var syncLink = model.getSyncLink();
            sync(e, syncLink.data("copyto"))
        };

        var syncClick = function (e) {
            e.preventDefault();
            var dialogElement = model.getDialog();
            dialogElement.dialog({ minHeight: 140, resizable: false });
        };
    };

    crm.project.syncTicketLinkController = function (model, widget) {
        var _self = this;

        crm.project.SyncLinkControllerBase.apply(this, arguments);

        this.initialize = function () {
            model.getSyncLink().click(syncClick);
            model.getCopyToSuiteCRMButton().click(copyToSuiteCRMClickHandler);
            var infoModal = model.getInfoModal();
            infoModal.find("a").click(function () { infoModal.hide(); });
            model.getCancelButton().click(cancelButtonClickHandler)
        };

        var cancelButtonClickHandler = function (e) {
            model.getDialog().dialog('close');
        };

        var copyToSuiteCRMClickHandler = function (e) {
            var syncLink = model.getSyncLink();
            url = syncLink.data("copyto");
            var dialogElement = model.getDialog();
            var syncSubTasksCheckbox = model.getSyncSubTasksCheckbox(dialogElement);
            var doNotOverrideNewerValuesCheckbox = model.getDoNotOverrideNewerValuesCheckbox(dialogElement);

            var toSubmitData = {
                Tasks: [{
                    SyncSubTasks: syncSubTasksCheckbox.is(":checked"),
                    DoNotOverrideNewerValues: doNotOverrideNewerValuesCheckbox.is(":checked"),
                    OrchardCollaborationTicketId: syncLink.data("taskid"),
                    SuiteCRMId: syncLink.data("externalid"),
                    IsProjectTask: syncLink.data("isprojecttask")
                }]
            };

            var verificationToken = _self.getRequestVerificationToken();
            $.extend(toSubmitData, verificationToken);

            var infoModal = model.getInfoModal();
            infoModal.show();

            $.ajax({
                url: url,
                type: "POST",
                data: toSubmitData,
                error: function (e) {
                    infoModal.find(".loading").hide();
                    infoModal.find(".error").show();
                    infoModal.find("a").show();
                }
            }).done(function (response) {
                if (response.Errors.length > 0) {
                    infoModal.find(".loading").hide();
                    infoModal.find(".error").show();
                    infoModal.find("a").show();
                    return;
                }

                if (response.IsDone) {
                    var syncTime = model.getSyncTimeElement();
                    model.getSyncTimeLabelElement().removeClass("hidden");
                    model.getNotYetSyncElement().hide();
                    model.getViewInSuiteCRMLinkElement().removeClass("hidden").attr("href", response.Data[0].SuiteCRMUrl);
                    syncTime.removeClass("hidden");
                    var date = new Date(response.Data[0].LastSyncTime);
                    syncTime.text(_self.formatDate(date));
                    infoModal.hide();
                }
            });

            dialogElement.dialog('close');
        };

        var syncClick = function (e) {
            e.preventDefault();
            var dialogElement = model.getDialog();
            dialogElement.dialog({ minHeight: 140, minWidth: 400, resizable: false });
        };
    };

    crm.project.SyncToSuiteCRMLinkModel = function (widget) {
        var _self = this;

        this.options = {
            syncLinkClass: "sync-hyperlink",
            dialogId: "syncProjectDialog",
            copyFromButtonClass: "copy-from-suitecrm",
            copyToButtonClass: "copy-to-suitecrm",
            syncTimeClass: "sync-time",
            syncTimeLabelClass: "last-sync-time-label",
            notSyncYetClass: "not-sync-yet",
            cancelButtonClass: "suitecrm-cancel-sync",
            viewInSuitecrmLinkClass: "view-in-suitecrm"
        };

        this.getInfoModal = function () {
            return $("#syncInfoModal");
        };

        this.getCancelButton = function () {
            return _self.getDialog().find("." + _self.options.cancelButtonClass);
        };

        this.getSyncTimeElement = function () {
            return widget.element.find("." + _self.options.syncTimeClass);
        };

        this.getViewInSuiteCRMLinkElement = function () {
            return widget.element.find("." + _self.options.viewInSuitecrmLinkClass);
        };

        this.getSyncTimeLabelElement = function () {
            return widget.element.find("." + _self.options.syncTimeLabelClass);
        };

        this.getNotYetSyncElement = function () {
            return widget.element.find("." + _self.options.notSyncYetClass);
        };

        this.getSyncLink = function () {
            return widget.element.find("." + _self.options.syncLinkClass);
        };

        this.getCopyToSuiteCRMButton = function () {
            return _self.getDialog().find("." + _self.options.copyToButtonClass);
        };

        this.getCopyFromSuiteCRMButton = function () {
            return _self.getDialog().find("." + _self.options.copyFromButtonClass);
        };

        this.getSyncTicketsCheckbox = function (dialog) {
            return dialog.find("input[name='syncTickets']");
        }

        this.getSyncSubTasksCheckbox = function (dialog) {
            return dialog.find("input[name='syncSubTasks']");
        }

        this.getDoNotOverrideNewerValuesCheckbox = function (dialog) {
            return dialog.find("input[name='doNotOverrideNewerValues']");
        }

        this.getDialog = function () {
            return $("#" + _self.options.dialogId);
        };
    }

    crm.project.MilestoneBase = function () {

        var _self = this;

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

        this.getPart = function (shape, partName) {
            if (!shape.ContentItem && !shape.ContentItem.Parts && !shape.ContentItem.Parts.length) {
                return null;
            }

            for (var i = 0; i < shape.ContentItem.Parts.length; i++) {
                var part = shape.ContentItem.Parts[i];
                if (part.PartDefinition && part.PartDefinition.Name == partName) {
                    return part;
                }
            }

            return null;
        }

        this.getSubShape = function (shape, metadataType, prefix) {

            var isMatch = function (metadata, metadataType, prefix) {
                var typeMatch = metadata.Type == metadataType;

                if (typeMatch && prefix) {
                    return prefix == metadata.Prefix;
                }

                return typeMatch;
            }

            // Content
            if (shape.Content && shape.Content.length) {
                for (var i = 0; i < shape.Content.length; i++) {
                    if (isMatch(shape.Content[i].Metadata, metadataType, prefix)) {
                        return shape.Content[i];
                    }
                }
            }

            // Header
            if (shape.Header && shape.Header.length) {
                for (var i = 0; i < shape.Header.length; i++) {
                    if (isMatch(shape.Header[i].Metadata, metadataType, prefix)) {
                        return shape.Header[i];
                    }
                }
            }

            // Footer
            if (shape.Footer && shape.Footer.length) {
                for (var i = 0; i < shape.Footer.length; i++) {
                    if (isMatch(shape.Footer[i].Metadata, metadataType, prefix)) {
                        return shape.Footer[i];
                    }
                }
            }

            return null;
        }

        this.addMainAggregateFields = function (model) {
            model.totalSize = 0;

            for (var j = 0; j < model.Items.length; j++) {
                var item = model.Items[j];

                var part = _self.getPart(item, "AttachToMilestonePart");

                if (part.Record.Size) {
                    model.totalSize += part.Record.Size;
                }
            }
        };
    };

    crm.project.MilestoneGanttChartController = function (contentContainer, dataContainer) {
        var _self = this;

        crm.project.MilestoneBase.apply(this, arguments);
        var data = JSON.parse($("#" + dataContainer).html());
        data.Model = JSON.parse(data.Model.join(""));

        var T = function (key, text) {

            return _self.translate(data, key, text);
        };

        var app_handle_listing_horisontal_scroll = function (listing_obj) {
            //get table object   
            table_obj = $('.milestone-gantt-items', listing_obj);

            //get count fixed collumns params
            count_fixed_collumns = table_obj.attr('data-count-fixed-columns')

            if (count_fixed_collumns > 0) {
                //get wrapper object
                wrapper_obj = $('.milestone-gantt-container', listing_obj);

                wrapper_left_margin = 0;

                table_collumns_width = new Array();
                table_collumns_margin = new Array();

                //calculate wrapper margin and fixed column width
                $('th', table_obj).each(function (index) {
                    if (index < count_fixed_collumns) {
                        wrapper_left_margin += $(this).outerWidth();
                        table_collumns_width[index] = $(this).outerWidth();
                    }
                })

                //calcualte margin for each column  
                $.each(table_collumns_width, function (key, value) {
                    if (key == 0) {
                        table_collumns_margin[key] = wrapper_left_margin;
                    }
                    else {
                        next_margin = 0;
                        $.each(table_collumns_width, function (key_next, value_next) {
                            if (key_next < key) {
                                next_margin += value_next;
                            }
                        });

                        table_collumns_margin[key] = wrapper_left_margin - next_margin;
                    }
                });

                //set wrapper margin               
                if (wrapper_left_margin > 0) {
                    wrapper_obj.css('cssText', 'margin-left:' + wrapper_left_margin + 'px !important; width: auto')
                }

                //set position for fixed columns
                $('tr', table_obj).each(function () {

                    //get current row height
                    current_row_height = $(this).outerHeight();

                    $('th,td', $(this)).each(function (index) {

                        //set row height for all cells
                        $(this).css('height', current_row_height)

                        //set position 
                        if (index < count_fixed_collumns) {
                            $(this).css('position', 'absolute')
                                   .css('margin-left', '-' + table_collumns_margin[index] + 'px')
                                   .css('width', table_collumns_width[index])

                            $(this).addClass('table-fixed-cell')
                        }
                    })
                })
            }
        };

        var makeFirstColumnsFix = function (domNode) {
            var container = domNode.find(".milestoneGantt");
            var parentOfset = container.offset();
            var table = container.find(".milestone-gantt-items");

            var firstColumnsWidth = 0;
            table.find("tbody tr").each(function () {
                var tds = $(this).find("td");
                if (tds.length < 3) {
                    return;
                }

                var idColumn = tds.slice(0, 1);
                var titleColumn = tds.slice(1, 2);
                var thirdColumn = tds.slice(2, 3);

                var idColumnOffset = idColumn.offset();
                var titleColumnOffset = titleColumn.offset();
                var idColumnWidth = idColumn.outerWidth();
                var titleColumnHeight = titleColumn.outerHeight();
                var idColumnHeight = idColumn.outerHeight();
                var titleColumnWidth = titleColumn.outerWidth();
                firstColumnsWidth = idColumnWidth + titleColumnWidth + 30;

                var idColumnLeft = (idColumnOffset.left - parentOfset.left).toString() + "px";
                var idColumnTop = (idColumnOffset.top - parentOfset.top).toString() + "px";
                idColumn.css({ position: "absolute", left: idColumnLeft, top: idColumnTop });

                var titleColumnLeft = (titleColumnOffset.left - parentOfset.left + 25).toString() + "px";
                var titleColumnTop = (titleColumnOffset.top - parentOfset.top).toString() + "px";
                titleColumn.css({ position: "absolute", left: titleColumnLeft, top: titleColumnTop });

                var maxHeight = titleColumnHeight > idColumnHeight ? titleColumnHeight : idColumnHeight;
                var thirdColumnHeight = thirdColumn.outerHeight();
                if (maxHeight > thirdColumnHeight) {
                    thirdColumn.css("height", maxHeight + "px");
                    titleColumn.css("height", maxHeight + "px");
                    idColumn.css("height", maxHeight + "px");
                }
            });

            container.find(".milestone-gantt-container").get(0).style.marginLeft = firstColumnsWidth + "px";
        }

        var setAsyncState = function (state) {
            data.asyncState = state;
            this.setState(this.state);
        };

        var filterTicketsClickHandler = function (filterText) {
            data.filterText = filterText;
            this.setState(this.state);
        };

        var filterTickets = function (data, filterText) {
            filterText = filterText || "";
            filterText = filterText.toLowerCase();
            data.Model.FilteredItems = [];

            if (filterText == "") {
                data.Model.FilteredItems = data.Model.Items;
                return;
            }

            for (var i = 0; i < data.Model.Items.length; i++) {
                var item = data.Model.Items[i];
                var part = _self.getPart(item, "TicketPart");
                if (part.Record.Identity && part.Record.Identity.Id == filterText) {
                    data.Model.FilteredItems.push(item);
                    continue;
                }

                var ticketTitle = part.Record.Title.toLowerCase();
                if (ticketTitle.indexOf(filterText) !== -1) {
                    data.Model.FilteredItems.push(item);
                }
            }
        };

        var getInitialState = function () {
            data.asyncState = "normal";
            data.filterText = "";
            return data;
        };

        var render = function () {
            var model = {
                state: data.asyncState,
                filterText: data.filterText,
                shape: this.state,
                root: {
                    T: T,
                    Routes: data.Routes,
                    Controller: _self,
                    actions: { setAsyncState: this.setAsyncState, filter: this.filterTicketsClickHandler }
                }
            };

            filterTickets(model.shape, model.filterText);
            _self.addMainAggregateFields(this.state.Model);

            return React.createElement(
               "div",
               null,
               React.createElement(orchardcollaboration.react.allComponents.InfoPage, model),
               React.createElement(orchardcollaboration.react.allComponents.MilestoneGantt, model));
        };

        var milestoneComponent = React.createClass({
            getInitialState: getInitialState,
            render: render,
            setAsyncState: setAsyncState,
            filterTicketsClickHandler: filterTicketsClickHandler
        });

        var element = React.createElement(milestoneComponent);
        var _reactComponent = ReactDOM.render(element, document.getElementById(contentContainer));
    };

    crm.project.MilestonePlannerController = function (contentContainer, dataContainer) {
        var _self = this;

        // inherits from the base class
        crm.project.MilestoneBase.apply(this, arguments);

        var data = JSON.parse($("#" + dataContainer).html());
        data.Model = JSON.parse(data.Model.join(""));

        // Translate function
        var T = function (key, text) {
            return _self.translate(data, key, text);
        };

        var getInitialState = function () {
            data.asyncState = "normal";
            return data;
        };

        var setAsyncState = function (state) {
            this.state.asyncState = state;
            this.setState(this.state);
        };

        var render = function () {

            var shape = this.state;

            var model = {
                shape: shape,
                state: shape.asyncState,
                root: {
                    T: T,
                    Routes: data.Routes,
                    Controller: _self,
                    actions: {
                        ticketMenu: ticketContextMenuAction,
                        setAsyncState: this.setAsyncState
                    }
                }
            };

            _self.addMainAggregateFields(this.state.Model);

            // Add necessary menu items for tickets in current milestone
            var milestoneTicketMenu = [
                {
                    Text: T("SendToBacklog", "Send to Backlog"),
                    Id: "SendToBacklog"
                },
                {
                    Text: T("MoveToTop", "Move to top"),
                    Id: "MoveToTop"
                },
                {
                    Text: T("MoveToBottom", "Move to bottom"),
                    Id: "MoveToBottom"
                },
                {
                    Text: T("Edit"),
                    Id: "Edit"
                }
            ];

            // Add necessary menu items for tickets of backlog
            var backlogTicketMenu = [
                {
                    Text: T("SendToTop", "Send to the top of the milestone"),
                    Id: "SendFromBackLogToTop"
                },
                {
                    Text: T("SendToBottom", "Send to the bottom of the milestone "),
                    Id: "SendFromBackLogToBottom"
                },
                {
                    Text: T("Edit"),
                    Id: "Edit"
                }
            ];

            if (data.Model.CanEdit) {
                for (var i = 0; i < shape.Model.Items.length; i++) {
                    var subShape = _self.getSubShape(shape.Model.Items[i], "Parts_Ticket_TableRow");
                    subShape.Model.Menu = milestoneTicketMenu;
                }

                for (var i = 0; i < shape.Model.BacklogMembers.length; i++) {
                    var subShape = _self.getSubShape(shape.Model.BacklogMembers[i], "Parts_Ticket_TableRow");
                    subShape.Model.Menu = backlogTicketMenu;
                }
            }

            return React.createElement(
            "div",
            null,
            React.createElement(orchardcollaboration.react.allComponents.InfoPage, model),
            React.createElement(orchardcollaboration.react.allComponents.MilestonePlanner, model));
        };

        var componentDidUpdate = function () {
            this.applyWidgets();
        };

        var componentDidMount = function () {
            this.applyWidgets();
        };

        var applyWidgets = function () {

            if (!data.Model.CanEdit) {
                return;
            }

            var domNode = $(ReactDOM.findDOMNode(this));

            var _this = this;

            sortableHelper = function (e, ui) {
                ui.children().each(function () {
                    $(this).width($(this).width());
                });
                return ui;
            };

            domNode.find(".current-milestone tbody").sortable({
                helper: sortableHelper,
                cancel: ".pivot",
                stop: function () {
                    handleSortableUpdate(domNode.find(".current-milestone"), false);
                }
            }).disableSelection();

            domNode.find(".backlog tbody").sortable({
                helper: sortableHelper,
                cancel: ".pivot",
                stop: function () {
                    handleSortableUpdate(domNode.find(".backlog"), true);
                }
            }).disableSelection();
        };

        var handleSortableUpdate = function (domNode, isBacklog) {

            var sortableNode = domNode.find("tbody");
            var ids = sortableNode.sortable('toArray', { attribute: 'data-id' });

            // check whether the list is changed or not, in case of no change, nothing must send back to server
            var originalList = isBacklog ? data.Model.BacklogMembers : data.Model.Items;
            if (ids.length == originalList.length) {
                var listOrderIsChanged = false;
                for (var i = 0; i < ids.length; i++) {
                    if (ids[i] != originalList[i].ContentItem.Id) {
                        listOrderIsChanged = true;
                        break;
                    }
                }

                if (!listOrderIsChanged) {
                    sortableNode.sortable('cancel');
                    return;
                }
            }

            var toSubmitData = {
                ProjectId: data.Model.ProjectId,
                Items: []
            };

            for (var i = 0; i < ids.length; i++) {
                toSubmitData.Items.push({
                    MilestoneId: isBacklog ? data.Model.BacklogId : data.Model.MilestoneId,
                    ContentItemId: ids[i],
                    OrderId: i
                });
            }

            var editBaseWidget = new crm.project.EditBaseWidget();
            var verificationToken = editBaseWidget.getRequestVerificationToken();
            $.extend(toSubmitData, verificationToken);

            var url = data.Routes.UpdateMilestoneItems;

            sortableNode.sortable('cancel');
            updateList(isBacklog ? null : ids, isBacklog ? ids : null);
            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                type: "POST",
                url: url,
                data: toSubmitData,
                error: function (e) {
                    data.asyncState = "error";
                    _reactComponent.setState(data);
                }
            }).done(function () {
                data.asyncState = "normal";
                updateList(isBacklog ? null : ids, isBacklog ? ids : null);

                _reactComponent.setState(data);
            });
        };

        var updateList = function (ids, backlogIds) {
            var newItems = [];

            if (ids) {
                for (var i = 0; i < ids.length; i++) {
                    for (var j = 0; j < data.Model.Items.length; j++) {
                        if (ids[i] == data.Model.Items[j].ContentItem.Id) {
                            newItems.push(data.Model.Items[j]);
                        }
                    }

                    for (var j = 0; j < data.Model.BacklogMembers.length; j++) {
                        if (ids[i] == data.Model.BacklogMembers[j].ContentItem.Id) {
                            newItems.push(data.Model.BacklogMembers[j]);
                        }
                    }
                }
            }

            if (backlogIds) {
                var newBackLogMembers = [];
                for (var i = 0; i < backlogIds.length; i++) {
                    for (var j = 0; j < data.Model.BacklogMembers.length; j++) {
                        if (backlogIds[i] == data.Model.BacklogMembers[j].ContentItem.Id) {
                            newBackLogMembers.push(data.Model.BacklogMembers[j]);
                        }
                    }

                    for (var j = 0; j < data.Model.Items.length; j++) {
                        if (backlogIds[i] == data.Model.Items[j].ContentItem.Id) {
                            newBackLogMembers.push(data.Model.Items[j]);
                        }
                    }
                }

                data.Model.BacklogMembers = newBackLogMembers;
            }

            // refreshing the model must be done in the last step, in order to prevent side effect
            // in updating the backlogIds
            if (ids) {
                data.Model.Items = newItems;
            }
        }

        var ticketContextMenuAction = function (actionName, contentItemId) {
            var milestoneIds = [];
            for (var j = 0; j < data.Model.Items.length; j++) {
                if (data.Model.Items[j].ContentItem.Id != contentItemId) {
                    milestoneIds.push(data.Model.Items[j].ContentItem.Id);
                }
            }

            var backlogIds = [];
            for (var j = 0; j < data.Model.BacklogMembers.length; j++) {
                if (data.Model.BacklogMembers[j].ContentItem.Id != contentItemId) {
                    backlogIds.push(data.Model.BacklogMembers[j].ContentItem.Id);
                }
            }

            var toSubmitData = {
                ProjectId: data.Model.ProjectId,
                Items: []
            };

            var editBaseWidget = new crm.project.EditBaseWidget();
            var verificationToken = editBaseWidget.getRequestVerificationToken();
            $.extend(toSubmitData, verificationToken);
            var url = data.Routes.UpdateMilestoneItems;

            switch (actionName) {
                case "Edit":
                    var ticketDisplayRoute = data.Routes.EditTicket;
                    ticketDisplayRoute = decodeURI(ticketDisplayRoute);
                    window.location.href = ticketDisplayRoute.replace("{id}", contentItemId);
                    return;
                case "MoveToBottom":
                    milestoneIds.push(contentItemId);

                    for (var i = 0; i < milestoneIds.length; i++) {
                        toSubmitData.Items.push({
                            MilestoneId: data.Model.MilestoneId,
                            ContentItemId: milestoneIds[i],
                            OrderId: i
                        });
                    } break;
                case "MoveToTop":
                case "SendFromBackLogToTop":
                    milestoneIds.splice(0, 0, contentItemId);

                    for (var i = 0; i < milestoneIds.length; i++) {
                        toSubmitData.Items.push({
                            MilestoneId: data.Model.MilestoneId,
                            ContentItemId: milestoneIds[i],
                            OrderId: i
                        });
                    }
                    break;

                case "SendFromBackLogToBottom":
                    milestoneIds.push(contentItemId);

                    for (var i = 0; i < milestoneIds.length; i++) {
                        toSubmitData.Items.push({
                            MilestoneId: data.Model.MilestoneId,
                            ContentItemId: milestoneIds[i],
                            OrderId: i
                        });
                    }
                    break;

                case "SendToBacklog":
                    backlogIds.push(contentItemId);
                    for (var i = 0; i < backlogIds.length; i++) {
                        toSubmitData.Items.push({
                            MilestoneId: data.Model.BacklogId,
                            ContentItemId: backlogIds[i],
                            OrderId: i
                        });
                    }
                    break;
            }

            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                type: "POST",
                url: url,
                data: toSubmitData,
                error: function (e) {
                    data.asyncState = "error";
                    _reactComponent.setState(data);
                }
            }).done(function () {
                updateList(milestoneIds, backlogIds);
                data.asyncState = "normal";

                // make a deep copy
                var jsonData = JSON.stringify(data);
                data = JSON.parse(jsonData);

                _reactComponent.setState(data);
            });
        };

        // main component
        var milestoneComponent = React.createClass({
            getInitialState: getInitialState,
            setAsyncState: setAsyncState,
            render: render,
            componentDidUpdate: componentDidUpdate,
            componentDidMount: componentDidMount,
            applyWidgets: applyWidgets
        });

        var element = React.createElement(milestoneComponent);
        var _reactComponent = ReactDOM.render(element, document.getElementById(contentContainer));
    };

    crm.project.MilestoneController = function (contentContainer, dataContainer) {
        var _self = this;

        crm.project.MilestoneBase.apply(this, arguments);
        var data = JSON.parse($("#" + dataContainer).html());
        data.mode = "ticketPerColumn";
        data.Model = JSON.parse(data.Model.join(""));

        data.showModal = false;

        $(document).on('focusin', function (e) {
            if ($(e.target).closest(".mce-window").length || $(e.target).closest(".moxman-window").length) {
                e.stopImmediatePropagation();
            }
        });

        var T = function (key, text) {

            return _self.translate(data, key, text);
        };

        var setAsyncState = function (state) {
            data.asyncState = state;
            this.setState(this.state);
        };

        var filterTicketsClickHandler = function (filterText) {
            data.filterText = filterText;
            this.setState(this.state);
        };

        var filterTickets = function (data, filterText) {
            filterText = filterText || "";
            filterText = filterText.toLowerCase();
            data.Model.FilteredItems = [];

            if (filterText == "") {
                data.Model.FilteredItems = data.Model.Items;
                return;
            }

            for (var i = 0; i < data.Model.Items.length; i++) {
                var item = data.Model.Items[i];
                var part = _self.getPart(item, "TicketPart");
                if (part.Record.Identity && part.Record.Identity.Id == filterText) {
                    data.Model.FilteredItems.push(item);
                    continue;
                }

                var ticketTitle = part.Record.Title.toLowerCase();
                if (ticketTitle.indexOf(filterText) !== -1) {
                    data.Model.FilteredItems.push(item);
                }
            }
        };

        var getInitialState = function () {
            data.asyncState = "normal";
            data.filterText = "";
            return data;
        };

        var addAggregateFieldsToStatusRecords = function (model) {
            // Add aggregation fields to the status records (Number of tickets plus their associated size)
            for (var i = 0; i < model.StatusRecords.length; i++) {
                var statusRecord = model.StatusRecords[i];
                statusRecord.count = 0;
                statusRecord.size = 0;

                for (var j = 0; j < model.Items.length; j++) {
                    var item = model.Items[j];

                    if ((item.StatusId == null && statusRecord.Id == 0) ||
									 item.StatusId == statusRecord.Id) {
                        statusRecord.count++;

                        var part = _self.getPart(item, "AttachToMilestonePart");

                        if (part.Record.Size) {
                            statusRecord.size += part.Record.Size;
                        }
                    }
                }
            }
        };

        var closeEditTicketModal = function () {
            data.showEditModal = false;
            _reactComponent.setState(data);
        }

        var closeAddCommentModal = function () {
            data.showAddCommentModal = false;
            _reactComponent.setState(data);
        }

        var createModel = function () {
            var model = {
                mode: data.mode,
                state: data.asyncState,
                filterText: data.filterText,
                showEditModal: data.showEditModal,
                editModalData: data.editModalData,
                showDisplayModal: data.showDisplayModal,
                showAddCommentModal: data.showAddCommentModal,
                selectedTicketId: data.selectedTicketId,
                displayModalShape: data.displayModalShape,
                shape: this.state,
                root: {
                    T: T,
                    Routes: data.Routes,
                    Controller: _self,
                    actions: {
                        setAsyncState: this.setAsyncState,
                        filter: this.filterTicketsClickHandler,
                        ticketMenu: ticketContextMenuAction,
                        closeEditTicketModal: closeEditTicketModal,
                        closeAddCommentModal: closeAddCommentModal,
                        saveComment: saveComment,
                        saveTicket: saveTicket,
                        changeMode: changeMode
                    }
                }
            };

            if (data.editModalData) {
                var fullEditModelUrl = model.root.Routes.EditTicket;
                fullEditModelUrl = decodeURI(fullEditModelUrl);
                fullEditModelUrl = fullEditModelUrl.replace("{id}", data.editModalData.id);
                model.fullEditModelUrl = fullEditModelUrl;

                var ticketShape = _self.getSubShape(data.editModalShape, "EditorTemplate", "TicketPart");
                data.editModalData.priorities = ticketShape.Model.Priorities;
                data.editModalData.priorityId = ticketShape.Model.PriorityId;
                data.editModalData.ticketTypes = ticketShape.Model.Types;
                data.editModalData.ticketTypeId = ticketShape.Model.TypeId;
            }

            filterTickets(model.shape, model.filterText);
            addAggregateFieldsToStatusRecords(this.state.Model);
            _self.addMainAggregateFields(this.state.Model);

            // Add necessary menu items for tickets in current milestone
            var milestoneTicketMenu = [
                {
                    Text: T("Edit", "Edit"),
                    Id: "EditTicket"
                },
                {
                    Text: T("AddComment", "Add Comment"),
                    Id: "AddComment"
                },
                {
                    Text: T("View", "View"),
                    Id: "ViewTicket"
                }
            ];

            for (var i = 0; i < model.shape.Model.Items.length; i++) {
                var subShape = _self.getSubShape(model.shape.Model.Items[i], "Parts_Ticket_Pinboard");

                var menu = milestoneTicketMenu.slice(0);

                if (!subShape.Model.CurrentUserCanEditItem) {
                    menu = menu.slice(1);
                }

                subShape.Model.Menu = menu;
            }

            return model;
        };

        var changeMode = function (mode) {
            data.mode = mode;
            _reactComponent.setState(data);
        }

        var render = function () {
            var model = createModel.call(this);

            var ticketsModel = model.mode == "ticketPerRow" ?
                React.createElement(orchardcollaboration.react.allComponents.MilestoneTickets, model) :
                React.createElement(orchardcollaboration.react.allComponents.MilestoneTicketsStatusBars, model);

            return React.createElement(
               "div",
               null,
               React.createElement(orchardcollaboration.react.allComponents.InfoPage, model),
               React.createElement(orchardcollaboration.react.allComponents.EditTicketModal, model),
               React.createElement(orchardcollaboration.react.allComponents.AddCommentModal, model),
               ticketsModel);
        };

        var saveComment = function (commentData) {
            var url = data.Routes.AddComment;

            var toSubmitData = {
                contentId: commentData.id,
                comment: commentData.comment
            }

            var editBaseWidget = new crm.project.EditBaseWidget();
            var verificationToken = editBaseWidget.getRequestVerificationToken();

            $.extend(toSubmitData, verificationToken);
            data.showAddCommentModal = false;
            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                type: "POSt",
                url: url,
                data: toSubmitData,
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

                data.asyncState = "normal";
                _reactComponent.setState(data);
            });
        };

        var saveTicket = function (ticket) {
            var quickTicketUpdateUrl = data.Routes.QuickTicketUpdate;

            var toSubmitData = {
                Ids: [ticket.id],
                Title: ticket.title,
                TypeId: ticket.ticketTypeId,
                PriorityId: ticket.priorityId,
                Description: $('<div/>').html(ticket.description).text(),
                UpdateDescription: true,
                UpdatePriority: true,
                UpdateTypeId: true,
                displyType: "Pinboard"
            };

            var editBaseWidget = new crm.project.EditBaseWidget();
            var verificationToken = editBaseWidget.getRequestVerificationToken();

            $.extend(toSubmitData, verificationToken);

            data.showEditModal = false;
            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                type: "POSt",
                url: quickTicketUpdateUrl,
                data: toSubmitData,
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

                data.asyncState = "normal";

                if (response.Data) {
                    var responseDataTickets = JSON.parse(response.Data.Tickets);
                    for (var i = 0; i < responseDataTickets.length; i++) {
                        var ticketPart = _self.getPart(responseDataTickets[i], "TicketPart");
                        responseDataTickets[i].StatusId = ticketPart.Record.Status != null ? ticketPart.Record.Status.Id : null;
                    }

                    replaceTicket(responseDataTickets[0]);
                }

                _reactComponent.setState(data);
            });
        }

        var replaceTicket = function (newTicket) {
            for (var i = 0; i < data.Model.Items.length; i++) {
                if (newTicket.ContentItem.Id === data.Model.Items[i].ContentItem.Id) {
                    data.Model.Items[i] = newTicket;
                    break;
                }
            }
        }

        var ticketContextMenuAction = function (actionName, contentItemId) {
            switch (actionName) {
                case "AddComment":
                    _reactComponent.setState(data);
                    data.selectedTicketId = contentItemId;
                    data.showAddCommentModal = true;
                    break;
                case "ViewTicket":
                    var ticketDisplayRoute = data.Routes.DisplayTicket;
                    ticketDisplayRoute = decodeURI(ticketDisplayRoute);
                    ticketDisplayRoute = ticketDisplayRoute.replace("{displayType}", "Detail");
                    ticketDisplayRoute = ticketDisplayRoute.replace("{id}", contentItemId);
                    window.open(ticketDisplayRoute, "_blank");

                    return;
                case "EditTicket":
                    var ticketEditRoute = data.Routes.EditTicket;
                    ticketEditRoute = decodeURI(ticketEditRoute);
                    ticketEditRoute = ticketEditRoute.replace("{id}", contentItemId);

                    data.asyncState = "loading";
                    _reactComponent.setState(data);

                    $.ajax({
                        type: "GET",
                        url: ticketEditRoute,
                        data: null,
                        error: function (e) {
                            data.asyncState = "error";
                            _reactComponent.setState(data);
                        }
                    }).done(function (response) {
                        if (!response.Errors || !response.IsDone || response.Errors.length > 0) {
                            data.asyncState = "error";
                            _reactComponent.setState(data);
                            return;
                        }

                        data.asyncState = "normal";
                        data.showEditModal = true;

                        if (response.Data) {
                            data.editModalShape = JSON.parse(response.Data);
                            var ticketPart = _self.getPart(data.editModalShape, "TicketPart");
                            data.editModalData = {
                                id: data.editModalShape.ContentItem.Id,
                                title: ticketPart.Record.Title,
                                description: ticketPart.Record.Description
                            };
                        }

                        _reactComponent.setState(data);
                    });

                    break;
            }
        };

        var componentDidUpdate = function () {
            this.applyDragAndDrop();
        };

        var componentDidMount = function () {
            this.applyDragAndDrop();
        };

        var applyDragAndDrop = function () {

            var domNode = $(ReactDOM.findDOMNode(this));

            var _reactComponent = this;

            domNode.find("article[data-canedit='true']")
            .draggable({
                cancel: ".pivot a, .ticket-milestone-menu",
                helper: function (e) {
                    var item = $(e.target).closest(".ticket-pinboard");
                    var width = item.width();
                    var height = item.height();
                    return "<div style='border: 1px dotted gray; width:" + width + "px; height:" + height + "px; background-color:transparent'></div>";
                }
            });

            if (data.mode == "ticketPerRow") {

                domNode.find(".empty-td").droppable({
                    accept: function (event) {
                        var draggableId = $(event).data("contentid");

                        var droppableId = $(this).data("contentid");
                        return draggableId != null && droppableId != null && draggableId == droppableId;
                    },
                    drop: function (event, ui) {
                        var draggable = ui.draggable;
                        handleDragDropUpdate(draggable, $(this), function (newData) {
                            _reactComponent.setState(newData);
                        });
                    },
                    activeClass: "milestone-drop-active",
                    hoverClass: "milestone-drop-hover"
                });
            }
            else {
                domNode.find(".bar").droppable({
                    accept: function (event) {
                        var draggableBar = $(event).closest(".bar");
                        var draggableBarId = draggableBar.data("id");

                        var droppableId = $(this).data("id");
                        return draggableBarId != droppableId;
                    },
                    drop: function (event, ui) {
                        var draggable = ui.draggable;
                        handleDragDropUpdate(draggable, $(this), function (newData) {
                            _reactComponent.setState(newData);
                        });
                    },
                    activeClass: "milestone-drop-active",
                    hoverClass: "milestone-drop-hover"
                });
            }
        };

        var handleDragDropUpdate = function (draggable, droppable, callBack) {
            var contentItemId = draggable.attr("data-contentid");
            var targetStateId = droppable.data("status");
            if (targetStateId <= 0) {
                targetStateId = null;
            }

            var toSubmitData = {
                returnUrl: location.href,
                UpdateStatusId: true,
                StatusId: targetStateId,
                Ids: [contentItemId]
            };

            var editBaseWidget = new crm.project.EditBaseWidget();
            var verificationToken = editBaseWidget.getRequestVerificationToken();

            $.extend(toSubmitData, verificationToken);
            var url = data.Routes.QuickTicketUpdate;

            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                type: "POST",
                url: url,
                data: toSubmitData,
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

                data.asyncState = "normal";

                // find item in data
                var item = null;
                var ticketPart = null;
                for (var i = 0; i < data.Model.Items.length; i++) {
                    if (data.Model.Items[i].ContentItem.Id == contentItemId) {
                        item = data.Model.Items[i];
                        break;
                    }
                }

                // update item StatusId
                item.StatusId = targetStateId;

                // update TicketPart
                for (var i = 0; i < item.ContentItem.Parts.length; i++) {
                    if (item.ContentItem.Parts[i].PartDefinition.Name == "TicketPart") {
                        ticketPart = item.ContentItem.Parts[i];
                        break;
                    }
                }

                ticketPart.Record.Status = { Id: targetStateId };

                // Update Model
                for (var i = 0; i < item.Content.length; i++) {
                    var subShape = _self.getSubShape(item, "Parts_Ticket_Pinboard");
                    if (subShape) {
                        subShape.Model.StatusId = targetStateId;
                    }
                }

                callBack(data);
            });
        };

        var milestoneComponent = React.createClass({
            getInitialState: getInitialState,
            render: render,
            setAsyncState: setAsyncState,
            filterTicketsClickHandler: filterTicketsClickHandler,
            componentDidUpdate: componentDidUpdate,
            componentDidMount: componentDidMount,
            applyDragAndDrop: applyDragAndDrop
        });

        var element = React.createElement(milestoneComponent);
        var _reactComponent = ReactDOM.render(element, document.getElementById(contentContainer));
    };

    $.widget("crm.getContactLink", {
        _create: function () {
            var _self = this;
            var url = this.element.data("getcontacturl");
            var email = this.element.data("email");
            var text = this.element.data("text");
            url = url + "?email=" + email;
            url = encodeURI(url);

            $.ajax({
                url: url,
                type: "GET"
            }).done(function (response) {
                if (response.IsDone && response.Data.Found) {
                    var appendingLink = "<a target='_blank' href='{url}'>{text}</a>";
                    appendingLink = appendingLink.replace("{url}", response.Data.Url);
                    appendingLink = appendingLink.replace("{text}", text);
                    _self.element.append(appendingLink);
                }
            });
        }
    });

    $.widget("crm.syncProjectWidget", {
        _create: function () {
            var model = new crm.project.SyncToSuiteCRMLinkModel(this);
            $.extend(model.options, this.options);
            this.controller = new crm.project.syncProjectLinkController(model, this);
            this.controller.initialize();
        }
    });

    $.widget("crm.syncTicketWidget", {
        _create: function () {
            var model = new crm.project.SyncToSuiteCRMLinkModel(this);
            model.options.dialogId = "syncTaskDialog";
            $.extend(model.options, this.options);
            this.controller = new crm.project.syncTicketLinkController(model, this);
            this.controller.initialize();
        }
    });

    $.widget("CRM.AttachToProject", {
        options: {
            projectCssClass: "project-select",
            milestoneCssClass: "milestone-select"
        },
        _create: function () {
            var milestoneCombobox = this.element.find("." + this.options.milestoneCssClass);
            var projectCombobox = this.element.find("." + this.options.projectCssClass);

            projectCombobox.change(function () {
                var selectedValue = $(this).val();

                if (!selectedValue || selectedValue == "") {
                    milestoneCombobox.find("option").remove();
                    return;
                };

                var url = milestoneCombobox.data("projectmilestones");
                url = url + "?projectId=" + selectedValue;

                $.get(url, null, function (response) {
                    if (response.IsDone) {
                        milestoneCombobox.find("option").remove();

                        response.Data.Items.forEach(function (item) {
                            var option = "<option value={value}>{text}</option>"
                            var text = item.Text || "";
                            var value = item.Value || "";
                            option = option.replace("{value}", value).replace("{text}", text);
                            milestoneCombobox.append(option);
                        });
                    }
                });
            });
        }
    });

    $.widget("CRM.ProjectMenu", {
        options: {
            pivotCssClass: "pivot",
            projectMenuCssClass: "project-item-menu",
            menuHiddenCssClass: "menu-hidden",
            highlightedPivotCssClass: "selected-pivot"
        },
        _create: function () {
            var _self = this;
            var pivot = this.element.find("." + this.options.pivotCssClass);
            pivot.data("state", "off");
            var menu = this.element.find("." + this.options.projectMenuCssClass);
            menu.removeClass(this.options.menuHiddenCssClass);
            menu.hide();

            // The purpose of this variable is preventing the menu from closing in case the next click outside the menu is so fast
            var latestTimeClick = null;

            pivot.click(function (event) {

                // If the last click happened few handred mili seconds ago, then do noting
                var now = new Date();
                var newTime = now.getTime();

                if (latestTimeClick != null && latestTimeClick + 300 > newTime) {
                    return;
                }
                else {
                    latestTimeClick = newTime;
                }

                event.stopPropagation();
                if (pivot.data("state") == "off") {
                    pivot.data("state", "on");
                    pivot.addClass(_self.options.highlightedPivotCssClass)
                    menu.show();
                }
                else {
                    pivot.data("state", "off");
                    menu.hide();
                    pivot.removeClass(_self.options.highlightedPivotCssClass)
                }
            });

            $("html").click(function () {
                pivot.data("state", "off");
                pivot.removeClass(_self.options.highlightedPivotCssClass)
                menu.hide();
            });
        }
    });

    $.widget("CRM.EditMilestone", {
        _create: function () {
            var model = new crm.project.editMilestoneModel(this);
            $.extend(model.options, this.options);
            this.controller = new crm.project.editMilestoneController(this, model);
            this.controller.initialize();
        }
    });

    $.widget("CRM.EditProjectDashboardPortlets", {
        _create: function () {
            var model = new crm.project.editProjectDashboadPortletsModel(this);
            $.extend(model.options, this.options);
            this.controller = new crm.project.editProjectDashboadPortletsController(this, model);
            this.controller.initialize();
        }
    });

    $.widget("CRM.Skype", {
        _create: function () {
            var model = new crm.project.skypeTooltipModel(this);
            $.extend(model.options, this.options);
            this.controller = new crm.project.skypeTooltipController(this, model);
            this.controller.initialize();
        }
    });

    $.widget("CRM.FollowLink", {
        _create: function () {
            var model = new crm.project.followerLinkModel(this);
            $.extend(model.options, this.options);
            this.controller = new crm.project.followerLinkController(this, model);
            this.controller.initialize();
        }
    });


    $.widget("CRM.ActivityStreamViewer", {
        _create: function () {
            var model = new crm.project.activityStreamDetailViewerModel(this);
            $.extend(model.options, this.options);
            this.controller = new crm.project.activityStreamDetailViewerController(this, model);
            this.controller.initialize();
        }
    });

    $.widget("CRM.WikiItem", {
        options: {},
        _create: function () {
            var tabsModel = new crm.project.wikiTabControlModel(this);
            $.extend(tabsModel.options, this.options);
            this.tabsController = new crm.project.wikiTabsController(this, tabsModel);
            this.tabsController.initialize();

            var deleteModel = new crm.project.deleteDialogModel(this);
            $.extend(deleteModel.options, this.options);
            this.deleteController = new crm.project.deleteController(this, deleteModel);
            this.deleteController.initialize();
        }
    });

    $.widget("CRM.Discussion", {
        options: {},
        _create: function () {

            var deleteModel = new crm.project.deleteDialogModel(this);
            $.extend(deleteModel.options, this.options);
            this.deleteController = new crm.project.deleteController(this, deleteModel);
            this.deleteController.initialize();
        }
    });

    $.widget("CRM.Folder", {
        options: {},
        _create: function () {

            var deleteModel = new crm.project.deleteDialogModel(this);
            deleteModel.options.toolbarId = "folder-toolbar";
            deleteModel.options.removeCssClass = "remove-folder";
            $.extend(deleteModel.options, this.options);
            this.deleteController = new crm.project.deleteController(this, deleteModel);
            this.deleteController.initialize();
        }
    });

    $.widget("CRM.PeopleInitialization", {
        options: {},
        _create: function () {
            this.peopleInitialization = new crm.project.PeopleInitializationWidget(this);
            $.extend(this.peopleInitialization.options, this.options);
            this.peopleInitialization.initialize();
        }
    });

    $.widget("CRM.FolderList", {
        options: {},
        _create: function () {
            this.folderWidget = new crm.project.FolderListWidget(this);
            $.extend(this.folderWidget.options, this.options);
            this.folderWidget.initialize();
        }
    });

    $.widget("CRM.EditFolder", {
        options: {},
        _create: function () {
            this.editFolder = new crm.project.EditFolderWidget(this);
            $.extend(this.editFolder.options, this.options);
            this.editFolder.initialize();
        }
    });

    $.widget("CRM.EditFolderItem", {
        options: {},
        _create: function () {
            this.editFolderItem = new crm.project.EditFolderItemWidget(this);
            $.extend(this.editFolderItem.options, this.options);
            this.editFolderItem.initialize();
        }
    });
})();