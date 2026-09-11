using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    /// <summary>
    /// Repasse simples para o serviço de IA (app_llm, porta 8001) em coisas que
    /// não precisam tocar o banco: dica de fonema (RAG) e health-check.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class IaController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IaController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("dica-fonema")]
        [Authorize]
        public async Task<IActionResult> DicaFonema([FromQuery] string fonema = "A")
        {
            return await Repassar($"/ia/dica-fonema?fonema={Uri.EscapeDataString(fonema)}");
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> Health()
        {
            return await Repassar("/ia/health");
        }

        private async Task<IActionResult> Repassar(string caminho)
        {
            var httpClient = _httpClientFactory.CreateClient("IA");
            try
            {
                var resposta = await httpClient.GetAsync(caminho);
                var corpo = await resposta.Content.ReadAsStringAsync();

                if (!resposta.IsSuccessStatusCode)
                    return StatusCode(502, new { message = "O serviço de IA retornou um erro.", detalhe = corpo });

                return Content(corpo, "application/json");
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { message = "O serviço de IA está indisponível." });
            }
        }
    }
}
