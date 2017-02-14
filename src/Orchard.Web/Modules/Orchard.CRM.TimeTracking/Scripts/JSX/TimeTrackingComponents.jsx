
window.orchardcollaboration = window.orchardcollaboration || {};
var orchardcollaboration = window.orchardcollaboration;
orchardcollaboration.react = orchardcollaboration.react || {};
orchardcollaboration.react.allComponents = orchardcollaboration.react.allComponents || {};

(function () {

    var TimeTrackingList = React.createClass({
        render: function () {
            return (<div>It is for testing</div>);
        }
    });

    orchardcollaboration.react.allComponents.TimeTrackingList = TimeTrackingList;

    var EditLogWorkModal = React.createClass({
        closeSyncModel: function () {
        },

        save: function () {
        },

        render: function () {
            return (
        <ReactBootstrap.Modal className="edit-logwork-modal" show={_self.props.showEditModal} onHide={_self.closeSyncModel }>
				<ReactBootstrap.Modal.Header closeButton>
					<ReactBootstrap.Modal.Title>
					    {_self.props.title}
					</ReactBootstrap.Modal.Title>
				</ReactBootstrap.Modal.Header>
				<ReactBootstrap.Modal.Body>
					<div className="edit-logwork-modal">
					<div>
						<div>
							<div className='label-row'>{root.T("Date", "Date")}</div>
							<div><input ref="title" type='text' defaultValue={date } /></div>
						</div>
						<div>
							<div className='label-row'>{root.T("Time spend", "Time spend")}</div>
							<div><input ref="title" type='text' defaultValue={timeSpend } /></div>
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