using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TanScreenshot
{
    public partial class HotKeySettings : Form
    {
        public Keys[] NewHotKey { get; private set; }
        private readonly GlobalKeyboardHook _hook;
        private readonly HashSet<Keys> _currentKeys = new HashSet<Keys>();
        private readonly System.Windows.Forms.Timer _debounceTimer;

        public HotKeySettings(Keys[] currentHotKey)
        {
            InitializeComponent();
            NewHotKey = currentHotKey ?? Array.Empty<Keys>();

            _hook = new GlobalKeyboardHook(null);
            _hook.KeyboardPressed += Hook_KeyboardPressed;

            _debounceTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _debounceTimer.Tick += DebounceTimer_Tick;

            UpdateKeyDisplay();
        }

        private void Hook_KeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown ||
                e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown)
            {
                if (!IsModifierKey(e.KeyboardData.Key) || !_currentKeys.Contains(e.KeyboardData.Key))
                {
                    _currentKeys.Add(e.KeyboardData.Key);
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                }
                e.Handled = true;
            }
            else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp ||
                     e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyUp)
            {
                _currentKeys.Remove(e.KeyboardData.Key);
            }
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            UpdateCombinationDisplay();
        }

        private void UpdateCombinationDisplay()
        {
            var orderedKeys = new List<Keys>();

            // Handle modifiers
            if (_currentKeys.Contains(Keys.LControlKey) || _currentKeys.Contains(Keys.RControlKey))
                orderedKeys.Add(Keys.Control);

            if (_currentKeys.Contains(Keys.LMenu) || _currentKeys.Contains(Keys.RMenu))
                orderedKeys.Add(Keys.Alt);

            if (_currentKeys.Contains(Keys.LShiftKey) || _currentKeys.Contains(Keys.RShiftKey))
                orderedKeys.Add(Keys.Shift);

            // Add non-modifier keys
            orderedKeys.AddRange(_currentKeys.Where(k => !IsModifierKey(k)));

            // Update both display and stored keys
            this.Invoke((MethodInvoker)delegate
            {
                NewHotKey = orderedKeys.Distinct().ToArray();
                lblHotKey.Text = NewHotKey.Length > 0 ? string.Join(" + ", NewHotKey.Select(KeyDisplayHelper.GetDisplayName)) : "No key selected";
            });
        }

        private void UpdateKeyDisplay()
        {
            lblHotKey.Text = NewHotKey.Length > 0 ? string.Join(" + ", NewHotKey.Select(KeyDisplayHelper.GetDisplayName)) : "No key selected";
        }

        private bool IsModifierKey(Keys key)
        {
            return key == Keys.LControlKey || key == Keys.RControlKey ||
                   key == Keys.LMenu || key == Keys.RMenu ||
                   key == Keys.LShiftKey || key == Keys.RShiftKey;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (NewHotKey.Length > 0 && NewHotKey.Any(k => !IsModifierKey(k)))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid combination! Must include at least one non-modifier key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _hook.Dispose();
            _debounceTimer.Dispose();
            base.OnFormClosing(e);
        }
    }
}