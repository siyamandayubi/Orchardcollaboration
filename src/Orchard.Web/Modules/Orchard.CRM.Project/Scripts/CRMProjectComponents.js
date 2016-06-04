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
	var x = orchardcollaboration.react.allComponents;
	var Parts_Ticket_TableRow = React.createClass({
		displayName: "Parts_Ticket_TableRow",

		menuClick: function (action, contentId) {
			this.props.root.actions.ticketMenu(action, contentId);
		},

		componentDidMount: function () {
			var domNode = $(ReactDOM.findDOMNode(this));
			domNode.ProjectMenu({
				projectMenuCssClass: "milestone-item-menu"
			});

			domNode.on("mouseleave", function () {
				domNode.find(".milestone-item-menu").hide();
				domNode.find(".pivot").data("state", "off");
			});
		},

		render: function () {
			var _self = this;
			var mainProps = this.props;
			var root = mainProps.root;
			var ticketDisplayRoute = root.Routes.DisplayTicket;
			var url = decodeURI(ticketDisplayRoute);
			url = url.replace("{id}", mainProps.Model.TicketId);

			var menu = null;
			if (mainProps.Model.Menu && mainProps.Model.Menu.length > 0) {
				var menuItems = mainProps.Model.Menu.map(function (menuItem) {
					return React.createElement(
						"li",
						{ onClick: _self.menuClick.bind(null, menuItem.Id, mainProps.Model.ContentItemId) },
						menuItem.Text
					);
				});

				menu = React.createElement(
					"div",
					{ className: "milestone-item-menu-container" },
					React.createElement("span", { className: "pivot" }),
					React.createElement(
						"ul",
						{ className: "milestone-item-menu menu-hidden z2" },
						menuItems
					)
				);
			} else {
				menu = React.createElement("span", null);
			}

			return React.createElement(
				"tr",
				{ "data-id": mainProps.Model.ContentItemId },
				React.createElement(
					"td",
					{ key: "1", className: "ticket-id-col" },
					React.createElement(
						"a",
						{ key: "12", href: url },
						mainProps.Model.TicketNumber
					)
				),
				React.createElement(
					"td",
					{ key: "2", className: "ticket-title-col" },
					React.createElement(
						"a",
						{ key: "14", href: url },
						mainProps.Model.Title
					)
				),
				React.createElement(
					"td",
					{ key: "3", className: "ticket-status-col" },
					mainProps.Model.StatusName
				),
				React.createElement(
					"td",
					{ key: "4", className: "ticket-menu-col" },
					menu
				)
			);
		}
	});
	orchardcollaboration.react.allComponents.Parts_Ticket_TableRow = Parts_Ticket_TableRow;

	var TicketContextMenu = React.createClass({
		displayName: "TicketContextMenu",

		menuClick: function (action, contentId) {
			this.props.root.actions.ticketMenu(action, contentId);
		},

		// http://stackoverflow.com/questions/25630035/javascript-getboundingclientrect-changes-while-scrolling
		absolutePosition: function (el) {
			var found,
			    left = 0,
			    top = 0,
			    width = 0,
			    height = 0,
			    offsetBase = this.absolutePosition.offsetBase;
			if (!offsetBase && document.body) {
				offsetBase = this.absolutePosition.offsetBase = document.createElement('div');
				offsetBase.style.cssText = 'position:absolute;left:0;top:0';
				document.body.appendChild(offsetBase);
			}
			if (el && el.ownerDocument === document && 'getBoundingClientRect' in el && offsetBase) {
				var boundingRect = el.getBoundingClientRect();
				var baseRect = offsetBase.getBoundingClientRect();
				found = true;
				left = boundingRect.left - baseRect.left;
				top = boundingRect.top - baseRect.top;
				width = boundingRect.right - boundingRect.left;
				height = boundingRect.bottom - boundingRect.top;
			}
			return {
				found: found,
				left: left,
				top: top,
				width: width,
				height: height,
				right: left + width,
				bottom: top + height
			};
		},
		pivotClick: function (e) {
			var menuItems = this.refs.menuItems;
			var $window = $(window);
			var scrollLeft = $window.scrollLeft();
			var scrollTop = $window.scrollTop();
			var pivotOffset = this.refs.pivot.getBoundingClientRect(); // this.absolutePosition(this.refs.pivot);
			menuItems.style.top = pivotOffset.top.toString() + "px";
			menuItems.style.left = pivotOffset.left.toString() + "px";
			$(menuItems).removeClass("menu-hidden");
			e.stopPropagation();
			e.preventDefault();
		},
		getInitialState: function () {
			var _self = this;
			$("html").click(function () {
				$(_self.refs.menuItems).addClass("menu-hidden");
			});

			return {};
		},

		componentDidMount: function () {
			var _self = this;
			$(_self.refs.pivot).find("a").click(_self.pivotClick);
		},

		render: function () {
			var _self = this;
			var mainProps = this.props;
			var root = mainProps.root;
			var menu = null;

			if (mainProps.Menu && mainProps.Menu.length > 0) {
				var menuItems = mainProps.Menu.map(function (menuItem) {
					return React.createElement(
						"li",
						{ key: menuItem.Id, onClick: _self.menuClick.bind(null, menuItem.Id, mainProps.ContentItemId) },
						menuItem.Text
					);
				});

				menu = React.createElement(
					"div",
					{ className: "ticket-menu-container" },
					React.createElement(
						"div",
						{ ref: "pivot", className: "pivot" },
						this.props.children
					),
					React.createElement(
						"ul",
						{ ref: "menuItems", className: "ticket-milestone-menu menu-hidden z2" },
						menuItems
					)
				);
			} else {
				menu = React.createElement("span", null);
			}

			return menu;
		}
	});

	var Parts_Ticket_TitleOnly = React.createClass({
		displayName: "Parts_Ticket_TitleOnly",

		render: function () {
			var root = this.props.root;
			var ticketDisplayRoute = root.Routes.DisplayTicket;
			var url = decodeURI(ticketDisplayRoute);
			url = url.replace("{id}", this.props.Model.TicketId);

			return React.createElement(
				"div",
				null,
				React.createElement(
					"div",
					{ "class": "ticket-list-id" },
					React.createElement(
						"a",
						{ href: "{url}" },
						this.props.Model.TicketNumber
					)
				),
				React.createElement(
					"div",
					{ "class": "ticket-list-name" },
					React.createElement(
						"a",
						{ href: "{url}" },
						this.props.Model.Title
					)
				),
				React.createElement("div", { "class": "clear" })
			);
		}
	});
	orchardcollaboration.react.allComponents.Parts_Ticket_TitleOnly = Parts_Ticket_TitleOnly;

	var Parts_Ticket_GanttChart = React.createClass({
		displayName: "Parts_Ticket_GanttChart",

		render: function () {
			var getStatusClass = function (typeId) {
				switch (typeId) {
					case 10:
						return "new-status-type";
					case 20:
						return "open-status-type";
					case 30:
						return "deferred-status-type";
					case 35:
						return "pending-status-type";
					case 40:
						return "closed-status-type";
					default:
						return "";
				}
			};

			var root = this.props.root;
			var model = this.props.Model;
			var ticketDisplayRoute = root.Routes.DisplayTicket;
			var url = decodeURI(ticketDisplayRoute);
			url = url.replace("{id}", model.TicketId);
			var dueDate = model.DueDate != null ? new Date(model.DueDate) : null;
			var cells = [];
			var todayParts = root.today.split('-');
			var today = new Date(todayParts[0], todayParts[1] - 1, todayParts[2]);

			if (model.StatusTimes.length == 0) {
				cells.push(React.createElement("td", { colspan: root.days.length }));
			} else {

				var latestStatusId = -1;
				var isClosed = false;
				for (var i = 0; i < root.days.length; i++) {
					var date = new Date(root.days[i]);

					if (date > today) {
						cells.push(React.createElement("td", null));
						continue;
					}

					var newStatus = null;
					for (var j = 0; j < model.StatusTimes.length; j++) {
						var statusDate = new Date(model.StatusTimes[j].Value);

						if (date.getFullYear() == statusDate.getFullYear() && date.getMonth() == statusDate.getMonth() && date.getDate() == statusDate.getDate()) {
							newStatus = model.StatusTimes[j];
						}
					}

					if (isClosed && (newStatus == null || newStatus == 40)) {
						cells.push(React.createElement("td", null));
						continue;
					}

					if (dueDate && date > dueDate) {
						cells.push(React.createElement(
							"td",
							null,
							React.createElement("div", { className: "overdue-status" })
						));
						continue;
					}

					if (newStatus != null) {
						isClosed = newStatus.Key == 40;
						latestStatusId = newStatus.Key;
						cells.push(React.createElement(
							"td",
							null,
							React.createElement("div", { className: getStatusClass(newStatus.Key) })
						));
					} else if (latestStatusId != null) {
						cells.push(React.createElement(
							"td",
							null,
							React.createElement("div", { className: getStatusClass(latestStatusId) })
						));
					} else {
						cells.push(React.createElement("td", null));
					}
				}
			};

			var cssClass = "";
			for (var i = 0; i < this.props.Classes.length; i++) {
				cssClass = this.props.Classes[i] + " ";
			}
			return React.createElement(
				"tr",
				{ className: cssClass },
				React.createElement(
					"td",
					{ className: "ticket-gantt-row-id" },
					React.createElement(
						"a",
						{ href: url },
						model.TicketNumber
					)
				),
				React.createElement(
					"td",
					{ className: "ticket-gantt-row-name" },
					React.createElement(
						"a",
						{ title: model.Title, href: url },
						model.Title
					)
				),
				cells
			);
		}
	});
	orchardcollaboration.react.allComponents.Parts_Ticket_GanttChart = Parts_Ticket_GanttChart;

	var Parts_Ticket_Edit = React.createClass({
		displayName: "Parts_Ticket_Edit",

		getInitialState: function () {},

		render: function () {
			var root = this.props.root;
			return React.createElement(
				"div",
				null,
				React.createElement(
					"div",
					null,
					React.createElement(
						"div",
						{ className: "label" },
						root.T("Title", "Title")
					),
					React.createElement(
						"div",
						null,
						React.createElement("input", { type: "text", value: this })
					)
				)
			);
		}
	});
	orchardcollaboration.react.allComponents.Parts_Ticket_Edit = Parts_Ticket_Edit;

	var Parts_Ticket_Pinboard = React.createClass({
		displayName: "Parts_Ticket_Pinboard",

		render: function () {
			var root = this.props.root;
			var ticketDisplayRoute = root.Routes.DisplayTicket;
			var url = decodeURI(ticketDisplayRoute);
			url = url.replace("{id}", this.props.Model.TicketId);
			return React.createElement(
				"article",
				{ "data-canedit": this.props.Model.CurrentUserCanEditItem, className: "ticket-pinboard", "data-contentid": this.props.Model.ContentItemId },
				React.createElement(
					"h4",
					null,
					React.createElement(
						TicketContextMenu,
						{ root: this.props.root, Menu: this.props.Model.Menu, ContentItemId: this.props.Model.ContentItemId },
						React.createElement(
							"a",
							{ href: "#" },
							React.createElement(
								"span",
								{ className: "identity" },
								"#",
								this.props.Model.TicketNumber
							),
							React.createElement(
								"span",
								null,
								this.props.Model.Title
							)
						)
					)
				),
				React.createElement(
					"div",
					null,
					React.createElement(
						"label",
						{ className: "label-container" },
						root.T("Priority"),
						":"
					),
					React.createElement(
						"span",
						{ className: "ticket-service-value result" },
						React.createElement(
							"span",
							null,
							this.props.Model.PriorityName
						)
					)
				),
				React.createElement(
					"div",
					null,
					React.createElement(
						"label",
						{ className: "label-container" },
						root.T("Type"),
						":"
					),
					React.createElement(
						"span",
						{ className: "ticket-service-value result" },
						React.createElement(
							"span",
							null,
							this.props.Model.TypeName
						)
					)
				)
			);
		}
	});
	orchardcollaboration.react.allComponents.Parts_Ticket_Pinboard = Parts_Ticket_Pinboard;

	var MilestoneTickets = React.createClass({
		displayName: "MilestoneTickets",

		filterKeyDown: function (event) {
			if (event.keyCode == 13) {
				this.filter();
			}
		},
		changeMode: function (mode) {
			this.props.root.actions.changeMode(mode);
		},
		filter: function () {
			var domNode = $(ReactDOM.findDOMNode(this));
			var filterText = domNode.find(".filter-tickets").val();
			this.props.root.actions.filter(filterText);
		},
		render: function () {
			var _self = this;
			var itemsComponents = null;
			var mainContainer = null;
			var statusRecords = this.props.shape.Model.StatusRecords;

			var props = this.props;
			var root = props.root;
			var filterText = props.filterText;
			if (props.shape.Model.Items.length > 0) {

				var latestRootTicketId = null;
				var altCssClass = "alt-row";
				itemsComponents = props.shape.Model.FilteredItems.map(function (item) {
					var cells = [];

					if (item.InRoot) {
						altCssClass = altCssClass === "alt-row" ? "normal-row" : "alt-row";
					}

					for (var i = 0; i < statusRecords.length; i++) {
						var statusColumn = statusRecords[i];
						if (item.StatusId == null && i == 0 || item.StatusId == statusColumn.Id) {
							cells.push(React.createElement(
								"td",
								{ key: "status" + i, "data-status": "" },
								React.createElement(x.Display, { root: props.root, shape: item })
							));
						} else {
							cells.push(React.createElement(
								"td",
								{ key: "status" + i, "data-contentid": item.ContentItem.Id, className: "empty-td", "data-status": statusColumn.Id },
								React.createElement("div", { className: "empty-cell" })
							));
						}
					}

					return React.createElement(
						"tr",
						{ key: "tr" + item.ContentItem.Id, className: altCssClass },
						cells
					);
				});
				var tableColumns = statusRecords.map(function (status) {
					return React.createElement(
						"th",
						{ key: "status" + status.Id, "data-id": status.Id },
						React.createElement(
							"span",
							{ className: "col-header" },
							status.Name
						),
						React.createElement(
							"span",
							{ title: root.T("NumberOfTickets", "Number of tickets"), className: "count" },
							status.count
						),
						React.createElement(
							"span",
							{ title: root.T("SumSizeOfTickets", "Total size of tickets in this column"), className: "size" },
							status.size
						)
					);
				});

				mainContainer = React.createElement(
					"div",
					{ key: "milestoneTickets" },
					React.createElement(
						"div",
						{ className: "filter-box" },
						React.createElement("input", { className: "filter-tickets", onKeyDown: _self.filterKeyDown, defaultValue: filterText, type: "text" }),
						React.createElement("input", { type: "button", value: root.T("Filter"), onClick: _self.filter }),
						React.createElement(
							"span",
							{ className: "render-mode" },
							root.T("OneTicketPerRow", "One Ticket per row")
						),
						React.createElement(
							"a",
							{ className: "render-mode", onClick: this.changeMode.bind(null, "ticketPerColumn") },
							root.T("Cardwall", "Cardwall")
						)
					),
					React.createElement(
						"div",
						{ className: "milestone-wild-card-container" },
						React.createElement(
							"table",
							{ className: "milestone-items" },
							React.createElement(
								"thead",
								null,
								React.createElement(
									"tr",
									null,
									tableColumns
								)
							),
							React.createElement(
								"tbody",
								null,
								itemsComponents
							)
						)
					)
				);
			} else {
				mainContainer = React.createElement(
					"h6",
					null,
					root.T("NoItemInMilestone", "There is no item in the Milestone")
				);
			}
			return mainContainer;
		}
	});

	orchardcollaboration.react.allComponents.MilestoneTickets = MilestoneTickets;

	var MilestoneTicketsStatusBars = React.createClass({
		displayName: "MilestoneTicketsStatusBars",

		filterKeyDown: function (event) {
			if (event.keyCode == 13) {
				this.filter();
			}
		},
		filter: function () {
			var domNode = $(ReactDOM.findDOMNode(this));
			var filterText = domNode.find(".filter-tickets").val();
			this.props.root.actions.filter(filterText);
		},
		changeMode: function (mode) {
			this.props.root.actions.changeMode(mode);
		},
		render: function () {
			var _self = this;
			var itemsComponents = null;
			var mainContainer = null;
			var statusRecords = this.props.shape.Model.StatusRecords;

			var props = this.props;
			var root = props.root;
			var filterText = props.filterText;
			if (props.shape.Model.Items.length > 0) {

				var bars = statusRecords.map(function (status) {
					var items = [];
					var barKey = "bar" + status.Id;

					for (var i = 0; i < props.shape.Model.FilteredItems.length; i++) {
						var item = props.shape.Model.FilteredItems[i];
						if (item.StatusId == status.Id || status.StatusTypeId == 10 && item.StatusId == null) {
							items.push(React.createElement(x.Display, { root: props.root, shape: item }));
						}
					}

					return React.createElement(
						"div",
						{ className: "bar", key: barKey, "data-status": status.Id, "data-id": status.Id },
						React.createElement(
							"div",
							{ className: "header" },
							React.createElement(
								"span",
								{ className: "col-header" },
								status.Name
							),
							React.createElement(
								"span",
								{ title: root.T("NumberOfTickets", "Number of tickets"), className: "count" },
								status.count
							),
							React.createElement(
								"span",
								{ title: root.T("SumSizeOfTickets", "Total size of tickets in this column"), className: "size" },
								status.size
							)
						),
						React.createElement(
							"div",
							{ className: "body" },
							items
						)
					);
				});

				mainContainer = React.createElement(
					"div",
					{ key: "milestoneTickets" },
					React.createElement(
						"div",
						{ className: "filter-box" },
						React.createElement("input", { className: "filter-tickets", onKeyDown: _self.filterKeyDown, defaultValue: filterText, type: "text" }),
						React.createElement("input", { type: "button", value: root.T("Filter"), onClick: _self.filter }),
						React.createElement(
							"a",
							{ className: "render-mode", onClick: this.changeMode.bind(null, "ticketPerRow") },
							root.T("OneTicketPerRow", "One Ticket per row")
						),
						React.createElement(
							"span",
							{ className: "render-mode" },
							root.T("Cardwall", "Cardwall")
						)
					),
					React.createElement(
						"div",
						{ className: "milestone-wild-card-container" },
						React.createElement(
							"div",
							{ className: "milestone-items" },
							React.createElement(
								"div",
								{ className: "bars-table" },
								bars
							)
						)
					)
				);
			} else {
				mainContainer = React.createElement(
					"h6",
					null,
					root.T("NoItemInMilestone", "There is no item in the Milestone")
				);
			}
			return mainContainer;
		}
	});

	orchardcollaboration.react.allComponents.MilestoneTicketsStatusBars = MilestoneTicketsStatusBars;
	var MilestonePlanner = React.createClass({
		displayName: "MilestonePlanner",

		render: function () {
			var milestoneMembers = null;
			var mainContainer = null;
			var props = this.props;
			var root = props.root;

			if (props.shape.Model.Items.length > 0) {

				var milestoneMembersList = props.shape.Model.Items.map(function (item) {
					return React.createElement(x.Display, { root: root, notWrap: "true", shape: item });
				});

				milestoneMembers = React.createElement(
					"div",
					{ className: "milestone-wild-card-container" },
					React.createElement(
						"table",
						{ className: "milestone-items-list current-milestone" },
						React.createElement(
							"thead",
							null,
							React.createElement(
								"tr",
								null,
								React.createElement(
									"th",
									{ className: "ticket-id-col" },
									"#"
								),
								React.createElement(
									"th",
									{ className: "ticket-title-col" },
									root.T("Summary")
								),
								React.createElement(
									"th",
									{ className: "ticket-status-col" },
									root.T("Status")
								),
								React.createElement("th", { className: "ticket-menu-col" })
							)
						),
						React.createElement(
							"tbody",
							null,
							milestoneMembersList
						)
					)
				);
			} else {
				milestoneMembers = React.createElement(
					"div",
					{ className: "drop-target" },
					React.createElement(
						"h5",
						{ className: "milestone-no-item" },
						root.T("NoItemsInMilestone", "There is no item in the Milestone")
					)
				);
			}

			var backLogComponent = null;
			if (props.shape.Model.BacklogMembers != null && props.shape.Model.BacklogMembers.length > 0) {
				var backLogItems = props.shape.Model.BacklogMembers.map(function (item) {
					return React.createElement(x.Display, { root: root, notWrap: "true", shape: item, key: item.ContentItem.Id });
				});
				backLogComponent = React.createElement(
					"div",
					null,
					React.createElement(
						"h4",
						null,
						root.T("Backlog")
					),
					React.createElement(
						"table",
						{ className: "backlog milestone-items-list" },
						React.createElement(
							"thead",
							null,
							React.createElement(
								"tr",
								null,
								React.createElement(
									"th",
									{ className: "ticket-id-col" },
									"#"
								),
								React.createElement(
									"th",
									{ className: "ticket-title-col" },
									root.T("Summary")
								),
								React.createElement(
									"th",
									{ className: "ticket-status-col" },
									root.T("Status")
								),
								React.createElement("th", { className: "ticket-menu-col" })
							)
						),
						React.createElement(
							"tbody",
							null,
							backLogItems
						)
					)
				);
			} else {
				backLogComponent = React.createElement(
					"div",
					null,
					React.createElement(
						"h4",
						null,
						root.T("Backlog")
					),
					React.createElement(
						"div",
						{ className: "drop-target" },
						React.createElement(
							"h5",
							{ className: "milestone-no-item" },
							root.T("NoItemsInBacklog", "There is no item in the Backlog")
						)
					)
				);
			}

			return React.createElement(
				"div",
				null,
				milestoneMembers,
				backLogComponent
			);
		}
	});
	orchardcollaboration.react.allComponents.MilestonePlanner = MilestonePlanner;

	var MilestoneGantt = React.createClass({
		displayName: "MilestoneGantt",

		filterKeyDown: function (event) {
			if (event.keyCode == 13) {
				this.filter();
			}
		},
		filter: function () {
			var domNode = $(ReactDOM.findDOMNode(this));
			var filterText = domNode.find(".filter-tickets").val();
			this.props.root.actions.filter(filterText);
		},
		render: function () {
			var _self = this;
			var itemsComponents = null;
			var mainContainer = null;
			var props = this.props;
			var root = props.root;
			var milestone = props.shape.Model.Milestone;

			// check start-time and end-time of milestone
			if (milestone.Start == null || milestone.End == null || milestone.Start >= milestone.End) {
				return React.createElement(
					"h6",
					null,
					root.T("StartTimeEndTimeMismatch", "Start-time or End-time of the Milestone is null or not valid")
				);
			};

			var startDate = new Date(milestone.Start);
			var endDate = new Date(milestone.End);

			var monthColumns = props.shape.Model.Months.map(function (month) {
				return React.createElement(
					"th",
					{ className: "month-col", colSpan: month.Days.length },
					React.createElement(
						"span",
						{ className: "month" },
						month.Name
					),
					React.createElement(
						"span",
						{ className: "year" },
						month.Year
					)
				);
			});

			var tableColumns = [];
			var days = [];

			for (var i = 0; i < props.shape.Model.Months.length; i++) {
				var month = props.shape.Model.Months[i];

				for (var j = 0; j < month.Days.length; j++) {
					var temp = month.Days[j].split("-");
					days.push(new Date(temp[0], temp[1] - 1, temp[2]));
					tableColumns.push(React.createElement(
						"th",
						{ className: "date-col" },
						React.createElement(
							"span",
							null,
							temp[2]
						)
					));
				}
			}

			root.days = days;
			root.today = props.shape.Model.Today;

			var filterText = props.filterText;
			if (props.shape.Model.Items.length > 0) {

				var latestRootTicketId = null;
				var altCssClass = "alt-row";
				itemsComponents = props.shape.Model.FilteredItems.map(function (item) {

					if (item.InRoot) {
						altCssClass = altCssClass === "alt-row" ? "normal-row" : "alt-row";
					}

					for (var i = 0; i < item.Content.length; i++) {
						item.Content[i].Classes.push(altCssClass);
					}

					return React.createElement(x.Display, { key: item.ContentItem.Id, id: item.ContentItem.Id, root: props.root, shape: item });
				});

				mainContainer = React.createElement(
					"div",
					{ className: "milestoneGantt", key: "milestoneGantt" },
					React.createElement(
						"div",
						{ className: "filter-box" },
						React.createElement("input", { className: "filter-tickets", onKeyDown: _self.filterKeyDown, defaultValue: filterText, type: "text" }),
						React.createElement("input", { type: "button", value: root.T("Filter"), onClick: _self.filter })
					),
					React.createElement(
						"div",
						{ className: "milestone-gantt-container1" },
						React.createElement(
							"div",
							{ className: "milestone-tickets-header" },
							React.createElement(
								"div",
								null,
								React.createElement(
									"span",
									{ className: "milestone-tickets-title" },
									root.T("Tickets")
								),
								React.createElement(
									"span",
									{ className: "milestone-size-label" },
									root.T("TotalSize", "Total Size:")
								),
								React.createElement(
									"span",
									{ className: "milestone-size-value" },
									props.shape.Model.totalSize
								)
							)
						),
						React.createElement(
							"div",
							{ className: "milestone-gantt-container" },
							React.createElement(
								"table",
								{ cellPadding: "0", cellSpacing: "0", className: "milestone-gantt-items", "data-count-fixed-columns": "2" },
								React.createElement(
									"thead",
									null,
									React.createElement("tr", null),
									React.createElement(
										"tr",
										null,
										React.createElement("td", null),
										React.createElement("td", null),
										monthColumns
									),
									React.createElement(
										"tr",
										null,
										React.createElement("td", null),
										React.createElement("td", null),
										tableColumns
									)
								),
								React.createElement(
									"tbody",
									null,
									itemsComponents
								)
							)
						)
					),
					React.createElement(
						"div",
						{ className: "ganttchart-guide" },
						React.createElement(
							"h4",
							null,
							root.T("StatusGuide", "Ticket status guide")
						),
						React.createElement(
							"span",
							{ className: "new-status-type" },
							root.T("New", "New")
						),
						React.createElement(
							"span",
							{ className: "open-status-type" },
							root.T("InProgress", "In Progress")
						),
						React.createElement(
							"span",
							{ className: "deferred-status-type" },
							root.T("Deferred", "Deferred")
						),
						React.createElement(
							"span",
							{ className: "pending-status-type" },
							root.T("Pending", "Pending")
						),
						React.createElement(
							"span",
							{ className: "closed-status-type" },
							root.T("Completed", "Completed")
						),
						React.createElement(
							"span",
							{ className: "overdue-status" },
							root.T("Overdue", "Overdue")
						)
					)
				);
			} else {
				mainContainer = React.createElement(
					"h6",
					null,
					root.T("NoItemInMilestone", "There is no item in the Milestone")
				);
			}

			return mainContainer;
		}
	});

	orchardcollaboration.react.allComponents.MilestoneGantt = MilestoneGantt;

	var AddCommentModal = React.createClass({
		displayName: "AddCommentModal",

		render: function () {
			var root = this.props.root;

			return React.createElement(
				ReactBootstrap.Modal,
				{ className: "ticket-add-comment-modal", show: this.props.showAddCommentModal, onHide: this.closeSyncModel },
				React.createElement(
					ReactBootstrap.Modal.Header,
					{ closeButton: true },
					React.createElement(
						ReactBootstrap.Modal.Title,
						null,
						root.T("AddComment", "Add Comment")
					)
				),
				React.createElement(
					ReactBootstrap.Modal.Body,
					null,
					React.createElement(
						"div",
						{ className: "add-comment-modal" },
						React.createElement(
							"div",
							null,
							React.createElement(
								"div",
								null,
								React.createElement(
									"div",
									{ className: "label-row" },
									root.T("Comment", "Comment")
								),
								React.createElement(
									"div",
									null,
									React.createElement("textarea", { rows: "10", ref: "comment" })
								)
							)
						)
					)
				),
				React.createElement(
					ReactBootstrap.Modal.Footer,
					null,
					React.createElement(
						ReactBootstrap.Button,
						{ onClick: this.save },
						root.T("Save", "Save")
					),
					React.createElement(
						ReactBootstrap.Button,
						{ onClick: this.closeSyncModel },
						root.T("Cancel", "Cancel")
					)
				)
			);
		},

		save: function () {
			var data = {
				comment: this.refs.comment.innerText,
				id: this.props.selectedTicketId
			};

			this.props.root.actions.saveComment(data);
		},

		closeSyncModel: function () {
			this.props.root.actions.closeAddCommentModal();
		}
	});

	orchardcollaboration.react.allComponents.AddCommentModal = AddCommentModal;

	var EditTicketModal = React.createClass({
		displayName: "EditTicketModal",

		getInitialState: function () {
			return {
				tinyMceIsCreated: false
			};
		},

		save: function () {
			tinyMCE.triggerSave();

			var priority = this.refs.priority;
			var ticketType = this.refs.ticketType;

			var data = {
				title: this.refs.title.value,
				description: this.refs.description.innerHTML,
				priorityId: priority.selectedIndex ? priority.options[priority.selectedIndex].value : null,
				ticketTypeId: ticketType.selectedIndex ? ticketType.options[ticketType.selectedIndex].value : null,
				id: this.props.editModalData.id
			};

			this.props.root.actions.saveTicket(data);
		},

		componentDidUpdate: function () {

			if (this.props.showEditModal) {

				try {
					tinymce.init({
						selector: '.ticket-edit-modal textarea',
						menubar: false,
						plugins: ["advlist autolink lists link image charmap print preview hr anchor pagebreak", "searchreplace wordcount visualblocks visualchars code fullscreen", "insertdatetime media nonbreaking table contextmenu directionality", "emoticons template paste textcolor colorpicker textpattern", "fullscreen autoresize"],
						toolbar: "cut copy paste | bold italic | bullist numlist outdent indent formatselect | alignleft aligncenter alignright alignjustify ltr rtl | link unlink charmap | code fullscreen",
						height: 200,
						resize: true,
						plugins: 'code'
					});
				} catch (e) {
					alert(e);
				}
				if (this.state.tinyMceIsCreated) {
					tinymce.execCommand('mceAddEditor', true, 'ticketEditModalTextArea');
				}

				this.state.tinyMceIsCreated = true;
			} else if (this.state.tinyMceIsCreated) {
				tinymce.execCommand('mceRemoveEditor', true, 'ticketEditModalTextArea');
			}
		},

		render: function () {

			var root = this.props.root;
			var _self = this;

			var title = "";
			var priorityId = "";
			var ticketTypeId = "";
			var description = "";
			var priorityOptions = null;
			var ticketTypeOptions = null;

			if (this.props.editModalData) {
				priorityOptions = this.props.editModalData.priorities.map(function (item) {
					return React.createElement(
						"option",
						{ value: item.Value },
						item.Text
					);
				});

				ticketTypeOptions = this.props.editModalData.ticketTypes.map(function (item) {
					return React.createElement(
						"option",
						{ value: item.Value },
						item.Text
					);
				});
				priorityId = this.props.editModalData.priorityId;
				ticketTypeId = this.props.editModalData.ticketTypeId;
				title = this.props.editModalData.title;
				description = this.props.editModalData.description;
			}

			return React.createElement(
				ReactBootstrap.Modal,
				{ className: "ticket-edit-modal", show: _self.props.showEditModal, onHide: _self.closeSyncModel },
				React.createElement(
					ReactBootstrap.Modal.Header,
					{ closeButton: true },
					React.createElement(
						ReactBootstrap.Modal.Title,
						null,
						root.T("EditTicket", "Edit Ticket"),
						React.createElement(
							"a",
							{ className: "full-mode-link", href: _self.props.fullEditModelUrl },
							root.T("FullView", "View in full model")
						)
					)
				),
				React.createElement(
					ReactBootstrap.Modal.Body,
					null,
					React.createElement(
						"div",
						{ className: "edit-ticket-modal" },
						React.createElement(
							"div",
							null,
							React.createElement(
								"div",
								null,
								React.createElement(
									"div",
									{ className: "label-row" },
									root.T("Title", "Title")
								),
								React.createElement(
									"div",
									null,
									React.createElement("input", { ref: "title", type: "text", defaultValue: title })
								)
							),
							React.createElement(
								"div",
								null,
								React.createElement(
									"div",
									{ className: "label-row" },
									root.T("Type", "Type")
								),
								React.createElement(
									"div",
									null,
									React.createElement(
										"select",
										{ ref: "ticketType", defaultValue: ticketTypeId },
										ticketTypeOptions
									)
								)
							),
							React.createElement(
								"div",
								null,
								React.createElement(
									"div",
									{ className: "label-row" },
									root.T("Priority", "Priority")
								),
								React.createElement(
									"div",
									null,
									React.createElement(
										"select",
										{ ref: "priority", defaultValue: priorityId },
										priorityOptions
									)
								)
							),
							React.createElement(
								"div",
								null,
								React.createElement(
									"div",
									{ className: "label-row" },
									root.T("Description", "Description")
								),
								React.createElement(
									"div",
									null,
									React.createElement(
										"textarea",
										{ ref: "description", id: "ticketEditModalTextArea" },
										description
									)
								)
							)
						)
					)
				),
				React.createElement(
					ReactBootstrap.Modal.Footer,
					null,
					React.createElement(
						ReactBootstrap.Button,
						{ onClick: _self.save },
						root.T("Save", "Save")
					),
					React.createElement(
						ReactBootstrap.Button,
						{ onClick: _self.closeSyncModel },
						root.T("Cancel", "Cancel")
					)
				)
			);
		},

		closeSyncModel: function () {
			this.props.root.actions.closeEditTicketModal();
		}
	});

	orchardcollaboration.react.allComponents.EditTicketModal = EditTicketModal;
})();