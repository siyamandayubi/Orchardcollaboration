
window.orchardcollaboration = window.orchardcollaboration || {};
var orchardcollaboration = window.orchardcollaboration;
orchardcollaboration.react = orchardcollaboration.react || {};
orchardcollaboration.react.allComponents  = orchardcollaboration.react.allComponents  || {};

(function(){
var x = orchardcollaboration.react.allComponents;
var Parts_Ticket_TableRow = React.createClass({
		menuClick: function(action, contentId){
			this.props.root.actions.ticketMenu(action, contentId)
		},

		componentDidMount: function(){
                var domNode = $(ReactDOM.findDOMNode(this));
				domNode.ProjectMenu({
					projectMenuCssClass: "milestone-item-menu"
				});

				domNode.on("mouseleave", function(){
					domNode.find(".milestone-item-menu").hide();
					domNode.find(".pivot").data("state", "off");
				})
		},

		render: function(){
		var _self = this;
		var mainProps = this.props;
		var root = mainProps.root;
		var ticketDisplayRoute = root.Routes.DisplayTicket;
		var url = decodeURI(ticketDisplayRoute);
		url = url.replace("{id}", mainProps.Model.TicketId);

		var menu = null;
		if (mainProps.Model.Menu && mainProps.Model.Menu.length > 0){
			var menuItems = mainProps.Model.Menu.map(function(menuItem){
			return <li onClick={_self.menuClick.bind(null, menuItem.Id, mainProps.Model.ContentItemId)}>{menuItem.Text}</li>
			});

			menu = <div className="milestone-item-menu-container">
					  <span className="pivot">
						  
                      </span>
					  <ul className="milestone-item-menu menu-hidden z2">
							{menuItems}
					  </ul>
				   </div>
		}
		else{
			menu = <span></span>
		}

		return (
		  <tr data-id={mainProps.Model.ContentItemId}>
			<td key="1" className="ticket-id-col"><a key="12" href={url}>{mainProps.Model.TicketNumber}</a></td>
			<td key="2" className="ticket-title-col"><a key="14" href={url}>{mainProps.Model.Title}</a></td>
			<td key="3" className="ticket-status-col">{mainProps.Model.StatusName}</td>
			<td key="4" className="ticket-menu-col">{menu}</td>
		 </tr>
			);
	}
});
orchardcollaboration.react.allComponents.Parts_Ticket_TableRow = Parts_Ticket_TableRow;

var TicketContextMenu = React.createClass({
	menuClick: function(action, contentId){
		this.props.root.actions.ticketMenu(action, contentId)
	},

	// http://stackoverflow.com/questions/25630035/javascript-getboundingclientrect-changes-while-scrolling
	absolutePosition: function(el) {
		var
			found,
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
	pivotClick: function(e){
		var menuItems = this.refs.menuItems;
		var $window = $(window);
	    var scrollLeft = $window.scrollLeft();
        var scrollTop = $window.scrollTop();
	    var pivotOffset = this.refs.pivot.getBoundingClientRect()// this.absolutePosition(this.refs.pivot);
		menuItems.style.top = (pivotOffset.top).toString() + "px";
		menuItems.style.left = (pivotOffset.left).toString() + "px";
		$(menuItems).removeClass("menu-hidden");		
        e.stopPropagation();
		e.preventDefault();
	},
	getInitialState: function(){
		var _self = this;
		$("html").click(function(){
			$(_self.refs.menuItems).addClass("menu-hidden");		
		});

		return {};
	},

	componentDidMount: function(){
		var _self = this;
		$(_self.refs.pivot).find("a").click(_self.pivotClick);
	},

	render: function(){
		var _self = this;
		var mainProps = this.props;
		var root = mainProps.root;
		var menu = null;

		if (mainProps.Menu && mainProps.Menu.length > 0){
			var menuItems = mainProps.Menu.map(function(menuItem){
			return <li key={menuItem.Id} onClick={_self.menuClick.bind(null, menuItem.Id, mainProps.ContentItemId)}>{menuItem.Text}</li>
			});

			menu = <div className="ticket-menu-container">
					  <div ref="pivot" className="pivot">
						  {this.props.children}
                      </div>
					  <ul ref="menuItems" className="ticket-milestone-menu menu-hidden z2">
							{menuItems}
					  </ul>
				   </div>
		}
		else{
			menu = <span></span>
		}

		return menu;
	}
});

var Parts_Ticket_TitleOnly = React.createClass({
	render: function(){
		var root = this.props.root;
		var ticketDisplayRoute = root.Routes.DisplayTicket;
		var url = decodeURI(ticketDisplayRoute);
		url = url.replace("{id}", this.props.Model.TicketId);

		return(
		<div>
			<div class="ticket-list-id"><a href="{url}">{this.props.Model.TicketNumber}</a></div>
			<div class="ticket-list-name"><a href="{url}">{this.props.Model.Title}</a></div>
			<div class="clear"></div>
		</div>
		);
	}
});
orchardcollaboration.react.allComponents.Parts_Ticket_TitleOnly = Parts_Ticket_TitleOnly;

var Parts_Ticket_GanttChart = React.createClass({
	render: function(){
		var getStatusClass = function(typeId){
			switch(typeId){
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
		}
		
		var root = this.props.root;
		var model = this.props.Model;
		var ticketDisplayRoute = root.Routes.DisplayTicket;
		var url = decodeURI(ticketDisplayRoute);
		url = url.replace("{id}", model.TicketId);
		var dueDate = model.DueDate != null? new Date(model.DueDate): null;
		var cells = [];
		var todayParts = root.today.split('-');
		var today = new Date(todayParts[0], todayParts[1] - 1, todayParts[2]);

		if (model.StatusTimes.length == 0){
			cells.push(<td colspan={root.days.length}></td>);
		}
		else{

			var latestStatusId = -1;
			var isClosed = false;
			for(var i = 0; i < root.days.length; i++){
				var date = new Date(root.days[i]);

				if (date > today){
					cells.push(<td></td>);
					continue;
				}

				var newStatus = null;
				for (var j = 0; j < model.StatusTimes.length; j++){
					var statusDate = new Date(model.StatusTimes[j].Value);

					if (date.getFullYear() == statusDate.getFullYear() && date.getMonth() == statusDate.getMonth() && date.getDate() == statusDate.getDate())
					{
						newStatus = model.StatusTimes[j];
					}
				}


				if (isClosed && (newStatus == null || newStatus == 40)){
					cells.push(<td></td>);
					continue;
				}

				if (dueDate && date > dueDate){
					cells.push(
							<td>
								<div className='overdue-status'></div>
							</td>);
					continue;
				}
				
				if (newStatus != null){
					isClosed = newStatus.Key == 40;
					latestStatusId = newStatus.Key;
					cells.push(
							<td>
								<div className={getStatusClass(newStatus.Key)}></div>
							</td>);
				}
				else if (latestStatusId != null){
					cells.push(
							<td>
								<div className={getStatusClass(latestStatusId)}></div>
							</td>);
				}
				else{
					cells.push(<td></td>);
				}
			}
		};

		var cssClass = "";
		for(var i = 0; i < this.props.Classes.length; i++){
			cssClass = this.props.Classes[i] + " ";
		}
		return(
		<tr className={cssClass}>
			<td className="ticket-gantt-row-id"><a href={url}>{model.TicketNumber}</a></td>
			<td className="ticket-gantt-row-name"><a title={model.Title} href={url}>{model.Title}</a></td>
			{cells}
		</tr>
		);
	}
});
orchardcollaboration.react.allComponents.Parts_Ticket_GanttChart = Parts_Ticket_GanttChart;

var Parts_Ticket_Edit = React.createClass({
	getInitialState: function(){
	},

	render: function(){
		var root = this.props.root;
		return(<div>
					<div>
						<div className='label'>{root.T("Title", "Title")}</div>
						<div><input type='text' value={this}/></div>
					</div>
			   </div>);
	}
});
orchardcollaboration.react.allComponents.Parts_Ticket_Edit = Parts_Ticket_Edit;

var Parts_Ticket_Pinboard = React.createClass({
	render: function(){
		var root = this.props.root;
		var ticketDisplayRoute = root.Routes.DisplayTicket;
		var url = decodeURI(ticketDisplayRoute);
		url = url.replace("{id}", this.props.Model.TicketId);
		return(
		<article data-canedit={this.props.Model.CurrentUserCanEditItem} className='ticket-pinboard' data-contentid={this.props.Model.ContentItemId}>
			<h4>
				<TicketContextMenu root={this.props.root} Menu={this.props.Model.Menu} ContentItemId={this.props.Model.ContentItemId}>
					<a href="#">
						<span className="identity">#{this.props.Model.TicketNumber}</span>
						<span>{this.props.Model.Title}</span>
					</a>
				</TicketContextMenu>
			</h4>
			<div>
				<label className="label-container">{root.T("Priority")}:</label>
				<span className="ticket-service-value result">
					<span>{this.props.Model.PriorityName}</span>
				</span>
			</div>
			<div>
				<label className="label-container">{root.T("Type")}:</label>
				<span className="ticket-service-value result">
					<span>{this.props.Model.TypeName}</span>
				</span>
			</div>
		</article>
		);
	}
});
orchardcollaboration.react.allComponents.Parts_Ticket_Pinboard = Parts_Ticket_Pinboard;

var MilestoneTickets = React.createClass({
	filterKeyDown: function(event){
		if (event.keyCode == 13){
			this.filter();
		}
	},
	changeMode: function(mode){
		this.props.root.actions.changeMode(mode);
	},
	filter: function(){
        var domNode = $(ReactDOM.findDOMNode(this));
		var filterText = domNode.find(".filter-tickets").val();
		this.props.root.actions.filter(filterText);
	},
	render: function(){
	var _self = this;
	var itemsComponents = null;
	var mainContainer = null;
	var statusRecords = this.props.shape.Model.StatusRecords;

	var props = this.props;
	var root = props.root;
	var filterText = props.filterText;
	if (props.shape.Model.Items.length > 0){ 
	
		var latestRootTicketId = null;
		var altCssClass = "alt-row";
		itemsComponents = props.shape.Model.FilteredItems.map(function(item){
							var cells = [];

							if (item.InRoot){
								altCssClass = altCssClass  === "alt-row" ? "normal-row": "alt-row";
							}

							for(var i = 0; i < statusRecords.length; i++){
								var statusColumn = statusRecords[i];
								if ((item.StatusId == null && i == 0) ||
									 item.StatusId == statusColumn.Id)
								{
									cells.push(<td key={"status" + i} data-status=''><x.Display root={props.root} shape={item}></x.Display></td>)
								}
								else{
									cells.push(<td  key={"status" + i} data-contentid={item.ContentItem.Id} className='empty-td' data-status={statusColumn.Id}><div className='empty-cell'></div></td>)
								}
							}

							return(<tr key={"tr" + item.ContentItem.Id} className={altCssClass}>{cells}</tr>)
						});
		var tableColumns = statusRecords.map(function(status){
			return (
					<th key={"status" + status.Id} data-id={status.Id}>
						<span className="col-header">{status.Name}</span>
						<span title={root.T("NumberOfTickets", "Number of tickets")} className="count">{status.count}</span>
						<span title={root.T("SumSizeOfTickets", "Total size of tickets in this column")} className="size">{status.size}</span>
					</th>);
		});
		 
		mainContainer = <div key='milestoneTickets'>
						<div className='filter-box'>
							<input className='filter-tickets' onKeyDown={_self.filterKeyDown} defaultValue={filterText} type='text'/><input type='button' value={root.T("Filter")} onClick={_self.filter}/>
							<span className="render-mode">{root.T("OneTicketPerRow", "One Ticket per row")}</span>
							<a className="render-mode" onClick={this.changeMode.bind(null, "ticketPerColumn")}>{root.T("Cardwall", "Cardwall")}</a>
						</div>
						<div className="milestone-wild-card-container">
							<table className='milestone-items'>
								<thead>
									<tr>{tableColumns}</tr>
								</thead>
								<tbody>
								{itemsComponents}							
								</tbody>
							</table>
						</div>
						</div>
		}
	else{
		mainContainer = <h6>{root.T("NoItemInMilestone", "There is no item in the Milestone")}</h6>;
	}	
		return (mainContainer);
	}
});

orchardcollaboration.react.allComponents.MilestoneTickets = MilestoneTickets;

var MilestoneTicketsStatusBars = React.createClass({
	filterKeyDown: function(event){
		if (event.keyCode == 13){
			this.filter();
		}
	},
	filter: function(){
        var domNode = $(ReactDOM.findDOMNode(this));
		var filterText = domNode.find(".filter-tickets").val();
		this.props.root.actions.filter(filterText);
	},
	changeMode: function(mode){
		this.props.root.actions.changeMode(mode);
	},
	render: function(){
	var _self = this;
	var itemsComponents = null;
	var mainContainer = null;
	var statusRecords = this.props.shape.Model.StatusRecords;

	var props = this.props;
	var root = props.root;
	var filterText = props.filterText;
	if (props.shape.Model.Items.length > 0){ 
	
		var bars = statusRecords.map(function(status){
			var items = [];
			var barKey =  "bar" + status.Id;

			for(var i = 0; i < props.shape.Model.FilteredItems.length; i++){
				var item = props.shape.Model.FilteredItems[i];
				if (item.StatusId == status.Id || 
					(status.StatusTypeId == 10 && item.StatusId == null)){
					items.push(<x.Display root={props.root} shape={item}></x.Display>);
				}
			}

			return (
					<div className="bar" key={barKey} data-status={status.Id} data-id={status.Id}>
						<div className="header">
							<span className="col-header">{status.Name}</span>
							<span title={root.T("NumberOfTickets", "Number of tickets")} className="count">{status.count}</span>
							<span title={root.T("SumSizeOfTickets", "Total size of tickets in this column")} className="size">{status.size}</span>
						</div>
						<div className="body">
							{items}
						</div>
					</div>);
		});
		 
		mainContainer = <div key='milestoneTickets'>
						<div className='filter-box'>
							<input className='filter-tickets' onKeyDown={_self.filterKeyDown} defaultValue={filterText} type='text'/><input type='button' value={root.T("Filter")} onClick={_self.filter}/>
							<a className="render-mode" onClick={this.changeMode.bind(null, "ticketPerRow")}>{root.T("OneTicketPerRow", "One Ticket per row")}</a>
							<span className="render-mode">{root.T("Cardwall", "Cardwall")}</span>
						</div>
						<div className="milestone-wild-card-container">
							<div className='milestone-items'>
								<div className="bars-table">{bars}</div>
							</div>
						</div>
						</div>
		}
	else{
		mainContainer = <h6>{root.T("NoItemInMilestone", "There is no item in the Milestone")}</h6>;
	}	
		return (mainContainer);
	}
});

orchardcollaboration.react.allComponents.MilestoneTicketsStatusBars = MilestoneTicketsStatusBars;
var MilestonePlanner = React.createClass({
	render: function(){
		var milestoneMembers = null;
		var mainContainer = null;
		var props = this.props;
		var root = props.root;

		if (props.shape.Model.Items.length > 0){
			
			var milestoneMembersList = props.shape.Model.Items.map(function(item){
				return <x.Display root={root} notWrap="true" shape={item} ></x.Display>;
			});

			milestoneMembers = 	
							<div className="milestone-wild-card-container">
								<table className="milestone-items-list current-milestone">
									<thead>
										<tr>
											<th className="ticket-id-col">#</th>
											<th className="ticket-title-col">{root.T("Summary")}</th>
											<th className="ticket-status-col">{root.T("Status")}</th>
											<th className="ticket-menu-col"></th>
										</tr>
									</thead>
									<tbody>
										{milestoneMembersList}
									</tbody>
								</table>
							</div>
		}
		else{
			milestoneMembers =<div className="drop-target"><h5 className="milestone-no-item">{root.T("NoItemsInMilestone", "There is no item in the Milestone")}</h5></div>
		}

		var backLogComponent = null;
		if (props.shape.Model.BacklogMembers != null && props.shape.Model.BacklogMembers.length > 0){
			var backLogItems = props.shape.Model.BacklogMembers.map(function(item){
				return <x.Display root={root} notWrap="true" shape={item} key={item.ContentItem.Id} ></x.Display>;
				});
			backLogComponent = <div>
									<h4>
										{root.T("Backlog")}
										<span className="backlog-description">{root.T("BacklogDescription", "only non completed tickets of backlog are shown")}</span>
									</h4>
									<table className="backlog milestone-items-list">
										<thead>
											<tr>
												<th className="ticket-id-col">#</th>
												<th className="ticket-title-col">{root.T("Summary")}</th>
												<th className="ticket-status-col">{root.T("Status")}</th>
												<th className="ticket-menu-col"></th>
											</tr>
										</thead>
										<tbody>
											{backLogItems}
										</tbody>
									</table>
							   </div>
		}
		else{
			backLogComponent =
				<div>
					<h4>
						{root.T("Backlog")}
						<span className="backlog-description">{root.T("BacklogDescription", "only non completed tickets of backlog are shown")}</span>
					</h4>
					<div className="drop-target"><h5 className="milestone-no-item">{root.T("NoItemsInBacklog", "There is no item in the Backlog")}</h5></div>
				</div>
		}

		return <div>
					{milestoneMembers} 
					{backLogComponent}
			   </div>;
	}
 });
 orchardcollaboration.react.allComponents.MilestonePlanner = MilestonePlanner;

 var MilestoneGantt = React.createClass({
	filterKeyDown: function(event){
		if (event.keyCode == 13){
			this.filter();
		}
	},
	filter: function(){
        var domNode = $(ReactDOM.findDOMNode(this));
		var filterText = domNode.find(".filter-tickets").val();
		this.props.root.actions.filter(filterText);
	},
	render: function(){
		var _self = this;
		var itemsComponents = null;
		var mainContainer = null;
		var props = this.props;
		var root = props.root;
		var milestone = props.shape.Model.Milestone;
		
		// check start-time and end-time of milestone
		if (milestone.Start == null || 
		    milestone.End == null || 
			milestone.Start >= milestone.End){
			return <h6>{root.T("StartTimeEndTimeMismatch", "Start-time or End-time of the Milestone is null or not valid")}</h6>
			};

		var startDate = new Date(milestone.Start);
		var endDate = new Date(milestone.End);

		var monthColumns = props.shape.Model.Months.map (function(month){
			return (<th className='month-col' colSpan={month.Days.length}>
						<span className='month'>{month.Name}</span>
						<span className='year'>{month.Year}</span>
				   </th>);
		});

		var tableColumns = [];
		var days = [];

		for(var i = 0; i < props.shape.Model.Months.length; i++){
			var month = props.shape.Model.Months[i];

			for(var j = 0; j < month.Days.length; j++){
				var temp = month.Days[j].split("-")
				days.push(new Date(temp[0], temp[1] - 1, temp[2]));
				tableColumns.push(<th className='date-col'><span>{temp[2]}</span></th>);
			}
		}
		
		root.days = days;
		root.today = props.shape.Model.Today;

		var filterText = props.filterText;
		if (props.shape.Model.Items.length > 0){ 
				
			var latestRootTicketId = null;
			var altCssClass = "alt-row";
			itemsComponents = props.shape.Model.FilteredItems.map(function(item){

								if (item.InRoot){
									altCssClass = altCssClass  === "alt-row" ? "normal-row": "alt-row";
								}

								for(var i = 0; i < item.Content.length; i++){
									item.Content[i].Classes.push(altCssClass);
								}
								
								return (<x.Display key={item.ContentItem.Id} id={item.ContentItem.Id} root={props.root} shape={item}></x.Display>);
							});
	 
			mainContainer = <div className='milestoneGantt' key='milestoneGantt'>
							<div className='filter-box'>
								<input className='filter-tickets' onKeyDown={_self.filterKeyDown} defaultValue={filterText} type='text'/><input type='button' value={root.T("Filter")} onClick={_self.filter}/>
							</div>
							<div className="milestone-gantt-container1">
								<div className="milestone-tickets-header">
									<div >
										<span className='milestone-tickets-title'>{root.T("Tickets")}</span>
										<span className="milestone-size-label">{root.T("TotalSize", "Total Size:")}</span>
										<span className="milestone-size-value">{props.shape.Model.totalSize}</span>
									</div>						
								</div>
								<div className="milestone-gantt-container">
									<table cellPadding="0" cellSpacing="0" className='milestone-gantt-items' data-count-fixed-columns="2" >
										<thead>
											<tr>
											</tr>
											<tr>
												<td></td>
												<td></td>
												{monthColumns}
											</tr>
											<tr>
												<td></td>
												<td></td>
												{tableColumns}
											</tr>
										</thead>
										<tbody>
										{itemsComponents}							
										</tbody>
									</table>
								</div>
							</div>
							<div className='ganttchart-guide'>
								<h4>{root.T("StatusGuide", "Ticket status guide")}</h4>
								<span className='new-status-type'>
									{root.T("New", "New")}
								</span>
								<span className='open-status-type'>
									{root.T("InProgress", "In Progress")}
								</span>
								<span className='deferred-status-type'>
									{root.T("Deferred", "Deferred")}
								</span>
								<span className='pending-status-type'>
									{root.T("Pending", "Pending")}
								</span>
								<span className='closed-status-type'>
									{root.T("Completed", "Completed")}
								</span>
								<span className='overdue-status'>
									{root.T("Overdue", "Overdue")}
								</span>
							</div>
							</div>
			}
		else{
			mainContainer = <h6>{root.T("NoItemInMilestone", "There is no item in the Milestone")}</h6>;
		}	

		return (mainContainer);
	}
});

orchardcollaboration.react.allComponents.MilestoneGantt = MilestoneGantt;

var AddCommentModal = React.createClass({
	render: function(){
		var root = this.props.root;
		
		return (
			<ReactBootstrap.Modal className="ticket-add-comment-modal" show={this.props.showAddCommentModal} onHide={this.closeSyncModel}>
				<ReactBootstrap.Modal.Header closeButton>
					<ReactBootstrap.Modal.Title>
						{root.T("AddComment", "Add Comment")}
					</ReactBootstrap.Modal.Title>					
				</ReactBootstrap.Modal.Header>
				<ReactBootstrap.Modal.Body>
					<div className="add-comment-modal">
						<div>
							<div>
								<div className='label-row'>{root.T("Comment", "Comment")}</div>
								<div><textarea rows="10" ref="comment"></textarea></div>
							</div>
	  					</div>
					</div>
				</ReactBootstrap.Modal.Body>
				<ReactBootstrap.Modal.Footer>
					<ReactBootstrap.Button onClick={this.save}>{root.T("Save", "Save")}</ReactBootstrap.Button>
					<ReactBootstrap.Button onClick={this.closeSyncModel}>{root.T("Cancel", "Cancel")}</ReactBootstrap.Button>
				</ReactBootstrap.Modal.Footer>
			</ReactBootstrap.Modal>
			);
	},

	save: function(){
		var data = {
			comment: this.refs.comment.value,
			id : this.props.selectedTicketId
		};

		this.props.root.actions.saveComment(data);
	},

	closeSyncModel: function(){
		this.props.root.actions.closeAddCommentModal();
	}
});

orchardcollaboration.react.allComponents.AddCommentModal = AddCommentModal;

var EditTicketModal = React.createClass({
	getInitialState: function(){
		return {
			tinyMceIsCreated: false
		};
	},
	
	save: function(){
		tinyMCE.triggerSave();
		
		var priority = this.refs.priority;
		var ticketType = this.refs.ticketType;

		var data = {
			title: this.refs.title.value,
			description: this.refs.description.value,
			priorityId: priority.selectedIndex? priority.options[priority.selectedIndex].value: null,
			ticketTypeId: ticketType.selectedIndex? ticketType.options[ticketType.selectedIndex].value: null,
			id: this.props.editModalData.id
		};

		this.props.root.actions.saveTicket(data);
	},

	componentDidUpdate: function(){

		if (this.props.showEditModal){

			try{
			tinymce.init({
				selector: '.ticket-edit-modal textarea'
				,menubar: false
				,plugins: [
					"advlist autolink lists link image charmap print preview hr anchor pagebreak",
					"searchreplace wordcount visualblocks visualchars code fullscreen",
					"insertdatetime media nonbreaking table contextmenu directionality",
					"emoticons template paste textcolor colorpicker textpattern",
					"fullscreen autoresize"
				]
				,toolbar: "cut copy paste | bold italic | bullist numlist outdent indent formatselect | alignleft aligncenter alignright alignjustify ltr rtl | link unlink charmap | code fullscreen"
				, height : 200
				, resize : true
				, plugins : 'code'
			});
			}
			catch(e){
				alert(e);
			}
			if (this.state.tinyMceIsCreated){
				tinymce.execCommand('mceAddEditor',true,'ticketEditModalTextArea');
			}

			this.state.tinyMceIsCreated = true;
		}
		else if (this.state.tinyMceIsCreated){
			tinymce.execCommand('mceRemoveEditor', true, 'ticketEditModalTextArea');
		}
	},

	render: function(){

		var root = this.props.root;
		var _self = this;

		var title = "";
		var priorityId = "";
		var ticketTypeId = "";
		var description = "";	
		var priorityOptions = null;
		var ticketTypeOptions = null;

		if (this.props.editModalData){
			priorityOptions = this.props.editModalData.priorities.map(function(item){
								return <option value={item.Value}>{item.Text}</option>
							  });

			ticketTypeOptions = this.props.editModalData.ticketTypes.map(function(item){
								return <option value={item.Value}>{item.Text}</option>
							  });
			priorityId = this.props.editModalData.priorityId;
			ticketTypeId = this.props.editModalData.ticketTypeId;
			title = this.props.editModalData.title;
			description = this.props.editModalData.description;
		}

		return (
			<ReactBootstrap.Modal className="ticket-edit-modal" show={_self.props.showEditModal} onHide={_self.closeSyncModel}>
				<ReactBootstrap.Modal.Header closeButton>
					<ReactBootstrap.Modal.Title>
						{root.T("EditTicket", "Edit Ticket")}
						<a className="full-mode-link" href={_self.props.fullEditModelUrl}>{root.T("FullView", "View in full model")}</a>
					</ReactBootstrap.Modal.Title>					
				</ReactBootstrap.Modal.Header>
				<ReactBootstrap.Modal.Body>
					<div className="edit-ticket-modal">
					<div>
						<div>
							<div className='label-row'>{root.T("Title", "Title")}</div>
							<div><input ref="title" type='text' defaultValue={title}/></div>
					   </div>
						<div>
							<div className='label-row'>{root.T("Type", "Type")}</div>
							<div>
								<select ref="ticketType" defaultValue={ticketTypeId}>
									{ticketTypeOptions}
								</select>
							</div>
					   </div>
						<div>
							<div className='label-row'>{root.T("Priority", "Priority")}</div>
							<div>
								<select ref="priority" defaultValue={priorityId}>
									{priorityOptions}
								</select>
							</div>
					   </div>
						<div>
							<div className='label-row'>{root.T("Description", "Description")}</div>
							<div><textarea ref="description" id="ticketEditModalTextArea">{description}</textarea></div>
					   </div>
			   </div>
					</div>
				</ReactBootstrap.Modal.Body>
				<ReactBootstrap.Modal.Footer>
					<ReactBootstrap.Button onClick={_self.save}>{root.T("Save", "Save")}</ReactBootstrap.Button>
					<ReactBootstrap.Button onClick={_self.closeSyncModel}>{root.T("Cancel", "Cancel")}</ReactBootstrap.Button>
				</ReactBootstrap.Modal.Footer>
			</ReactBootstrap.Modal>
			);
	},

	closeSyncModel: function(){
		this.props.root.actions.closeEditTicketModal();
	}
});

orchardcollaboration.react.allComponents.EditTicketModal = EditTicketModal;

})();