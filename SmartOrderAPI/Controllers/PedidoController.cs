using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartOrderAPI.Data;
using SmartOrderAPI.Models;

namespace SmartOrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidoController(AppDbContext context)
        {
            _context = context;
        }

        // Criar ou buscar conta aberta da mesa
        [HttpPost("abrir")]
        public async Task<ActionResult<Pedido>> AbrirOuBuscarConta(int mesaNumero)
        {
            if (mesaNumero < 1 || mesaNumero > 5)
                return BadRequest("Mesa inválida. Temos apenas mesas 1 a 5.");

            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.MesaNumero == mesaNumero && p.Status == "Aberto");

            if (pedido != null)
                return pedido;

            pedido = new Pedido
            {
                MesaNumero = mesaNumero,
                Status = "Aberto",
                ValorTotal = 0
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return pedido;
        }

        // Adicionar item na conta (SEM juntar itens)
        [HttpPost("adicionar-item")]
        public async Task<ActionResult<Pedido>> AdicionarItem(AdicionarItemRequest request)
        {
            if (request.MesaNumero < 1 || request.MesaNumero > 5)
                return BadRequest("Mesa inválida.");

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.MesaNumero == request.MesaNumero && p.Status == "Aberto");

            if (pedido == null)
                return NotFound("Conta não encontrada.");

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == request.ProdutoId && p.Disponivel);

            if (produto == null)
                return NotFound("Produto não encontrado ou indisponível.");

            // SEMPRE cria novo item (não junta)
            var novoItem = new PedidoItem
            {
                PedidoId = pedido.Id,
                ProdutoId = produto.Id,
                Quantidade = request.Quantidade,
                PrecoUnitario = produto.Preco,
                DataCriacao = DateTime.Now,
                Pronto = false
            };

            await _context.PedidoItens.AddAsync(novoItem);
            await _context.SaveChangesAsync();

            // Recalcular total da conta
            pedido.ValorTotal = await _context.PedidoItens
                .Where(i => i.PedidoId == pedido.Id)
                .SumAsync(i => i.Quantidade * i.PrecoUnitario);

            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();

            // Retorna conta atualizada
            var pedidoAtualizado = await _context.Pedidos
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.Id == pedido.Id);

            return Ok(pedidoAtualizado);
        }

        // Ver conta da mesa
        [HttpGet("mesa/{mesaNumero}")]
        public async Task<ActionResult<Pedido>> VerConta(int mesaNumero)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.MesaNumero == mesaNumero && p.Status == "Aberto");

            if (pedido == null)
                return NotFound("Conta não encontrada.");

            return pedido;
        }
    }
}