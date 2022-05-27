using kbox.EnumClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kbox.Utils
{
    static public class EncodingConverter
    {
        static public byte[] ConvertByte(EncodingSelector endcode, string data)
        {

            Encoding UTF8 = Encoding.UTF8;
            Encoding UNICODE = Encoding.Unicode;
            Encoding EUCKR = Encoding.GetEncoding(51949);

            byte[] result;

            if (endcode == EncodingSelector.UTF8)
            {
                result = UTF8.GetBytes(data);
            }
            else if (endcode == EncodingSelector.UNICODE)
            {
                result = UNICODE.GetBytes(data);
            }
            else
            {
                result = EUCKR.GetBytes(data);
            }

            return result;
        }


        static public string ConvertString(EncodingSelector endcode, byte[] data)
        {

            Encoding UTF8 = Encoding.UTF8;
            Encoding UNICODE = Encoding.Unicode;
            Encoding EUCKR = Encoding.GetEncoding(51949);

            string result;

            if (endcode == EncodingSelector.UTF8)
            {
                result = UTF8.GetString(data);
            }
            else if (endcode == EncodingSelector.UNICODE)
            {
                result = UNICODE.GetString(data);
            }
            else
            {
                result = EUCKR.GetString(data);
            }

            return result;
        }

    }
}
