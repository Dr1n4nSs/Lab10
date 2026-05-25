namespace Компилятор
{
    public struct TextPosition
    {
        private uint _lineNumber;
        private byte _charNumber;

        public uint LineNumber
        {
            get
            {
                return _lineNumber;
            }
            set
            {
                _lineNumber = value;
            }
        }
        
        public byte CharNumber
        {
            get
            {
                return _charNumber;
            }
            set
            {
                _charNumber = value;
            }
        }
        
        public TextPosition(uint ln = 0, byte c = 0)
        {
            _lineNumber = ln;
            _charNumber = c;
        }
    }
}