using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elm327
{
    public static class BitHelper
    {
        public static byte[] FirstHexString(string[] response)
        {
            var firstHexString = AllHexStrings(response).FirstOrDefault();

            if (firstHexString != null)
            {
                return HexToBytes(firstHexString);
            }

            return null;
        }

        public static IEnumerable<string> AllHexStrings(IEnumerable<string> response)
        {
            if (response != null)
            {
                foreach (var line in response)
                {
                    if (IsHexString(line))
                    {
                        yield return line.Trim().Replace(" ", string.Empty);
                    }
                }
            }
            else
                yield break;
        }

        public static bool IsHexString(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (!(str[i] >= 48 && str[i] <= 57)
                    && !(str[i] >= 65 && str[i] <= 70)
                    && !(str[i] >= 97 && str[i] <= 102)
                    && str[i] != 32)
                    return false;
            }

            return true;
        }

        public static byte[] HexToBytes(string str)
        {
            str = str.Replace(" ", string.Empty);

            var ret = new byte[str.Length / 2];

            for (int i = 0; i < str.Length; i += 2)
            {
                ret[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }

            return ret;
        }
    }
}
