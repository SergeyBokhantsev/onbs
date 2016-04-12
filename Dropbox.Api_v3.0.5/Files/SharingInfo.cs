// <auto-generated>
// Auto-generated by BabelAPI, do not modify.
// </auto-generated>

namespace Dropbox.Api.Files
{
    using sys = System;
    using col = System.Collections.Generic;
    using re = System.Text.RegularExpressions;

    using enc = Dropbox.Api.Babel;

    /// <summary>
    /// <para>Sharing info for a file or folder.</para>
    /// </summary>
    /// <seealso cref="FileSharingInfo" />
    /// <seealso cref="FolderSharingInfo" />
    public class SharingInfo
    {
        #pragma warning disable 108

        /// <summary>
        /// <para>The encoder instance.</para>
        /// </summary>
        internal static enc.StructEncoder<SharingInfo> Encoder = new SharingInfoEncoder();

        /// <summary>
        /// <para>The decoder instance.</para>
        /// </summary>
        internal static enc.StructDecoder<SharingInfo> Decoder = new SharingInfoDecoder();

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="SharingInfo" /> class.</para>
        /// </summary>
        /// <param name="readOnly">True if the file or folder is inside a read-only shared
        /// folder.</param>
        public SharingInfo(bool readOnly)
        {
            this.ReadOnly = readOnly;
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="SharingInfo" /> class.</para>
        /// </summary>
        /// <remarks>This is to construct an instance of the object when
        /// deserializing.</remarks>
        public SharingInfo()
        {
        }

        /// <summary>
        /// <para>True if the file or folder is inside a read-only shared folder.</para>
        /// </summary>
        public bool ReadOnly { get; protected set; }

        #region Encoder class

        /// <summary>
        /// <para>Encoder for  <see cref="SharingInfo" />.</para>
        /// </summary>
        private class SharingInfoEncoder : enc.StructEncoder<SharingInfo>
        {
            /// <summary>
            /// <para>Encode fields of given value.</para>
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="writer">The writer.</param>
            public override void EncodeFields(SharingInfo value, enc.IJsonWriter writer)
            {
                WriteProperty("read_only", value.ReadOnly, writer, enc.BooleanEncoder.Instance);
            }
        }

        #endregion


        #region Decoder class

        /// <summary>
        /// <para>Decoder for  <see cref="SharingInfo" />.</para>
        /// </summary>
        private class SharingInfoDecoder : enc.StructDecoder<SharingInfo>
        {
            /// <summary>
            /// <para>Create a new instance of type <see cref="SharingInfo" />.</para>
            /// </summary>
            /// <returns>The struct instance.</returns>
            protected override SharingInfo Create()
            {
                return new SharingInfo();
            }

            /// <summary>
            /// <para>Set given field.</para>
            /// </summary>
            /// <param name="value">The field value.</param>
            /// <param name="fieldName">The field name.</param>
            /// <param name="reader">The json reader.</param>
            protected override void SetField(SharingInfo value, string fieldName, enc.IJsonReader reader)
            {
                switch (fieldName)
                {
                    case "read_only":
                        value.ReadOnly = enc.BooleanDecoder.Instance.Decode(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        #endregion
    }
}
