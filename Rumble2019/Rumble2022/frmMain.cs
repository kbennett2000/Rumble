using System.Text;
using NAudio.Wave;
using DtmfDetection.NAudio;
using System.Speech.Synthesis;
using System.Diagnostics;
using System.Reflection;
using Utility;
using System.Timers;

namespace Rumble2022
{
    /// <summary>
    /// A line in the Rumble .CSV configuration file.
    /// These are the all the fields contained in the file.
    /// </summary>
    public struct RumbleConfigLine
    {
        public string ServerNumber;
        public string ChannelNumber;
        public string ServerURL;
        public string Port;
        public string UserName;
        public string Password;
        public string ChannelPath;
        public string ServerNickname;
        public string ChannelNickname;
    } // RumbleConfigLine

    public partial class frmMain : Form
    {

        #region Variables and Constants

        // String variable used for keeping the callstack
        string TraceString = string.Empty;

        // The WAV input device
        WaveInEvent micIn = new WaveInEvent { WaveFormat = new WaveFormat(8000, 32, 1) };

        // The DTMF tone analyzer
        BackgroundAnalyzer? analyzer;

        // The current DTMF command. Used when tones are being recieved.
        string CurrentDTMFCommand = string.Empty;

        // The final DTMF command. Used when tones are being recieved.
        string FinalDTMFCommand = string.Empty;

        // The current Mumble.exe client process
        Process? currentMumbleProcess;

        // ProcessStartInfo object for the Mumble.exe client
        ProcessStartInfo? currentMumbleProcessStartInfo;

        // Null channel to connect to. This is used by the Disconnect option. The client is connected to a dummy channel.
        string ResetURI = @"mumble://noUser@0.0.0.0:0/";

        // The number of the WAV input device.
        int DeviceInNo = 0;

        // The number of the WAV input device.
        int DeviceOutNo = 0;

        // A List of all the lines in the CSV configuration file.
        List<RumbleConfigLine> MyConfigs = new List<RumbleConfigLine>();

        /// <summary>
        /// DTMF command states
        /// These are used when tones are being received and processed.
        /// "ignore" means no tones are actively being recieved or processed.
        /// </summary>
        enum DTMFCommandStates
        {
            ignore,
            isCommand,
            isDisconnect,
            isNotDisconnect,
            isLoadConfig,
            isAdminSettingORChannelChange,
            isAdminSetting,
            isChangeChannel,
            isAdminSettingNotFinal,
            isChannelChangeNoChannelNumber,
            isChannelChangeNotFinal,
            isAdminSettingFinal,
            isChannelChangeFinal
        } // DTMFCommandStates

        // The current DTMFCommandState for the application.
        DTMFCommandStates MyState;

        // The path to the folder where the config CSV file lives.
        string ConfigFilePath = string.Empty;

        // TODO: Put this in the UI
        // The path to the Mumble.exe client
        string MumbleExePath = Environment.ExpandEnvironmentVariables(@"%PROGRAMFILES%\mumble\mumble.exe");

        // If TRUE, the Mumble client should stay muted.
        // Any attempts to unmute while this is TRUE will fail.
        bool StayMuted = false;

        // Path to the WAV file to use for periodic node ID broadcast.
        string IDWaveFile = string.Empty;

        // The interval in seconds between node ID broadcasts.
        int IDTimerInterval = 6000;

        // The timer that will be used for periodic node ID broadcasts.
        System.Timers.Timer? MyTimer;

        //TODO: delete this?
        // Delegate method to set the form status text
        delegate void SetTextCallback(string text);

        // TODO: delete
        // Delegate method for updating status fields on the form.
        //delegate void SetStatusUpdateCallback();

        // The number of the Server Mumble is currently connected to.
        string CurrentServerNumber = string.Empty;

        // The name of the Server Mumble is currently connected to.
        string CurrentServerName = string.Empty;

        // The number of the Channel Mumble is currently connected to.
        string CurrentChannelNumber = string.Empty;

        // The name of the Channel Mumble is currently connected to.
        string CurrentChannelName = string.Empty;

        // The URI of the Server Mumble is currently connected to.
        string CurrentServerURI = string.Empty;

        // The User Name Mumble is currently connected to the server with.
        string CurrentServerUserName = string.Empty;

        // The Port of the Server Mumble is currently connected to.
        string CurrentServerPort = string.Empty;

        #endregion // Variables and Constants


        #region Event Handlers

        /// <summary>
        /// Constructor
        /// </summary>
        public frmMain()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Required by .NET
                InitializeComponent();

                // Update status text
                SetText("starting...");

                // Initialize form
                lblWavIDFile.Text = "Please select a node ID WAV file.";

                // Populate list of sound devices
                PopulateWaveInDevices();
                PopulateWaveOutDevices();

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch           
        } // frmMain

        /// <summary>
        /// This method executes when a DTMF tone is detected and transmitting.
        /// </summary>
        /// <param name="obj"></param>
        private void Analyzer_DtmfToneStarted(DtmfDetection.DtmfChange obj)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Insert a blank line in the status text
                SetText(string.Empty);

                // Mute the Mumble client to prevent DTMF tones from going on the channel
                MumbleMute();
                // TODO: delete?
                //IssueCommand(@MumbleExePath, @"rpc mute");

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // Analyzer_DtmfToneStarted

        /// <summary>
        /// This method executes after a DTMF tone has stopped.
        /// The tone will be interpreted and processed if valid.
        /// </summary>
        /// <param name="obj"></param>
        private void Analyzer_DtmfToneStopped(DtmfDetection.DtmfChange obj)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Get the DTMF character that was just transmitted.
                string currentDTMFChar = GetDTMFShortHand(obj.Key.ToString());

                // Create fallThrough variable.
                // When TRUE, all processing sections below will be skipped.
                bool fallThrough = false;

