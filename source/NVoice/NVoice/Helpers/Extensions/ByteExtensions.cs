using System;
using System.Text;

namespace NVoice.Helpers.Extensions
{
    public static class ByteExtensions
    {
        /// <summary>
        /// Converts the byte array to an UTF8 string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String ToStringContent(this byte[] value)
        {
            return Encoding.UTF8.GetString(value, 0, value.Length);
        }
    }
}
