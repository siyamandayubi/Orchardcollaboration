
namespace OC.GITConnector.Handlers
{
    using Activities;
    using LibGit2Sharp;
    using Models;
    using Orchard;
    using Orchard.ContentManagement;
    using Orchard.Data;
    using Orchard.Environment.Configuration;
    using Orchard.FileSystems.AppData;
    using Orchard.FileSystems.Media;
    using Orchard.Localization;
    using Orchard.Logging;
    using Orchard.Tasks.Scheduling;
    using Orchard.Users.Models;
    using Orchard.Workflows.Services;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Take a look at http://stackoverflow.com/questions/8916146/scheduled-tasks-using-orchard-cms
    /// and http://stackoverflow.com/questions/11475187/how-to-run-scheduled-tasks-in-orchard
    /// </summary>
    public class GITClientTaskHandler : IScheduledTaskHandler
    {
        private const string TaskType = "GITClient";
        private const int PeriodInMinutes = 2;

        private readonly IScheduledTaskManager _taskManager;
        private readonly IOrchardServices orchardServices;
        private readonly ITransactionManager transactionManager;
        private readonly IAppDataFolder appDataFolder;
        private readonly ShellSettings shellSettings;
        private readonly IWorkflowManager workflowManager;
        private readonly IRepository<GITServerRecord> svnServerRepository;
        private readonly IRepository<GITServerBranchRecord> gitServerBranchRepository;
        private IRepository<UserPartRecord> userRepository;
        public ILogger Logger { get; set; }

        public Localizer T { get; set; }

