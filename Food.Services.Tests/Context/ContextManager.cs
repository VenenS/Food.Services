using System;

namespace Food.Services.Tests.Context
{
    static class ContextManager
    {
        private static FakeContext _context;

        public static void Set(FakeContext context)
        {
            _context = context;
        }

        public static FakeContext Get()
        {
            if(_context == null)
            { throw new ArgumentNullException("", "FakeContext should be implemented.");}
            return _context;
        }
    }
}
