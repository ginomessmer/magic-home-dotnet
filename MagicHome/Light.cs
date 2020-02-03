using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MagicHome
{
    public class Light
    {
        /// <summary>
        /// Determines whether this instance is connected to the Light.
        /// </summary>
        public bool Connected => _socket.Connected;

        /// <summary>
        /// Gets whether the light is on or off.
        /// </summary>
        public bool IsOn { get; private set; }

        /// <summary>
        /// Gets the current color of the light.
        /// </summary>
        public Color Color { get; private set; }

        /// <summary>
        /// Specifies whether or not to append checksum to outgoing requests.
        /// </summary>
        public bool UseChecksum { get; set; } = true;

        /// <summary>
        /// The maximum timeout during read operations.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Network socket to the light.
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// Magic Home's default port.
        /// </summary>
        public const int Port = 5577;

        public Light()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task ConnectAsync(IPAddress address)
        {
            // TODO: Add retries?
            await _socket.ConnectAsync(address, Port);

            if (!_socket.Connected)
                throw new LightConnectionException($"Not able to connect to light with IP address {address.ToString()}");

            await UpdateAsync();
        }

        /// <summary>
        /// Updates the internal state of this instance by gathering all necessary data from the light.
        /// </summary>
        public async Task UpdateAsync()
        {
            await SendAsync(0x81, 0x8a, 0x8b); // Send instruction to retrieve data later
            var result = await ReadAsync();

            // TODO:
        }

        /// <summary>
        /// Reads data from the light
        /// </summary>
        /// <returns></returns>
        private async Task<byte[]> ReadAsync()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Sends data to the light.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public Task SendAsync(params byte[] bytes) => SendAsync(bytes.ToList());

        /// <summary>
        /// Sends data to the light.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task SendAsync(IEnumerable<byte> bytes)
        {
            if (UseChecksum)
                bytes = ApplyChecksum(bytes.ToList());

            _socket.Send(bytes.ToArray());
        }

        public static IEnumerable<byte> ApplyChecksum(IReadOnlyList<byte> bytes)
        {
            var packet = new List<byte>();
            byte checksum = 0;

            checksum = Convert.ToByte(packet.Sum(b => b)); // checksum = 'sum of all byte elements in array'
            checksum = Convert.ToByte(checksum & 0xFF); // checksum = checksum AND 255

            packet.AddRange(bytes); // First things first, add our bytes
            packet.Add(checksum); // Then, append the checksum at the end

            return packet;
        }
    }
}
