
window.orchardcollaboration = window.orchardcollaboration || {};
var orchardcollaboration = window.orchardcollaboration;
orchardcollaboration.react = orchardcollaboration.react || {};
orchardcollaboration.react.allComponents = orchardcollaboration.react.allComponents || {};

(function () {

				var TimeTrackingList = React.createClass({
								displayName: "TimeTrackingList",

								render: function () {
												return React.createElement(
																"div",
																null,
																"It is for testing"
												);
								}
				});

				orchardcollaboration.react.allComponents.TimeTrackingList = TimeTrackingList;

				var EditLogWorkModal = React.createClass({
								displayName: "EditLogWorkModal",

								closeSyncModel: function () {},

								save: function () {},

								render: function () {
												return React.createElement(
																ReactBootstrap.Modal,
																{ className: "edit-logwork-modal", show: _self.props.showEditModal, onHide: _self.closeSyncModel },
																React.createElement(
																				ReactBootstrap.Modal.Header,
																				{ closeButton: true },
																				React.createElement(
																								ReactBootstrap.Modal.Title,
																								null,
																								_self.props.title
																				)
																),
																React.createElement(
																				ReactBootstrap.Modal.Body,
																				null,
																				React.createElement(
																								"div",
																								{ className: "edit-logwork-modal" },
																								React.createElement(
																												"div",
																												null,
																												React.createElement(
																																"div",
																																null,
																																React.createElement(
																																				"div",
																																				{ className: "label-row" },
																																				root.T("Date", "Date")
																																),
																																React.createElement(
																																				"div",
																																				null,
																																				React.createElement("input", { ref: "title", type: "text", defaultValue: date })
																																)
																												),
																												React.createElement(
																																"div",
																																null,
																																React.createElement(
																																				"div",
																																				{ className: "label-row" },
																																				root.T("Time spend", "Time spend")
																																),
																																React.createElement(
																																				"div",
																																				null,
																																				React.createElement("input", { ref: "title", type: "text", defaultValue: timeSpend })
																																)
																												),
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
																																				React.createElement(
																																								"textarea",
																																								{ ref: "comment" },
																																								comment
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
								}
				});

				orchardcollaboration.react.allComponents.EditLogWorkModal = EditLogWorkModal;
})();