                // GET FIRST CHARACTER
                // is this the beginning of a new command?
                if (string.IsNullOrEmpty(CurrentDTMFCommand))
                {
                    // is this character initiating a new command? (#)
                    if (currentDTMFChar == @"#")
                    {
                        // This could be the start of a command. Set the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isCommand;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand = currentDTMFChar;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // if
                    else
                    {
                        // An invalid character was received. Reset the DTMFCommandState.
                        ResetDTMFCommandState();
                    } // else
                } // if


                // GET SECOND CHARACTER
                // command has started, get 2nd char
                if (MyState == DTMFCommandStates.isCommand && fallThrough == false)
                {
                    if (currentDTMFChar == @"*")
                    {
                        // 2nd position is *, command is #*
                        // A Disconnect command has been received
                        MyState = DTMFCommandStates.isDisconnect;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // A complete command has been received, no other characters are expected.
                        // Set the FinalDTMFCommand variable for processing.
                        FinalDTMFCommand = CurrentDTMFCommand;
                    } // if
                    // 2nd char is NOT *, so it MUST be 0-9
                    else if (IsNumeric(currentDTMFChar))
                    {
                        // Whatever this command is going to be, it's not a Disconnect.
                        // Set the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isNotDisconnect;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // else if
                      // illegal char passed, reset
                    else
                    {
                        // An invalid character was received. Reset the DTMFCommandState.
                        ResetDTMFCommandState();
                    } // else
                } // if


                // GET THIRD CHARACTER
                // command has started and is not a disconnect, get 3rd char
                if (MyState == DTMFCommandStates.isNotDisconnect && fallThrough == false)
                {
                    // if 3rd char is * this is a config change
                    if (currentDTMFChar == @"*")
                    {
                        // Command is a Load Config request.
                        // Set DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isLoadConfig;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // A complete command has been received, no other characters are expected.
                        // Set the FinalDTMFCommand variable for processing.
                        FinalDTMFCommand = CurrentDTMFCommand;
                    } // if
                      // if 3rd char is 0-9, this is isAdminSettingORChannelChange
                    else if (IsNumeric(currentDTMFChar))
                    {
                        // Command is either an Admin Setting change or a Channel Change request.
                        // Update the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isAdminSettingORChannelChange;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // else if
                      // illegal char passed, reset
                    else
                    {
                        // An invalid character was received. Reset the DTMFCommandState.
                        ResetDTMFCommandState();
                    } // else
                } // if


                // GET FOURTH CHARACTER
                //command has started, and is either Admin Setting or Channel Change, get 4th char
                if (MyState == DTMFCommandStates.isAdminSettingORChannelChange && fallThrough == false)
                {
                    // if 4th char is # this is an Admin Setting change
                    if (currentDTMFChar == @"#")
                    {
                        // Command is an Admin Setting change.
                        // Update the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isAdminSetting;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // if
                      // if 4th char is 0-9, this is a channel change
                    else if (IsNumeric(currentDTMFChar))
                    {
                        // Command is a Change Channel request.
                        // Update the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isChangeChannel;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // else if
                      // illegal char passed, reset
                    else
                    {
                        // An invalid character was received. Reset the DTMFCommandState.
                        ResetDTMFCommandState();
                    } // else
                } // if


                // GET FIFTH CHARACTER
                //command has started, and is Admin Setting, get 5th char
                if (MyState == DTMFCommandStates.isAdminSetting && fallThrough == false)
                {
                    // Admin Setting change, 5th char is 0-9
                    if (IsNumeric(currentDTMFChar))
                    {
                        // The command is an Admin Settings change, and the command is not final - more characters are expected.
                        // Update the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isAdminSettingNotFinal;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // if
                      // illegal char passed, reset
                    else
                    {
                        // An invalid character was received. Reset the DTMFCommandState.
                        ResetDTMFCommandState();
                    } // else
                } // if

                //command has started, and is Channel Change, get 5th char
                if (MyState == DTMFCommandStates.isChangeChannel && fallThrough == false)
                {
                    // Admin Setting change, 5th char is #
                    if (currentDTMFChar == @"#")
                    {
                        // The command is a channel change request, the channel number is still expected to come.
                        // Update the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isChannelChangeNoChannelNumber;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // if
                      // illegal char passed, reset
                    else
                    {
                        // An invalid character was received. Reset the DTMFCommandState.
                        ResetDTMFCommandState();
                    } // else
                } // if


                //GET SIXTH CHARACTER
                // command has started, is Admin Setting, get 6th char
                if (MyState == DTMFCommandStates.isAdminSettingNotFinal && fallThrough == false)
                {
                    // Admin Setting change, 6th char is *
                    if (currentDTMFChar == @"*")
                    {
                        // The command is an Admin Settings change, and the final character has just been received. No further characters are expected for this command.
                        // Update the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isAdminSettingFinal;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // A complete command has been received, no other characters are expected.
                        // Set the FinalDTMFCommand variable for processing.
                        FinalDTMFCommand = CurrentDTMFCommand;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // if
                      // illegal char passed, reset
                    else
                    {
                        // An invalid character was received. Reset the DTMFCommandState.
                        ResetDTMFCommandState();
                    } // else
                } // if

                // command has started, is Channel Change, get 6th char
                if (MyState == DTMFCommandStates.isChannelChangeNoChannelNumber && fallThrough == false)
                {
                    // Channel Change, 6th char is 0-9
                    if (IsNumeric(currentDTMFChar))
                    {
                        // The command is a Channel Change request, and the Channel Number character has just been received. One more character is expected for this command.
                        // Update the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isChannelChangeNotFinal;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // if
                      // illegal char passed, reset
                    else
                    {
                        // An invalid character was received. Reset the DTMFCommandState.
                        ResetDTMFCommandState();
                    } // else
                } // if


                //GET SEVENTH CHARACTER
                if (MyState == DTMFCommandStates.isChannelChangeNotFinal && fallThrough == false)
                {
                    // Channel Change, 7th char is *
                    if (currentDTMFChar == @"*")
                    {
                        // The command is a Channel Change request, and the final character has just been received. No further characters are expected for this command.
                        // Update the DTMFCommandState to reflect this.
                        MyState = DTMFCommandStates.isChannelChangeFinal;
                        // Add the current character to the command being received and processed.
                        CurrentDTMFCommand += currentDTMFChar;
                        // A complete command has been received, no other characters are expected.
                        // Set the FinalDTMFCommand variable for processing.
                        FinalDTMFCommand = CurrentDTMFCommand;
                        // No other processing needs to be done for this character.
                        // Fall Through the remaining code blocks.
                        fallThrough = true;
                    } // if
                      // illegal char passed, reset
                    else
                    {
                        // An invalid character was received. Reset the DTMFCommandState.
                        ResetDTMFCommandState();
                    } // else
                } // if

                // Process the final DTMF command.
                ProcessDTMFCommand(FinalDTMFCommand, MyState);
                
                // Update the form status with the character received and the command so far.
                SetText(currentDTMFChar + "-" + CurrentDTMFCommand);

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // Analyzer_DtmfToneStopped

        /// <summary>
        /// This method executes on any DTMF tones being received.
        /// This method wires up Analyzer_DtmfToneStarted and Analyzer_DtmfToneStopped event handlers.
        /// </summary>
        /// <param name="obj"></param>
        private void Analyzer_OnDtmfDetected(DtmfDetection.DtmfChange obj)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // If the tone is starting, call the Analyzer_DtmfToneStarted handler.
                if (obj.IsStart)
                {
                    // Tone is started
                    Analyzer_DtmfToneStarted(obj);
                } // if
                else if (obj.IsStop) // if the tone is ending, call the Analyzer_DtmfToneStopped handler.
                {
                    // Tone has stopped
                    Analyzer_DtmfToneStopped(obj);
                } // else if
                else
                {
                    // Something other than Start or Stop was received.
                    throw new Exception("Unknown DTMF code received!");
                } // else

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch  
        } // Analyzer_OnDtmfDetected

        /// <summary>
        /// Starts listening for DTMF control tones.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdListen_Click(object sender, EventArgs e)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Launch the Mumble client.
                LaunchMumble(MumbleExePath);

                // Disable the form's Listen button.
                cmdListen.Enabled = false;

                // Enable the form's Stop button.
                cmdStop.Enabled = true;
                
                // Start the Timer job for the periodic node ID broadcast.
                StartIDTimerJob();

                // Reset (start) the DTMF tone analyzer.
                ResetDTMFAnalyzer();

                // Broadcast a vocal node status update.
                SpeakIt("Now listening for commands.");

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch           
        } // cmdListen_Click

        /// <summary>
        /// TODO: what is this method supposed to do? is it needed?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdMute_Click(object sender, EventArgs e)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // TODO: what is this method supposed to do? is it needed?
                StringBuilder mySB = new StringBuilder();
                mySB.AppendLine(string.Format("Selected Input Device is {0} using Device Number {1}",
                    comboWaveIn.SelectedItem,
                    comboWaveIn.SelectedValue.ToString()));
                mySB.AppendLine(string.Format("Selected Output Device is {0} using Device Number {1}",
                    comboWaveOut.SelectedItem,
                    comboWaveOut.SelectedValue.ToString()));

                MessageBox.Show(mySB.ToString());

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch            
        } // cmdMute_Click

        /// <summary>
        /// Open a folder browse dialog so the user can select the folder containing the CSV configuration file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdSelectConfigLocation_Click(object sender, EventArgs e)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Set the Browse Dialog control's description.
                folderBrowserDialog1.Description = "Select the Config File location";
                
                // Show the Browse Dialog control.
                folderBrowserDialog1.ShowDialog();

                // Set the ConfigFilePath state variable based on the user's Browse Dialog selection.
                ConfigFilePath = folderBrowserDialog1.SelectedPath;
                
                // Update the form to reflect the selected config file location.
                lblConfigFilePath.Text = ConfigFilePath;

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // cmdSelectConfigLocation_Click

        /// <summary>
        /// Open a file browse dialog so the user can select the periodic node ID broadcast WAV file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdSelectIDFile_Click(object sender, EventArgs e)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Set the Browse Dialog Title.
                openFileDialog1.Title = "Select the ID .wav file";
                
                // Filter for files with a .wav extenion.
                openFileDialog1.Filter = @"Wav files(*.wav)|*.wav";
                
                // Display the Browse Dialog.
                openFileDialog1.ShowDialog();

                // Set the IDWaveFile state variable based on the user's Browse Dialog selection.
                IDWaveFile = openFileDialog1.FileName;

                // Update the form to reflect the selected ID file.
                lblWavIDFile.Text = IDWaveFile;

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // cmdSelectIDFile_Click

        /// <summary>
        /// Stop the DTMF analyzer and shut down the Mumble client application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdStop_Click(object sender, EventArgs e)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Disable the form's Stop button.
                cmdStop.Enabled = false;
                
                // Enable the form's Listen button.
                cmdListen.Enabled = true;

                // Dispose of the current DTMF analyzer.
                analyzer.Dispose();

                // Stop the periodic ID broadcast timer job.
                StopIDTimerJob();

                // Shut down the Mumble client application.
                KillMumble();

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch           
        } // cmdStop_Click

        /// <summary>
        /// Confirm the devices and configuration settings in the form's UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdUseDevices_Click(object sender, EventArgs e)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // ******************************************


                // TODO: Delete this!!!
                //  Hardcoded values for developer testing...
#if DEBUG
                ConfigFilePath = @"E:\misc\Rumble";
                lblConfigFilePath.Text = ConfigFilePath;
                IDWaveFile = @"E:\misc\Rumble\morse.wav";
                lblWavIDFile.Text = IDWaveFile;
#endif
                // ******************************************

                // Get the index number of the selected audio input device.
                DeviceInNo = (int)comboWaveIn.SelectedValue;

                // Get the index number of the selected audio output device.
                DeviceOutNo = (int)comboWaveOut.SelectedValue;

                // Update the form's status field.
                SetText(string.Format("DeviceIn set to {0} -- DeviceOut set to {1}", comboWaveIn.Items[DeviceInNo].ToString(), comboWaveOut.Items[DeviceOutNo].ToString()));

                // No DTMF commands are currently being processed.
                // Update the DTMFCommandState to reflect this.
                MyState = DTMFCommandStates.ignore;

                // Provide an audio greeting to the user.
                // TODO: move the greeting text to application config file?
                SpeakIt("Welcome to Rumble!");

                // Load the settings from the default configration file.
                // TODO: replace hardcoded 0
                LoadConfig("0");

                // Enable the Listen command button.
                cmdListen.Enabled = true;

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // cmdUseDevices_Click

        /// <summary>
        /// Event Handler for Periodic Node ID Broadcast Timer Job.
        /// Plays the Node ID WAV file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Play the Node ID WAV file.
                PlaySound(IDWaveFile);

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch            
        } // OnAnalyzerTimedEvent

        #endregion // Event Handlers


        #region Supporting Methods

        /// <summary>
        /// Build the URI for a Mumble Channel connection based on the input Configuration File line.
        /// </summary>
        /// <param name="ConfigLine">The Configuration File line to use to build the Mumble URI line.</param>
        /// <returns>The properly formatted Mumble URI line.</returns>
        private string BuildMumbleURI(RumbleConfigLine ConfigLine)
        {
            string MumbleURI = string.Empty;

            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Get the port number from the configi file line.
                string portToUse = ConfigLine.Port;

                // If no port was specified, just use the Mumble default of 64738.
                if (string.IsNullOrEmpty(portToUse))
                {
                    portToUse = "64738";
                } // if

                // Create a variable to hold the selected channel path.
                string channelPath;

                // Set the newly created channelPath variable based on the channel path fromt the config file line.
                // If the current channel path doesn't start with a slash "/" go ahead and append one to the beginning.
                if (ConfigLine.ChannelPath.Substring(0, 1) == @"/")
                {
                    // Channel path begins with "/", no need to append.
                    channelPath = ConfigLine.ChannelPath;
                } // if
                else
                {
                    // Channel path does not begin with "/", append one.
                    channelPath = @"/" + ConfigLine.ChannelPath;
                } // else

                // The password for the server was not provided, but a URI with no password in it.
                if (string.IsNullOrEmpty(ConfigLine.Password))
                {
                    // Build the Mumble URI.
                    MumbleURI = string.Format("mumble://{0}@{1}:{2}{3}", ConfigLine.UserName, ConfigLine.ServerURL, portToUse, channelPath);
                } // if
                // The password for the server was provided, but a URI with a password in it.
                else
                {
                    MumbleURI = string.Format("mumble://{0}:{1}@{2}:{3}{4}", ConfigLine.UserName, ConfigLine.Password, ConfigLine.ServerURL, portToUse, channelPath);
                } // else

                // Set the state variable for Server Port Number.
                CurrentServerPort = portToUse;

                // Set the state variable for Server URL.
                CurrentServerURI = ConfigLine.ServerURL;

                // Set the state variable for Server User Name.
                CurrentServerUserName = ConfigLine.UserName;

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch

            // Return the finished URI to the caller.
            return MumbleURI;

        } // BuildMumbleURI

        /// <summary>
        /// Changes an Administrative Setting for the application.
        /// </summary>
        /// <param name="AdminSetting">The settings to change.</param>
        /// <param name="AdminSettingValue">The value the setting to changed to.</param>
        private void ChangeAdminSetting(string AdminSetting, string AdminSettingValue)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Update the form's status field.
                SetText(string.Format("changing admin setting {0} to value {1}", AdminSetting, AdminSettingValue));

                // Evaluete the provided Admin Setting to be changed.
                switch (AdminSetting)
                {
                    // The user wants to Mute or Unmute the Mumble client application.
                    case "00": // Mute / Unmute
                        switch (AdminSettingValue)
                        {
                            // The user wants to Mute the Mumble client application.
                            case "0":
                                // TODO: why do I need this variable? where is it evaluated???
                                StayMuted = true;
                                
                                // Mute the Mumble client.
                                MumbleMute();

                                // Provide an audio status update.
                                SpeakIt("muted");
                                
                                break;
                            // The user wants to Unmute the Mumble client application.
                            case "1":
                                // TODO: why do I need this variable? where is it evaluated???
                                StayMuted = false;
                                
                                // Unmute the Mumble client.
                                MumbleUnmute();

                                // Provide an audio status update.
                                SpeakIt("un-muted");

                                break;
                            default:
                                // TODO: should I throw an exception here?
                                // Supplied admin setting value was invalid!
                                break;
                        } // switch
                        break;
                    // The user wants to Deaf or Undeaf the Mumble client application.
                    case "01": // Deaf / Undeaf
                        switch (AdminSettingValue)
                        {
                            // The user wants to Deaf the Mumble client application.
                            case "0":
                                // TODO: why do I need this variable? where is it evaluated???
                                StayMuted = true;

                                // Deaf the Mumble client.
                                MumbleDeaf();

                                // Provide an audio status update.
                                SpeakIt("deaf");
                                
                                break;
                            // The user wants to Undeaf the Mumble client application.
                            case "1":
                                // TODO: why do I need this variable? where is it evaluated???
                                StayMuted = false;

                                // Undeaf the Mumble client.
                                MumbleUndeaf();

                                // Provide an audio status update.
                                SpeakIt("un deaf");
                                
                                break;
                            default:
                                // TODO: should I throw an exception here?
                                // Supplied admin setting value was invalid!
                                break;
                        } // switch
                        break;
                    // User wants a status update.
                    case "03":
                        switch (AdminSettingValue)
                        {
                            // Provide a General Status Update.
                            case "0":
                                // Deaf the Mumble client.
                                MumbleDeaf();

                                // TODO: document this!
                                // Provide an audio status update.
                                StringBuilder myStatus = new StringBuilder();

                                myStatus.AppendLine(string.Format("Hello. Today is {0}. It is currently {1}.", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
                                myStatus.AppendLine(string.Format("You are connected to the server {0}.", CurrentServerName));
                                myStatus.AppendLine(string.Format("This server has a server number value of {0}.", CurrentServerNumber));
                                myStatus.AppendLine(string.Format("The URL of this server is {0}.", CurrentServerURI));
                                myStatus.AppendLine(string.Format("You are connected to port {0}.", CurrentServerPort));
                                myStatus.AppendLine(string.Format("You are connected to channel {0}.", CurrentChannelName));
                                myStatus.AppendLine(string.Format("This channel has a channel number value of {0}.", CurrentChannelNumber));
                                myStatus.AppendLine(string.Format("Your Mumble User Name is {0}.", CurrentServerUserName));

                                SetText(myStatus.ToString());

                                SpeakIt(myStatus.ToString());

                                // Undeaf the Mumble client.
                                MumbleUndeaf();

                                break;
                            case "1": // fun fact
                                // Deaf the Mumble client.
                                MumbleMute();

                                // TODO: document this!
                                Random rnd = new Random();
                                //myStatus.AppendLine(string.Format("Today's lucky number is {0}!", rnd.Next(1000).ToString()));

                                int counter = 0;
                                int totalLines = 0;

                                // Read the file
                                foreach (string line in System.IO.File.ReadLines(@"E:\misc\Rumble\funFacts.txt"))
                                {
                                    totalLines++;
                                } // foreach

                                int lineNumber = rnd.Next(totalLines);
                                string currentFact = string.Empty;

                                // Read the file
                                foreach (string line in System.IO.File.ReadLines(@"E:\misc\Rumble\funFacts.txt"))
                                {
                                    //System.Console.WriteLine(line);
                                    counter++;
                                    if (counter == lineNumber)
                                    {
                                        currentFact = line;
                                        break;
                                    } // if
                                } // foreach

                                SetText(string.Format("And now it's time for today's fun fact! {0}.", currentFact));

                                SpeakIt(string.Format("And now it's time for today's fun fact! {0}.", currentFact));

                                // Undeaf the Mumble client.
                                MumbleUnmute();

                                break;
                            default:
                                // TODO: should I throw an exception here?
                                // Supplied admin setting value was invalid!
                                break;
                        } // switch
                        break;
                    // TODO: add more settings
                    default:
                        break;
                } // switch

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // ChangeAdminSetting

        /// <summary>
        /// Handles a Channel Change request.
        /// </summary>
        /// <param name="ServerNumber">The Server Number to connect to. Specified in Node Configuration CSV file.</param>
        /// <param name="ChannelNumber">The Channel Number to connect to. Specified in Node Configuration CSV file.</param>
        private void ChangeChannel(string ServerNumber, string ChannelNumber)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Update the form's status field.
                SetText(string.Format("changing channel to server {0}, channel {1}", ServerNumber, ChannelNumber));

                // Set the state variable for Server Number.
                CurrentServerNumber = ServerNumber;

                // Set the state variable for Channel Number.
                CurrentChannelNumber = ChannelNumber;

                // If the Channel Number requeseted was zero "0" reset the channel connection.
                // This will launch a connection to a null channel.
                if (ChannelNumber == "0")
                {
                    // Connect to a null channel address.
                    IssueCommand(MumbleExePath, ResetURI);
                    
                    // Sleep for one-half second / 500 milliseconds.
                    Thread.Sleep(500);
                } // if

                // Create a new RumbleConfigLine variable.
                // The provided Server and Channel Numbers will be used as index values to search the configuration settings.
                RumbleConfigLine matchingConfig = new RumbleConfigLine();

                // Loop through all configuration file lines.
                foreach (RumbleConfigLine myLine in MyConfigs)
                {
                    // Does the Server Number of the current line match the desired Server Number?
                    if (myLine.ServerNumber == ServerNumber)
                    {
                        // Does the Channel Number of the current line match the desired Channel Number?
                        if (myLine.ChannelNumber == ChannelNumber)
                        {
                            // The desired configuration line has been found.
                            // Store it in the matchingConfig variable.
                            matchingConfig = myLine;

                            break;
                        } // if
                    } // if
                } // foreach

                // Get URL from the config line based on the server and channel numbers.
                // Check to make sure the config URL isn't blank.
                if (!string.IsNullOrEmpty(matchingConfig.ServerURL))
                {
                    // Get a properly formatted Mumble URI based on the config file URL.
                    string mumbleURI = BuildMumbleURI(matchingConfig);

                    // Connect to a null channel address.
                    IssueCommand(MumbleExePath, ResetURI);

                    // Connect to the desired server and channel using the returned URI.
                    IssueCommand(MumbleExePath, mumbleURI);

                    // Sleep for two and a half seconds / 2,500 milliseconds.
                    Thread.Sleep(2500);

                    // Create a variable to hold the display / audio name of the server.
                    // If a nickname was provided in the CSV configuration file, that value will be used.
                    // If one was not provided, the server numebr will be used instead.
                    string serverName = string.Empty;

                    // Create a variable to hold the display / audio name of the server.
                    // If a nickname was provided in the CSV configuration file, that value will be used.
                    // If one was not provided, the server numebr will be used instead.
                    string channelName = string.Empty;

                    // Check to see if a server nickname was provided in the CSV configuration file.
                    if (!string.IsNullOrEmpty(matchingConfig.ServerNickname))
                    {
                        // Use the provided nickname.
                        serverName = matchingConfig.ServerNickname;
                    } // if
                    else
                    {
                        // No nickname was provided. Use the Server Number instead.
                        serverName = string.Format("server {0}", ServerNumber);
                    } // else

                    // Check to see if a channel nickname was provided in the CSV configuration file.
                    if (!string.IsNullOrEmpty(matchingConfig.ChannelNickname))
                    {
                        // Use the provided nickname.
                        channelName = matchingConfig.ChannelNickname;
                    } // if
                    else
                    {
                        // No nickname was provided. Use the Channel Number instead.
                        channelName = string.Format("channel {0}", ChannelNumber);
                    } // else

                    // Unmute the Mumble client application.
                    // The channel has been changed, unmute so users can speak on the channel.
                    MumbleUnmute();

                    // Update the state variable for Server Name.
                    CurrentServerName = serverName;

                    // Update the state variable for Channel Name.
                    CurrentChannelName = channelName;

                    // Update the form's status field.
                    SetText(string.Format("Channel changed to {1} on the {0} server.", serverName, channelName));
                    
                    // Provide an audio status update.
                    SpeakIt(string.Format("Channel changed to {1} on the {0} server.", serverName, channelName));
                } // if
                // The requested Server / Channel pair counld not be found.
                else
                {
                    // Update the form's status field.
                    SetText(string.Format("Requested channel {1} on server {0} could not be found.", ServerNumber, ChannelNumber));

                    // Provide an audio status update.
                    SpeakIt(string.Format("Requested channel {1} on server {0} could not be found.", ServerNumber, ChannelNumber));
                } // else

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // Change channel

        /// <summary>
        /// Disconnect from the current channel and connect to a null channel.
        /// </summary>
        private void Disconnect()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Connect to a null channel.
                IssueCommand(MumbleExePath, ResetURI);

                // Update the form's status field.
                SetText("Mumble Client Disconnected from Channel");

                // Provide an audio status update.
                SpeakIt("Mumble Client Disconnected from Channel");

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // Disconnect

        /// <summary>
        /// The DTMF Tone Analyzer returns long string names for numbers (ie - one, two three).
        /// This method shortens the long string name to the expected numeric value.
        /// EXAMPLE: One is returned as 1, Two is returned as 2, etc.
        /// </summary>
        /// <param name="DTMFKey">The long-form DTMF key value to evaluate.</param>
        /// <returns>A short-form version of the provided DTMF value.</returns>
        private string GetDTMFShortHand(string DTMFKey)
        {
            // A variable to hold the return value.
            string retVal = string.Empty;

            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Evaluate the long-form DTMF value.
                switch (DTMFKey)
                {
                    // An unrecognized value was passed in.
                    case "None":
                        // Set the return value.
                        retVal = "?";
                        break;
                    case "Zero":
                        // Set the return value.
                        retVal = "0";
                        break;
                    case "One":
                        // Set the return value.
                        retVal = "1";
                        break;
                    case "Two":
                        // Set the return value.
                        retVal = "2";
                        break;
                    case "Three":
                        // Set the return value.
                        retVal = "3";
                        break;
                    case "Four":
                        // Set the return value.
                        retVal = "4";
                        break;
                    case "Five":
                        // Set the return value.
                        retVal = "5";
                        break;
                    case "Six":
                        // Set the return value.
                        retVal = "6";
                        break;
                    case "Seven":
                        // Set the return value.
                        retVal = "7";
                        break;
                    case "Eight":
                        // Set the return value.
                        retVal = "8";
                        break;
                    case "Nine":
                        // Set the return value.
                        retVal = "9";
                        break;
                    case "Star":
                        // Set the return value.
                        retVal = "*";
                        break;
                    case "Hash":
                        // Set the return value.
                        retVal = "#";
                        break;
                    case "A":
                        // Set the return value.
                        retVal = "A";
                        break;
                    case "B":
                        // Set the return value.
                        retVal = "B";
                        break;
                    case "C":
                        // Set the return value.
                        retVal = "C";
                        break;
                    case "D":
                        // Set the return value.
                        retVal = "D";
                        break;
                    default:
                        // TODO: throw exception?
                        // Something unknown passed in!
                        break;
                } // switch

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch

            // Return the value to the caller.
            return retVal;

        } // GetDTMFShorthand

        /// <summary>
        /// Evaluates the supplied string to see if it's a numeric value or not.
        /// </summary>
        /// <param name="EvaluateString">The character to be evaluted.</param>
        /// <returns>TRUE if numeric, FALSE if not.</returns>
        private bool IsNumeric(string EvaluateString)
        {
            // The value to be returned to the caller.
            bool retVal = false;

            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Evaluate the supplied character.
                switch (EvaluateString)
                {
                    // The value is 0-9.
                    case @"0":
                    case @"1":
                    case @"2":
                    case @"3":
                    case @"4":
                    case @"5":
                    case @"6":
                    case @"7":
                    case @"8":
                    case @"9":
                        // Set the return value to TRUE.
                        retVal = true;
                        break;
                    default:
                        break;
                } // switch

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch

            // Return the result to the caller.
            return retVal;

        } // IsNumeric

        /// <summary>
        /// Issues a command to the Mumble Client application.
        /// </summary>
        /// <param name="CommandText">The Mumble client's Mubmle.exe path.</param>
        /// <param name="Arguments">The command to pass to the Mumble client.</param>
        private void IssueCommand(string CommandText, string Arguments)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Create a new Process object.
                Process process = new System.Diagnostics.Process();
                
                // Create the ProcessStartInfo parameters for the new Process.
                ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

                // Configure the ProcessStartInfo parameters.
                // Required to resolve elevated permissions errors.
                startInfo.UseShellExecute = true;
                // Required to resolve elevated permissions errors.
                startInfo.Verb = "runas";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = CommandText;
                startInfo.Arguments = Arguments;
                
                // Associate the settings with the Process object.
                process.StartInfo = startInfo;
                
                // Launch the Process.
                process.Start();

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // IssueCommand

        /// <summary>
        /// Shuts down the Mumble client application.
        /// </summary>
        private void KillMumble()
        {
            MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
            Process[] procs = null;

            try
            {
                // logging
                MethodBeginLogging(myMethod);

                // Find the Mumble process thread by name.
                procs = Process.GetProcessesByName("mumble");
                
                // If there are multiple Mumble processes running, shut down the first one.
                if (procs.Count<object>() > 0)
                {
                    // Get the first Mumble process.
                    Process mumbleProc = procs[0];
                    
                    // If that process is still running, kill it.
                    if (!mumbleProc.HasExited)
                    {
                        // Kil. the process.
                        mumbleProc.Kill();
                    } // if
                } // if
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch            
            finally
            {
                
                // If the Mumble process thread collection is not null, nullify it!
                if (procs != null)
                {
                    // Loop through all Mumble process threads.
                    foreach (Process p in procs)
                    {
                        // Clean up process thread handle.
                        p.Dispose();
                    } // foreach
                } // if

                // logging
                MethodEndLogging(myMethod);
            } // finally
        } // KillMumble

        // TODO: resume commenting here...
        private void LaunchMumble(string CommandText)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // start new process
                currentMumbleProcess = new System.Diagnostics.Process();

                currentMumbleProcessStartInfo = new System.Diagnostics.ProcessStartInfo();
                currentMumbleProcessStartInfo.FileName = CommandText;
                currentMumbleProcessStartInfo.UseShellExecute = true;
                currentMumbleProcessStartInfo.Verb = "runas";
                currentMumbleProcess.StartInfo = currentMumbleProcessStartInfo;
                currentMumbleProcess.Start();
                Thread.Sleep(500);

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch                   
        } // LaunchMumbleCommand

        /// <summary>
        /// Loads a CSV configuration settings file.
        /// </summary>
        /// <param name="ConfigNumber">0 to 9.
        /// The number of the file to read. 
        /// Files should be named in a rumbleConfig_#.csv format.</param>
        private void LoadConfig(string ConfigNumber)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Update the form's status field.
                SetText(string.Format("Loading config {0}", ConfigNumber));

                // Reset the configuration List object.
                MyConfigs = new List<RumbleConfigLine>();

                // Build the name of the configuration file.
                string configFileName = string.Format(@"\rumbleConfig_{0}.csv", ConfigNumber);
                
                // Build the full path to the configuration file.
                string filePath = string.Format("{0}{1}", ConfigFilePath, configFileName);

                // Create a new StreamReader and point it to the config file.
                StreamReader sr = new StreamReader(filePath);

                // Create a variable to hold each config file line for parsing.
                string line;

                // Create a string array for config file lines.
                string[] dataRow = new string[6];
                RumbleConfigLine thisRumbleConfigLine;

                // Read the first line.
                // This line just contains field headers and can be ignored.
                line = sr.ReadLine();

                // Read the remaining lines.
                while ((line = sr.ReadLine()) != null)
                {
                    // Split each row on the comma character.
                    dataRow = line.Split(',');

                    // Create a new RumbleConfigLine object to parse the current CSV file line.
                    thisRumbleConfigLine = new RumbleConfigLine();
                    
                    // Parse the Server Number.
                    thisRumbleConfigLine.ServerNumber = dataRow[0];

                    // Parse the Channel Number.
                    thisRumbleConfigLine.ChannelNumber = dataRow[1];
                    
                    // Parse the Server URL.
                    thisRumbleConfigLine.ServerURL = dataRow[2];
                    
                    // Parse the Server Port Number.
                    thisRumbleConfigLine.Port = dataRow[3];
                    
                    // Parse the Server UserName.
                    thisRumbleConfigLine.UserName = dataRow[4];
                    
                    // Parse the Server Password.
                    thisRumbleConfigLine.Password = dataRow[5];
                    
                    // Parse the Server Channel Path.
                    thisRumbleConfigLine.ChannelPath = dataRow[6];
                    
                    // Parse the Server Name.
                    thisRumbleConfigLine.ServerNickname = dataRow[7];
                    
                    // Parse the Channel Name.
                    thisRumbleConfigLine.ChannelNickname = dataRow[8];
                    
                    // Add the newly parsed line to the collection.
                    MyConfigs.Add(thisRumbleConfigLine);
                } // while

                // Update the form's status field.
                SetText(string.Format("Configuration file number {0} has been loaded.", ConfigNumber));

                // Provide an audio status update.
                SpeakIt(string.Format("Configuration file number {0} has been loaded.", ConfigNumber));

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // LoadConfig

        /// <summary>
        /// Enable the 'Deaf' feature of the Mumble client.
        /// </summary>
        private void MumbleDeaf()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Issue the Deaf command to the Mumble client.
                IssueCommand(@MumbleExePath, @"rpc deaf");

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch            
        } // MumbleDeaf

        /// <summary>
        /// Enable the 'Mute' feature of the Mumble client.
        /// </summary>
        private void MumbleMute()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Issue the Mute command to the Mumble client.
                IssueCommand(@MumbleExePath, @"rpc mute");

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // MumbleMute

        /// <summary>
        /// Disable the 'Mute' feature of the Mumble client.
        /// </summary>
        private void MumbleUnmute()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // If the StayMuted state variable is TRUE, ignore this request.
                if (!StayMuted)
                {
                    // Issue the Unmute command to the Mumble client.
                    IssueCommand(@MumbleExePath, @"rpc unmute");
                } // if

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch           
        } // MumbleUnmute

        /// <summary>
        /// Disable the 'Deaf' feature of the Mumble client.
        /// </summary>
        private void MumbleUndeaf()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Issue the Undeaf command to the Mumble client.
                IssueCommand(@MumbleExePath, @"rpc undeaf");

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // MumbleUndeaf

        /// <summary>
        /// Plays the specified WAV file.
        /// </summary>
        /// <param name="FileToPlay">The WAV file to play.</param>
        private void PlaySound(string FileToPlay)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Check the current DTMFCommandState.
                // Only proceed if state is 'ignore'. Any other value means DTMF tones are being received and processed.
                if (MyState == DTMFCommandStates.ignore)
                {
                    // Mute the Mumble client to prevent WAV audio from spilling on to the channel.
                    MumbleMute();

                    // Create a WaveFileReader and point it to the file to be played.
                    var waveReader = new WaveFileReader(FileToPlay);

                    // Create a new WaveOut object.
                    var waveOut = new WaveOut();

                    // Set the WaveOut device to the selected audio output device.
                    waveOut.DeviceNumber = DeviceOutNo;

                    // Initialite the WaveOut object.
                    waveOut.Init(waveReader);

                    // Play the WAV file.
                    waveOut.Play();

                    // Unmute the Mumble client.
                    MumbleUnmute();
                } // if
                else
                {
                    // DTMF commands are currently being received and processed.
                    SetText("*** FAILED: Tried to play node ID wav but was blocked by DTMF");
                } // else

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // PlaySound

        /// <summary>
        /// Populate the list of audio input devices.
        /// </summary>
        private void PopulateWaveInDevices()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Create a Dictionary to hold device names and numbers.
                Dictionary<string, int> myDevices = new Dictionary<string, int>();

                // Loop through all audio input devices.
                for (int deviceId = 0; deviceId < WaveIn.DeviceCount; deviceId++)
                {
                    // Get the device capabilities (this contains the device name).
                    var capabilitiesIn = WaveIn.GetCapabilities(deviceId);

                    // Add the device name and number to the Dictionary.
                    myDevices.Add(capabilitiesIn.ProductName, deviceId);
                } // for

                // Bind ComboBox to Dictionary.
                comboWaveIn.DataSource = new BindingSource(myDevices, null);
                
                // Display the device name.
                comboWaveIn.DisplayMember = "Key";
                
                // Bind to the device number.
                comboWaveIn.ValueMember = "Value";

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // PopulateWaveInDevices

        /// <summary>
        /// Populate the list of audio output devices.
        /// </summary>
        private void PopulateWaveOutDevices()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Create a Dictionary to hold device names and numbers.
                Dictionary<string, int> myDevices = new Dictionary<string, int>();
                
                // Loop through all audio output devices.
                for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
                {
                    // Get the device capabilities (this contains the device name).
                    var capabilitiesOut = WaveOut.GetCapabilities(deviceId);

                    // Add the device name and number to the Dictionary.
                    myDevices.Add(capabilitiesOut.ProductName, deviceId);
                } // for

                // Bind ComboBox to Dictionary.
                comboWaveOut.DataSource = new BindingSource(myDevices, null);

                // Display the device name.
                comboWaveOut.DisplayMember = "Key";

                // Bind to the device number.
                comboWaveOut.ValueMember = "Value";

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // PopulateWaveInDevices

        /// <summary>
        /// Processes and handles and complete DTMF command.
        /// </summary>
        /// <param name="DTMFCommand">The complete DTMF command to process.</param>
        /// <param name="CommandState">The application's current DTMFCommandState.</param>
        private void ProcessDTMFCommand(string DTMFCommand, DTMFCommandStates CommandState)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // Update the form's status field.
                SetText("Processing Command -- " + DTMFCommand);


                // Check to make sure the command isn't empty.
                if (!string.IsNullOrEmpty(DTMFCommand))
                {
                    switch (CommandState)
                    {
                        // No further processing is required for the following DTMF Command States:
                        //   ignore
                        //   isCommand
                        //   isNotDisconnect
                        //   isAdminSettingORChannelChange
                        //   isAdminSetting
                        //   isChangeChannel
                        //   isAdminSettingNotFinal
                        //   isChannelChangeNoChannelNumber
                        //   isChannelChangeNotFinal
                        case DTMFCommandStates.ignore:
                        case DTMFCommandStates.isCommand:
                        case DTMFCommandStates.isNotDisconnect:
                        case DTMFCommandStates.isAdminSettingORChannelChange:
                        case DTMFCommandStates.isAdminSetting:
                        case DTMFCommandStates.isChangeChannel:
                        case DTMFCommandStates.isAdminSettingNotFinal:
                        case DTMFCommandStates.isChannelChangeNoChannelNumber:
                        case DTMFCommandStates.isChannelChangeNotFinal:
                            break;
                        // Process a Disconnect request.
                        case DTMFCommandStates.isDisconnect:
                            // Disconnect the Mumble client from the current channel.
                            Disconnect();

                            // Reset the current DTMFCommandState.
                            ResetDTMFCommandState();

                            break;
                        // Load a new CSV configuration settings file.
                        case DTMFCommandStates.isLoadConfig:
                            // Get the file configuration number from the DTMF command string.
                            string configNumber = DTMFCommand.Substring(1, 1);
                            
                            // Load the requested configuration file.
                            LoadConfig(configNumber);

                            // Reset the current DTMFCommandState.
                            ResetDTMFCommandState();

                            break;
                        // Process a Admin Settings change request.
                        case DTMFCommandStates.isAdminSettingFinal:

                            // Get admin setting number from the DTMF command string.
                            string adminSetting = DTMFCommand.Substring(1, 2);
                            
                            // Get admin setting value from the DTMF command string.
                            string adminSettingValue = DTMFCommand.Substring(4, 1);

                            // Change the requested admin setting.
                            ChangeAdminSetting(adminSetting, adminSettingValue);

                            // Reset the current DTMFCommandState.
                            ResetDTMFCommandState();

                            break;
                        // Process a Channel Change request.
                        case DTMFCommandStates.isChannelChangeFinal:
                            // Get server number from the DTMF command string.
                            string serverNumber = DTMFCommand.Substring(1, 3);
                            
                            // Get channel number from the DTMF command string.
                            string channelNumber = DTMFCommand.Substring(5, 1);
                            
                            // Change the channel.
                            ChangeChannel(serverNumber, channelNumber);

                            // Reset the current DTMFCommandState.
                            ResetDTMFCommandState();
                            
                            break;
                        
                        default:
                            break;
                    } // switch

                } // if

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // ProcessDTMFCommand

        private void ResetDTMFAnalyzer()
        {
            if (analyzer is not null)
            {
                analyzer.OnDtmfDetected -= Analyzer_OnDtmfDetected;
                analyzer.Dispose();
                analyzer = null;
            } // if            
            analyzer = new BackgroundAnalyzer(micIn, forceMono: false);
            analyzer.OnDtmfDetected += Analyzer_OnDtmfDetected;
        } // ResetDTMFAnalyzer

        private void ResetDTMFCommandState()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                CurrentDTMFCommand = string.Empty;
                FinalDTMFCommand = string.Empty;
                MyState = DTMFCommandStates.ignore;

                MumbleUnmute();

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // ResetDTMFCommandState

        private void SetText(string text)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (this.textBox1.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text });
                } // if
                else
                {
                    string timestampString = DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToLongTimeString() + @": ";
                    string currentText = this.textBox1.Text;
                    this.textBox1.Text = timestampString + text + System.Environment.NewLine + currentText;
                } // else

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch      
        } // SetText

        private void SpeakIt(string TextToSpeak)
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    synth.Speak(TextToSpeak);
                }

