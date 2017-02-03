window.crm = window.crm || {};

(function () {
    crm.suiteCRM = crm.suiteCRM || {};

    // helper class
    // TODO: This logic is repreated across Orchard.CRM.Project/Scripts/ProjectWidgets/EditBaseWidget, Orchard.CRM.Core/Scripts/CRMWidgets/EditBaseWidget 
    // and Orchard.SuiteCRM.Connector/Scripts/SyncCRM/Helper. It must move to a common js library
    crm.suiteCRM.Helper = function (widget) {
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
    crm.suiteCRM.ImportExportData = function (contentContainer, dataContainer) {
        var _self = this;
        var data = JSON.parse($("#" + dataContainer).html());
          
        this.translate = function (data, key, text) {
            if (!data.TranslateTable) {
                console.log(key + "  =  " + text);
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

        var render = function () {
            var mainModel = {
                state: data.asyncState,
                model: this.state,
                root: {
                    T: T,
                    Routes: data.Routes,
                    Controller: _self,
                    actions: {
                        copySuiteProjectToOrchard: copySuiteProjectToOrchard,
                        copyOrchardProjectToSuiteProject: copyOrchardProjectToSuiteProject,
                        copyAllOrchardProjectToSuiteProject: copyAllOrchardProjectToSuiteProject,
                        copyAllSuiteProjectToOrchardProject: copyAllSuiteProjectToOrchardProject,
                        nextPage: nextPage,
                        previousPage: previousPage,
                        switchProjectList: switchProjectList,
                        showUsers: showUsers,
                        showProjects: showProjects,
                        copyAllUsersToOrchard: copyAllUsersToOrchard,
                        copyUserToOrchard: copyUserToOrchard
                    }
                }
            };

            return React.createElement(
               "div",
               null,
             React.createElement(orchardcollaboration.react.allComponents.InfoPage, mainModel),
             React.createElement(orchardcollaboration.react.allComponents.SuiteCRMSyncUsers, mainModel),
             React.createElement(orchardcollaboration.react.allComponents.SuiteCRMDataSync, mainModel));
        };

        var getInitialState = function () {
            data.asyncState = "normal";
            return data;
        };

        var copyUserToOrchard = function (user, password) {
            syncUsers([user], password);
        };

        var copyAllUsersToOrchard = function (password) {
            var toSubmitUsers = [];

            for (var i = 0; i < data.Users.length; i++) {
                if (!data.Users[i].IsSync) {
                    toSubmitUsers.push(data.Users[i]);
                }
            }

            syncUsers(toSubmitUsers, password);
        };

        var showProjects = function () {
            data.ListedBasedOnSuiteCRM = true;
            var projectsRoute = data.Routes.ProjectsList;
            var url = decodeURI(projectsRoute);
            url = url.replace("{listedBasedOnSuiteCRM}", data.ListedBasedOnSuiteCRM);
            url = url.replace("{page}", 0);
            reloadData(url);
        };

        var showUsers = function () {
            var usersRoute = data.Routes.UserssList;
            var url = decodeURI(usersRoute);
            url = url.replace("{page}", 0);
            reloadData(url);
        };

        var switchProjectList = function () {
            data.ListedBasedOnSuiteCRM = !data.ListedBasedOnSuiteCRM;
            var projectsRoute = data.Routes.ProjectsList;
            var url = decodeURI(projectsRoute);
            url = url.replace("{listedBasedOnSuiteCRM}", data.ListedBasedOnSuiteCRM);
            url = url.replace("{page}", 0);
            reloadData(url);
        };

        var reloadData = function (url) {

            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                url: url,
                type: "GET",
                data: null,
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

                if (response.IsDone) {
                    var routes = data.Routes;
                    var translate = data.TranslateTable;
                    $.extend(data, response.Data);
                    data.Routes = routes;
                    data.TranslateTable = translate;
                    data.asyncState = "normal";
                    _reactComponent.setState(data);
                }
            });
        };

        var previousPage = function () {
            var projectsRoute = data.Routes.ProjectsList;
            var url = decodeURI(projectsRoute);
            url = url.replace("{listedBasedOnSuiteCRM}", data.ListedBasedOnSuiteCRM);

            var page = data.ListedBasedOnSuiteCRM ? data.SuiteCRMPage : data.SuiteCRMPage;
            var nextUrl = url.replace("{page}", page - 1);

            reloadData(nextUrl);
        };

        var nextPage = function () {
            var projectsRoute = data.Routes.ProjectsList;
            var url = decodeURI(projectsRoute);
            url = url.replace("{listedBasedOnSuiteCRM}", data.ListedBasedOnSuiteCRM);

            var page = data.ListedBasedOnSuiteCRM ? data.SuiteCRMPage : data.SuiteCRMPage;
            var nextUrl = url.replace("{page}", page + 1);

            reloadData(nextUrl);
        };

        var copyAllOrchardProjectToSuiteProject = function (syncTickets, doNotOverrideNewerValues, syncSubTasks) {
            var toSubmitProjects = [];

            for (var i = 0; i < data.Projects.length; i++) {
                if (data.Projects[i].OrchardCollaborationProject) {
                    toSubmitProjects.push(data.Projects[i]);
                }
            }

            copyOrchardProjecstToSuiteProjects(toSubmitProjects, syncTickets, doNotOverrideNewerValues, syncSubTasks);
        }

        var copyAllSuiteProjectToOrchardProject = function (syncTickets, doNotOverrideNewerValues) {
            var toSubmitProjects = [];

            for (var i = 0; i < data.Projects.length; i++) {
                if (data.Projects[i].SuiteCRMProject) {
                    toSubmitProjects.push(data.Projects[i]);
                }
            }

            copySuiteProjectsToOrchardProjects(toSubmitProjects, syncTickets, doNotOverrideNewerValues);
        }

        var copyOrchardProjectToSuiteProject = function (project, syncTickets, doNotOverrideNewerValues, syncSubTasks) {
            if (project.OrchardCollaborationProject == null) {
                // show a better error message
                alert(T("ThereIsNoOrchardProject", "OrchardCollaboration must not be null"));
                return;
            }

            copyOrchardProjecstToSuiteProjects([project], syncTickets, doNotOverrideNewerValues, syncSubTasks);
        };

        var syncUsers = function (users, password) {
            var toSubmitData = {
                Users: users,
                DefaultPassword: password
            };

            var helper = new crm.suiteCRM.Helper();
            var verificationToken = helper.getRequestVerificationToken();

            $.extend(toSubmitData, verificationToken);
            var url = data.Routes.CopyUsersToOrchard;

            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                url: url,
                type: "POST",
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

                if (response.IsDone) {
                    for (var i = 0; i < response.Data.length; i++) {
                        for (var j = 0; j < data.Users.length; j++) {
                            if (response.Data[i].SuiteCRMUserId == data.Users[j].SuiteCRMUserId){
                                data.Users[j] = response.Data[i];
                                break;
                            }
                        }
                    }
                    data.asyncState = "normal";
                    _reactComponent.setState(data);
                }
            });
        };

        var copyOrchardProjecstToSuiteProjects = function (projects, syncTickets, doNotOverrideNewerValues, syncSubTasks) {

            var toSubmitData = {
                Projects: []
            };

            for (var i = 0; i < projects.length; i++) {
                var project = projects[i];

                if (project.OrchardCollaborationProject == null) {
                    // show a better error message
                    alert(T("ThereIsNoOrchardProject", "OrchardCollaboration must not be null"));
                    return;
                }

                var toSubmitProject = {
                    SyncTasks: syncTickets,
                    SyncSubTasks: syncSubTasks,
                    DoNotOverrideNewerValues: doNotOverrideNewerValues,
                    OrchardCollaborationProjectId: project.OrchardCollaborationProject.Id
                };

                if (project.SuiteCRMProject != null) {
                    toSubmitProject.SuiteCRMId = project.SuiteCRMProject.Id;
                }

                toSubmitData.Projects.push(toSubmitProject);
            }

            var helper = new crm.suiteCRM.Helper();
            var verificationToken = helper.getRequestVerificationToken();

            $.extend(toSubmitData, verificationToken);
            var url = data.Routes.CopyOrchardProjectToSuiteProject;

            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                url: url,
                type: "POST",
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

                if (response.IsDone) {
                    replaceProjects(response.Data.Projects, data.Projects);
                    data.asyncState = "normal";
                    _reactComponent.setState(data);
                }
            });
        };

        var copySuiteProjectsToOrchardProjects = function (projects, syncTickets, doNotOverrideNewerValues) {
            var toSubmitData = {
                Projects: []
            };

            for (var i = 0; i < projects.length; i++) {
                var project = projects[i];

                if (project.SuiteCRMProject == null) {
                    // show a better error message
                    alert(T("ThereIsNoSuiteCRMProject", "SuiteCRM project must not be null"));
                    return;
                }

                var toSubmitProject = {
                    SyncTasks: syncTickets,
                    DoNotOverrideNewerValues: doNotOverrideNewerValues,
                    SuiteCRMId: project.SuiteCRMProject.Id
                };

                if (project.OrchardCollaborationProject != null) {
                    toSubmitProject.OrchardCollaborationProjectId = project.OrchardCollaborationProject.Id;
                }

                toSubmitData.Projects.push(toSubmitProject);
            }

            var helper = new crm.suiteCRM.Helper();
            var verificationToken = helper.getRequestVerificationToken();

            $.extend(toSubmitData, verificationToken);
            var url = data.Routes.CopySuiteToOrchardProjects;

            data.asyncState = "loading";
            _reactComponent.setState(data);

            $.ajax({
                url: url,
                type: "POST",
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
                if (response.IsDone) {
                    data.asyncState = "normal";
                    replaceProjects(response.Data.Projects, data.Projects);
                    data.OrchardCollaborationProjectsCount = response.Data.OrchardCollaborationProjectsCount;
                    _reactComponent.setState(data);
                }
            });
        };

        var replaceProjects = function (source, destination) {
            for (var i = 0; i < source.length; i++) {
                var found = false;
                if (source[i].OrchardCollaborationProject != null) {
                    for (var j = 0; j < destination.length; j++) {
                        if (destination[j].OrchardCollaborationProject != null &&
                            destination[j].OrchardCollaborationProject.Id == source[i].OrchardCollaborationProject.Id) {
                            destination[j] = source[i];
                            found = true;
                            break;
                        }
                    }
                }

                if (!found && source[i].SuiteCRMProject != null) {
                    for (var j = 0; j < destination.length; j++) {
                        if (destination[j].SuiteCRMProject != null &&
                            destination[j].SuiteCRMProject.Id == source[i].SuiteCRMProject.Id) {
                            destination[j] = source[i];
                            break;
                        }
                    }
                }
            }
        }

        var copySuiteProjectToOrchard = function (project, syncTickets, doNotOverrideNewerValues) {
            if (project.SuiteCRMProject == null) {
                // show a better error message
                alert(T("ThereIsNoSuiteCRMProject", "SuiteCRM project must not be null"));
                return;
            }

            copySuiteProjectsToOrchardProjects([project], syncTickets, doNotOverrideNewerValues);
        };

        var suiteCRMDataSyncContainerComponent = React.createClass({
            getInitialState: getInitialState,
            render: render
        });

        var element = React.createElement(suiteCRMDataSyncContainerComponent);
        var _reactComponent = ReactDOM.render(element, document.getElementById(contentContainer));

    };
})();