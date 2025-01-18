namespace DTO
{
    public class CostumerDTO
    {
        public Guid? Id            {  get; init; }
        public string? Nome        { get; set; }
        public string? Email       { get; set; }
        public DateTime CreateDate { get; set; }
        public CostumerDTO() { }
        public CostumerDTO(Guid id, string nome, string email, DateTime createDate)
        {
            Id = id;
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            CreateDate = createDate;
        }

        public CostumerDTO CreateSortCostumer()
        {
            return new CostumerDTO(
                Guid.NewGuid(),
                GeneratoRandomCaracter(), 
                $"{GeneratoRandomCaracter()}@{GeneratoRandomCaracter()}.com", 
                DateTime.Now);
        }

        public string GeneratoRandomCaracter()
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
