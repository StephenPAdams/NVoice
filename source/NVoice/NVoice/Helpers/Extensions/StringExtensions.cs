using System;
using System.Collections.Generic;

namespace NVoice.Helpers.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Will chop up the given string input into chunks up to the given maximum chunk size.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxChunkSize"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<string> ChunksUpTo(this String str, int maxChunkSize, int chunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(chunkSize, str.Length - i));
        }
    }
}
