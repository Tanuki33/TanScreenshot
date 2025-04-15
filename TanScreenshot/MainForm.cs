using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;
using System.Threading;
using System.Media;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TanScreenshot
{
    public partial class MainForm : Form
    {
        private const string TaskName = "TanScreenshot";
        private GlobalKeyboardHook _hook;
        private string _screenshotDir;
        private AppConfig _config;
        private HashSet<Keys> _pressedKeys = new HashSet<Keys>();
        private volatile bool _processingScreenshot;
        private DateTime _lastScreenshotTime = DateTime.MinValue;
        private ContextMenuStrip trayMenu;
        private bool _isExiting = false;
        private bool _forceHidden = false;
        private static Mutex _mutex;


        public MainForm()
        {
            bool createdNew;
            _mutex = new Mutex(true, "TanScreenshot-Mutex", out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("Another instance is already running");
                Environment.Exit(0);
                return;
            }
            InitializeComponent();
            LoadConfiguration();
            InitializeConsoleBox();

            this.Text = "TanScreenshot 1.2";

            // Redirect console output
            Console.SetOut(new RichTextBoxWriter(consoleBox));

            // Initialize NotifyIcon
            InitializeNotifyIcon();

            // Check for /tray command-line argument instead of TaskExists()
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("/tray"))
            {
                _forceHidden = true; // Enable forced hidden state
            }

            // Handle form closing
            this.FormClosing += MainForm_FormClosing;
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = "TanScreenshot",
                Visible = true
            };
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open TanScreenshot", null, OpenMenuItem_Click);
            trayMenu.Items.Add("Exit", null, ExitMenuItem_Click);
            notifyIcon.ContextMenuStrip = trayMenu;
        }

        // Override to control initial visibility
        protected override void SetVisibleCore(bool value)
        {
            // Ensure handle is created before trying to hide/show
            if (!this.IsHandleCreated)
            {
                if (_forceHidden)
                {
                    base.CreateHandle();
                    _forceHidden = false;
                    base.SetVisibleCore(false);
                    this.BeginInvoke((System.Action)GoToTray); // Safely call GoToTray later
                }
                else
                {
                    base.SetVisibleCore(value);
                }
            }
            else // Handle exists, proceed normally
            {
                if (_forceHidden && !value) // Check if we intend to hide due to /tray
                {
                    _forceHidden = false;
                    base.SetVisibleCore(false);
                    GoToTray();
                }
                else
                {
                    base.SetVisibleCore(value);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_isExiting)
            {
                e.Cancel = true;
                GoToTray();
            }
            else
            {
                // Clean up resources on actual exit
                _hook?.Dispose();
                notifyIcon?.Dispose();
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
        }

        private void GoToTray()
        {
            this.Hide();
            if (notifyIcon != null)
            {
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(2000, "TanScreenshot", "Running in background.\nUse hotkey or double-click icon.", ToolTipIcon.Info);
            }
        }
        private void RestoreFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            _isExiting = true;
            this.Close();
        }

        // Centralized logging to consoleBox, handling cross-thread calls
        private void LogToConsole(string message)
        {
            if (consoleBox.InvokeRequired)
            {
                consoleBox.BeginInvoke((System.Action)(() => {
                    consoleBox.AppendText(message + Environment.NewLine);
                    consoleBox.ScrollToCaret();
                }));
            }
            else
            {
                consoleBox.AppendText(message + Environment.NewLine);
                consoleBox.ScrollToCaret();
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                _config = ConfigManager.LoadConfig();
                _screenshotDir = _config.ScreenshotDirectory;

                // Ensure directory exists or create it
                if (!string.IsNullOrWhiteSpace(_screenshotDir))
                {
                    try
                    {
                        if (!Directory.Exists(_screenshotDir))
                        {
                            Directory.CreateDirectory(_screenshotDir);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogToConsole($"[{DateTime.Now:T}] WARNING: Could not create screenshot directory '{_screenshotDir}'. Using default. Error: {ex.Message}");
                        // Fallback to a default directory if creation fails
                        _screenshotDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "TanScreenshots");
                        _config.ScreenshotDirectory = _screenshotDir; // Update config potentially
                        Directory.CreateDirectory(_screenshotDir); // Try creating default
                    }
                }
                else // If config directory is empty, use default
                {
                    _screenshotDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "TanScreenshots");
                    _config.ScreenshotDirectory = _screenshotDir;
                    Directory.CreateDirectory(_screenshotDir);
                }


                var keys = ConfigManager.ConvertToKeys(_config.HotKeys);
                if (keys == null || keys.Length == 0)
                {
                    keys = new[] { Keys.PrintScreen };
                    _config.HotKeys = new List<string> { Keys.PrintScreen.ToString() };
                    ConfigManager.SaveConfig(_config); // Save the default back
                    LogToConsole($"[{DateTime.Now:T}] WARN: Invalid hotkey in config, reset to default: {KeyDisplayHelper.GetDisplayName(Keys.PrintScreen)}");
                }
                lblActiveHotKey.Text = string.Join(" + ", keys.Select(KeyDisplayHelper.GetDisplayName));

                // Temporarily remove event handlers
                // So it not triggering the checkbox changed event
                cbAutorun.CheckedChanged -= cbAutorun_CheckedChanged;
                cbAutorun.Checked = TaskExists();
                cbAutorun.CheckedChanged += cbAutorun_CheckedChanged;

                cbCopy.CheckedChanged -= cbCopy_CheckedChanged;
                cbCopy.Checked = _config.CopyToClipboard;
                cbCopy.CheckedChanged += cbCopy_CheckedChanged;

                SetupKeyboardHook(keys);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}\n\nApplication might not function correctly.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblActiveHotKey.Text = "Error";
            }
        }

        private void SetupKeyboardHook(Keys[] keys)
        {
            try
            {
                _hook?.Dispose();
                _hook = new GlobalKeyboardHook(keys);
                _hook.KeyboardPressed += OnPrintScreenPressed;
                LogToConsole($"[{DateTime.Now:T}] Keyboard hook activated for: {string.Join(" + ", keys.Select(KeyDisplayHelper.GetDisplayName))}");
            }
            catch (Exception ex)
            {
                LogToConsole($"[{DateTime.Now:T}] FATAL: Failed to set keyboard hook: {ex.Message}");
                MessageBox.Show($"Failed to set keyboard hook: {ex.Message}\n\nScreenshot via hotkey will not work.", "Hook Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnChangeHotkey.Enabled = false;
                lblActiveHotKey.Text = "Hook Failed";
            }
        }

        private void InitializeConsoleBox()
        {
            consoleBox.BackColor = Color.Black;
            consoleBox.ForeColor = Color.LimeGreen;
            consoleBox.Font = new Font("Consolas", 9.5f);
            consoleBox.Dock = DockStyle.Fill;
            consoleBox.ReadOnly = true;
            consoleBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            consoleBox.HideSelection = false;
            consoleBox.DetectUrls = true; // Make file paths clickable (might need custom handling not implemented yet)
        }

        private void OnPrintScreenPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            // Prevent processing if the form doesn't have a handle or is disposing
            if (!this.IsHandleCreated || this.IsDisposed || this.Disposing) return;

            try
            {
                if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
                {
                    _pressedKeys.Add(e.KeyboardData.Key);
                    bool triggerMet = !_processingScreenshot && _hook.RegisteredKeys.All(k => _pressedKeys.Contains(k));

                    if (triggerMet)
                    {
                        // --- Debounce Check ---
                        if ((DateTime.Now - _lastScreenshotTime).TotalMilliseconds < 500)
                        {
                            return; // Exit if too soon
                        }
                        _lastScreenshotTime = DateTime.Now;

                        _processingScreenshot = true;
                        e.Handled = true;

                        // Clear tracked keys as soon as it recognizes a hotkey combination.
                        // This prevents subsequent unrelated KeyDown events from accidentally happening
                        // Matches stale hotkey states if a KeyUp event is missed or delayed.
                        // In general this fixes any key trigger screenshots after the first screenshot is taken
                        // After Windows starts 
                        _pressedKeys.Clear();

                        // Run the screenshot capture and save on a background thread
                        System.Threading.Tasks.Task.Run(() => CaptureAndSaveScreenshot())
                            .ContinueWith(task =>
                            {
                                if (task.IsFaulted)
                                {
                                    LogToConsole($"[{DateTime.Now:T}] ASYNC SCREENSHOT ERROR: {task.Exception?.InnerException?.Message ?? task.Exception?.Message}");
                                }
                                _processingScreenshot = false;
                            }, TaskScheduler.Default);
                    }
                }
                else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
                {
                    // Remove the key when it's released.
                    // This remains important for scenarios involving modifier keys (Ctrl, Shift, Alt)
                    // and ensures keys aren't left lingering if the KeyDown didn't trigger a screenshot.
                    _pressedKeys.Remove(e.KeyboardData.Key);
                }
            }
            catch (Exception ex)
            {
                LogToConsole($"[{DateTime.Now:T}] Error in OnPrintScreenPressed: {ex.Message}");
                _processingScreenshot = false;
                _pressedKeys.Clear(); // Clear keys as state might be corrupt
            }
        }

        private void CaptureAndSaveScreenshot()
        {
            string filePath = "";
            Bitmap bmpForClipboard = null;

            try
            {
                // Ensure the screenshot directory exists (thread-safe check)
                // It's better to ensure this at startup/config load, but check again just in case
                if (!Directory.Exists(_screenshotDir))
                {
                    try { Directory.CreateDirectory(_screenshotDir); } catch { /* Ignore secondary check error */ }
                }

                // Capture the screenshot
                using (var bmp = new Bitmap(
                    SystemInformation.VirtualScreen.Width,
                    SystemInformation.VirtualScreen.Height,
                    PixelFormat.Format32bppArgb)) // Specify pixel format
                {
                    using (var graphics = Graphics.FromImage(bmp))
                    {
                        graphics.CopyFromScreen(
                            SystemInformation.VirtualScreen.X,
                            SystemInformation.VirtualScreen.Y,
                            0, 0,
                            SystemInformation.VirtualScreen.Size,
                            CopyPixelOperation.SourceCopy
                        );
                    } // Dispose graphics
                    filePath = GetNewFilePath();

                    // Save the screenshot to a file
                    bmp.Save(filePath, ImageFormat.Png);

                    if (_config.CopyToClipboard)
                    {
                        // Clone requires the source bitmap not to be disposed yet
                        bmpForClipboard = (Bitmap)bmp.Clone();
                    }

                } // Dispose original bmp
                LogToConsole($"[{DateTime.Now:T}] Screenshot saved to: {filePath}");

                // Handle clipboard operation if enabled and clone was successful
                if (bmpForClipboard != null)
                {
                    // Clipboard operations MUST be done on the UI thread.
                    this.Invoke((System.Action)(() =>
                    {
                        try
                        {
                            SetClipboardImageWithRetry(bmpForClipboard);
                            LogToConsole($"[{DateTime.Now:T}] Screenshot copied to clipboard.");
                        }
                        catch (Exception ex)
                        {
                            LogToConsole($"[{DateTime.Now:T}] ERROR copying to clipboard: {ex.Message}");
                        }
                        finally
                        {
                            bmpForClipboard.Dispose();
                        }
                    }));
                }
                // Play screenshot sound
                this.BeginInvoke((System.Action)(() => PlayShutterSound()));

                LogToConsole(new string('-', 80));
            }
            catch (Exception ex)
            {
                LogToConsole($"[{DateTime.Now:T}] ERROR during screenshot capture/save: {ex.Message}{(string.IsNullOrEmpty(filePath) ? "" : $" (Path: {filePath})")}");
                bmpForClipboard?.Dispose();
            }
        }

        private void SetClipboardImageWithRetry(Bitmap image)
        {
            // Clipboard can be locked by other processes. Retry a few times.
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Clipboard.SetImage(image);
                    return;
                }
                catch (ExternalException)
                {
                    if (i == 2) throw;
                    Thread.Sleep(100);
                }
            }
        }

        private void PlayShutterSound()
        {
            try
            {
                using (SoundPlayer player = new SoundPlayer(Properties.Resources.sfx))
                {
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                LogToConsole($"[{DateTime.Now:T}] WARN: Could not play shutter sound: {ex.Message}");
            }
        }

        private string GetNewFilePath()
        {
            // Ensure directory exists shortly before saving, handle potential race conditions gracefully
            try
            {
                if (!Directory.Exists(_screenshotDir))
                {
                    Directory.CreateDirectory(_screenshotDir);
                }
                return Path.Combine(_screenshotDir, $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmssfff}.png");
            }
            catch (Exception ex)
            {
                LogToConsole($"[{DateTime.Now:T}] ERROR accessing screenshot directory '{_screenshotDir}': {ex.Message}. Saving to Desktop.");
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                return Path.Combine(desktopPath, $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmssfff}.png");
            }
        }

        private class RichTextBoxWriter : TextWriter
        {
            private readonly RichTextBox _consoleBox;
            private readonly StringBuilder _buffer = new StringBuilder();

            public RichTextBoxWriter(RichTextBox consoleBox)
            {
                _consoleBox = consoleBox;
                Encoding = Encoding.UTF8;
            }

            public override Encoding Encoding { get; }

            public override void Write(char value)
            {
                _buffer.Append(value);
                if (value == '\n')
                {
                    Flush();
                }
            }

            public override void Write(string value)
            {
                _buffer.Append(value);
                Flush();
            }

            public override void Flush()
            {
                if (_consoleBox.InvokeRequired)
                {
                    _consoleBox.BeginInvoke((System.Action)(() =>
                    {
                        _consoleBox.AppendText(_buffer.ToString());
                        _consoleBox.ScrollToCaret();
                    }));
                }
                else
                {
                    _consoleBox.AppendText(_buffer.ToString());
                    _consoleBox.ScrollToCaret();
                }
                _buffer.Clear();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            _isExiting = true;
            Application.Exit();
        }

        private void btnChangeHotkey_Click(object sender, EventArgs e)
        {
            if (_hook == null)
            {
                MessageBox.Show("Keyboard hook is not active. Cannot change hotkey.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Pass the *currently registered* keys to the settings form
            Keys[] currentKeys = _hook.RegisteredKeys ?? new Keys[0];

            using (var settingsForm = new HotKeySettings(currentKeys))
            {
                if (settingsForm.ShowDialog(this) == DialogResult.OK) // Set owner window
                {
                    Keys[] newKeys = settingsForm.NewHotKey;
                    if (newKeys != null && newKeys.Length > 0 && !newKeys.SequenceEqual(currentKeys)) // Check if changed
                    {
                        try
                        {
                            _config.HotKeys = newKeys.Select(k => k.ToString()).ToList();
                            ConfigManager.SaveConfig(_config);
                            SetupKeyboardHook(newKeys); // This will dispose the old hook and create a new one
                            lblActiveHotKey.Text = string.Join(" + ", newKeys.Select(KeyDisplayHelper.GetDisplayName));

                            LogToConsole($"[{DateTime.Now:T}] Hotkey updated to: {lblActiveHotKey.Text}");
                            LogToConsole(new string('-', 80));
                        }
                        catch (Exception ex)
                        {
                            LogToConsole($"[{DateTime.Now:T}] ERROR updating hotkey: {ex.Message}");
                            MessageBox.Show($"Failed to apply new hotkey: {ex.Message}", "Hotkey Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private bool TaskExists()
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    return ts.GetTask(TaskName) != null;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                LogToConsole($"[{DateTime.Now:T}] Access denied checking scheduled task: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                LogToConsole($"[{DateTime.Now:T}] Error checking scheduled task: {ex.Message}");
                return false;
            }
        }

        private void CreateAutorunTask()
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Starts TanScreenshot minimized in the system tray at user logon.";
                    td.RegistrationInfo.Author = "TanScreenshot";

                    td.Triggers.Add(new LogonTrigger { UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name });

                    // Run with highest privileges is often needed if app interacts with system/other apps
                    td.Principal.RunLevel = TaskRunLevel.Highest; // This is appropriate since the app requires admin
                    td.Settings.ExecutionTimeLimit = TimeSpan.Zero; // No time limit
                    td.Settings.StartWhenAvailable = true; // Run task as soon as possible after a scheduled start is missed

                    string exePath = Application.ExecutablePath;
                    td.Actions.Add(new ExecAction(exePath, "/tray", Path.GetDirectoryName(exePath))); // Set working directory

                    // Register the task, overwriting if it exists
                    ts.RootFolder.RegisterTaskDefinition(TaskName, td, TaskCreation.CreateOrUpdate, null, null);

                    LogToConsole($"[{DateTime.Now:T}] Autorun scheduled task created/updated successfully.");
                    LogToConsole($"[{DateTime.Now:T}] TanScreenshot will run minimized on next logon.");
                    LogToConsole(new string('-', 80));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Even as admin, other issues might deny access (e.g., policy, corrupted store)
                LogToConsole($"[{DateTime.Now:T}] ERROR: Access denied creating scheduled task (even as admin): {ex.Message}");
                MessageBox.Show($"Access denied when creating the scheduled task, even though running as administrator. Check Task Scheduler permissions or logs.\n\nError: {ex.Message}", "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert checkbox state
                cbAutorun.CheckedChanged -= cbAutorun_CheckedChanged;
                cbAutorun.Checked = false;
                cbAutorun.CheckedChanged += cbAutorun_CheckedChanged;
            }
            catch (Exception ex)
            {
                LogToConsole($"[{DateTime.Now:T}] Error creating scheduled task: {ex.Message}");
                MessageBox.Show($"An error occurred while creating the autorun task: {ex.Message}", "Task Scheduler Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert checkbox state
                cbAutorun.CheckedChanged -= cbAutorun_CheckedChanged;
                cbAutorun.Checked = false;
                cbAutorun.CheckedChanged += cbAutorun_CheckedChanged;
            }
        }

        private void DeleteAutorunTask()
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    if (ts.GetTask(TaskName) != null)
                    {
                        ts.RootFolder.DeleteTask(TaskName);
                        LogToConsole($"[{DateTime.Now:T}] Autorun scheduled task deleted successfully.");
                    }
                    else
                    {
                        LogToConsole($"[{DateTime.Now:T}] No autorun scheduled task found to delete.");
                    }
                    LogToConsole(new string('-', 80));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Even as admin, other issues might deny access (e.g., policy, corrupted store)
                LogToConsole($"[{DateTime.Now:T}] ERROR: Access denied deleting scheduled task (even as admin): {ex.Message}");
                MessageBox.Show($"Access denied when deleting the scheduled task, even though running as administrator. Check Task Scheduler permissions or logs.\n\nError: {ex.Message}", "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert checkbox state
                cbAutorun.CheckedChanged -= cbAutorun_CheckedChanged;
                cbAutorun.Checked = true; // Assume it couldn't be deleted
                cbAutorun.CheckedChanged += cbAutorun_CheckedChanged;
            }
            catch (Exception ex)
            {
                LogToConsole($"[{DateTime.Now:T}] Error deleting scheduled task: {ex.Message}");
                MessageBox.Show($"An error occurred while deleting the autorun task: {ex.Message}", "Task Scheduler Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert checkbox state
                cbAutorun.CheckedChanged -= cbAutorun_CheckedChanged;
                cbAutorun.Checked = true; // Keep checked as deletion failed
                cbAutorun.CheckedChanged += cbAutorun_CheckedChanged;
            }
        }

        private void cbAutorun_CheckedChanged(object sender, EventArgs e)
        {
            bool desiredState = cbAutorun.Checked;
            if (desiredState)
            {
                CreateAutorunTask();
            }
            else
            {
                DeleteAutorunTask();
            }
            bool actualState = TaskExists();
            if (cbAutorun.Checked != actualState)
            {
                cbAutorun.CheckedChanged -= cbAutorun_CheckedChanged; // Prevent recursion
                cbAutorun.Checked = actualState;
                cbAutorun.CheckedChanged += cbAutorun_CheckedChanged;
            }
        }

        private void cbCopy_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = cbCopy.Checked;
            _config.CopyToClipboard = isChecked;
            ConfigManager.SaveConfig(_config); // Save immediately
            LogToConsole($"[{DateTime.Now:T}] Setting 'Copy To Clipboard' changed to: {isChecked}");
            LogToConsole(new string('-', 80));
        }

    }
}