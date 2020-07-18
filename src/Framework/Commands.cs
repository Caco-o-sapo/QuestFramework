﻿using QuestFramework.Framework.Stats;
using StardewModdingAPI;
using StardewValley;
using System.Linq;
using System.Text;

namespace QuestFramework.Framework
{
    internal static class Commands
    {
        private static IMonitor Monitor => QuestFrameworkMod.Instance.Monitor;
        private static QuestManager QuestManager => QuestFrameworkMod.Instance.QuestManager;
        private static StatsManager StatsManager => QuestFrameworkMod.Instance.StatsManager;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Styl", "IDE0060", Justification = "Command handler")]
        public static void ListQuests(string name, string[] args)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"Quest framework has {QuestManager.Quests.Count} quests under management:");

            foreach (var quest in QuestManager.Quests)
            {
                builder.AppendLine(quest.GetFullName())
                    .AppendLine($"    Type: {quest.BaseType}")
                    .AppendLine($"    Custom type: {quest.CustomTypeId}")
                    .AppendLine($"    Name: {quest.Name}")
                    .AppendLine($"    Owned by: {quest.OwnedByModUid}")
                    .AppendLine($"    Current ID: {quest.id}")
                    .AppendLine($"    Trigger: {quest.Trigger ?? "null"}")
                    .AppendLine($"    Active: {(quest.id >= 0 ? "Yes" : "No")}");
            }

            Monitor.Log(builder.ToString(), LogLevel.Info);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Styl", "IDE0060", Justification = "Command handler")]
        internal static void ListLog(string name, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Can't list quest log if the game is not loaded", LogLevel.Info);
                return;
            }

            var managedLog = Game1.player.questLog
                .Where(q => QuestManager.IsManaged(q.id.Value))
                .Select(q => QuestManager.GetById(q.id.Value));
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"Quest framework has {managedLog.Count()} managed quests in player's quest log:");

            foreach(var quest in managedLog)
            {
                builder.AppendLine(quest.GetFullName())
                    .AppendLine($"    Type: {quest.BaseType}")
                    .AppendLine($"    Custom type: {quest.CustomTypeId}")
                    .AppendLine($"    Name: {quest.Name}")
                    .AppendLine($"    Owned by: {quest.OwnedByModUid}")
                    .AppendLine($"    Current ID: {quest.id}")
                    .AppendLine($"    Trigger: {quest.Trigger ?? "null"}")
                    .AppendLine($"    Cancelable: {quest.Cancelable}");
            }

            Monitor.Log(builder.ToString(), LogLevel.Info);
        }

        internal static void QuestStats(string name, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Can't show quest statistics if the game is not loaded", LogLevel.Info);
                return;
            }

            if (args.Length < 1)
            {
                Monitor.Log("Choose one of these statistics: accepted, completed, removed, summary, <fullQualifiedQuestName>", LogLevel.Info);
                return;
            }

            Stats.Stats stats = StatsManager.GetStats(Game1.player.UniqueMultiplayerID);
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{args[0]} quest stats:");

            switch (args[0])
            {
                case "accepted":
                    builder.AppendLine($"Show {stats.AcceptedQuests.Count} accepted quests.");
                    builder.AppendLine();
                    stats.AcceptedQuests.ForEach(stat => builder.AppendLine($"{stat.FullQuestName}\t{stat.Date.ToLocaleString()}"));
                    break;
                case "completed":
                    builder.AppendLine($"Show {stats.CompletedQuests.Count} completed quests.");
                    builder.AppendLine();
                    stats.CompletedQuests.ForEach(stat => builder.AppendLine($"{stat.FullQuestName}\t{stat.Date.ToLocaleString()}"));
                    break;
                case "removed":
                    builder.AppendLine($"Show {stats.RemovedQuests.Count} removed quests.");
                    builder.AppendLine();
                    stats.RemovedQuests.ForEach(stat => builder.AppendLine($"{stat.FullQuestName}\t{stat.Date.ToLocaleString()}"));
                    break;
                case "summary":
                    builder.AppendLine($"{stats.AcceptedQuests.Count} accepted quests");
                    builder.AppendLine($"{stats.CompletedQuests.Count} completed quests");
                    builder.AppendLine($"{stats.RemovedQuests.Count} removed quests");
                    break;
                default:
                    if (QuestManager.Fetch(args[0]) == null)
                    {
                        builder.AppendLine($"`{args[0]}` is not known managed quest");
                        break;
                    }

                    var summary = stats.GetQuestStatSummary(args[0]);

                    builder.AppendLine($"Last accepted:                  {summary.LastAccepted?.ToLocaleString() ?? "never"}");
                    builder.AppendLine($"Last completed:                 {summary.LastCompleted?.ToLocaleString() ?? "never"}");
                    builder.AppendLine($"Last removed from quest log:    {summary.LastRemoved?.ToLocaleString() ?? "never"}");
                    builder.AppendLine();
                    builder.AppendLine($"{summary.AcceptalCount} times accepted");
                    builder.AppendLine($"{summary.CompletionCount} times completed");
                    builder.AppendLine($"{summary.RemovalCount} times removed from quest log");
                    break;
            }

            Monitor.Log(builder.ToString(), LogLevel.Info);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Styl", "IDE0060", Justification = "Command handler")]
        internal static void InvalidateCache(string name, string[] args)
        {
            QuestFrameworkMod.InvalidateCache();
            Monitor.Log("Quest assets cache invalidated.", LogLevel.Info);
        }
    }
}
