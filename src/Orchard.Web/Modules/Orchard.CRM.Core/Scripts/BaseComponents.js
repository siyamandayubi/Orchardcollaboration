
window.orchardcollaboration = window.orchardcollaboration || {};
var orchardcollaboration = window.orchardcollaboration;
orchardcollaboration.react = orchardcollaboration.react || {};
orchardcollaboration.react.allComponents = orchardcollaboration.react.allComponents || {};

(function () {

    var InfoPage = React.createClass({
        displayName: "InfoPage",

        closeClickHandler: function () {
            this.props.root.actions.setAsyncState("normal");
        },
        render: function () {
            var root = this.props.root;

            switch (this.props.state) {
                case "loading":
                    return React.createElement(
                        "div",
                        { className: "info-page" },
                        root.T("Loading", "Loading ...")
                    );
                    break;

                case "error":
                    return React.createElement(
                        "div",
                        { className: "info-page" },
                        React.createElement(
                            "span",
                            { className: "error" },
                            root.T("ErrotConnectingToServer", "Error in connecting to the server")
                        ),
                        React.createElement(
                            "a",
                            { className: "close-link", onClick: this.closeClickHandler },
                            root.T("Close")
                        )
                    );
                    break;
            }

            return null;
        }
    });
    orchardcollaboration.react.allComponents.InfoPage = InfoPage;

    var Display = React.createClass({
        displayName: "Display",

        render: function () {

            var shape = this.props.shape;
            var root = this.props.root;

            var containerTag = this.props.containerTag;

            containerTag = containerTag || "div";
            var notWrap = this.props.notWrap || false;

            // render nothing if there is no shape
            if (shape == null) {
                return null;
            }

            var zoneTypes = ["Content", "ContentZone"];
            if (shape.Metadata && zoneTypes.indexOf(shape.Metadata.Type) < 0) {
                var componentName = shape.Metadata.Type;

                if (shape.Metadata.Alternates && shape.Metadata.Alternates.length > 0) {
                    for (var i = 0; i < shape.Metadata.Alternates.length; i++) {
                        if (orchardcollaboration.react.allComponents[shape.Metadata.Alternates[i]] != null) {
                            componentName = shape.Metadata.Alternates[i];
                            break;
                        }
                    }
                }

                if (typeof orchardcollaboration.react.allComponents[componentName] !== "undefined") {
                    return React.createElement(orchardcollaboration.react.allComponents[componentName], { Model: shape.Model, root: root, Classes: shape.Classes, ContentPart: shape.ContentPart, ContentItem: shape.ContentItem });
                }
            };

            if (shape.Items && shape.Items.length > 0) {
                var items = shape.Items.map(function (item) {
                    return React.createElement(Display, { shape: item, root: root });
                });

                return items.length == 1 ? items[0] : React.createElement(containerTag, null, items);
            }

            if (shape.length && shape.length > 0) {
                var items = shape.map(function (item) {
                    return React.createElement(Display, { shape: item, root: root });
                });

                return items.length == 1 ? items[0] : React.createElement(containerTag, null, items);
            }
            if (shape.Content && shape.Content.length > 0) {
                shape.Content.sort(function (a, b) {
                    if (a.Metadata.Position > b.Metadata.Position) {
                        return 1;
                    } else {
                        return -1;
                    }
                });

                var contentItems = shape.Content;
                if (notWrap) {
                    contentItems = [shape.Content[0]];
                    shape.Content[0].NextSiblings = shape.Content.slice(1, shape.Content.length - 1);
                }

                var contentItemsComponents = contentItems.map(function (content) {
                    return React.createElement(Display, { shape: content, root: root, Classes: shape.Classes });
                });

                return contentItemsComponents.length == 1 ? contentItemsComponents[0] : React.createElement(containerTag, null, contentItemsComponents);
            }

            return null;
        }
    });
    orchardcollaboration.react.allComponents.Display = Display;

    orchardcollaboration.BaseComponent = function () {

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
        };

        this.getSubShape = function (shape, metadataType, prefix) {

            var isMatch = function (metadata, metadataType, prefix) {
                var typeMatch = metadata.Type == metadataType;

                if (typeMatch && prefix) {
                    return prefix == metadata.Prefix;
                }

                return typeMatch;
            };

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
        };
    };

    orchardcollaboration.react.Render = function (viewName, container, dataContainer) {
        var _self = this;

        orchardcollaboration.BaseComponent.apply(this, arguments);
        var data = JSON.parse($("#" + dataContainer).html());

        var T = function (key, text) {

            return _self.translate(data, key, text);
        };

        var dashboardComponent = React.createClass({
            displayName: "dashboardComponent",

            getInitialState: function () {
                return data;
            },
            render: function () {
                var model = {
                    data: data,
                    root: {
                        T: T,
                        Routes: data.Routes
                    }
                };

                var view = orchardcollaboration.react.allComponents[viewName] || "div";

                return React.createElement("div", null, React.createElement(view, model));
            }
        });

        var element = React.createElement(dashboardComponent);
        var _reactComponent = ReactDOM.render(element, document.getElementById(container));
    };
})();