using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartOrderAPI.Data;

namespace SmartOrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Listar todas as mesas abertas
        [HttpGet("mesas-abertas")]
        public async Task<ActionResult> ListarMesasAbertas()
        {
            var mesas = await _context.Pedidos
                .Where(p => p.Status == "Aberto")
                .Include(p => p.Itens)
                .Select(p => new
                {
                    MesaNumero = p.MesaNumero,
                    ValorTotal = p.ValorTotal,
                    QuantidadeItens = p.Itens.Sum(i => i.Quantidade),
                    DataAbertura = p.DataAbertura
                })
                .ToListAsync();

            return Ok(mesas);
        }
        // Fechar conta da mesa
        [HttpPost("fechar-conta/{mesaNumero}")]
        public async Task<ActionResult> FecharConta(int mesaNumero)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.MesaNumero == mesaNumero && p.Status == "Aberto");

            if (pedido == null)
                return NotFound("Conta aberta não encontrada.");

            pedido.Status = "Pago";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mesa = pedido.MesaNumero,
                Total = pedido.ValorTotal,
                Status = pedido.Status,
                Mensagem = "Conta fechada com sucesso."
            });
        }

        // Remover ou diminuir item da conta
        [HttpPost("remover-item")]
        public async Task<ActionResult> RemoverItem(int mesaNumero, int produtoId, int quantidade)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.MesaNumero == mesaNumero && p.Status == "Aberto");

            if (pedido == null)
                return NotFound("Conta aberta não encontrada.");

            var item = await _context.PedidoItens
                .FirstOrDefaultAsync(i =>
                    i.PedidoId == pedido.Id &&
                    i.ProdutoId == produtoId);

            if (item == null)
                return NotFound("Item não encontrado na conta.");

            if (quantidade >= item.Quantidade)
            {
                _context.PedidoItens.Remove(item);
            }
            else
            {
                item.Quantidade -= quantidade;
                _context.PedidoItens.Update(item);
            }

            await _context.SaveChangesAsync();

            // Recalcular total
            pedido.ValorTotal = await _context.PedidoItens
                .Where(i => i.PedidoId == pedido.Id)
                .SumAsync(i => i.Quantidade * i.PrecoUnitario);

            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mesa = mesaNumero,
                TotalAtualizado = pedido.ValorTotal,
                Mensagem = "Item atualizado com sucesso."
            });
        }
    }
}