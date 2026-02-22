namespace SmartOrderAPI.Models
{
    public class PedidoItem
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }

        public int ProdutoId { get; set; }

        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }

        public bool Pronto { get; set; } = false;

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public decimal SubTotal => Quantidade * PrecoUnitario;

        // Relacionamentos
        public Produto? Produto { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Pedido? Pedido { get; set; }
    }
}