
window.orchardcollaboration = window.orchardcollaboration || {};
var orchardcollaboration = window.orchardcollaboration;
orchardcollaboration.react = orchardcollaboration.react || {};
orchardcollaboration.react.allComponents = orchardcollaboration.react.allComponents || {};

(function () {

    var TimeTrackingList = React.createClass({
        showAddDialog: function () {
            this.props.root.actions.addItem();
        },
        edit: function (item) {

        },
        render: function () {
            var _self = this;
            var root = this.props.root;

            var items = this.props.data.Items.map(function (item) {
                return (<li>
                            <div>{item.FullUsername}</div>
                            <div>{item.TrackedTimeInString}</div>
                            <div>{item.Comment}</div>
                            <div><button onClick={_self.edit.bind(null, item)}>{root.T("Edit", "Edit")}</button></div>
                </li>);
            });

            return (<div>
                        <div>{root.T("Log items", "Log Items")}</div>
                        <div><button onClick={this.showAddDialog}>{root.T("Log new work", "Log new work")}</button></div>
                        <div>
                            <ul>
                                {items}
                            </ul>
                        </div>
            </div>);
        }
    });

    orchardcollaboration.react.allComponents.TimeTrackingList = TimeTrackingList;

    var EditLogWorkModal = React.createClass({
        closeSyncModel: function () {
            this.props.root.actions.closeModal();
        },

        save: function () {
            var data = {
                date: this.refs.date.value,
                comment: this.refs.comment.value,
                timeSpend: this.refs.timeSpend
            };

            if (this.props.data.selectedItem) {
                data.id = this.props.data.selectedItem.id;
            }

            this.props.root.actions.saveItem(data)
        },

        render: function () {
            var _self = this;
            var root = this.props.root;
            var selectedItem = this.props.data.selectedItem;

            var title = "Log new item";
            var comment = "";
            var date = "";
            var timeSpend = "";
            if (selectedItem) {
                title = selectedItem.title;
                comment = selectedItem.comment;
                timeSpend = selectedItem.timeSpend;
            }

            return (
        <ReactBootstrap.Modal className="edit-logwork-modal" show={_self.props.data.showModal} onHide={_self.closeSyncModel }>
				<ReactBootstrap.Modal.Header closeButton>
					<ReactBootstrap.Modal.Title>
					    {title}
					</ReactBootstrap.Modal.Title>
				</ReactBootstrap.Modal.Header>
				<ReactBootstrap.Modal.Body>
					<div className="edit-logwork-modal">
					<div>
						<div>
							<div className='label-row'>{root.T("Date", "Date")}</div>
							<div><input ref="date" type='text' defaultValue={date } /></div>
						</div>
						<div>
							<div className='label-row'>{root.T("Time spend", "Time spend")}</div>
							<div><input ref="timeSpend" type='text' defaultValue={timeSpend } /></div>
						</div>
						<div>
							<div className='label-row'>{root.T("Comment", "Comment")}</div>
							<div><textarea ref="comment">{comment}</textarea></div>
						</div>
					</div>
					</div>
				</ReactBootstrap.Modal.Body>
				<ReactBootstrap.Modal.Footer>
					<ReactBootstrap.Button onClick={_self.save }>{root.T("Save", "Save")}</ReactBootstrap.Button>
					<ReactBootstrap.Button onClick={_self.closeSyncModel }>{root.T("Cancel", "Cancel")}</ReactBootstrap.Button>
				</ReactBootstrap.Modal.Footer>
        </ReactBootstrap.Modal>
			);
        }
    });

    orchardcollaboration.react.allComponents.EditLogWorkModal = EditLogWorkModal;

})();