        public GITClientTaskHandler(
            ShellSettings settings,
            IRepository<UserPartRecord> userRepository,
            IRepository<GITServerRecord> svnServerRepository,
            IRepository<GITServerBranchRecord> gitServerBranchRepository,
            IAppDataFolder appDataFolder,
            IWorkflowManager workflowManager,
            IOrchardServices orchardServices,
            ITransactionManager transactionManager,
            IScheduledTaskManager taskManager)
        {
            this.gitServerBranchRepository = gitServerBranchRepository;
            this.svnServerRepository = svnServerRepository;
            this.userRepository = userRepository;
            this.appDataFolder = appDataFolder;
            this.transactionManager = transactionManager;
            this.workflowManager = workflowManager;
            this.orchardServices = orchardServices;
            this.shellSettings = settings;
            _taskManager = taskManager;
            Logger = NullLogger.Instance;
            this.T = NullLocalizer.Instance;

            try
            {
                DateTime firstDate = DateTime.UtcNow.AddMinutes(PeriodInMinutes);
                ScheduleNextTask(firstDate);
            }
            catch (Exception e)
            {
                this.Logger.Error(e, e.Message);
            }
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TaskType)
            {
                var settings = this.orchardServices.WorkContext.CurrentSite.As<GITSettingsPart>();
                if (string.IsNullOrEmpty(settings.Server))
                {
                    return;
                }

                try
                {
                    this.transactionManager.Demand();
                    var gitPath = this.appDataFolder.Combine(shellSettings.Name, "GIT");
                    if (!this.appDataFolder.DirectoryExists(gitPath))
                    {
                        this.appDataFolder.CreateDirectory(gitPath);
                    }

                    var serverRecord = this.svnServerRepository.Table.FirstOrDefault(c => c.Server == settings.Server);

                    var path = Path.Combine(gitPath, settings.LocalFolder);
                    var mappedPath = this.appDataFolder.MapPath(path);
                    Repository repo = null;
                    if (serverRecord == null)
                    {
                        if (this.appDataFolder.DirectoryExists(path))
                        {
                            Directory.Delete(mappedPath, true);
                        }

                        this.appDataFolder.CreateDirectory(path);
                        // var destination = Repository.Init(mappedPath);
                        CloneOptions options = new CloneOptions();
                        options.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler(
                            (url, usernameFromUrl, types) => new UsernamePasswordCredentials()
                            {
                                Username = settings.Username,
                                Password = settings.Password
                            });
                        Repository.Clone(settings.Server, mappedPath, options);
                        repo = new Repository(mappedPath);

                        serverRecord = new GITServerRecord { Server = settings.Server, LastRevision = settings.LastRevision, FromDate = DateTime.UtcNow };
                        this.svnServerRepository.Create(serverRecord);
                        this.svnServerRepository.Flush();
                    }

                    repo = repo ?? new Repository(mappedPath);
                    var branches = !string.IsNullOrEmpty(settings.Branches) ? settings.Branches.Split(',') : new[] { "master" };

                    if (string.IsNullOrEmpty(settings.Password) || string.IsNullOrEmpty(settings.Username))
                    {
                        repo.Network.Fetch("origin", branches.Select(c => c + ":" + c));
                    }
                    else
                    {
                        FetchOptions options = new FetchOptions();
                        options.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler(
                            (url, usernameFromUrl, types) => new UsernamePasswordCredentials()
                            {
                                Username = settings.Username,
                                Password = settings.Password
                            });
                        repo.Network.Fetch("origin", branches.Select(c => c + ":" + c), options);
                    }

                    var branchRecords = this.gitServerBranchRepository.Table.Where(c => c.ServerRecord.Id == serverRecord.Id).ToList();

                    foreach (var branchName in branches)
                    {
                        var branch = repo.Branches.FirstOrDefault(c => c.FriendlyName == branchName);
                        if (branch == null)
                        {
                            continue;
                        }

                        var branchRecord = branchRecords.FirstOrDefault(c => c.BranchName == branchName);
                        if (branchRecord == null)
                        {
                            branchRecord = new GITServerBranchRecord { BranchName = branchName, ServerRecord = serverRecord, LastUpdate = DateTime.UtcNow };
                            this.gitServerBranchRepository.Create(branchRecord);
                            this.gitServerBranchRepository.Flush();
                        }

                        var commits = branch.Commits.ToList();
                        if (!string.IsNullOrEmpty(branchRecord.Sha))
                        {
                            var commit = commits.FirstOrDefault(c => c.Sha == branchRecord.Sha);
                            if (commit != null)
                            {
                                int index = commits.IndexOf(commit);
                                commits = commits.Skip(index).ToList();
                            }
                        }

                        foreach (var commit in commits)
                        {
                            RaiseWorkflow(branchRecord, commit);
                        }
                    }

                    settings.LastSuccessfullConnectionTime = DateTime.UtcNow;
                    settings.LatestError = null;
                    settings.LatestErrorTime = null;
                }
                catch (Exception e)
                {
                    this.transactionManager.Cancel();

                    this.Logger.Error(e, e.Message);

                    // We need a new transaction for storing the data
                    this.transactionManager.RequireNew();
                    settings.LatestError = e.Message;
                    settings.LatestErrorTime = DateTime.UtcNow;
                    var settingContentItem = this.orchardServices.ContentManager.Get(settings.Id);
                    var svnSettingPart = settingContentItem.As<GITSettingsPart>();
                    svnSettingPart.LatestError = e.Message;
                    svnSettingPart.LatestErrorTime = settings.LatestErrorTime;
                    this.transactionManager.Demand();
                }
                finally
                {
                    DateTime nextTaskDate = DateTime.UtcNow.AddMinutes(PeriodInMinutes);
                    this.ScheduleNextTask(nextTaskDate);
                }
            }
        }

        private void RaiseWorkflow(GITServerBranchRecord hostRecord, Commit log)
        {
            var contentManager = this.orchardServices.ContentManager;
            var gitLogContentItem = contentManager.New(GITCommitPart.ContentItemTypeName);
            var gitLogPart = gitLogContentItem.As<GITCommitPart>();


            gitLogPart.LogMessage = log.Message;
            gitLogPart.Author = log.Author.Name;
            gitLogPart.Sha = log.Sha;
            gitLogPart.Time = log.Author.When.DateTime;

            contentManager.Create(gitLogContentItem);
            contentManager.Publish(gitLogContentItem);

            workflowManager.TriggerEvent(
                GITCommitReceivedActivity.ActivityName,
                gitLogContentItem,
                () => new Dictionary<string, object> { { "Content", gitLogContentItem } });

            hostRecord.Sha = log.Sha;
            this.svnServerRepository.Flush();
        }

        private void ScheduleNextTask(DateTime date)
        {
            if (date > DateTime.UtcNow)
            {
                this.transactionManager.RequireNew();
                var tasks = this._taskManager.GetTasks(TaskType);
                if (tasks == null || tasks.Count(c => c.ScheduledUtc.HasValue && c.ScheduledUtc.Value > DateTime.UtcNow) == 0)
                    this._taskManager.CreateTask(TaskType, date, null);
            }
        }
    }
}