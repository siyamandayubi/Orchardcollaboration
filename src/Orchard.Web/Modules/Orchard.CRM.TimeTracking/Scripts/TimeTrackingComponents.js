window.orchardcollaboration = window.orchardcollaboration || {};
var orchardcollaboration = window.orchardcollaboration;
orchardcollaboration.react = orchardcollaboration.react || {};
orchardcollaboration.react.allComponents = orchardcollaboration.react.allComponents || {};

(function () {

    var TimeTrackingList = React.createClass({
        displayName: 'TimeTrackingList',

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
            };
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

                var buttons = item.UserCanEdit ? React.createElement(
                    'div',
                    { className: 'actions' },
                    React.createElement(
                        'button',
                        { className: 'edit-link', onClick: _self.edit.bind(null, item), title: root.T("Edit", "Edit") },
                        '\xA0'
                    ),
                    React.createElement(
                        'button',
                        { className: 'delete-link', onClick: _self.deleteItem.bind(null, item), title: root.T("Delete", "Delete") },
                        'X'
                    )
                ) : "";

                var key = 'item' + item.TrackingItemId;
                var date = new Date(item.TrackingDate);
                var dateStr = date.getFullYear() + '-' + (date.getMonth() + 1).toString() + '-' + date.getDate();
                return React.createElement(
                    'li',
                    { key: key },
                    React.createElement(
                        'div',
                        { className: 'row' },
                        React.createElement(
                            'div',
                            { className: 'col-lg-2' },
                            React.createElement(
                                'div',
                                { className: 'hidden-lg first-column' },
                                React.createElement(
                                    'span',
                                    { className: 't-label' },
                                    root.T("TimeSpent:", "Time Spent:")
                                ),
                                React.createElement(
                                    'span',
                                    null,
                                    item.TrackedTimeInString
                                )
                            ),
                            React.createElement(
                                'div',
                                { className: 'hidden-sm hidden-md hidden-xs' },
                                item.TrackedTimeInString
                            )
                        ),
                        React.createElement(
                            'div',
                            { className: 'col-lg-2' },
                            React.createElement(
                                'div',
                                { className: 'hidden-lg' },
                                React.createElement(
                                    'span',
                                    { className: 't-label' },
                                    root.T("DateStarted", "Date Started:")
                                ),
                                React.createElement(
                                    'span',
                                    null,
                                    dateStr
                                )
                            ),
                            React.createElement(
                                'div',
                                { className: 'hidden-sm hidden-md hidden-xs' },
                                dateStr
                            )
                        ),
                        React.createElement(
                            'div',
                            { className: 'col-lg-2' },
                            React.createElement(
                                'div',
                                { className: 'hidden-lg' },
                                React.createElement(
                                    'span',
                                    { className: 't-label' },
                                    root.T("User", "User:")
                                ),
                                React.createElement(
                                    'span',
                                    null,
                                    item.FullUsername
                                )
                            ),
                            React.createElement(
                                'div',
                                { className: 'hidden-sm hidden-md hidden-xs' },
                                item.FullUsername
                            )
                        ),
                        React.createElement(
                            'div',
                            { className: 'col-lg-3' },
                            React.createElement(
                                'div',
                                { className: 'hidden-lg' },
                                React.createElement(
                                    'span',
                                    { className: 't-label' },
                                    root.T("WorkDescription", "Work Description:")
                                ),
                                React.createElement(
                                    'span',
                                    null,
                                    item.Comment
                                )
                            ),
                            React.createElement(
                                'div',
                                { className: 'hidden-sm hidden-md hidden-xs' },
                                item.Comment
                            )
                        ),
                        React.createElement(
                            'div',
                            { className: 'col-lg-3 hidden-sm hidden-md hidden-xs' },
                            buttons
                        ),
                        React.createElement(
                            'div',
                            { className: 'corner hidden-lg' },
                            buttons
                        )
                    ),
                    React.createElement('hr', { className: 'hidden-lg' })
                );
            });

            if (this.props.data.Model.Items.length == 0) {
                items = React.createElement(
                    'div',
                    { className: 'no-item' },
                    root.T("ThereIsNoItem", "There is no item")
                );
            }

            return React.createElement(
                'div',
                { className: 'log-works' },
                React.createElement(
                    'div',
                    { className: 'header' },
                    React.createElement(
                        'div',
                        { className: 'title' },
                        root.T("Log items", "Log Items")
                    ),
                    React.createElement(
                        'div',
                        { className: 'add-item' },
                        React.createElement(
                            'button',
                            { className: 'btn btn-primary', onClick: this.showAddDialog },
                            root.T("Log new work", "Log new work")
                        )
                    ),
                    React.createElement('div', { className: 'clear' })
                ),
                React.createElement(
                    'div',
                    null,
                    React.createElement(
                        'ul',
                        null,
                        React.createElement(
                            'li',
                            { className: 'hidden-sm hidden-md hidden-xs header-row' },
                            React.createElement(
                                'div',
                                { className: 'col-lg-2' },
                                root.T("TimeSpent:", "Time Spent:")
                            ),
                            React.createElement(
                                'div',
                                { className: 'col-lg-2' },
                                root.T("DateStarted", "Date Started:")
                            ),
                            React.createElement(
                                'div',
                                { className: 'col-lg-2' },
                                root.T("User", "User")
                            ),
                            React.createElement(
                                'div',
                                { className: 'col-lg-3' },
                                root.T("WorkDescription", "Work Description:")
                            ),
                            React.createElement('div', { className: 'col-lg-3' })
                        ),
                        items
                    ),
                    React.createElement(
                        ReactBootstrap.Modal,
                        { className: 'confirm-modal', show: _self.state.showDeleteConfirm },
                        React.createElement(
                            ReactBootstrap.Modal.Header,
                            { closeButton: true },
                            React.createElement(
                                ReactBootstrap.Modal.Title,
                                null,
                                root.T("Confirm", "Confirm")
                            )
                        ),
                        React.createElement(
                            ReactBootstrap.Modal.Body,
                            null,
                            React.createElement(
                                'div',
                                null,
                                root.T("DeleteItemConfirmMessage", "Are you sure you want to delete the selected item?")
                            )
                        ),
                        React.createElement(
                            ReactBootstrap.Modal.Footer,
                            null,
                            React.createElement(
                                ReactBootstrap.Button,
                                { onClick: _self.yesDelete },
                                root.T("Yes", "Yes")
                            ),
                            React.createElement(
                                ReactBootstrap.Button,
                                { onClick: _self.noDelete },
                                root.T("No", "No")
                            )
                        )
                    )
                )
            );
        }
    });

    orchardcollaboration.react.allComponents.TimeTrackingList = TimeTrackingList;

    var EditLogWorkModal = React.createClass({
        displayName: 'EditLogWorkModal',

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
            };
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
            } else {
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

            this.props.root.actions.saveItem(data);
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

            var dateValidation = _self.state.dateValid ? "" : React.createElement(
                'div',
                { className: 'error' },
                _self.state.dateErrorMessage
            );
            var timeSpanValidation = _self.state.timeSpendValid ? "" : React.createElement(
                'div',
                { className: 'error' },
                _self.state.timeSpendErrorMessage
            );

            return React.createElement(
                ReactBootstrap.Modal,
                { className: 'edit-logwork-modal', show: _self.props.data.showModal, onHide: _self.closeSyncModel },
                React.createElement(
                    ReactBootstrap.Modal.Header,
                    { closeButton: true },
                    React.createElement(
                        ReactBootstrap.Modal.Title,
                        null,
                        title
                    )
                ),
                React.createElement(
                    ReactBootstrap.Modal.Body,
                    null,
                    React.createElement(
                        'div',
                        { className: 'edit-logwork-modal' },
                        React.createElement(
                            'div',
                            null,
                            React.createElement(
                                'div',
                                null,
                                React.createElement(
                                    'div',
                                    { className: 'label-row' },
                                    root.T("DateStarted", "Date Started:"),
                                    React.createElement(
                                        'span',
                                        { className: 'error' },
                                        '*'
                                    )
                                ),
                                React.createElement(
                                    'div',
                                    null,
                                    React.createElement('input', { ref: 'trackingDate', name: 'trackingDate', type: 'text', defaultValue: dateStr }),
                                    dateValidation
                                )
                            ),
                            React.createElement(
                                'div',
                                null,
                                React.createElement(
                                    'div',
                                    { className: 'label-row' },
                                    ' ',
                                    React.createElement(
                                        'span',
                                        null,
                                        root.T("Time spent", "Time spent")
                                    ),
                                    React.createElement(
                                        'span',
                                        { className: 'error' },
                                        '*'
                                    ),
                                    React.createElement(
                                        'span',
                                        { className: 'hint' },
                                        root.T("timeSample", "(eg. 5d 4h 15m)")
                                    )
                                ),
                                React.createElement(
                                    'div',
                                    null,
                                    React.createElement('input', { ref: 'timeSpend', type: 'text', defaultValue: timeSpend }),
                                    timeSpanValidation
                                )
                            ),
                            React.createElement(
                                'div',
                                null,
                                React.createElement(
                                    'div',
                                    { className: 'label-row' },
                                    root.T("WorkDescription", "Work Description:")
                                ),
                                React.createElement(
                                    'div',
                                    null,
                                    React.createElement('textarea', { ref: 'comment', defaultValue: comment })
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