using Gadgeteer;
using Gadgeteer.Interfaces;
using Gadgeteer.Modules;
using Microsoft.SPOT.Hardware;


namespace Modules
{
    // http://www.dfrobot.com/wiki/index.php/I2C/TWI_LCD1602_Module_(Gadgeteer_Compatible)_(SKU:_DFR0063)
    // https://www.ghielectronics.com/community/codeshare/entry/177

    /// <summary>
    /// FEZ Driver for the DFRobot I2C/TWI LCD1602 Module
    /// http://www.dfrobot.com/index.php?route=product/product&path=53&product_id=135
    /// 
    /// This display uses a JHD 162A LCD module with a DFRobot I2C Module
    /// The I2C module uses a PCA8574 I/O Expander at Address 0x27
    /// http://www.nxp.com/documents/data_sheet/PCA8574_PCA8574A.pdf
    /// 
    /// Code is adapted from the arduino code: 
    /// http://www.dfrobot.com/image/data/DFR0063/Arduino_library.zip
    /// 
    /// The module should be connected to the I2C port on the FEZ - sda (Data2) and scl (Data3)
    /// 
    /// Refer to documentation on the Hitachi HD44780 for more detailed operational information
    /// Eg: http://lcd-linux.sourceforge.net/pdfdocs/lcd1.pdf
    /// </summary>
    public class CharacterDisplay : Module
    {
        // The following are the first 4 bits of each byte.

        const byte RS = 0x01;  // Register select bit. 0=command 1=data
        const byte RW = 0x02;  // Read/Write bit.  We usually want to write (0).
        const byte EN = 0x04;  // Enable bit. Data is set on the falling edge - see hitachi doco
        // flags for backlight control
        const byte LCD_BACKLIGHT = 0x08;
        const byte LCD_NOBACKLIGHT = 0x00;

        // The following are the high 4 bits - compounded with the flags below
        // Note that everything must be done in 4bit mode, so set 4bit mode first.

        // commands
        const byte LCD_CLEARDISPLAY = 0x01;
        const byte LCD_RETURNHOME = 0x02;
        const byte LCD_ENTRYMODESET = 0x04;
        const byte LCD_DISPLAYCONTROL = 0x08;
        const byte LCD_CURSORSHIFT = 0x10;
        const byte LCD_FUNCTIONSET = 0x20;
        const byte LCD_SETCGRAMADDR = 0x40;
        const byte LCD_SETDDRAMADDR = 0x80;

        // Flags to be used with the above commands

        // flags for display entry mode (0x04)
        const byte LCD_ENTRYRIGHT = 0x00;
        const byte LCD_ENTRYLEFT = 0x02;
        const byte LCD_ENTRYSHIFTINCREMENT = 0x01;
        const byte LCD_ENTRYSHIFTDECREMENT = 0x00;

        // flags for display on/off control (0x08)
        const byte LCD_DISPLAYON = 0x04;
        const byte LCD_DISPLAYOFF = 0x00;
        const byte LCD_CURSORON = 0x02;
        const byte LCD_CURSOROFF = 0x00;
        const byte LCD_BLINKON = 0x01;
        const byte LCD_BLINKOFF = 0x00;

        // flags for display/cursor shift (0x10)
        const byte LCD_DISPLAYMOVE = 0x08;
        const byte LCD_CURSORMOVE = 0x00;
        const byte LCD_MOVERIGHT = 0x04;
        const byte LCD_MOVELEFT = 0x00;

        // flags for function set (0x20)
        const byte LCD_8BITMODE = 0x10;
        const byte LCD_4BITMODE = 0x00;
        const byte LCD_2LINE = 0x08;
        const byte LCD_1LINE = 0x00;
        const byte LCD_5x10DOTS = 0x04;
        const byte LCD_5x8DOTS = 0x00;

        //private I2CDevice MyI2C;
        I2CBus _i2cBus;
        byte backLight = LCD_BACKLIGHT;

        byte[] _commandArray1;
        byte[] _commandArray2;
        byte[] _commandArray3;


        public CharacterDisplay(int socketNumber, ushort address = 0x27, int clockRateKhz = 400)
        {
            var socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('I', this);

            _i2cBus = new I2CBus(socket, address, clockRateKhz, this);

            // Set 4 Bit mode - copied from arduino code
            Write(LCD_FUNCTIONSET | LCD_8BITMODE);
            Write(LCD_FUNCTIONSET | LCD_8BITMODE);
            Write(LCD_FUNCTIONSET | LCD_8BITMODE);
            Write(LCD_FUNCTIONSET | LCD_4BITMODE);

            // COMMAND | FLAG1 | FLAG2 | ...
            WriteNibble(LCD_FUNCTIONSET | LCD_4BITMODE | LCD_2LINE | LCD_5x8DOTS);
            WriteNibble(LCD_DISPLAYCONTROL | LCD_DISPLAYON);

            // Screen may not be cleared after a reset
            Clear();
        }

        /// <summary>
        /// Writes a byte in 4bit mode, also known as a nibble (half a byte)
        /// </summary>
        /// <param name="byteOut">The byte to write</param>
        /// <param name="mode">Additional Parameters - eg RS for data mode</param>
        public void WriteNibble(byte byteOut, byte mode = 0)
        {
            Write((byte)(byteOut & 0xF0), mode);
            Write((byte)((byteOut << 4) & 0xF0), mode);
        }


        /// <summary>
        /// Writes a byte to the I2C LCD.
        /// </summary>
        /// <param name="byteOut">The byte to write</param>
        /// <param name="mode">Additional Parameters - eg RS for data mode</param>
        public void Write(byte byteOut, byte mode = 0)
        {
            I2CDevice.I2CTransaction[] xActions = new I2CDevice.I2CTransaction[3];
            // Write the byte
            _commandArray1 = new byte[] { (byte)(byteOut | backLight | mode) };
            xActions[0] = I2CDevice.CreateWriteTransaction(_commandArray1);
            // Set the En bit high
            _commandArray2 = new byte[] { (byte)(byteOut | backLight | mode | EN) };
            xActions[1] = I2CDevice.CreateWriteTransaction(_commandArray2);
            // Set the En bit low
            _commandArray3 = new byte[] { (byte)(byteOut | backLight | mode | ~EN) };
            xActions[2] = I2CDevice.CreateWriteTransaction(_commandArray3);

            // Write the commands (it seems to work without any delays added between calls).
            _i2cBus.Execute(xActions, 1000);
        }

        /// <summary>
        /// Prints text at current location
        /// </summary>
        /// <param name="text">String to print</param>
        public void PrintString(string text)
        {
            for (int i = 0; i < text.Length; i++)
                WriteNibble((byte)(text[i]), RS);
        }

        /// <summary>
        /// Clear screen and return to home
        /// </summary>
        public void Clear()
        {
            WriteNibble(LCD_CLEARDISPLAY);
            WriteNibble(LCD_RETURNHOME);
        }

        /// <summary>
        /// Sets the cursor position.  Zero based column and row.
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public void SetCursor(byte col, byte row)
        {
            byte[] row_offsets = { 0x00, 0x40, 0x14, 0x54 };
            WriteNibble((byte)(LCD_SETDDRAMADDR | (col + row_offsets[row])));
        }

        /// <summary>
        /// Turn the backlight off.
        /// </summary>
        public void TurnOffBacklight()
        {
            backLight = LCD_NOBACKLIGHT;
            Write(0);
        }

        /// <summary>
        /// Turn the backlight on.
        /// </summary>
        public void TurnOnBacklight()
        {
            backLight = LCD_BACKLIGHT;
            Write(0);
        }
    }

}
