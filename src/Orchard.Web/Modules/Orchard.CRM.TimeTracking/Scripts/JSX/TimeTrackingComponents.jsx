
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
            this.props.root.actions.editItem(item);
        },

        render: function () {
            var _self = this;
            var root = this.props.root;

            var items = this.props.data.Model.Items.map(function (item) {
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

        getInitialState: function () {
            return {
                isValid: true,
                dateValid: true,
                dateErrorMessage: "",
                timeSpendValid: true,
                timeSpendErrorMessage: ""
            }
        },

        checkValidation: function () {
            var timeSpendExpression = /^(\d[d])?(\s*\d[h])?(\s*\d[m])?\s*$/;

            this.state.isValid = true;
            this.state.timeSpendValid = true;
            this.state.dateValid = true;

            if (!this.refs.timeSpend.value) {
                this.state.isValid = false;
                this.state.timeSpendValid = false;
                this.state.timeSpendErrorMessage = this.props.root.T("TimespendRequiredError", "Spent time is a required field");
            }
            else {
                var match = timeSpendExpression.exec(this.refs.timeSpend.value);
                if (!match) {
                    this.state.isValid = false;
                    this.state.timeSpendValid = false;
                    this.state.timeSpendErrorMessage = this.props.root.T("TimespendFormatError", "The string format is not correct");
                }
            }

            if (!this.refs.trackingDate.value) {
                this.state.dateErrorMessage = "Date is required";
                this.state.isValid = false;
                this.state.dateValid = false;
            }
        },


        save: function () {

            this.checkValidation();

            if (!this.state.isValid) {
                this.setState(this.state);
                return;
            }

            var data = {
                trackingDate: this.refs.trackingDate.value,
                comment: this.refs.comment.value,
                timeSpend: this.refs.timeSpend.value                
            };

            if (this.props.data.selectedItem) {
                data.trackingItemId = this.props.data.selectedItem.TrackingItemId;
                data.userId = this.props.data.selectedItem.UserId;
            }

            this.props.root.actions.saveItem(data)
        },
        
        componentDidUpdate: function () {
            $(this.refs.trackingDate).datepicker();
        },

        render: function () {
            var _self = this;
            var root = this.props.root;
            var selectedItem = this.props.data.selectedItem;

            var title = "Log new item";
            var comment = "";
            var date = "";
            var username = "";
            var timeSpend = "";
            if (selectedItem) {
                userName = selectedItem.FullUsername;
                title = selectedItem.title;
                comment = selectedItem.Comment;
                timeSpend = selectedItem.TrackedTimeInString;
                date = selectedItem.TrackingDate;
            }

            var dateValidation = _self.state.dateValid ? "" : (<div className='error'>{_self.state.dateErrorMessage}</div>);
            var timeSpanValidation = _self.state.timeSpendValid ? "" : (<div className='error'>{_self.state.timeSpendErrorMessage}</div>);

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
							<div><input ref="trackingDate" name="trackingDate" type='text' defaultValue={date } />{dateValidation}</div>
						</div>
						<div>
							<div className='label-row'>{root.T("Time spend", "Time spend")}</div>
							<div><input ref="timeSpend" type='text' defaultValue={timeSpend } />{timeSpanValidation}</div>
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