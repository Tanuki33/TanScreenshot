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
            // Map specific L/R keys to generic Control/Alt/Shift
            Keys currentKey = e.KeyboardData.Key;
            Keys mappedKey = currentKey;

            if (currentKey == Keys.LControlKey || currentKey == Keys.RControlKey)
                mappedKey = Keys.Control;
            else if (currentKey == Keys.LMenu || currentKey == Keys.RMenu)
                mappedKey = Keys.Alt;
            else if (currentKey == Keys.LShiftKey || currentKey == Keys.RShiftKey)
                mappedKey = Keys.Shift;

            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown ||
                e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown)
            {
                // Add the *mapped* key if it's not already there
                if (!_currentKeys.Contains(mappedKey))
                {
                    _currentKeys.Add(mappedKey);
                    _debounceTimer.Stop(); // Reset debounce timer on new key press
                    _debounceTimer.Start();
                }
                e.Handled = true; // Prevent further processing of the key press
            }
            else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp ||
                     e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyUp)
            {
                // Remove the *mapped* key on release
                _currentKeys.Remove(mappedKey);
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

            if (_currentKeys.Contains(Keys.Control))
                orderedKeys.Add(Keys.Control);
            if (_currentKeys.Contains(Keys.Alt))
                orderedKeys.Add(Keys.Alt);
            if (_currentKeys.Contains(Keys.Shift))
                orderedKeys.Add(Keys.Shift);

            orderedKeys.AddRange(_currentKeys.Where(k => !IsModifierKey(k)).OrderBy(k => k.ToString()));

            this.Invoke((MethodInvoker)delegate
            {
                NewHotKey = orderedKeys.ToArray();
                lblHotKey.Text = NewHotKey.Length > 0 ? string.Join(" + ", NewHotKey.Select(KeyDisplayHelper.GetDisplayName)) : "No key selected";
            });
        }

        private void UpdateKeyDisplay()
        {
            lblHotKey.Text = NewHotKey.Length > 0 ? string.Join(" + ", NewHotKey.Select(KeyDisplayHelper.GetDisplayName)) : "No key selected";
        }

        private bool IsModifierKey(Keys key)
        {
            return key == Keys.Control || key == Keys.LControlKey || key == Keys.RControlKey ||
                   key == Keys.Alt || key == Keys.LMenu || key == Keys.RMenu ||
                   key == Keys.Shift || key == Keys.LShiftKey || key == Keys.RShiftKey;
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