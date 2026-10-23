# Area Gourmet

Sistema web de atendimento e gestão de marmitaria no **projeto original**, com ASP.NET Core MVC/Razor (.NET 10), EF Core 9, Pomelo 9 e MySQL. A rota de saúde original foi preservada e agora exige autenticação.

## O que está integrado

- Login por cookie, hash PBKDF2 via `PasswordHasher`, perfis Administrador/Caixa, revogação de sessão após alterações de acesso, CSRF e limitação de tentativas de login.
- Clientes, produtos e categorias: cadastro, detalhes, edição, filtro no MySQL e desativação lógica. Clientes têm múltiplos endereços.
- Vendas com Mesa, Retirada e Delivery, cliente por telefone, comanda, quantidades, PIX/Cartão/Dinheiro, taxa e cálculo no servidor.
- Pedidos abertos ou finalizados, acompanhamento de preparo/entrega, preços e endereço congelados no momento da venda, idempotência e operador autenticado.
- Estoque próprio de bebidas/produtos e consumo associado de embalagens, entradas/saídas, baixa na finalização e estorno administrativo no cancelamento.
- Relatórios LINQ: pedidos/cliente/operador/itens, somas, pagamentos, modalidades, produtos mais vendidos e pivot por dia/modalidade.
- Dados fictícios de demonstração repetíveis, auditoria e interface responsiva inspirada na referência fornecida.

Veja [a matriz de conformidade](docs/CONFORMIDADE.md), [as regras de negócio](docs/REGRAS.md), [o diagnóstico](docs/DIAGNOSTICO.md) e [os testes](docs/TESTES.md).

## Pré-requisitos

- SDK **.NET 10** (validado com 10.0.402).
- **MySQL 8.4** (validado com 8.4.13), banco `area_gourmet` em utf8mb4 e usuário com permissões neste banco. Para criar ou migrar o esquema são necessárias permissões DDL; em produção separe a conta de migração da conta de execução.
- Git e, opcionalmente, VS Code com C# Dev Kit.
- Ferramenta `dotnet-ef` 9.0.0 fixada no manifesto `.config/dotnet-tools.json`. Os pacotes existentes foram mantidos; EF/Pomelo continuam na mesma versão principal.

Não coloque senhas no código, em commits, no README ou em arquivos `.http`. `appsettings.json` contém a conexão vazia por intenção. Use variáveis do processo ou User Secrets em Development (`UserSecretsId` já configurado). Não há administrador com senha fixa nem criação automática de usuários na inicialização.

## Windows: passo a passo com VS Code ou terminal

1. Abra a pasta **AreaGourmet.Api** no VS Code e o terminal PowerShell. Todos os comandos abaixo são executados na pasta que contém `AreaGourmet.Api.csproj`.
2. Inicie seu MySQL e prepare o banco `area_gourmet`. Use uma conta própria de desenvolvimento. Se já existe banco, faça backup e confira seu histórico de migrações antes de prosseguir.
3. Configure a conexão sem ecoar a senha no terminal ou gravá-la no histórico:

```powershell
$cs = Read-Host 'Conexão MySQL completa (Server=...;Database=area_gourmet;User=...;Password=...)' -AsSecureString
$env:ConnectionStrings__DefaultConnection = [System.Net.NetworkCredential]::new('', $cs).Password
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ROLL_FORWARD = 'Major'
dotnet restore
dotnet tool restore
dotnet build --no-restore
```

`DOTNET_ROLL_FORWARD=Major` permite executar a ferramenta EF 9 com somente o runtime 10 instalado. Não é necessário trocar as referências NuGet.

4. **Revise as migrations antes de aplicá-las**:

```powershell
dotnet ef migrations list
# Para um banco novo, gere o script completo para inspeção:
dotnet ef migrations script --output "$env:TEMP\area-gourmet-schema.sql"
# Só depois de conferir banco de destino, backup e script:
dotnet ef database update
```

A migration nova `GestaoIntegrada` adiciona colunas/tabelas e troca exclusões em cascata por `RESTRICT`. Ela não apaga registros nem altera a precisão das colunas antigas. A migration histórica `AjustarEstoqueParaBebidas`, anterior a esta implementação, **remove EstoqueInsumos**. Se seu banco antigo ainda precisa aplicá-la e contém insumos, **pare antes do update**: preserve e mapeie esses dados em um procedimento específico. Não reverta migrations para tentar corrigir erros; `Down` pode eliminar dados. No banco deste ambiente as quatro migrations antigas já estavam aplicadas antes do desenvolvimento.

5. Crie o **primeiro Administrador**, exclusivamente pelo terminal local:

```powershell
$env:Bootstrap__Login = Read-Host 'Login do primeiro administrador'
$senha = Read-Host 'Senha forte (12 a 128 caracteres)' -AsSecureString
$env:Bootstrap__Senha = [System.Net.NetworkCredential]::new('', $senha).Password
dotnet run --no-build --no-launch-profile -- --criar-admin
Remove-Item Env:Bootstrap__Login, Env:Bootstrap__Senha
$senha = $null
```

