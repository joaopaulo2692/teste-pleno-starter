Alterações no Sistema Parking
1. Validação de Clientes (PUT /api/clientes)

Adicionada validação para impedir que dois clientes diferentes possuam o mesmo par Nome + Telefone.
Adicionada Validação com ModelState para pegar dados requeridos dos DTOS, para isso foi necessário alterar o "AddControllers" no Program, para retornar para o front a mensagem estruturada.
Retorna HTTP 409 (Conflict) com mensagem descritiva quando a combinação já existe, permitindo ao front-end exibir o erro de forma clara.

2. Edição Completa de Veículos

Permitida alteração de modelo, ano e cliente associado.

Implementada geração e fechamento automático de histórico de vínculo entre veículo e cliente.

Criada migration para a tabela VeiculoHistorico, armazenando histórico necessário para faturamento proporcional.

3. Upload CSV

Modificado o código da página CsvUploadPage para apresentar erros e resultados em um grid, detalhando a linha e motivo de cada falha.

4. Faturamento Proporcional

Refatorado FaturamentoService.GerarAsync para calcular valores proporcionais com base nas datas de início e fim de associação dos veículos (VeiculosHistorico), ao invés de usar apenas o dono atual.

Implementada idempotência: faturas já existentes para a mesma competência não são duplicadas.

Cada veículo é adicionado uma única vez na fatura, evitando duplicações no rastreamento do EF Core.

5. Validação de Placas

Atualizado PlacaService para aceitar tanto o formato antigo (ABC1234) quanto o novo padrão Mercosul (ABC1D23).

6. Front-end de Veículos

Permitida a troca de cliente associado ao veículo diretamente na tela.

Mensagens de sucesso e erro exibidas ao criar ou atualizar veículos.

Criada nova migration para aplicar Cascade Delete na tabela de histórico, garantindo que ao deletar um veículo seu histórico seja removido automaticamente.

Nota: Em sistemas críticos, o uso de CreatedAt e DisabledAt seria mais seguro para manter histórico sem perda de dados.