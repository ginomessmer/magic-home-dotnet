using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MagicHome
{
    public class Light : IDisposable
    {
        #region Properties
        /// <summary>
        /// IP address of this light
        /// </summary>
        public string IpAddress => _address.ToString();

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
        /// The initial color of this light before a connection was established. Can be used to restore the color.
        /// </summary>
        public Color InitialColor { get; private set; }

        /// <summary>
        /// The initial power state of this light before a connection was established. true = on, false = off.
        /// </summary>
        public bool InitialPowerState { get; set; }

        /// <summary>
        /// Gets the light mode.
        /// </summary>
        public LightMode Mode { get; private set; }

        /// <summary>
        /// Specifies whether or not to append checksum to outgoing requests.
        /// </summary>
        public bool UseChecksum { get; set; } = true;

        /// <summary>
        /// The maximum timeout during read operations.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Indicates whether to refresh the internal properties state of this instance.
        /// </summary>
        public bool AutoRefreshEnabled { get; set; } = false;

        /// <summary>
        /// The interval for auto refresh. Default is 5 seconds.
        /// </summary>
        public TimeSpan AutoRefreshInterval { get; set; } = TimeSpan.FromSeconds(5);

        #endregion

        #region Fields
        /// <summary>
        /// Network socket to the light.
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// IP Address of the light.
        /// </summary>
        private IPAddress _address;

        /// <summary>
        /// Internal timer for refresh operations
        /// </summary>
        private Timer _autoRefreshTimer;
        #endregion

        #region Constants
        /// <summary>
        /// Magic Home's default port.
        /// </summary>
        public const int Port = 5577;

        /// <summary>
        /// Buffer size for socket communication.
        /// </summary>
        public const int BufferSize = 14;
        #endregion

        #region Constructors
        public Light()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public Light(IPAddress ipAddress) : this()
        {
            _address = ipAddress;
        }

        public Light(string ipAddress) : this(IPAddress.Parse(ipAddress))
        {
        }
        #endregion

        /// <summary>
        /// Connects to the light. You need to assign the IP address manually before you call this method.
        /// </summary>
        /// <returns></returns>
        public Task ConnectAsync() => ConnectAsync(this._address);

        /// <summary>
        /// Connects to the light with the specified IP address.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task ConnectAsync(IPAddress ipAddress)
        {
            _address = ipAddress;

            // TODO: Add retries?
            await _socket.ConnectAsync(_address, Port);

            if (!_socket.Connected)
                throw new LightConnectionException($"Not able to connect to light with IP address {_address}");

            // Refresh
            await RefreshAsync();

            // Set initial values
            InitialColor = Color;
            InitialPowerState = IsOn;

            // Initialize auto refresher
            _autoRefreshTimer = new Timer(async state =>
            {
                // Only refresh when it's enabled
                if (AutoRefreshEnabled)
                    await this.RefreshAsync();
            }, null, TimeSpan.Zero, AutoRefreshInterval);
        }

        /// <summary>
        /// Refreshes the internal state of this instance by gathering all necessary data from the light.
        /// </summary>
        public async Task RefreshAsync()
        {
            await SendAsync(0x81, 0x8a, 0x8b); // Send instruction to retrieve data later
            var result = await ReadAsync();
            
            var resultAsHex = result.Select(r => r.ToString("X")).ToArray(); // Convert to hex

            // Populate properties
            IsOn = DeterminePowerState(resultAsHex[2]); // Power state
            Mode = DetermineLightMode(resultAsHex[3]);
            Color = DetermineColor(Mode, result);
        }

        /// <summary>
        /// Restores this light to its initial state before any dirty changes have been made.
        /// </summary>
        /// <returns></returns>
        public async Task RestoreAsync()
        {
            await SetColorAsync(InitialColor);
            await SetPowerAsync(InitialPowerState);
        }

        #region I/O
        /// <summary>
        /// Reads data from the light
        /// </summary>
        /// <returns></returns>
        private Task<byte[]> ReadAsync()
        {
            return Task.Run(() =>
            {
                var buffer = new byte[BufferSize];
                _socket.ReceiveTimeout = Timeout.Milliseconds;

                var result = _socket.Receive(buffer);
                // TODO: Check result
                return buffer;
            });
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
        public Task SendAsync(IEnumerable<byte> bytes)
        {
            return Task.Run(() =>
            {
                if (UseChecksum)
                    bytes = CalculateChecksum(bytes.ToList());

                _socket.Send(bytes.ToArray());
            });
        }

        /// <summary>
        /// Applies the checksum to the given data.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static IEnumerable<byte> CalculateChecksum(IReadOnlyList<byte> bytes)
        {
            var packet = new List<byte>();
            byte checksum = 0;

            checksum = (byte) bytes.Sum(b => b); // checksum = 'sum of all byte elements in array'
            checksum &= 0xFF; // checksum = checksum AND 255

            packet.AddRange(bytes); // First things first, add our bytes
            packet.Add(checksum); // Then, append the checksum at the end

            return packet;
        }
        #endregion

        #region Operations
        #region Power
        /// <summary>
        /// Sets the power state of the light.
        /// </summary>
        /// <param name="isOn"></param>
        /// <returns></returns>
        public async Task SetPowerAsync(bool isOn)
        {
            var packet = isOn ? new byte[] {0x71, 0x23, 0x0f} : new byte[] {0x71, 0x24, 0x0f};
            
            await SendAsync(packet);
            IsOn = isOn;
        }

        /// <summary>
        /// Turns on the light.
        /// </summary>
        /// <returns></returns>
        public Task TurnOnAsync() => SetPowerAsync(true);

        /// <summary>
        /// Turns off the light.
        /// </summary>
        /// <returns></returns>
        public Task TurnOffAsync() => SetPowerAsync(false);
        #endregion

        /// <summary>
        /// Sets the colors for this light.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public async Task SetColorAsync(Color color)
        {
            await SendAsync(0x41, 
                color.R, color.G, color.B,
                0x00, 0x00, 0x0f);

            Color = color;
            Mode = LightMode.Color;
        }

        /// <summary>
        /// Sets the colors for this light.
        /// </summary>
        /// <param name="r">R</param>
        /// <param name="g">G</param>
        /// <param name="b">B</param>
        /// <returns></returns>
        public Task SetColorAsync(byte r, byte g, byte b) => SetColorAsync(Color.FromArgb(r, g, b));
        #endregion

        #region Utils
        private static bool DeterminePowerState(string hex) => hex == "23";

        private static LightMode DetermineLightMode(string hex)
        {
            // Check if it's color or custom
            switch (hex)
            {
                case "61":
                case "62":
                case "41":
                    return LightMode.Color;
                case "60":
                    return LightMode.Custom;
                case "2a":
                case "2b": 
                case "2c":
                case "2d":
                case "2e":
                case "2f":
                    return LightMode.Preset;
            }

            // Fallback: check if it's preset when it's in range
            if (int.TryParse(hex, out var result))
            {
                if (25 <= result && result <= 38)
                {
                    return LightMode.Preset;
                }
            }

            // Fallback
            return LightMode.Unknown;
        }

        private static Color DetermineColor(LightMode mode, byte[] data)
        {
            switch (mode)
            {
                case LightMode.Color:
                    return Color.FromArgb(data[6], data[7], data[8]);
                case LightMode.White:
                    return Color.White;
                default:
                    return Color.Transparent;
            }
        }
        #endregion

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}
