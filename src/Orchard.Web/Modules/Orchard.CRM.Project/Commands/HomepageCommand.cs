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

using Orchard.Autoroute.Services;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Services;
using Orchard.Pages.Commands;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Widgets.Commands;
using Orchard.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Commands
{
    public class HomepageCommand : DefaultOrchardCommandHandler
    {
        private readonly WidgetCommands widgetCommands;

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public string Name { get; set; }

        [OrchardSwitch]
        public bool RenderTitle { get; set; }

        [OrchardSwitch]
        public string Zone { get; set; }

        [OrchardSwitch]
        public string Position { get; set; }

        [OrchardSwitch]
        public string Layer { get; set; }

        [OrchardSwitch]
        public string Identity { get; set; }

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public string Text { get; set; }

        [OrchardSwitch]
        public bool UseLoremIpsumText { get; set; }

        [OrchardSwitch]
        public bool Publish { get; set; }

        [OrchardSwitch]
        public string MenuName { get; set; }

        public HomepageCommand(IWidgetsService widgetsService,
            ISiteService siteService,
            IMembershipService membershipService,
            IMenuService menuService,
            IContentManager contentManager)
        {
            widgetCommands = new WidgetCommands(widgetsService, siteService, membershipService, menuService, contentManager);
        }

        [CommandName("orchardcollaboration homepage create")]
        [CommandHelp("orchardcollaboration homepage create <type> /Title:<title> /Name:<name> /Zone:<zone> /Position:<position> /Layer:<layer> [/Identity:<identity>] [/RenderTitle:true|false] [/Owner:<owner>] [/Text:<text>] [/UseLoremIpsumText:true|false] [/MenuName:<name>]\r\n\t" + "Creates a new widget")]
        [OrchardSwitches("Title,Name,Zone,Position,Layer,Identity,Owner,Text,UseLoremIpsumText,MenuName,RenderTitle")]
        public void Create()
        {
            widgetCommands.Name = this.Name;
            widgetCommands.Title = this.Title;
            widgetCommands.RenderTitle = this.RenderTitle;
            widgetCommands.Text = string.IsNullOrEmpty(this.Text) ? DefaultText : this.Text;
            widgetCommands.Owner = this.Owner;
            widgetCommands.Zone = this.Zone;
            widgetCommands.Publish = this.Publish;
            widgetCommands.Position = this.Position;
            widgetCommands.Layer = this.Layer;
            widgetCommands.Identity = this.Identity;
            widgetCommands.UseLoremIpsumText = this.UseLoremIpsumText;
            widgetCommands.MenuName = this.MenuName;

            widgetCommands.Context = this.Context;
            widgetCommands.Create("HtmlWidget");
        }

        private const string DefaultText = @"<p>Orchard Collaboration is a free, open source ticketing system and collaboration framework build on top of the Orchard CMS. It natively integrates with Orchard CMS and extends its features by allowing its users to collaboratively work on the projects or by simplifying communication among team members and the customers.</p>
<div class='panel panel-default'>
<div class='panel-heading'>
<h4>Some of the features of Orchard Collaboration:</h4>
</div>
<div class='panel-body'>
<ul>
<li>Integrated &amp; flexible ticketing system</li>
<li>Email ticketing & Email Templates</li>
<li>Project Management</li>
<li>Wikis per Project</li>
<li>Discussions per Project</li>
<li>Issue List per Project</li>
<li>Users Dashboard</li>
<li>Projects Dashboard</li>
<li>Very flexible authorization management based on roles and permissions (ideal for Enterprise)</li>
<li>Very customizable (Custom fields, Custom forms, custom reports, ...)</li>
<li>Strong integration with Orchard Workflows</li>
</ul>
<p>for complete list of the features please visit <a href='http://OrchardCollaboration.com'>Orchard Collaboration website</a>.</p>
</div>
</div>
<div class='panel panel-default'>
<div class='panel-heading'>
<h4>Some features of the Orchard CMS:</h4>
</div>
<div class='panel-body'>
<p>Orchard is a robust CMS with lots of features one can expect from a CMS such as (custom pages, layouts, themes, menus, blogs...), you can see a list of all of the features <a href='http://docs.orchardproject.net/Documentation/Builtin-features'>here</a>. 
Some of the nice features that helps us customizing Orchard Collaborations are as follows:</p>
<ul>
<li>Extensibility &amp; Modularity (The ability to extend &amp; customize the core features by developing new modules)</li>
<li>Custom Themes</li>
<li>Strong &amp; flexible Workflow management module</li>
</ul>
<p>For more information about Orchard CMS please visit <a href='http://orchardproject.net//'>Orchardproject.net website</a></p>
</div>
</div>
";
    }
}