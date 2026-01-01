using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Xml.Linq;

namespace WindowsCleaner
{
    /// <summary>
    /// Information sur une tâche planifiée
    /// </summary>
    public class ScheduledTaskInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime NextRunTime { get; set; }
        public string Schedule { get; set; } = "";
        public bool IsEnabled { get; set; }
        public CleaningProfile? Profile { get; set; }
    }
    
    /// <summary>
    /// Gestionnaire de tâches planifiées Windows pour le nettoyage automatique
    /// </summary>
    public static class TaskSchedulerManager
    {
        private const string TaskPrefix = "WindowsCleaner_";
        private static readonly string ExecutablePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
        
        /// <summary>
        /// Crée une tâche planifiée quotidienne
        /// </summary>
        public static bool CreateDailyTask(string taskName, CleaningProfile profile, TimeSpan time)
        {
            if (!IsAdministrator())
            {
                Logger.Log(LogLevel.Warning, "Droits administrateur requis pour créer une tâche planifiée");
                return false;
            }
            
            var fullTaskName = TaskPrefix + taskName;
            var args = $"--profile \"{profile.Name}\" --silent";
            
            var xml = GenerateTaskXml(
                fullTaskName,
                $"Nettoyage automatique Windows - {profile.Name}",
                ExecutablePath,
                args,
                time,
                ScheduleType.Daily
            );
            
            return CreateTask(fullTaskName, xml);
        }
        
        /// <summary>
        /// Crée une tâche planifiée hebdomadaire
        /// </summary>
        public static bool CreateWeeklyTask(string taskName, CleaningProfile profile, DayOfWeek dayOfWeek, TimeSpan time)
        {
            if (!IsAdministrator())
            {
                Logger.Log(LogLevel.Warning, "Droits administrateur requis pour créer une tâche planifiée");
                return false;
            }
            
            var fullTaskName = TaskPrefix + taskName;
            var args = $"--profile \"{profile.Name}\" --silent";
            
            var xml = GenerateTaskXml(
                fullTaskName,
                $"Nettoyage automatique Windows - {profile.Name}",
                ExecutablePath,
                args,
                time,
                ScheduleType.Weekly,
                dayOfWeek
            );
            
            return CreateTask(fullTaskName, xml);
        }
        
        /// <summary>
        /// Crée une tâche planifiée mensuelle
        /// </summary>
        public static bool CreateMonthlyTask(string taskName, CleaningProfile profile, int dayOfMonth, TimeSpan time)
        {
            if (!IsAdministrator())
            {
                Logger.Log(LogLevel.Warning, "Droits administrateur requis pour créer une tâche planifiée");
                return false;
            }
            
            var fullTaskName = TaskPrefix + taskName;
            var args = $"--profile \"{profile.Name}\" --silent";
            
            var xml = GenerateTaskXml(
                fullTaskName,
                $"Nettoyage automatique Windows - {profile.Name}",
                ExecutablePath,
                args,
                time,
                ScheduleType.Monthly,
                dayOfMonth: dayOfMonth
            );
            
            return CreateTask(fullTaskName, xml);
        }
        
        /// <summary>
        /// Supprime une tâche planifiée
        /// </summary>
        public static bool DeleteTask(string taskName)
        {
            if (!IsAdministrator())
            {
                Logger.Log(LogLevel.Warning, "Droits administrateur requis pour supprimer une tâche planifiée");
                return false;
            }
            
            var fullTaskName = TaskPrefix + taskName;
            
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Delete /TN \"{fullTaskName}\" /F",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                    return false;
                
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    Logger.Log(LogLevel.Info, $"Tâche '{taskName}' supprimée avec succès");
                    return true;
                }
                else
                {
                    var error = process.StandardError.ReadToEnd();
                    Logger.Log(LogLevel.Error, $"Erreur suppression tâche: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur suppression tâche: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Active ou désactive une tâche
        /// </summary>
        public static bool SetTaskEnabled(string taskName, bool enabled)
        {
            if (!IsAdministrator())
            {
                Logger.Log(LogLevel.Warning, "Droits administrateur requis pour modifier une tâche planifiée");
                return false;
            }
            
            var fullTaskName = TaskPrefix + taskName;
            var action = enabled ? "Enable" : "Disable";
            
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Change /TN \"{fullTaskName}\" /{action}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                    return false;
                
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur modification tâche: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Liste toutes les tâches Windows Cleaner
        /// </summary>
        public static ScheduledTaskInfo[] ListTasks()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = "/Query /FO CSV /V",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                    return Array.Empty<ScheduledTaskInfo>();
                
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                // Parser le CSV et filtrer les tâches Windows Cleaner
                var tasks = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1) // Skip header
                    .Where(line => line.Contains(TaskPrefix))
                    .Select(ParseTaskLine)
                    .Where(task => task != null)
                    .ToArray();
                
                return tasks!;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur listage tâches: {ex.Message}");
                return Array.Empty<ScheduledTaskInfo>();
            }
        }
        
        private static ScheduledTaskInfo? ParseTaskLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                if (parts.Length < 2)
                    return null;
                
                var taskName = parts[0].Trim('"').Replace(TaskPrefix, "");
                
                return new ScheduledTaskInfo
                {
                    Name = taskName,
                    IsEnabled = !line.Contains("Disabled", StringComparison.OrdinalIgnoreCase)
                };
            }
            catch
            {
                return null;
            }
        }
        
        private static bool CreateTask(string taskName, string xmlContent)
        {
            try
            {
                // Créer un fichier XML temporaire
                var tempXml = Path.Combine(Path.GetTempPath(), $"{taskName}.xml");
                File.WriteAllText(tempXml, xmlContent);
                
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Create /TN \"{taskName}\" /XML \"{tempXml}\" /F",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                    return false;
                
                process.WaitForExit();
                
                // Nettoyer le fichier temporaire
                try { File.Delete(tempXml); } catch { }
                
                if (process.ExitCode == 0)
                {
                    Logger.Log(LogLevel.Info, $"Tâche planifiée '{taskName}' créée avec succès");
                    return true;
                }
                else
                {
                    var error = process.StandardError.ReadToEnd();
                    Logger.Log(LogLevel.Error, $"Erreur création tâche: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur création tâche: {ex.Message}");
                return false;
            }
        }
        
        private enum ScheduleType { Daily, Weekly, Monthly }
        
        private static string GenerateTaskXml(
            string taskName,
            string description,
            string executablePath,
            string arguments,
            TimeSpan time,
            ScheduleType scheduleType,
            DayOfWeek? dayOfWeek = null,
            int? dayOfMonth = null)
        {
            var startBoundary = DateTime.Today.Add(time).ToString("yyyy-MM-ddTHH:mm:ss");
            
            string scheduleTrigger = scheduleType switch
            {
                ScheduleType.Daily => $@"
      <CalendarTrigger>
        <StartBoundary>{startBoundary}</StartBoundary>
        <ScheduleByDay>
          <DaysInterval>1</DaysInterval>
        </ScheduleByDay>
      </CalendarTrigger>",
                
                ScheduleType.Weekly => $@"
      <CalendarTrigger>
        <StartBoundary>{startBoundary}</StartBoundary>
        <ScheduleByWeek>
          <DaysOfWeek>
            <{dayOfWeek ?? DayOfWeek.Sunday} />
          </DaysOfWeek>
          <WeeksInterval>1</WeeksInterval>
        </ScheduleByWeek>
      </CalendarTrigger>",
                
                ScheduleType.Monthly => $@"
      <CalendarTrigger>
        <StartBoundary>{startBoundary}</StartBoundary>
        <ScheduleByMonth>
          <DaysOfMonth>
            <Day>{dayOfMonth ?? 1}</Day>
          </DaysOfMonth>
          <Months>
            <January /><February /><March /><April /><May /><June />
            <July /><August /><September /><October /><November /><December />
          </Months>
        </ScheduleByMonth>
      </CalendarTrigger>",
                
                _ => ""
            };
            
            return $@"<?xml version=""1.0"" encoding=""UTF-16""?>
<Task version=""1.4"" xmlns=""http://schemas.microsoft.com/windows/2004/02/mit/task"">
  <RegistrationInfo>
    <Description>{description}</Description>
    <URI>\{taskName}</URI>
  </RegistrationInfo>
  <Triggers>
    {scheduleTrigger}
  </Triggers>
  <Principals>
    <Principal id=""Author"">
      <UserId>{WindowsIdentity.GetCurrent().Name}</UserId>
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>true</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>false</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT2H</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions Context=""Author"">
    <Exec>
      <Command>{executablePath}</Command>
      <Arguments>{arguments}</Arguments>
    </Exec>
  </Actions>
</Task>";
        }
        
        private static bool IsAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
