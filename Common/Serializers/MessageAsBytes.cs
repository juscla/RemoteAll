namespace RemoteCore
{
    using System;

    public struct MessageAsBytes
    {
        /// <summary>
        /// The length of size as bytes
        /// </summary>
        private const int LengthOfSizeAsBytes = sizeof(int);

        /// <summary>
        /// The size of the data.
        /// </summary>
        private readonly int dataSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageAsBytes"/> struct.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="count">The count.</param>
        public MessageAsBytes(byte[] data, int count = 0)
        {
            this.Data = data;
            this.dataSize = count > 0 ? count : this.Data.Length;
        }

        /// <summary>
        /// Gets the raw data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; }

        /// <summary>
        /// Converts the Byte[] into the message format as an the array.
        /// </summary>
        /// <returns></returns>
        public byte[] AsArray()
        {
            if (Data == null)
                return new byte[0];

            // conver the size into its byte representation.
            var sizeinBytes = BitConverter.GetBytes(this.dataSize);

            // create a buffer of the correct Size.
            // we need to add the Size bytes [4 bytes for int] and the the expected Size.
            var buffer = new byte[LengthOfSizeAsBytes + this.dataSize];

            // copy the size bytes into the first buffer.
            Buffer.BlockCopy(sizeinBytes, 0, buffer, 0, LengthOfSizeAsBytes);

            // copy the data into the buffer with an offset of the 4 bytes [the first 4 bytes represent the packet size]. 
            Buffer.BlockCopy(Data, 0, buffer, LengthOfSizeAsBytes, this.dataSize);

            // return the buffer. 
            return buffer;
        }
    }
}
