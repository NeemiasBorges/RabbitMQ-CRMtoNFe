using System.Text;

namespace DTO
{
    public class CostumerDTO : IDisposable
    {
        private static readonly Random _random = new();
        private static readonly char[] _chars = "aeioudadedido".ToCharArray();
        private static readonly StringBuilder _stringBuilder = new(10);
        private bool _disposed;

        public Guid Id { get; set; }
        public string Nome { get; private set; }
        public string Email { get; private set; }
        public DateTime CreateDate { get; private set; }

        private static readonly char[] _buffer = new char[5];

        public CostumerDTO()
        {
            Nome = string.Empty;
            Email = string.Empty;
            CreateDate = DateTime.Now;
        }

        public CostumerDTO(Guid id, string nome, string email, DateTime createDate)
        {
            Id = id;
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            CreateDate = createDate;
        }

        public CostumerDTO CreateSortCostumer()
        {
            lock (_random)
            {
                return new CostumerDTO(
                    Guid.NewGuid(),
                    GeneratoRandomCaracter(),
                    $"{GeneratoRandomCaracter()}@{GeneratoRandomCaracter()}.com",
                    DateTime.Now);
            }
        }

        public string GeneratoRandomCaracter()
        {
            _stringBuilder.Clear();

            for (int i = 0; i < 5; i++)
            {
                _stringBuilder.Append(_chars[_random.Next(_chars.Length)]);
            }

            return _stringBuilder.ToString();
        }

        public void Reset()
        {
            Id = Guid.Empty;
            Nome = string.Empty;
            Email = string.Empty;
            CreateDate = DateTime.Now;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Reset();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static CostumerDTO Create()
        {
            var customer = new CostumerDTO();
            return customer.CreateSortCostumer();
        }

        public void Regenerate()
        {
            lock (_random)
            {
                Id = Guid.NewGuid();
                Nome = GeneratoRandomCaracter();
                Email = $"{GeneratoRandomCaracter()}@{GeneratoRandomCaracter()}.com";
                CreateDate = DateTime.Now;
            }
        }
    }
}