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

        getInitialState: function () {
            return {
                showDeleteConfirm: false,
                selectedItem: null
            }
        },

        deleteItem: function (item) {
            this.state.selectedItem = item;
            this.state.showDeleteConfirm = true;
            this.setState(this.state);
        },

        yesDelete: function () {
            this.state.showDeleteConfirm = false;
            this.props.root.actions.deleteItem(this.state.selectedItem);
            this.setState(this.state);
        },

        noDelete: function () {
            this.state.showDeleteConfirm = false;
            this.setState(this.state);
        },

        render: function () {
            var _self = this;
            var root = this.props.root;

            var items = this.props.data.Model.Items.map(function (item) {

                var buttons = item.UserCanEdit ?
                    (
                        <div className='actions'>
                            <button className='edit-link' onClick={_self.edit.bind(null, item) } title={root.T("Edit", "Edit")}>&nbsp;</button>
                            <button className='delete-link' onClick={_self.deleteItem.bind(null, item) } title={root.T("Delete", "Delete")}>X</button>
                        </div>
                    ) : "";

                var key = 'item' + item.TrackingItemId;
                var date = new Date(item.TrackingDate);
                var dateStr = date.getFullYear() + '-' + (date.getMonth() + 1).toString() + '-' + date.getDate();
                return (<li key={key}>
                            <div className='row'>
                                <div className='col-lg-2'>
                                    <div className='hidden-lg first-column'>
                                       <span className='t-label'>{root.T("TimeSpent:", "Time Spent:")}</span>
                                      <span>{item.TrackedTimeInString}</span>
                                    </div>
                                    <div className='hidden-sm hidden-md hidden-xs'>{item.TrackedTimeInString}</div>
                                </div>
                                <div className='col-lg-2'>
                                    <div className='hidden-lg'>
                                        <span className='t-label'>{root.T("DateStarted", "Date Started:")}</span>
                                        <span>{dateStr}</span>
                                    </div>
                                    <div className='hidden-sm hidden-md hidden-xs'>{dateStr}</div>
                                </div>
                                <div className='col-lg-2'>
                                    <div className='hidden-lg'>
                                        <span className='t-label'>{root.T("User", "User:")}</span>
                                        <span>{item.FullUsername}</span>
                                    </div>
                                    <div className='hidden-sm hidden-md hidden-xs'>{item.FullUsername}</div>
                                </div>
                                <div className='col-lg-3'>
                                    <div className='hidden-lg'>
                                        <span className='t-label'>{root.T("WorkDescription", "Work Description:")}</span>
                                        <span>{item.Comment}</span>
                                    </div>
                                    <div className='hidden-sm hidden-md hidden-xs'>{item.Comment}</div>
                                </div>
                                <div className='col-lg-3 hidden-sm hidden-md hidden-xs'>
                                    {buttons}
                                </div>
                                <div className='corner hidden-lg'>
                                    {buttons}
                                </div>
                            </div>
                            <hr className='hidden-lg' />
                </li>);
            });

            if (this.props.data.Model.Items.length == 0) {
                items = <div className='no-item'>{root.T("ThereIsNoItem", "There is no item")}</div>
            }

            return (<div className="log-works">
                        <div className='header'>
                            <div className='title'>{root.T("Log items", "Log Items")}</div>
                            <div className="add-item"><button className='btn btn-primary' onClick={this.showAddDialog}>{root.T("Log new work", "Log new work")}</button></div>
                            <div className='clear'></div>
                        </div>
                        <div>
                            <ul>
                                <li className="hidden-sm hidden-md hidden-xs header-row">
                                    <div className='col-lg-2'>
                                        {root.T("TimeSpent:", "Time Spent:")}
                                    </div>
                                    <div className='col-lg-2'>
                                        {root.T("DateStarted", "Date Started:")}
                                    </div>
                                    <div className='col-lg-2'>
                                        {root.T("User", "User")}
                                    </div>
                                    <div className='col-lg-3'>
                                        {root.T("WorkDescription", "Work Description:")}
                                    </div>
                                    <div className='col-lg-3'>
                                    </div>
                                </li>
                                {items}
                            </ul>
                            <ReactBootstrap.Modal className="confirm-modal" show={_self.state.showDeleteConfirm}>
				                <ReactBootstrap.Modal.Header closeButton>
					                <ReactBootstrap.Modal.Title>
					                    {root.T("Confirm", "Confirm")}
					                </ReactBootstrap.Modal.Title>
				                </ReactBootstrap.Modal.Header>
				                <ReactBootstrap.Modal.Body>
            					    <div>
            					        {root.T("DeleteItemConfirmMessage", "Are you sure you want to delete the selected item?")}
            					    </div>
				                </ReactBootstrap.Modal.Body>
			    	            <ReactBootstrap.Modal.Footer>
				    	            <ReactBootstrap.Button onClick={_self.yesDelete }>{root.T("Yes", "Yes")}</ReactBootstrap.Button>
					                <ReactBootstrap.Button onClick={_self.noDelete }>{root.T("No", "No")}</ReactBootstrap.Button>
			    	            </ReactBootstrap.Modal.Footer>
                            </ReactBootstrap.Modal>
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
            var timeSpendExpression = /^\s*(\d+[d])?(\s*\d+[h])?(\s*\d+[m])?\s*$/;

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
            $(this.refs.trackingDate).datepicker({ dateFormat: 'yy-mm-dd' });
        },

        render: function () {
            var _self = this;
            var root = this.props.root;
            var selectedItem = this.props.data.selectedItem;

            var title = "Log new item";
            var comment = "";
            var date = new Date();
            var username = "";
            var timeSpend = "";
            if (selectedItem) {
                userName = selectedItem.FullUsername;
                title = selectedItem.title;
                comment = selectedItem.Comment;
                timeSpend = selectedItem.TrackedTimeInString;

                if (selectedItem.trackingDate) {
                    date = new Date(selectedItem.TrackingDate);
                }
            }

            var dateStr = date.getFullYear() + '-' + (date.getMonth() + 1).toString() + '-' + date.getDate();

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
							<div className='label-row'>{root.T("DateStarted", "Date Started:")}<span className='error'>*</span></div>
							<div><input ref="trackingDate" name="trackingDate" type='text' defaultValue={dateStr} />{dateValidation}</div>
						</div>
						<div>
							<div className='label-row'> <span>{root.T("Time spent", "Time spent")}</span><span className='error'>*</span><span className='hint'>{root.T("timeSample", "(eg. 5d 4h 15m)")}</span></div>
							<div><input ref="timeSpend" type='text' defaultValue={timeSpend } />{timeSpanValidation}</div>
						</div>
						<div>
							<div className='label-row'>{root.T("WorkDescription", "Work Description:")}</div>
							<div><textarea ref="comment" defaultValue={comment}></textarea></div>
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