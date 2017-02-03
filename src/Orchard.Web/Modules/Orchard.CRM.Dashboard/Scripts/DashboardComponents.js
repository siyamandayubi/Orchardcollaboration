
var orchardcollaboration = orchardcollaboration || {};
orchardcollaboration.react = orchardcollaboration.react || {};
orchardcollaboration.react.allComponents = orchardcollaboration.react.allComponents || {};

(function () {
 
    $.widget("CRM.Leftbar", {
        options: {
            sidebarOpenIconId: "sidebar-icon",
            sidebarClass: "sidebar-content-id",
            sidebarCloseClass: "close-button",
            contentCss: "content",
            loadingClass: "loading-icon"
        },
        _create: function () {
            var _self = this;

            var openButton = $("#" + _self.options.sidebarOpenIconId);
            var closeButton = $("." + _self.options.sidebarCloseClass);
            var contentContainer = $("." + _self.options.sidebarClass);
            var contentElement = contentContainer.find("." + _self.options.contentCss);
            var loadingData = $("." + _self.options.loadingClass);
            var lastLoadData;
            var contentIsLoaded = false;
            openButton.click(function () {
                var now = new Date();
                var oneMinuteAgo = new Date(now - 60000);

                contentContainer.removeClass("hidden");
                contentContainer.addClass("sidebar-content");
                openButton.addClass("hidden");

                if (!lastLoadData || lastLoadData < oneMinuteAgo) {
                    loadData();
                    return;
                }

                if (typeof Storage !== "undefined") {
                    content = sessionStorage.getItem("left-bar-content");
                    contentElement.html(content);
                };
            });

            closeButton.click(function () {
                openButton.removeClass("hidden");
                contentContainer.removeClass("sidebar-content");
                contentContainer.addClass("hidden");
            });

            var loadData = function () {
                var url = _self.element.data("dashboardurl");
                loadingData.removeClass("hidden");
                $.ajax({
                    url: url,
                    type: "GET",
                    success: function (data) {

                        lastLoadData = new Date();
                        contentIsLoaded = true;
                        if (data.IsDone) {
                            contentElement.html(data.Html);
                            if (typeof Storage !== "undefined") {
                                sessionStorage.setItem("left-bar-content", data.Html);
                            };
                        }
                        loadingData.addClass("hidden");
                    },
                    error: function () {
                        if (typeof Storage !== "undefined") {
                            sessionStorage.setItem("left-bar-content", "");
                        };

                        lastLoadData = new Date();
                        contentIsLoaded = true;
                    }
                });
            };
        }
    });

    $.widget("CRM.Dashboard", {
        options: {
            expandCssClass: "expand-icon",
            collapseCssClass: "collapse-icon",
            panelBodyClass: "panel-body"
        },
        _create: function () {
            var expandIcon = this.element.find("." + this.options.expandCssClass);
            var collapseIcon = this.element.find("." + this.options.collapseCssClass);
            var body = this.element.find("." + this.options.panelBodyClass);

            expandIcon.click(function () {
                body.removeClass("hidden");
                collapseIcon.removeClass("hidden");
                expandIcon.addClass("hidden");
            });

            collapseIcon.click(function () {
                body.addClass("hidden");
                expandIcon.removeClass("hidden");
                collapseIcon.addClass("hidden");
            });
        }
    });
})();