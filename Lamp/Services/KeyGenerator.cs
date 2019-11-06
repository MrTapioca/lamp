using Lamp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Services
{
    public class KeyGenerator : IKeyGenerator
    {
        readonly char[] _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private Random _random;

        public KeyGenerator()
        {
            _random = new Random();
        }

        public string GenerateKey()
        {
            return GenerateKey(8);
        }

        public string GenerateKey(int length)
        {
            string key = "";
            
            for (int i = 1; i <= length; i++)
            {
                int index = _random.Next(_chars.Length);
                key += _chars[index];
            }

            return key;
        }
    }
}