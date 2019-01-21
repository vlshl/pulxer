using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulxer
{
    public struct AllTradesTick
    {
        public uint Second;
        public decimal Price;
        public int Lots;
    }

    public class AllTradesEncoding
    {
        const string VERSION0 = "AllTrades 1.0   ";
        const string VERSION1 = "AllTrades 1.1   ";
        const int SIG_SIZE = 16;
        const int VERSION0_TIMECORRECT = -3600; // 1 h

        private int _bufferIndex;
        private byte[] _buffer;
        private byte[] _ver0Sig;
        private byte[] _ver1Sig;
        private int _version;
        private int _k;
        private List<AllTradesTick> _ticks;
        private byte[] _tickBuffer;
        private byte _tickBufferCount;

        public AllTradesEncoding(int decimals)
        {
            _k = 1;
            switch (decimals)
            {
                case 0: _k = 1; break;
                case 1: _k = 10; break;
                case 2: _k = 100; break;
                case 3: _k = 1000; break;
                case 4: _k = 10000; break;
                case 5: _k = 100000; break;
                case 6: _k = 1000000; break;
                case 7: _k = 10000000; break;
                case 8: _k = 100000000; break;
                case 9: _k = 1000000000; break;
                default: break;
            }

            _ver0Sig = new ASCIIEncoding().GetBytes(VERSION0);
            _ver1Sig = new ASCIIEncoding().GetBytes(VERSION1);
            _version = -1;
            _ticks = new List<AllTradesTick>();
            _tickBuffer = new byte[32];
        }

        #region Decode
        /// <summary>
        /// Раскодирование бинарных данных в массив тиков
        /// </summary>
        /// <param name="buffer">Бинарные данные в формате AllTrades</param>
        /// <returns>Раскодированный массив тиков или null</returns>
        public IEnumerable<AllTradesTick> Decode(byte[] buffer)
        {
            if (buffer == null) return null;

            _buffer = buffer;
            _bufferIndex = 0;

            uint curSecond = 0;
            int curPrice = 0;

            int shiftIndex = DecodeHeader();
            if (shiftIndex == 0) return null;

            _bufferIndex += shiftIndex;
            List<AllTradesTick> ticks = new List<AllTradesTick>();

            bool isDelta = false;
            uint sec = 0;
            int price = 0;
            int lots = 0;

            while (true)
            {
                AllTradesTick tick = new AllTradesTick();

                shiftIndex = DecodeTime(out sec, out isDelta);
                if (shiftIndex == 0) break;
                _bufferIndex += shiftIndex;

                if (isDelta)
                    curSecond += sec;
                else
                    curSecond = sec;
                if (_version == 0)
                {
                    tick.Second = (uint)((int)curSecond + VERSION0_TIMECORRECT);
                }
                else
                {
                    tick.Second = curSecond;
                }

                shiftIndex = DecodePrice(out price, out isDelta);
                if (shiftIndex == 0) break;
                _bufferIndex += shiftIndex;

                if (isDelta)
                    curPrice += price;
                else
                    curPrice = price;
                tick.Price = (decimal)curPrice / _k;

                shiftIndex = DecodeLots(out lots);
                if (shiftIndex == 0) break;
                _bufferIndex += shiftIndex;

                tick.Lots = lots;
                ticks.Add(tick);
            }

            return ticks;
        }

        private int DecodeHeader()
        {
            if (_bufferIndex + SIG_SIZE > _buffer.Length) return 0;

            byte[] verSig = new byte[SIG_SIZE];
            Array.Copy(_buffer, _bufferIndex, verSig, 0, SIG_SIZE);
            if (IsEqual(verSig, _ver0Sig)) _version = 0;
            else if (IsEqual(verSig, _ver1Sig)) _version = 1;
            if (_version < 0) return 0;

            return SIG_SIZE;
        }

        private bool IsEqual(byte[] arr1, byte[] arr2)
        {
            if (arr1 == null && arr2 == null) return true;
            if (arr1 == null || arr2 == null) return false;
            if (arr1.Length != arr2.Length) return false;

            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i]) return false;
            }

            return true;
        }

        private int DecodeTime(out uint sec, out bool isDelta)
        {
            sec = 0;
            isDelta = false;
            if (_bufferIndex + 1 > _buffer.Length) return 0;

            if (_buffer[_bufferIndex] == 0xff)
            {
                if (_bufferIndex + 5 > _buffer.Length) return 0;
                sec = BitConverter.ToUInt32(_buffer, _bufferIndex + 1);
                return 5;
            }
            else
            {
                sec = (uint)_buffer[_bufferIndex];
                isDelta = true;
                return 1;
            }
        }

        private int DecodePrice(out int price, out bool isDelta)
        {
            price = 0;
            isDelta = false;
            if (_bufferIndex + 1 > _buffer.Length) return 0;

            if (_buffer[_bufferIndex] == 0xff)
            {
                if (_bufferIndex + 5 > _buffer.Length) return 0;
                price = BitConverter.ToInt32(_buffer, _bufferIndex + 1);
                return 5;
            }
            else
            {
                int c = (int)_buffer[_bufferIndex];
                if (c <= 127)
                {
                    price = c;
                }
                else
                {
                    price = 127 - c;
                }

                isDelta = true;
                return 1;
            }
        }

        private int DecodeLots(out int lots)
        {
            lots = 0;
            if (_bufferIndex + 1 > _buffer.Length) return 0;

            if (_buffer[_bufferIndex] == 0xff)
            {
                if (_bufferIndex + 5 > _buffer.Length) return 0;
                lots = BitConverter.ToInt32(_buffer, _bufferIndex + 1);
                return 5;
            }
            else
            {
                lots = (int)_buffer[_bufferIndex];
                return 1;
            }
        }
        #endregion

        #region Encode
        public void AddTick(uint second, decimal price, int lots)
        {
            _ticks.Add(new AllTradesTick() { Second = second, Price = price, Lots = lots });
        }

        public void ClearTicks()
        {
            _ticks.Clear();
        }

        public byte[] Encode()
        {
            List<byte> encodeBytes = new List<byte>();
            uint curSecond = 0;
            int curPrice = 0;

            encodeBytes.AddRange(_ver1Sig); // сигнатура

            foreach (var tick in _ticks)
            {
                _tickBufferCount = 0;
                uint diffSec = (uint)(tick.Second - curSecond);
                EncodeTime(tick.Second, diffSec);
                curSecond = tick.Second;

                int p = (int)Math.Round(tick.Price * _k);
                int dp = p - curPrice;
                EncodePrice(p, dp);
                curPrice = p;

                EncodeLots(tick.Lots);

                encodeBytes.AddRange(_tickBuffer.Take(_tickBufferCount));
            }

            return encodeBytes.ToArray();
        }

        private void EncodePrice(int p, int dp)
        {
            if (dp >= -127 && dp <= 127)
            {
                byte c;
                if (dp >= 0)
                {
                    c = (byte)dp;
                }
                else
                {
                    c = (byte)(-dp - 1 + 0x80);
                }
                _tickBuffer[_tickBufferCount++] = c;
            }
            else
            {
                _tickBuffer[_tickBufferCount++] = 0xff;
                var bytes = BitConverter.GetBytes(p);
                _tickBuffer[_tickBufferCount++] = bytes[0];
                _tickBuffer[_tickBufferCount++] = bytes[1];
                _tickBuffer[_tickBufferCount++] = bytes[2];
                _tickBuffer[_tickBufferCount++] = bytes[3];
            }
        }

        private void EncodeTime(uint s, uint ds)
        {
            if (ds <= 254)
            {
                _tickBuffer[_tickBufferCount++] = (byte)ds;
            }
            else
            {
                _tickBuffer[_tickBufferCount++] = 0xff;
                var bytes = BitConverter.GetBytes(s);
                _tickBuffer[_tickBufferCount++] = bytes[0];
                _tickBuffer[_tickBufferCount++] = bytes[1];
                _tickBuffer[_tickBufferCount++] = bytes[2];
                _tickBuffer[_tickBufferCount++] = bytes[3];
            }
        }

        private void EncodeLots(int lots)
        {
            if (lots >= 0 && lots <= 254)
            {
                _tickBuffer[_tickBufferCount++] = (byte)lots;
            }
            else
            {
                _tickBuffer[_tickBufferCount++] = 0xff;
                var bytes = BitConverter.GetBytes(lots);
                _tickBuffer[_tickBufferCount++] = bytes[0];
                _tickBuffer[_tickBufferCount++] = bytes[1];
                _tickBuffer[_tickBufferCount++] = bytes[2];
                _tickBuffer[_tickBufferCount++] = bytes[3];
            }
        }
        #endregion
    }
}