O comando cria hash, não imprime a senha e recusa sobrescrever usuários ou executar quando já existe administrador ativo. Não compartilhe essas variáveis com o processo do servidor. Para os próximos usuários e redefinições de senha, use **Usuários** como Administrador. Alterar senha/perfil/permissão ou desativar conta invalida as sessões anteriores. O último administrador ativo não pode ser rebaixado/desativado.

6. Inicie o sistema:

```powershell
dotnet run --launch-profile http
```

Abra o endereço informado pelo terminal, entre e acesse **Painel → Gerar dados de demonstração**. Essa ação existe somente em Development e exige Administrador. Depois, crie uma conta Caixa em **Usuários** e teste a troca de operador pelo botão de saída. A aplicação não aplica migrações nem gera dados implicitamente na inicialização.

## Linux / ambiente de nuvem

Use o mesmo fluxo na pasta do projeto. Exporte `ConnectionStrings__DefaultConnection`, `ASPNETCORE_ENVIRONMENT=Development` e `DOTNET_ROLL_FORWARD=Major`. Para entrada segura da conexão no Bash:

```bash
read -r -s -p 'Conexão MySQL: ' ConnectionStrings__DefaultConnection
export ConnectionStrings__DefaultConnection
export ASPNETCORE_ENVIRONMENT=Development DOTNET_ROLL_FORWARD=Major
dotnet restore
dotnet tool restore
dotnet build --no-restore
# Inspecione o script e o histórico, faça backup e só então aplique:
dotnet ef migrations list
dotnet ef database update
```

Primeiro administrador:

```bash
read -r -p 'Login: ' Bootstrap__Login
read -r -s -p 'Senha (12–128 caracteres): ' Bootstrap__Senha
export Bootstrap__Login Bootstrap__Senha
dotnet run --no-build --no-launch-profile -- --criar-admin
unset Bootstrap__Login Bootstrap__Senha
dotnet run --launch-profile http
```

Na instância de nuvem preparada, os auxiliares e as credenciais MySQL locais estão **fora do checkout**, em `/workspace/.cloud-setup`. `source /workspace/.cloud-setup/activate.sh` ativa o SDK e a conexão, sem imprimir valores. `bash /workspace/.cloud-setup/start.sh` inicia MySQL/API; não cria o administrador. Serviços precisam reiniciar em novas tarefas. Nenhum usuário foi criado no banco principal durante os testes; os usuários e pedidos fictícios ficam no banco separado `area_gourmet_validacao`. Não publique esse banco como dados reais.

## Demonstração acadêmica

1. Entre como Administrador, abra o painel e gere os dados de demonstração (duas vezes para mostrar que não duplica).
2. Mostre os CRUDs Clientes, Produtos e Categorias com filtros e validação. No cliente fictício há Casa e Trabalho. Registre um terceiro endereço.
3. Crie Caixa com senha inicial própria e permissão de cadastro de clientes. Saia e entre com esse perfil. Tente abrir uma página administrativa: o servidor nega o acesso.
4. Em Vendas, monte Mesa (número obrigatório), Retirada e Delivery (cliente, endereço e taxa). Salve uma comanda aberta para mostrar o fluxo de preparo e finalize as demais.
5. Confira os pedidos e o operador. Altere o preço de um produto como Administrador: o pedido anterior conserva o preço.
6. Em Estoque e Movimentações, confira a baixa de bebidas e embalagens. Uma saída superior ao saldo é rejeitada. Cancele uma venda finalizada como Administrador e confira o estorno único.
7. Em Relatórios, filtre por período, produto, categoria, pagamento e modalidade. Mostre totalizações, agrupamento e a pivot Mesa/Retirada/Delivery. Cancelados e abertos não entram nas receitas.
8. Mostre a auditoria e os commits reais. A pontuação do GitHub e a avaliação individual são do professor; os testes não garantem nota.

## Segurança, dados antigos e limites

- Contas antigas sem hash não são autenticadas como se fossem hashes. O Administrador deve redefinir suas senhas por um canal seguro. A migração não tenta descobrir, imprimir ou converter senhas antigas. No banco inicial deste ambiente não existiam contas.
- As ilustrações locais são demonstrativas e podem ser substituídas por fotos próprias. A aparência foi inspirada na imagem fornecida; não é uma cópia pixel a pixel. A aplicação funciona sem CDN, fontes ou imagens externas.
- Datas persistidas e filtros usam **UTC**, indicado nas telas. Não há conversão silenciosa de fuso em relatórios.
- Produção exige HTTPS, configuração de proxy confiável quando aplicável, gestão persistente/protegida das chaves Data Protection e backups. A demonstração usa HTTP somente em Development. Não habilite Development com dados reais expostos à Internet.
- A autorização protege páginas e a API, incluindo operações diretas. O usuário Caixa pode consultar o histórico operacional, mas só Administrador cancela pedidos, altera preços, estoque, usuários e relatórios.
- O sistema não integra adquirente de cartão/PIX: registra o meio de pagamento informado. Não envia cobranças, mensagens ou pedidos para terceiros.
- Para alterações no banco existente, trate backup e revisão de DDL como requisitos. Não execute testes de integração em produção.
