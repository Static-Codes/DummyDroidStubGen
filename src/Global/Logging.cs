/*
 * Copyright (C) 2026 Static Codes
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
namespace DummyDroidStubGen.Global;

using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using System.Globalization;
using static Constants;
using static Messaging;

public class EventLogger 
{

# region Class Variables
    
    [Obsolete("This path required the creation of a new user group, then ownership of the directory being transferred.")]
    private readonly string OldRootAppLogDirectory = Path.Combine("/", "var", "log", ApplicationName);
    

    /// <summary> 
    ///     Points to $HOME/.local/share/[ApplicationName]/logs/ <br/>
    ///     Where [ApplicationName] is defined in Global.Constants.
    /// </summary>
    private readonly string RootAppLogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        ApplicationName, 
        "logs"
    );

    
    /// <summary> FileLoggerProvider object that will be used to create an instance of FileLogger. </summary>
    private FileLoggerProvider? _provider;
    

    /// <summary> The filename that will be used for the log file associated with the EventLogger object. </summary>
    private string? _fileName;


    /// <summary> 
    ///     The instance of FileLogger that is created when a new object of the type EventLogger is initialized. 
    /// </summary>
    private FileLogger? _instance;


    /// <summary>
    ///     The current numerical identifier associated with the last logged event. <br/>
    ///     This value can be returned using GetEventID
    private int _eventID = 0;

# endregion


# region Initialization

    public EventLogger() 
    {
        var tempLogName = $"{GetCurrentTimeStamp()}.log";
        WriteWarningMessage($"No filename was specified, creating log file with name: {tempLogName}");
        CreateInstance(tempLogName);
    }


    public EventLogger(string fileName) {
        CreateInstance(fileName);
    }

#endregion


# region Functions

    /// <summary> 
    ///     Helper method used during the initialization of an EventLogger instance. 
    /// </summary>
    private void CreateInstance(string fileName) 
    {
        if (!TryCreateAppLogRootDir()) {
            WriteInformation("The root application directory is required for internal logging operations.");
            WriteInformation("Due to the absence of this directory, the application will exit now.");
            Environment.Exit(1);
        }

        _fileName = fileName;
        _provider = new(GetLogFilePath());
        _instance = new FileLogger(_fileName, _provider);
    }


    /// <summary> 
    ///     Resets all internal data, excluding the root application directory path. <br/> 
    ///     Informs the Garbage Collector (GC) that the memory associated with the FileLogger instance may be freed.
    /// </summary>
    public void DisposeInstance() {
        _fileName = null;
        _provider = null; 
        _eventID = 0;
        _instance = null; 
    }


    /// <summary> 
    ///     Returns the current timestamp. <br/>
    ///     This value is used in place of the fileName parameter if a non-null value is not provided. 
    /// </summary>
    private static string GetCurrentTimeStamp()
    {
        return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
    }


    /// <summary> 
    ///     Returns the numeric event identifier associated with the last logged message.
    /// </summary>
    public int GetEventID() {
        return _eventID;
    }


    /// <summary> 
    ///     Returns the underlying instance of FileLogger used by the custom EventLogger class. 
    /// </summary>
    public FileLogger GetInstance() {
        ArgumentException.ThrowIfNullOrEmpty(nameof(_instance));
        return _instance!;
    }


    /// <summary> 
    ///     Returns the path to the log file that will be or is currently used by an instance of EventLogger.
    /// </summary>
    public string GetLogFilePath() 
    {
        ArgumentNullException.ThrowIfNull(_fileName, nameof(_fileName));
        return Path.Combine(RootAppLogDirectory, _fileName);
    }


    /// <summary> 
    ///     Returns the root path to the subdirectory that will be used to store application log files.
    /// </summary>
    public string GetRootAppLogDir() {
        return RootAppLogDirectory;
    }


    /// <summary>
    ///     Writes a line to the log file associated with the current EventLogger instance.
    /// </summary>
    public void LogLine(string line, LogLevel logLevel = LogLevel.None, Exception? ex = null) 
    {
        if (_instance == null) 
        {
            WriteErrorMessage($"Unable to add line to log: \"{line}\"");
            return;
        }

        // Guard clause to ensure an overflow exception is not thrown.
        // This is likely overkill, however it is a slim possibility.
        // It would require 2,147,483,647 log entries in the same runtime session to cause this exception.

        if (_eventID == int.MaxValue) 
        {
            WriteWarningMessage("You have reached the maximum number of lines allocated to the current instance of EventLogger.");
            Thread.Sleep(750);
            
            _eventID = 0;

            WriteSuccessMessage("Reset the current instance's internal event counter to 0.");
            Thread.Sleep(250);
        }

        _eventID++;
        
        #pragma warning disable CA2254 // Template should be a static expression

        switch (logLevel)
        {
            case LogLevel.Critical:
                _instance.LogCritical(new EventId(_eventID), ex, line);
                break;

            case LogLevel.Debug:
                _instance.LogDebug(new EventId(_eventID), ex, line);
                break;

            case LogLevel.Error:
                _instance.LogError(new EventId(_eventID), ex, line);
                break;

            case LogLevel.Information:
                _instance.LogInformation(new EventId(_eventID), ex, line);
                break;

            case LogLevel.Trace:
                _instance.LogTrace(new EventId(_eventID), ex, line);
                break;

            case LogLevel.Warning:
                _instance.LogWarning(new EventId(_eventID), ex, line);
                break;

            default:
                _instance.Log(logLevel, line);
                break;
        }

        #pragma warning restore CA2254 // Template should be a static expression
        
    }
    
    private bool TryCreateAppLogRootDir() 
    {
        if (Directory.Exists(RootAppLogDirectory)) {
            WriteWarningMessage($"Root application log directory already exists at: {RootAppLogDirectory}, skipping creation.");
            return true;
        }

        try {
            Directory.CreateDirectory(RootAppLogDirectory);
            return true;
        }

        catch (Exception ex) {
            WriteWarningMessage($"Unable to create the Root application log directory at: {RootAppLogDirectory}.");
            WriteErrorMessage(ex.Message);
            return false;
        }
    }


# endregion

}