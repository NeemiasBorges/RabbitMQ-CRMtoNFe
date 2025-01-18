using System.Collections.Concurrent;

namespace DTO
{
    public class CostumerDTOPool
    {
        private readonly ConcurrentBag<CostumerDTO> _pool = new();
        private readonly int _maxSize;

        public CostumerDTOPool(int maxSize = 1000)
        {
            _maxSize = maxSize;
        }

        public CostumerDTO Rent()
        {
            if (_pool.TryTake(out var customer))
            {
                customer.Regenerate();
                return customer;
            }

            return CostumerDTO.Create();
        }

        public void Return(CostumerDTO customer)
        {
            if (_pool.Count < _maxSize)
            {
                customer.Reset();
                _pool.Add(customer);
            }
            else
            {
                customer.Dispose();
            }
        }
    }
}

