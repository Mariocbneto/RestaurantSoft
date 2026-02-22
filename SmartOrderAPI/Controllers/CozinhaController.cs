using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartOrderAPI.Data;

namespace SmartOrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CozinhaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CozinhaController(AppDbContext context)
        {
            _context = context;
        }

        // Listar pedidos pendentes agrupados por envio (Mesa + DataCriacao)
        [HttpGet("pedidos")]
        public async Task<ActionResult> ListarPedidos()
        {
            var itensPendentes = await _context.PedidoItens
                .Where(i => !i.Pronto && i.Pedido!.Status == "Aberto")
                .Include(i => i.Pedido)
                .Include(i => i.Produto)
                .ToListAsync();

            var resultado = itensPendentes
                .OrderBy(i => i.DataCriacao) // mais antigo primeiro
                .GroupBy(i => new
                {
                    i.Pedido!.MesaNumero,
                    i.DataCriacao
                })
                .Select(g => new
                {
                    Mesa = g.Key.MesaNumero,
                    DataCriacao = g.Key.DataCriacao,
                    Itens = g.Select(i => new
                    {
                        ItemId = i.Id,
                        Produto = i.Produto != null ? i.Produto.Nome : "Produto desconhecido",
                        Quantidade = i.Quantidade,
                        DataCriacao = i.DataCriacao
                    })
                })
                .ToList();

            return Ok(resultado);
        }

        // Marcar item como pronto
        [HttpPost("marcar-pronto/{itemId}")]
        public async Task<ActionResult> MarcarPronto(int itemId)
        {
            var item = await _context.PedidoItens
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
                return NotFound("Item não encontrado.");

            item.Pronto = true;

            await _context.SaveChangesAsync();

            return Ok("Item marcado como pronto.");
        }
    }
}