                /*
                IWaveProvider provider = null;
                var stream = new MemoryStream();
                using (var synth = new SpeechSynthesizer())
                {
                    synth.SetOutputToAudioStream(stream,
                    new SpeechAudioFormatInfo(28000, AudioBitsPerSample.Eight, AudioChannel.Mono));

                    //synth.SetOutputToWaveStream(stream);
                    synth.Rate = -1;

                    synth.Speak(TextToSpeak);
                    stream.Seek(0, SeekOrigin.Begin);
                    provider = new RawSourceWaveStream(stream, new WaveFormat(28000, 8, 1));
                }
                var waveOut = new WaveOut();
                waveOut.DeviceNumber = DeviceOutNo;
                waveOut.NumberOfBuffers = 250000;
                waveOut.Init(provider);
                waveOut.Play();
                waveOut.Dispose();
                */

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // SpeakIt

        private void StartIDTimerJob()
        {

            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                int timerInterval;
                int.TryParse(txtTimerInterval.Text, out timerInterval);
                IDTimerInterval = timerInterval * 1000;

                MyTimer = new System.Timers.Timer();
                MyTimer.Interval = IDTimerInterval;
                MyTimer.AutoReset = true;
                MyTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                MyTimer.Start();

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch
        } // StartIDTimerJob

