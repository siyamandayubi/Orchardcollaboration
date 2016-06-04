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

var orchardcollaboration = orchardcollaboration || {};
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
})();