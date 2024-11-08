using Dapper;
using IntegracaoClientes.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace IntegracaoClientes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CostumersController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly ILogger<CostumersController> _logger;

        public CostumersController(SqlConnection connection, ILogger<CostumersController> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Costumer>>> GetAllCostumers()
        {
            try
            {
                var sql = "SELECT * FROM CLIENTES";
                var costumers = await _connection.QueryAsync<Costumer>(sql);

                _logger.LogInformation("BUSCADOS TODOS OS CLIENTES");

                return Ok(costumers);
            }
            catch (Exception ex)
            {
                _logger.LogError("ERRO AO CONSULTAR OS CLIENTES: " + ex.Message);
                return StatusCode(500, "Ocorreu um erro ao buscar os clientes.");
            }
        }

        [HttpGet("Name")]
        public async Task<ActionResult<IEnumerable<Costumer>>> GetCostumerByName([FromHeader] string nome)
        {
            try
            {
                var sql = "SELECT * FROM CLIENTES WHERE NOME LIKE @Nome";
                var costumerDatabase = await _connection.QueryAsync<Costumer>(sql, new
                {
                    Nome = $"%{nome}%"
                });

                _logger.LogInformation("CLIENTE BUSCADO: " + nome);
                return Ok(costumerDatabase);
            }
            catch (Exception ex)
            {
                _logger.LogError("ERRO AO CONSULTAR CLIENTE " + nome + ": " + ex.Message);
                return StatusCode(500, "Ocorreu um erro ao buscar o cliente.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostCostumer(Costumer costumer)
        {
            try
            {
                var sql = "INSERT INTO CLIENTES (CPF, NOME, IDADE, EMAIL) VALUES (@CPF, @Nome, @Idade, @Email)";

                await _connection.ExecuteAsync(sql, new
                    {
                        CPF = costumer.CPF,
                        NOME = costumer.Nome,
                        IDADE = costumer.Idade,
                        EMAIL = costumer.Email
                    }
                );

                _logger.LogInformation("ADICIONADO O CLIENTE: " + costumer.Nome);

                return Ok("CADASTRO REALIZADO!");   
            } catch(Exception ex)
            {
                _logger.LogError("ERRO NO CADASTRO: " + ex.Message);
                return StatusCode(500, "Ocorreu um erro ao cadastrar o cliente.");
            }
        }

        [HttpPut("{cpf}")]
        public async Task<ActionResult> UpdateCostumer(string cpf, [FromBody] Costumer costumer)
        {
            try
            {
                var sql = "UPDATE CLIENTES SET NOME = @Nome, IDADE = @Idade, EMAIL = @Email WHERE CPF = @CPF";

                await _connection.ExecuteAsync(sql, new
                    {
                        CPF = cpf,
                        Nome = costumer.Nome,
                        Idade = costumer.Idade,
                        Email = costumer.Email
                    }
                );

                _logger.LogInformation($"CLIENTE ATUALIZADO COM SUCESSO: {cpf}");
                return StatusCode(200, $"CLIENTE ATUALIZADO COM SUCESSO: {cpf}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"ERRO AO ATUALIZAR O CLIENTE: {ex.Message}");
                return StatusCode(500, $"ERRO AO ATUALIZAR O CLIENTE: {ex.Message}");
            }
        }

        [HttpDelete("{cpf}")]
        public async Task<ActionResult<Costumer>> DeleteCostumer(string cpf)
        {
            try
            {
                var sql = "DELETE FROM CLIENTES WHERE CPF = @CPF";

                await _connection.ExecuteAsync(sql, new
                    {
                        CPF = cpf
                    }
                );

                _logger.LogInformation("CLIENTE DELETADO:");
                _logger.LogInformation($"CPF: {cpf}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"OCORREU UM ERRO AO DELETAR O CLIENTE {ex.Message}");
                return StatusCode(500, $"OCORREU UM ERRO AO DELETAR O CLIENTE {ex.Message}");
            }
        }
    }
}