        private void StopIDTimerJob()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                MyTimer.Stop();

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch            
        } // StopIDTimerJob

        private void UpdateUserInterfaceStatus()
        {
            try
            {
                // logging
                MethodBase myMethod = new StackTrace().GetFrame(0).GetMethod();
                MethodBeginLogging(myMethod);

                /*
                if (this.txtChannelName.InvokeRequired)
                {
                    SetStatusUpdateCallback d = new SetStatusUpdateCallback(UpdateUserInterfaceStatus);
                    this.Invoke(d);
                } // if
                else
                {
                    this.txtChannelName.Text = CurrentChannelName;
                } // else

                if (this.txtChannelNumber.InvokeRequired)
                {
                    SetStatusUpdateCallback d = new SetStatusUpdateCallback(UpdateUserInterfaceStatus);
                    this.Invoke(d);
                } // if
                else
                {
                    this.txtChannelNumber.Text = CurrentChannelNumber;
                } // else

                if (this.txtCurrentServerPort.InvokeRequired)
                {
                    SetStatusUpdateCallback d = new SetStatusUpdateCallback(UpdateUserInterfaceStatus);
                    this.Invoke(d);
                } // if
                else
                {
                    this.txtCurrentServerPort.Text = CurrentServerPort;
                } // else

                if (this.txtCurrentServerURL.InvokeRequired)
                {
                    SetStatusUpdateCallback d = new SetStatusUpdateCallback(UpdateUserInterfaceStatus);
                    this.Invoke(d);
                } // if
                else
                {
                    this.txtCurrentServerURL.Text = CurrentServerURI;
                } // else

                if (this.txtCurrentServerUserName.InvokeRequired)
                {
                    SetStatusUpdateCallback d = new SetStatusUpdateCallback(UpdateUserInterfaceStatus);
                    this.Invoke(d);
                } // if
                else
                {
                    this.txtCurrentServerUserName.Text = CurrentServerUserName;
                } // else

                if (this.txtServerName.InvokeRequired)
                {
                    SetStatusUpdateCallback d = new SetStatusUpdateCallback(UpdateUserInterfaceStatus);
                    this.Invoke(d);
                } // if
                else
                {
                    this.txtServerName.Text = CurrentServerName;
                } // else

                if (this.txtServerNumber.InvokeRequired)
                {
                    SetStatusUpdateCallback d = new SetStatusUpdateCallback(UpdateUserInterfaceStatus);
                    this.Invoke(d);
                } // if
                else
                {
                    this.txtServerNumber.Text = CurrentServerNumber;
                } // else
                */

                
                txtChannelName.Text = CurrentChannelName;
                txtChannelNumber.Text = CurrentChannelNumber;
                txtCurrentServerPort.Text = CurrentServerPort;
                txtCurrentServerURL.Text = CurrentServerURI;
                txtCurrentServerUserName.Text = CurrentServerUserName;
                txtServerName.Text = CurrentServerName;
                txtServerNumber.Text = CurrentServerNumber;
                

                // logging
                MethodEndLogging(myMethod);
            } // try
            catch (Exception ex)
            {
                UtilityMethods.ExceptionHandler(ex, TraceString);
            } // catch            
        } // UpdateUserInterfaceStatus

        #endregion // Supporting Methods


        #region Logging

        /// <summary>
        /// Generates trace strings and writes them to the debugger.
        /// </summary>
        /// <param name="CurrentMethod">A MethodBase object representing the calling method.</param>
        private void MethodBeginLogging(MethodBase CurrentMethod)
        {
            TraceString += @"|" + CurrentMethod.Name + "("; // Append method name to trace string
            IEnumerable<ParameterInfo> myParams = CurrentMethod.GetParameters(); // Get method parameter info
            foreach (ParameterInfo myParam in myParams) { TraceString += myParam.Name + ", "; } // Add parameter names to trace string
            if (TraceString.EndsWith(", ")) { TraceString = TraceString.Substring(0, (TraceString.Length - 2)); } // clean up trace string
            TraceString += ")"; // clean up trace string
            Debug.WriteLine(TraceString); // show trace string
        } // MethodBeginLogging

        /// <summary>
        /// Cleans up trace string.
        /// </summary>
        /// <param name="CurrentMethod">A MethodBase object representing the calling method.</param>
        private void MethodEndLogging(MethodBase CurrentMethod)
        {
            // Remove method name from end of trace string
            TraceString = TraceString.Substring(0, TraceString.LastIndexOf(@"|" + CurrentMethod.Name));
        } // MethodEndLogging

        #endregion // Logging

        // TODO: comment and move
        private void label5_Click(object sender, EventArgs e)
        {
            UpdateUserInterfaceStatus();
        }
    } // //frmMain

} // namespace Rumble2022