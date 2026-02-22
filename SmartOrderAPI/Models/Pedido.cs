namespace SmartOrderAPI.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public int MesaNumero { get; set; }

        public DateTime DataAbertura { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Aberto";

        public decimal ValorTotal { get; set; }

        // Relacionamento com os itens do pedido
        public List<PedidoItem> Itens { get; set; } = new();
    }
}