/*
*   2021/09/17
*   Author: Diego Delgado
*
*   Example of use for the KeyCodeToUnicodeSnippet. 
*   It creates a Textbox and configure it to only accepts a MAC as input. 
*/   
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DDM_FormsControls
{
    public partial class MACTextBox : MaskedTextBox
    {
        /// <summary>
        /// Regular expression for Hex characters
        /// </summary>
        Regex rxHex = new Regex(@"^[a-fA-F0-9]$");

        /// <summary>
        /// Regular expression for Hex characters
        /// </summary>
        Regex rxPartialMAC = new Regex(@"^([a-fA-F0-9\s]{2}:){5}([a-fA-F0-9\s])?([a-fA-F0-9\s])?$");

        /// <summary>
        /// Regular expression for Hex characters
        /// </summary>
        Regex rxValidMAC = new Regex(@"^([a-fA-F0-9]{2}:){5}[a-fA-F0-9]{2}$");

        /// <summary>
        /// Hide the mask so it cannot be changed. 
        /// </summary>
        private new string Mask { get => base.Mask; }

        public MACTextBox()
        {
            base.Mask = "AA:AA:AA:AA:AA";
            KeyDown += CheckKeyPress;
            PromptChar = ' ';
            Font = new System.Drawing.Font("Consolas",base.Font.Size);
            Width = 90;                        
        }

        /// <summary>
        /// Chack the pressed key. If it's a delete key (back and delete) move the pointer if the character
        /// to delete is ':'. It won't delete it anyway, as it is in the mask, and will move the pointer to 
        /// the next or previous character. However, this way will make every delete press to actualy delete
        /// a character. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckKeyPress(object sender, KeyEventArgs e)
        {
            // Chack the pressed key. 
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    // Move selection start if we are about to delete ":"
                    if (SelectionLength == 0 && (SelectionStart + 1) % 3 == 0)
                    {
                        SelectionStart++;
                    }
                    break;
                case Keys.Back:
                    // Move selection start if we are about to delete ":"
                    if (SelectionLength == 0 && SelectionStart % 3 == 0)
                    {
                        SelectionStart--;
                    }
                    break;
                default:
                    // Otherwise, verify the pressed key is a hex character. 
                    var s = KeyCodeToUnicode(e.KeyCode);
                    if (s != "" && !rxHex.IsMatch(s))
                    {
                        // If it is not an hex character, ignore the supress the key press. 
                        e.SuppressKeyPress = true;
                    }
                    break;
            }
        }

        /// <summary>
        ///     Get the character or characters that the key will print after pressed. 
        /// </summary>
        /// <param name="key"> 
        ///     KeyCode of the key. <see cref="Keys"/>
        /// </param>
        /// <returns> 
        ///     Unicode character or characters that will be printed y key is pressed. 
        /// </returns>
        public string KeyCodeToUnicode(Keys key)
        {
            byte[] keyboardState = new byte[255];
            bool keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
            {
                return "";
            }

            uint virtualKeyCode = (uint)key;
            uint scanCode = MapVirtualKey(virtualKeyCode, 0);
            IntPtr inputLocaleIdentifier = GetKeyboardLayout(0);

            StringBuilder result = new StringBuilder();
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, (int)5, (uint)0, inputLocaleIdentifier);

            return result.ToString();
        }

        /// <summary>
        ///     Copies the status of the 256 virtual keys to the specified buffer.
        /// </summary>
        /// <remarks>
        ///     From https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getkeyboardstate ./>
        /// </remarks>
        /// <param name="lpKeyState">
        ///     The 256-byte array that receives the status data for each virtual key. 
        /// </param>
        /// <returns> 
        ///     If the function succeeds, the return value is nonzero. 
        /// </returns>
        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        /// <summary>
        ///     Translates (maps) a virtual-key code into a scan code or character value, or translates a scan code into
        ///     a virtual-key code.
        /// </summary>
        /// <param name="uCode"> 
        ///     The virtual key code or scan code for a key. How this value is interpreted depends on the value of the
        ///     uMapType parameter.
        /// </param>
        /// <param name="uMapType">
        ///     The translation to be performed. The value of this parameter depends on the value of the uCode parameter.
        /// </param>
        /// <returns>
        ///     The return value is either a scan code, a virtual-key code, or a character value, depending on the 
        ///     value of uCode and uMapType. If there is no translation, the return value is zero.    
        /// </returns>
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        /// <summary>
        ///     Retrieves the active input locale identifier (formerly called the keyboard layout).
        /// </summary>
        /// <param name="idThread"> 
        ///     The identifier of the thread to query, or 0 for the current thread. 
        /// </param>
        /// <returns>
        ///     The return value is the input locale identifier for the thread. 
        ///     The low word contains a Language Identifier for the input language and the high word contains a device
        ///     handle to the physical layout of the keyboard.
        /// </returns>
        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        /// <summary>
        ///     Translates the specified virtual-key code and keyboard state to the corresponding Unicode character or characters.
        /// </summary>
        /// <remarks>
        ///     From https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-tounicodeex.
        /// </remarks>
        /// <param name="wVirtKey">
        ///     The virtual-key code to be translated. 
        /// </param>
        /// <param name="wScanCode">
        ///     The hardware scan code of the key to be translated. <see cref="MapVirtualKey(uint, uint)"/> 
        /// </param>
        /// <param name="lpKeyState">
        ///     A pointer to a 256-byte array that contains the current keyboard state. <see cref="GetKeyboardState(byte[])"/> 
        /// </param>
        /// <param name="pwszBuff">
        ///     The buffer that receives the translated Unicode character or characters. 
        /// </param>
        /// <param name="cchBuff">
        ///     The size, in characters, of the buffer pointed to by the pwszBuff parameter.
        /// </param>
        /// <param name="wFlags">
        ///     The behavior of the function.
        ///     If bit 0 is set, a menu is active.
        ///     If bit 2 is set, keyboard state is not changed
        ///     All other bits(through 31) are reserved.
        /// </param>
        /// <param name="dwhkl">
        ///     The input locale identifier used to translate the specified code <see cref="GetKeyboardLayout(uint)"/>
        /// </param>
        /// <returns>
        ///     The function returns one of the following values:
        ///     -  -1:              The specified virtual key is a dead-key character (accent or diacritic)
        ///     -   0:              The specified virtual key has no translation for the current state of the keyboard.
        ///     -   1:              One character was written to the buffer specified by pwszBuff.
        ///     -   value >= 2      Two or more characters were written to the buffer specified by pwszBuff.
        /// </returns>
        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

    }
}
