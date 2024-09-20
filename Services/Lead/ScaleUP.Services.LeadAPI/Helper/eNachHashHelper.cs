using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace ScaleUP.Services.LeadAPI.Helper
{
    public class eNachHashHelper
    {
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // var ticks = DateTime.Now.Ticks;
            input += Guid.NewGuid().ToString();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GenerateChecksum(string json)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(json));
                return BitConverter.ToString(bytes);
            }
        }

        public static string HashString(string text)
        {
            const string chars = "0234589ABCDEFGHOPQRTUWXYZ";
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);

            char[] hash2 = new char[16];

            // Note that here we are wasting bits of hash! 
            // But it isn't really important, because hash.Length == 32
            for (int i = 0; i < hash2.Length; i++)
            {
                hash2[i] = chars[hash[i] % chars.Length];
            }

            return new string(hash2);
        }

    }
    //public class FingerprintBuilder
    //{
    //    public static FingerprintBuilder<T> Create<T>(Func<byte[], byte[]> computeHash, T obj)
    //    {
    //        return new FingerprintBuilder<T>(computeHash);
    //    }
    //}
    public class FingerprintBuilder<T>
    {
        private readonly List<(string MemberName, Func<T, object> Fingerprint)> _fingerprints;

        public FingerprintBuilder()
        {
            _fingerprints = new List<(string Expression, Func<T, object> Fingerprint)>();
        }

        public static FingerprintBuilder<T> Empty => new FingerprintBuilder<T>();

        public FingerprintBuilder<T> For<TProperty>(Expression<Func<T, TProperty>> memberExpression, Expression<Func<TProperty, TProperty>> fingerprint)
        {
            var getProperty = memberExpression.Compile();
            var getFingerprint = fingerprint.Compile();
            _fingerprints.Add((
                ((MemberExpression)memberExpression.Body).Member.Name,
                (Func<T, object>)(obj => getFingerprint(getProperty(obj)))
            ));
            return this;
        }

        //public Func<T, byte[]> Build(Func<byte[], byte[]> computeHash)
        //{
        //    return obj =>
        //    {
        //        var buffer = new List<byte>();

        //        var binaryFormatter = new BinaryFormatter();
        //        foreach (var t in _fingerprints.OrderBy(e => e.MemberName, StringComparer.OrdinalIgnoreCase))
        //        {
        //            var fingerprint = t.Fingerprint(obj);
        //            using (var memory = new MemoryStream())
        //            {
        //                binaryFormatter.Serialize(memory, fingerprint);
        //                buffer.AddRange(memory.ToArray());
        //            }
        //        }

        //        return computeHash(buffer.ToArray());
        //    };
        //}
    }


    [Flags]
    public enum StringOptions
    {
        None = 0,
        IgnoreCase = 1 << 0,
        IgnoreWhitespace = 1 << 1,
    }

    public static class SHA1
    {

        public static byte[] ComputeHash(byte[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            using (var sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(source);
            }
        }
    }

    public static class SHA256
    {
        public static byte[] ComputeHash(byte[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            using (var sha256 = new SHA256Managed())
            {
                return sha256.ComputeHash(source);
            }
        }
    }

    public static class ByteExtensions
    {

        public static string ToHexString(this byte[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return
                source
                    .Aggregate(new StringBuilder(), (current, next) => current.Append(next.ToString("X2")))
                    .ToString();
        }

        // This cannot be ToString because the compiler picks the wrong method otherwise.

        public static string GetString(this byte[] source, Encoding encoding = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return (encoding ?? Encoding.UTF8).GetString(source);
        }

        //[ContractAnnotation("source: null => null; source: notnull => notnull")]
        public static string ToBase64String(this byte[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return Convert.ToBase64String(source);
        }
    }
    public static class StringExtensions
    {

        public static byte[] ToBytes(this string source, Encoding encoding = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return (encoding ?? Encoding.UTF8).GetBytes(source);
        }
    }
}
