
var orchardcollaboration = orchardcollaboration || {};
orchardcollaboration.react = orchardcollaboration.react || {};
orchardcollaboration.react.allComponents  = orchardcollaboration.react.allComponents  || {};

(function(){
		function formatDate(date) {
		  var hours = date.getHours();
		  var minutes = date.getMinutes();
		  var ampm = hours >= 12 ? 'pm' : 'am';
		  hours = hours % 12;
		  hours = hours ? hours : 12; // the hour '0' should be '12'
		  minutes = minutes < 10 ? '0'+minutes : minutes;
		  var strTime = hours + ':' + minutes + ' ' + ampm;
		  return date.getMonth()+1 + "/" + date.getDate() + "/" + date.getFullYear() + "  " + strTime;
		};

		var x = orchardcollaboration.react.allComponents;
		
		var Parts_SyncProjectTitle_Summary = React.createClass({
			render: function(){
				var projectDisplayRoute = this.props.root.Routes.ProjectUrl;
				var url = decodeURI(projectDisplayRoute);
				url = url.replace("{id}", this.props.Model.Record.Id);

				return (<h4><a href={url} target="_blank">{this.props.Model.Record.Title}</a></h4>);
			}
		});
		x.Parts_SyncProjectTitle_Summary = Parts_SyncProjectTitle_Summary;	
		
		var Parts_Common_Metadata__SyncProject = React.createClass({
			render: function(){
				var date = new Date(this.props.ContentPart.ModifiedUtc);
				var dateString = formatDate(date);
				return (<div className='modified-date'>
							<span className="date-label">{this.props.root.T("LastModifiedTime", "Last Modified Time")}:</span>
							<span className="date">{dateString}</span>
						</div>);
			}
		});
		x.Parts_Common_Metadata__SyncProject = Parts_Common_Metadata__SyncProject;	

		var Content_SyncProject_Detail = React.createClass({
			render: function(){

				var content = null;
				var footer = null;
				if (this.props.shape.Content != null){
					content = (<x.Display root={this.props.root} shape={this.props.shape.Content}></x.Display>);
				}

				if (this.props.shape.Footer != null){
					footer = (<x.Display root={this.props.root} shape={this.props.shape.Footer}></x.Display>);
				}
				return (
							<div>
								{content}
								{footer}
							</div>
					   );
			}
		});
		x.Content_SyncProject_Detail = Content_SyncProject_Detail;

		var SuiteCRMSyncUsers = React.createClass({
			
			showSyncModal: function(){
			},

			getInitialState: function(){
				return {
					isSyncModalOpen: false,
					operation: null,
					modalStateError: null,
					modalStateClass: "hidden",
					selectedUser: null
				};
			},

			showSyncModal: function(user, operation){
				
				// check is there any user to sync
				var isThereAnyUser = false;
				if (operation && operation == "copyAll"){
					for(var i = 0; i < this.props.model.Users.length; i++){
						if (!this.props.model.Users[i].IsSync){
							isThereAnyUser = true;
							break;
						}
					}

					if (!isThereAnyUser){
						return;
					}
				}
				
				this.state.modalStateClass = "hidden";
				this.state.modalStateError = "";
				this.state.isSyncModalOpen = true;
				this.state.operation = operation
				this.state.selectedUser = user;
				this.setState(this.state);
			},

			closeSyncModal: function(){
				this.state.isSyncModalOpen = false;
				this.setState(this.state);
			},

			runSyncing: function(){
				var root = this.props.root;			

				if (!this.refs.password.value){
					this.state.modalStateClass = "visible";
					this.state.modalStateError = root.T("ProvidePassword", "Please enter the password");
					this.setState(this.state);
					return;
				}
				
				if(this.refs.password.value != this.refs.confirmPassword.value){
					this.state.modalStateClass = "visible";
					this.state.modalStateError = root.T("ProvidePassword", "Confirm password doesn't match the given password.");
					this.setState(this.state);
					return;
				}

				this.state.isSyncModalOpen = false;
				this.setState(this.state);

				if (this.state.operation == "copyAll"){
					this.props.root.actions.copyAllUsersToOrchard(this.refs.password.value);
				}
				else{
					this.props.root.actions.copyUserToOrchard(this.state.selectedUser, this.refs.password.value);
				}
			},

			render: function(){
				var _self = this;
				var root = _self.props.root;			

				if (!_self.props.model.ViewUsersPage){
					return <script></script>
				}

				var users = this.props.model.Users.map(function(user){
					var orchardUser = null;
					if (user.IsSync){
							var userUrl = decodeURI(root.Routes.OperatorLink);
							userUrl = userUrl.replace("{userid}", user.OrchardUserId);
							orchardUser = 
								(<div className="orchard-project-container full">
									<div>
										<h4><a target="_blank" href={userUrl}>{user.OrchardUsername}</a></h4>
										<div className="email"><span className="email-label">{root.T("Email", "Email")}:</span><a target="_blank" href={userUrl}>{user.OrchardEmail}</a></div>
									</div>
								</div>);
					}
					else{
						orchardUser = <div className="orchard-project-container empty"></div>
					}

					var syncLink = null;
					if (!user.IsSync){
						syncLink = <a className="sync" title={root.T("sync")} onClick={_self.showSyncModal.bind(null, user, "")}></a>
					}
					
					return (
							<div className="sync-project-row data-row">
								{orchardUser}
								<div className="suitecrm-project-container full">
									<div>
										<h4>{user.SuiteCRMUsername}</h4>
										<div className='email'><span className="email-label">{root.T("Email", "Email")}:</span><span>{user.SuiteCRMEmail}</span></div>
									</div>
									{syncLink}
								</div>
							</div>);
				});

				if (users.length == 0){
					users = <div>{root.T("ThereIsNoUser", "There is no user to import")}</div>
				}

				var syncAllLink = null;
				// check is there any user to sync
				var isThereAnyUser = false;
				for(var i = 0; i < this.props.model.Users.length; i++){
					if (!this.props.model.Users[i].IsSync){
						isThereAnyUser = true;
						break;
					}
				}
				
				if (isThereAnyUser){
					syncAllLink = <a className="sync" title={root.T("sync")} onClick={_self.showSyncModal.bind(null, null, "copyAll")}></a>;
				}

				return (
						<article className="panel panel-default modal-container">
							<div className="panel-heading">
								<h2>{root.T("SuiteCRMIntegration", "Sugar CRM Integration")}</h2>
								<div className="suitecrm-top-linkbar">
									<a onClick={root.actions.showProjects}>{root.T("Projects", "Projects")}</a>
									<span>{root.T("Users", "Users")}</span>
								</div>
							</div>
							<div className="panel-body">
								<div className="suitecrm-sync-data">
									<div className="message">{root.T("PleaseNoteOnlyUsersWithAssociateEmail", "Please note that only users with associated email can be imported to Orchard")}</div>
									<div className="sync-project-row header-row">
										<div className="orchard-project-container">
											<h4 className='current-list'>{root.T("OrchardCollaborationUsers", "OrchardCollaboration Users")}</h4>
										</div>
										<div className="suitecrm-project-container">
											<h4 className='current-list'>{root.T("SuiteCRMUsers", "Sugar CRM Users")}</h4>
											{syncAllLink}
										</div>
									</div>
									{users}
								</div>
							</div>
							<ReactBootstrap.Modal show={_self.state.isSyncModalOpen} onHide={_self.closeSyncModal}>
								<ReactBootstrap.Modal.Header closeButton>
									<ReactBootstrap.Modal.Title>{root.T("SyncData", "Sync Data")}</ReactBootstrap.Modal.Title>
								</ReactBootstrap.Modal.Header>
								<ReactBootstrap.Modal.Body>
									<div className="sync-modal-body">
										<div className={'error ' + _self.state.modalStateClass}>{_self.state.modalStateError}</div>
										<div className="message">{root.T("PleaseEnterDefaultPassword","Please provide the default password for the selected users. They can change password after loging into the system.")}</div>
										<div className="data-row">
											<span className='modal-label'>{root.T("Password", "Password:")}</span>
											<input type='password' ref='password'/>
										</div>
										<div className="data-row">
											<span className='modal-label'>{root.T("ConfirmPassword", "Confirm Password:")}</span>
											<input type='password' ref='confirmPassword'/>
										</div>
									</div>
								</ReactBootstrap.Modal.Body>
								<ReactBootstrap.Modal.Footer>
									<ReactBootstrap.Button onClick={_self.runSyncing}>{root.T("Sync", "Sync")}</ReactBootstrap.Button>
									<ReactBootstrap.Button onClick={_self.closeSyncModal}>{root.T("Cancel", "Cancel")}</ReactBootstrap.Button>
								</ReactBootstrap.Modal.Footer>
							</ReactBootstrap.Modal>
						</article>);
			}
		});
		x.SuiteCRMSyncUsers = SuiteCRMSyncUsers;

		var SuiteCRMDataSync = React.createClass({
			
			copyOrchardProjectToSuiteProject: function(project){
				this.props.root.actions.copyOrchardProjectToSuiteProject(project);
			},			
			
			closeSyncModel: function(){
				this.state.syncFormData.isOpen = false;
				this.setState(this.state);
			},

			runSyncing: function(){
				this.state.syncFormData.isOpen = false;
				this.setState(this.state);

				var syncFormData = this.state.syncFormData;
				switch(this.state.selectedOperation){
					case "copySuiteProjectToOrchardProject":
						this.props.root.actions.copySuiteProjectToOrchard(this.state.selectedProject, syncFormData.syncTickets, syncFormData.doNotOverrideNewerValues, syncFormData.syncSubTasks);
					break;
					case "copyOrchardProjectToSuiteProject":
						this.props.root.actions.copyOrchardProjectToSuiteProject(this.state.selectedProject, syncFormData.syncTickets, syncFormData.doNotOverrideNewerValues, syncFormData.syncSubTasks);
					break;
					case "copyAllOrchardProjectToSuiteProject":
						this.props.root.actions.copyAllOrchardProjectToSuiteProject(syncFormData.syncTickets, syncFormData.doNotOverrideNewerValues, syncFormData.syncSubTasks);
					break;
					case "copyAllSuiteProjectToOrchardProject":
						this.props.root.actions.copyAllSuiteProjectToOrchardProject(syncFormData.syncTickets, syncFormData.doNotOverrideNewerValues, syncFormData.syncSubTasks);
					break;
				}
			},

			getInitialState: function(){
				return {
					selectedOperation: null,
					selectedProject: null,
					syncFormData : {
					    syncSubTasks: false,
						isOpen: false,
						doNotOverrideNewerValues: true,
						syncTickets: true,
						syncTicketsClass: "hidden"
					}
				};
			},

			showSyncModal: function(project, operation){
					this.state.selectedProject = project;
					this.state.selectedOperation = operation;
					this.state.syncFormData.isOpen = true;

					if (operation === "copyAllOrchardProjectToSuiteProject" || operation === "copyOrchardProjectToSuiteProject"){
						this.state.syncFormData.syncTicketsClass = "";
					}
					else{
						this.state.syncFormData.syncTicketsClass = "hidden";
					}

					this.setState(this.state);
			},
			
			getPager: function(page, count){
				var _self = this;
				var suiteCRMPager = null;
			    var root = _self.props.root;			
				
				var projectsPhrase = root.T("ProjectsCount", " of {0} projects");
				projectsPhrase = projectsPhrase.replace("{0}", count);

				var start = _self.props.model.PageSize * page;
				var end = _self.props.model.Projects.length + start;

				var previousLink = null;
				if (start > 0){
					previousLink = <a onClick={root.actions.previousPage}>{root.T("Previous", "Previous")}</a>
				}
				else{
					previousLink = <span></span>
				}

				var nextLink = null;					
				if (end < count){
					nextLink = <a onClick={root.actions.nextPage}>{root.T("Next", "Next")}</a>
				}
				else{
					nextLink = <span></span>
				}

				pager = (<div className='projects-pager'>
									{previousLink}
									<span className='current'>{start} - {end}</span>
									<span className='of-projects'>{projectsPhrase}</span>
									{nextLink}
									</div>);			

				return pager;
			},

			changeSyncFormCheckboxState: function(e){
				  var property = e.target.getAttribute('data-property');
	    		  var value = e.target.checked;

				  this.state.syncFormData[property] = value;
				  this.setState(this.state);
			},

			render: function(){
	
				var _self = this;
				var root = _self.props.root;			

				if (_self.props.model.ViewUsersPage){
					return <script></script>
				}

				var keyCounter = 1;
				var projects = this.props.model.Projects.map(function(project){
					
					var orchardProject = null;

					var syncDateString = "-";
					if (project.LastSyncTime){
						var date = new Date(project.LastSyncTime);
						syncDateString = formatDate(date);
					}
									
					if (project.OrchardCollaborationProject != null){
						orchardProject = <div className="orchard-project-container full">
											<x.Content_SyncProject_Detail root={root} shape={project.OrchardProjectShape}></x.Content_SyncProject_Detail>
											<a className="sync" title={root.T("sync")} onClick= {_self.showSyncModal.bind(null, project, "copyOrchardProjectToSuiteProject")}></a>
											<div className="last-sync-time"><span className="date-label">{root.T("LastSyncTime", "Last Sync Time")}:</span><span className="date">{syncDateString}</span></div>
										</div>
					}
					else{
						orchardProject = <div className="orchard-project-container empty">
											<div className="last-sync-time">{root.T("LastSyncTime", "Last Sync Time")}:{syncDateString}</div>
										 </div>
					}

					var suiteCRMProject = null;

					if (project.SuiteCRMProject != null){
						var date = new Date(project.SuiteCRMProject.ModifiedDateTime != null? project.SuiteCRMProject.ModifiedDateTime: project.SuiteCRMProject.CreationDateTime);
						var dateString = formatDate(date);
						suiteCRMProject =(<div className="suitecrm-project-container full">
											<div>
												<h4>{project.SuiteCRMProject.Name}</h4>
												<div className='modified-date'><span className="date-label">{root.T("LastModifiedTime", "Last Modified Time")}:</span><span className="date">{dateString}</span></div>
											</div>
											<a className="sync" title={root.T("sync")} onClick={_self.showSyncModal.bind(null, project, "copySuiteProjectToOrchardProject")}></a>
										 </div>);
					}
					else{
						suiteCRMProject = <div className="suitecrm-project-container empty"></div>
					}
					
					var key = "project" + keyCounter;
					keyCounter++;
					return (<div key={key} className="sync-project-row data-row">
								{orchardProject}
								{suiteCRMProject }
							</div>);
				});

				var suiteCRMPager = null;
				var orchardCollaborationPager = null;
				var orchardCollaborationListHeader = null;
				var suiteCRMListHeader = null;

				if (!_self.props.model.ListedBasedOnSuiteCRM){
					orchardCollaborationPager = _self.getPager(_self.props.model.OrchardCollaborationPage, _self.props.model.OrchardCollaborationProjectsCount);
					suiteCRMPager = <div>{_self.props.model.SuiteCRMProjectsCount} {root.T("Projects")}</div>
					orchardCollaborationListHeader = (<h4 className='current-list'>{root.T("OrchardCollaborationProjects", "OrchardCollaboration Projects")}</h4>);
					suiteCRMListHeader = (<h4><a href="#" onClick={root.actions.switchProjectList}>{root.T("SuiteCRMProjects", "Sugar CRM Projects")}</a></h4>);
				}
				else{
					orchardCollaborationPager = <div>{_self.props.model.OrchardCollaborationProjectsCount} {root.T("Projects")}</div>
					suiteCRMPager = _self.getPager(_self.props.model.SuiteCRMPage, _self.props.model.SuiteCRMProjectsCount);
					orchardCollaborationListHeader = (<h4>
														<a href="#" onClick={root.actions.switchProjectList}>{root.T("OrchardCollaborationProjects", "OrchardCollaboration Projects")}
														</a>
													 </h4>);
					suiteCRMListHeader = (<h4 className='current-list'>{root.T("SuiteCRMProjects", "Sugar CRM Projects")}</h4>);
				}
				

				return (
						<article className="panel panel-default modal-container">
							<div className="panel-heading">
								<h2>{root.T("SuiteCRMIntegration", "Sugar CRM Integration")}</h2>
								<div className="suitecrm-top-linkbar">
									<span>{root.T("Projects", "Projects")}</span>
									<a onClick={root.actions.showUsers}>{root.T("Users", "Users")}</a>
								</div>
							</div>
							<div className="panel-body">
								<div className="suitecrm-sync-data">
									<div className="sync-project-row header-row">
										<div className="orchard-project-container">
											{orchardCollaborationListHeader}
											{orchardCollaborationPager}
											<a className="sync" title={root.T("sync")} onClick= {_self.showSyncModal.bind(null, null, "copyAllOrchardProjectToSuiteProject")}></a>
										</div>
										<div className="suitecrm-project-container">
											{suiteCRMListHeader}
											{suiteCRMPager}
											<a className="sync" title={root.T("sync")} onClick={_self.showSyncModal.bind(null, null, "copyAllSuiteProjectToOrchardProject")}></a>
										</div>
									</div>
									{projects}
								</div>
							</div>
							<ReactBootstrap.Modal show={_self.state.syncFormData.isOpen} onHide={_self.closeSyncModel}>
								<ReactBootstrap.Modal.Header closeButton>
									<ReactBootstrap.Modal.Title>{root.T("SyncData", "Sync Data")}</ReactBootstrap.Modal.Title>
								</ReactBootstrap.Modal.Header>
								<ReactBootstrap.Modal.Body>
									<div className="sync-modal-body">
										<div>
											<input type="checkbox" onChange={_self.changeSyncFormCheckboxState} data-property='syncTickets' defaultChecked={_self.state.syncFormData.syncTickets} value='true'/>
											<span className="checkbox-label">{root.T("SyncTickets", "Sync Tickets")}</span>
										</div>
										<div>
											<input type="checkbox" onChange={_self.changeSyncFormCheckboxState} data-property='doNotOverrideNewerValues' defaultChecked={_self.state.syncFormData.doNotOverrideNewerValues} value='true'/>
											<span className="checkbox-label">{root.T("DoNotOverrideNewerValues", "Don't override newer version")}</span>
										</div>
										<div className={_self.state.syncFormData.syncTicketsClass}>
											<input type="checkbox"  onChange={_self.changeSyncFormCheckboxState} data-property='syncSubTasks' defaultChecked={_self.state.syncFormData.syncSubTasks} value='true'/>
											<span className="checkbox-label">{root.T("SyncSubTasks", "Sync Sub Tasks")}</span>
										</div>
									</div>
								</ReactBootstrap.Modal.Body>
								<ReactBootstrap.Modal.Footer>
									<ReactBootstrap.Button onClick={_self.runSyncing}>{root.T("Sync", "Sync")}</ReactBootstrap.Button>
									<ReactBootstrap.Button onClick={_self.closeSyncModel}>{root.T("Cancel", "Cancel")}</ReactBootstrap.Button>
								</ReactBootstrap.Modal.Footer>
							</ReactBootstrap.Modal>
						</article>);
			}
		});

		orchardcollaboration.react.allComponents.SuiteCRMDataSync = SuiteCRMDataSync;
})();